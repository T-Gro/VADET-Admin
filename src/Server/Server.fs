open System.IO
open System.Threading.Tasks

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open FSharp.Control.Tasks.V2
open Giraffe
open Saturn
open Shared

open Fable.Remoting.Server
open Fable.Remoting.Giraffe

let rnd = new System.Random()
let tryGetEnv = System.Environment.GetEnvironmentVariable >> function null | "" -> None | x -> Some x

let publicPath = Path.GetFullPath "../Client/public"

let port = "SERVER_PORT" |> tryGetEnv |> Option.map uint16 |> Option.defaultValue 8085us

let load () = task{return {Candidates = Map.empty}}
let reject (rejection:RejectionOfAttribute) = task {
    return {
                rejection.Subject with Status = Rejected(System.DateTime.UtcNow, rejection.Reason)                
            }}

let accept (acc:AcceptedAttribute) = task {
    return {
                acc.Candidate with Status = Accepted(System.DateTime.UtcNow, acc.NewName)                
            }}

let expand cand = task {
        return {
              Candidate = cand;
              Neighbors = List.init (rnd.Next(5,70)) (fun i -> {Hit = ImageId (i|>string); Distance =float i / 100.0; Accepted = false})}}

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

let app = application {
    url ("http://0.0.0.0:" + port.ToString() + "/")
    use_router webApp
    memory_cache
    use_static publicPath
    use_gzip
}

run app
