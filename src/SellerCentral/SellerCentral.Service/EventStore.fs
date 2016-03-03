[<RequireQualifiedAccess>]
module EventStore

open System
open System.Net
open EventStore.ClientAPI

let conn endPoint =
    let conn = EventStoreConnection.Create(endPoint)
    conn.Connect()
    conn

let makeRepository (conn:IEventStoreConnection) category (serialize:obj -> string * byte array, deserialize: Type * string * byte array -> obj) =

    let streamId(id:Guid) = category + "-" + id.ToString("N")

    let load(t,id) = async {
        let streamId = streamId id
        let! slice = conn.ReadStreamEventsForwardAsync(streamId, 1, Int32.MaxValue, false) |> Async.AwaitTask
        return slice.Events |> Seq.map (fun e -> deserialize(t, e.Event.EventType, e.Event.Data))
    }

    let commit (id, expectedVersion) e = async {
        let streamId = streamId id
        let eventType, data = serialize e
        let metadata = [||] : byte array
        let eventData = new EventData(Guid.NewGuid(), eventType, true, data, metadata)
        return! conn.AppendToStreamAsync(streamId, expectedVersion, eventData) |> Async.AwaitIAsyncResult |> Async.Ignore
    }
    
    load, commit

