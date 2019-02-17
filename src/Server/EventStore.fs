module EventStore 
    open System.Collections.Generic
    open System
    open System.IO
    open System.Configuration
    open Shared

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