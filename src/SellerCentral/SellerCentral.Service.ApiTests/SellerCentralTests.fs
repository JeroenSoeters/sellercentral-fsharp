module SellerCentralApiTests

open System.Net
open System.Net.Http
open System.Web.Http
open System.Web.Http.Owin
open FsUnit
open NUnit.Framework
open Microsoft.Owin.Hosting

[<Test>]
let ``seller central should be up and running`` () =
    use server = WebApp.Start("", Program.startup)
    use client = new HttpClient()

    let response = client.GetAsync("http://localhost:8888/Merchants").Result

    response.StatusCode |> should equal HttpStatusCode.OK