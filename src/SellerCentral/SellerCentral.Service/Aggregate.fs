[<RequireQualifiedAccess>]
module Aggregate

type Aggregate<'TState, 'TCommand, 'TEvent> = {
    zero : 'TState;
    apply: 'TState -> 'TEvent -> 'TState;
    exec: 'TState -> 'TCommand -> Choice<'TEvent, string list>
}

type Id = System.Guid

let makeHandler (aggregate:Aggregate<'TState, 'TCommand, 'TEvent>) (load:System.Type * Id -> Async<obj seq>, commit:Id * int -> obj -> Async<unit>) =
    fun(id, version) command -> async {
        let! events = load (typeof<'TEvent>,id)
        let events = events |> Seq.cast :> 'TEvent seq
        let state = Seq.fold aggregate.apply aggregate.zero events
        let event = aggregate.exec state command
        match event with
        | Choice1Of2 event ->
            let! _ = event |> commit (id, version)
            return Choice1Of2()
        | Choice2Of2 errors ->
            return errors |> Choice2Of2
    }