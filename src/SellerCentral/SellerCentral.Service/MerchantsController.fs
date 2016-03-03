namespace SellerCentral.Service

open System
open System.Net
open System.Net.Http
open System.Web
open System.Web.Http

open Newtonsoft.Json

[<CLIMutable>]
[<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
type MerchantApplicationModel = {
    companyName : string;
    displayName : string
}

type MerchantsController() as self =
    inherit ApiController()
    
    let handleCommand' =
        Aggregate.makeHandler
            { zero = Merchant.State.Zero; apply = Merchant.apply; exec = Merchant.exec }
            ( EventStore.makeRepository Global.EventStore.Value "merchant" Serialization.serializer )

    let handleCommand (id, v) c = handleCommand' (id, v) c |> Async.RunSynchronously

    member x.Get() = 
        self.Content(HttpStatusCode.OK, "Welcome to Hepsiburada Seller Central!")
    
    member x.Post([<FromBody>] application:MerchantApplicationModel) =
        let id = Guid.NewGuid()
        let result = Merchant.Apply(application.companyName, application.displayName) |> handleCommand (id, -1)
        match result with
        | Choice1Of2 _ -> self.Created("http://localhost:8888/Merchants/" + id.ToString(), ()) :> IHttpActionResult
        | Choice2Of2 e -> self.BadRequest(JsonConvert.SerializeObject(e)) :> IHttpActionResult

