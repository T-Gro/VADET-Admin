module EventStore 
    open System.Collections.Generic
    open System
    open System.IO
    open System.Configuration
    open Shared
    open KnnResults.Domain.Models
    open Shared.Common

    type TimedEvent<'a> = {Data : 'a; Time : DateTime}

    let loadExistingRejections () =
        use dbContext = new VADETContext()
        let source = ConfigurationManager.AppSettings.["FilteredPatchesBin"]
        query {
            for attr in dbContext.AttributeRejections do
                where (attr.AttributeSource = source)
                select (attr.OriginalProposalId,{Time = attr.Time; Data = attr.Reason})
        }
        |> dict

    let Rejections = loadExistingRejections()

    let loadExistingApprovals() =
        use dbContext = new VADETContext()
        let source = ConfigurationManager.AppSettings.["FilteredPatchesBin"]
        query {
            for attr in dbContext.VisualAttributeDefinition do
                where (attr.AttributeSource = source)
                select (attr.OriginalProposalId,(attr.CreatedAt.Value,attr.Name))
        }
        |> dict

    let accept (acc:AcceptedAttribute) =
        use dbContext = new VADETContext()
        let imageDistances = acc.AcceptedMatches |> List.map (fun n -> (extractImgId n.Hit, n.Distance, n.Patches.Length))
        let rejected = imageDistances |> List.filter (fun (id,_,_) -> id.Contains("rejected.png"))
        let accepted = imageDistances |> List.filter (fun (id,_,_) -> id.Contains("rejected.png") |> not)
        
        let visAttr = new VisualAttributeDefinition()
        visAttr.OriginalProposalId <- acc.Candidate.Id
        visAttr.AttributeSource <- ConfigurationManager.AppSettings.["FilteredPatchesBin"]
        visAttr.Candidates <- sprintf "%A" acc.Candidate.Representatives
        visAttr.Quality <- acc.Quality
        visAttr.DiscardedCategories <- sprintf "%A" acc.IgnoredCategories
        visAttr.DiscardedProducts <- sprintf "%A" (rejected |> List.map (fun (id,_,_) -> id.Substring("rejected.png?orig=".Length)))
        visAttr.DistanceTreshold <- imageDistances |> List.map (fun (_,dist,_) -> float dist) |> List.tryLast |> Option.toNullable
        visAttr.Name <- acc.NewName
        visAttr.User <- acc.Username

        for (name,distance,coverage) in accepted do
            let cleanId = Path.GetFileNameWithoutExtension name
            visAttr.ProductVisualAttributes.Add(new ProductVisualAttributes(Distance = float distance, Coverage = float coverage, Attribute = visAttr, ProductId = cleanId))

        dbContext.VisualAttributeDefinition.Add visAttr |> ignore
        let saved = dbContext.SaveChanges()        
        printfn "Saved = %i results" saved

        visAttr
        

    let reject (rej:RejectionOfAttribute) =
        use dbContext = new VADETContext()
        let r = new AttributeRejection()
        r.Reason <- rej.Reason
        r.AttributeSource <- ConfigurationManager.AppSettings.["FilteredPatchesBin"]
        r.OriginalProposalId <- rej.Subject.Id
        r.Content <- sprintf "%A" rej.Subject.Representatives
        r.User <- rej.Username

        dbContext.AttributeRejections.Add r |> ignore
        let saved = dbContext.SaveChanges()        
        printfn "Saved = %i results" saved

        let ev = {Time = r.Time; Data = rej.Reason}
        Rejections.Add(rej.Subject.Id, ev)

        ev
        