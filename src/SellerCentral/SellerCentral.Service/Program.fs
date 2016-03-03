module Program

open Microsoft.Owin.Hosting
open Owin
open System.Web.Http
open System.Web.Http.Owin

type HttpRouteDefaults = { Controller : string; Id : obj }

let host = "http://localhost:8888"

let startup (app: Owin.IAppBuilder) =
    let config = new HttpConfiguration()
    config.Routes.MapHttpRoute(
        "DefaultAPI",
        "{controller}/{id}",
        { Controller = "Merchants"; Id = RouteParameter.Optional }) |> ignore
    app.UseWebApi(config) |> ignore

[<EntryPoint>]
let main argv =
    printfn "Starting Hepsiburada Seller Central.."
    let disposable = WebApp.Start(host, startup)
    printfn "Seller Central started, press <enter> to quit"    
    System.Console.ReadLine() |> ignore
    disposable.Dispose()
    0

