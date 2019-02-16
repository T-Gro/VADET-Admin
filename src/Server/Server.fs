open System.IO
open System.Threading.Tasks
open System.Configuration
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open FSharp.Control.Tasks.V2
open Giraffe
open Saturn
open Shared

open Fable.Remoting.Server
open Fable.Remoting.Giraffe



module EventStore =
    open System.Collections.Generic
    open System

    type TimedEvent<'a> = {Data : 'a; Time : DateTime}
    type CompactRejection = {Reason : string}
    type CompactApproval = {AttrName : string; AcceptedNeighbours : int}
    type Rejections = Dictionary<int,TimedEvent<CompactRejection>>
    type Approvals = Dictionary<int,TimedEvent<CompactApproval>>
    let timed x = {Data = x; Time = DateTime.UtcNow}

    let load<'a> name =
        let fileName = Path.Combine(ConfigurationManager.AppSettings.["Storage"],name)
        if File.Exists fileName then
            let data = File.ReadAllText(fileName)
            let obj = Newtonsoft.Json.JsonConvert.DeserializeObject<'a>(data)            
            Some obj
        else
            None

    let save obj name =
        let fileName = Path.Combine(ConfigurationManager.AppSettings.["Storage"],name)
        let json = Newtonsoft.Json.JsonConvert.SerializeObject(obj,Newtonsoft.Json.Formatting.Indented)
        File.WriteAllTextAsync(fileName,json) |> ignore

    let Rejections = load<Rejections> "Attribute-Rejections.json" |> Option.defaultValue (new Rejections())
       
    let Approvals = load<Approvals> "Attribute-Approvals.json" |> Option.defaultValue (new Approvals())
       

    let accept (acc:AcceptedAttribute) =
        let ev = timed {AttrName = acc.NewName;AcceptedNeighbours = acc.AcceptedMatches |> List.length}
        Approvals.Add(acc.Candidate.Id, ev)
        save Approvals "Attribute-Approvals.json" 
        ev

    let reject (rej:RejectionOfAttribute) =
        let ev = timed {Reason = rej.Reason}
        Rejections.Add(rej.Subject.Id, ev)
        save Rejections "Attribute-Rejections.json"
        ev


module PatchProposals =
    open KnnResults.Domain

    let Loaded = AllResults.Load(ConfigurationManager.AppSettings.["FilteredPatchesBin"])    
    let NameMap = Loaded.ImageEncoding |> Seq.map (fun kvp -> (kvp.Value,kvp.Key)) |> Map.ofSeq
    let PatchMap = Loaded.PatchEncoding |> Seq.map (fun kvp -> (kvp.Value,kvp.Key)) |> Map.ofSeq
    let IdMap = Loaded.Rows |> Seq.mapi (fun i x -> (x.Query, i)) |> dict
    let createCandidate (r : ResultsRow) =
        let imgs =
            [yield r.Query; yield! (r.Hits |> Seq.map (fun x -> x.Hit))]
            |> List.map (fun p -> (ImageId(NameMap.[p.ImageId]),PatchId(PatchMap.[p.PatchId])))
        let id = IdMap.[r.Query]
        let status =
            let (isRej,rej) = EventStore.Rejections.TryGetValue id
            let (isAcc,acc) = EventStore.Approvals.TryGetValue id
            match (isRej,isAcc) with
                | (true,false) -> Rejected(rej.Time, rej.Data.Reason) 
                | (_,true) -> Accepted(acc.Time, acc.Data.AttrName)
                | _ -> Offered       

        {Representatives = imgs; Status = status; Id = id}

let load () = task{
    let firstItems =
        PatchProposals.Loaded.Rows
        |> Seq.rev
        |> Seq.map PatchProposals.createCandidate
        |> Seq.filter (fun c -> c.Status = Offered)
        |> Seq.take 200
    let withIds = firstItems  |> List.ofSeq
    return
        { Candidates = withIds}
    }
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

let rnd = new System.Random()
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

let tryGetEnv = System.Environment.GetEnvironmentVariable >> function null | "" -> None | x -> Some x
let publicPath = Path.GetFullPath "../Client/public"
let port = "SERVER_PORT" |> tryGetEnv |> Option.map uint16 |> Option.defaultValue 8085us

let app = application {
    url ("http://0.0.0.0:" + port.ToString() + "/")
    use_router webApp
    memory_cache
    use_static publicPath
    use_gzip
}

run app
