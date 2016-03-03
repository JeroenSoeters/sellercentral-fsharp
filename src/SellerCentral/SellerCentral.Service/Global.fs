module Global

let EventStore = lazy(
    let endPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 1113)
    EventStore.conn endPoint)


