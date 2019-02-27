open System.IO
open FSharp.Control.Tasks.V2
open Saturn
open Shared
open Fable.Remoting.Server
open Fable.Remoting.Giraffe


let load () = task{   return  { Candidates = PatchProposals.loadInitialDisplay()}  }
let reject (rejection:RejectionOfAttribute) = task {
    let ev = EventStore.reject rejection
    return {
                rejection.Subject with Status = Rejected(ev.Time, rejection.Reason)                
            }}

let accept (acc:AcceptedAttribute) = task {
    let ev = EventStore.accept acc
    return {                
                acc.Candidate with Status = Accepted(ev.Time, acc.NewName)                
            }}

let expand cand = task {
        return {
              Candidate = cand;
              Neighbors = PatchProposals.expand cand }}

let counterApi = {      
    load = load >> Async.AwaitTask;
    rejectOfferedAttribute = reject >> Async.AwaitTask;
    acceptNewAttribute = accept >> Async.AwaitTask;
    expandCandidate = expand >> Async.AwaitTask
}

let webApp =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue counterApi
    |> Remoting.buildHttpHandler

let tryGetEnv = System.Environment.GetEnvironmentVariable >> function null | "" -> None | x -> Some x
let publicPath = Path.GetFullPath "../Client/public"
let port = "SERVER_PORT" |> tryGetEnv |> Option.map uint16 |> Option.defaultValue 8085us

let app = application {
#if DEBUG
    url ("http://0.0.0.0:" + port.ToString() + "/")
#else
    use_iis
#endif      
    use_router webApp
    memory_cache
    use_static publicPath
    
}

run app
