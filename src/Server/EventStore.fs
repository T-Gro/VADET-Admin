module EventStore 
    open System.Collections.Generic
    open System
    open System.IO
    open System.Configuration
    open Shared
    open KnnResults.Domain.Models
    open Shared.Common

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
       
    //let Approvals = load<Approvals> "Attribute-Approvals.json" |> Option.defaultValue (new Approvals())
       

    let accept (acc:AcceptedAttribute) =
        use dbContext = new VADETContext()
        let imageDistances = acc.AcceptedMatches |> List.map (fun n -> (extractImgId n.Hit, n.Distance, n.Patches.Length))
        let rejected = imageDistances |> List.filter (fun (id,_,_) -> id.Contains("rejected.png"))
        let accepted = imageDistances |> List.filter (fun (id,_,_) -> id.Contains("rejected.png") |> not)

        let visAttr = new VisualAttributeDefinition()
        visAttr.Id <- acc.Candidate.Id
        visAttr.Candidates <- sprintf "%A" acc.Candidate.Representatives
        visAttr.DiscardedCategories <- sprintf "%A" acc.IgnoredCategories
        visAttr.DiscardedProducts <- sprintf "%A" (rejected |> List.map (fun (id,_,_) -> id.Substring("rejected.png?orig=".Length)))
        visAttr.DistanceTreshold <- imageDistances |> List.map (fun (_,dist,_) -> float dist) |> List.tryLast |> Option.toNullable
        visAttr.Name <- acc.NewName

        for (name,distance,coverage) in accepted do
            let cleanId = Path.GetFileNameWithoutExtension name
            visAttr.ProductVisualAttributes.Add(new ProductVisualAttributes(Distance = float distance, Coverage = float coverage, Attribute = visAttr, ProductId = cleanId))
            
        printf "Saved = %i results" (dbContext.SaveChanges())

        visAttr
        

    let reject (rej:RejectionOfAttribute) =
        let ev = timed {Reason = rej.Reason}
        Rejections.Add(rej.Subject.Id, ev)
        save Rejections "Attribute-Rejections.json"
        ev