module PatchProposals 
    open KnnResults.Domain
    open System.Configuration
    open Shared

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

    let loadInitialDisplay() =
        let firstItems =
            Loaded.Rows
            |> Seq.rev
            |> Seq.map createCandidate
            |> Seq.filter (fun c -> c.Status = Offered)
            |> Seq.take 200
        firstItems  |> List.ofSeq

    let findHits (ImageId(i),PatchId(p)) = OverallDataAccessor.FindHitsInBigFile(i,p) |> Seq.toArray


    type QueryResultPair = {Query : ImagePatch; FoundHit : NamedHit}
    let expand (ac : AttributeCandidate) =
        let queryResults c =
            let knn = findHits c
            let maxDist = knn |> Array.map (fun nh -> nh.Distance) |> Array.max
            let byImageLookup = knn |> Seq.groupBy (fun nh -> nh.Img) |> Map.ofSeq
            let getDist img = byImageLookup |> Map.tryFind img |> Option.map (fun r -> r |> Seq.map (fun nh -> nh.Distance) |> Seq.min ) |> Option.defaultValue maxDist
            let foundImages = knn |> Seq.map (fun nh -> nh.Img) |> Seq.distinct |> Set.ofSeq

            (getDist, byImageLookup, foundImages, c)

        let combinedResults = ac.Representatives |> List.map queryResults
        let allFoundImages = combinedResults |> Seq.map (fun (_,_,found,_) -> found) |> Set.unionMany
        let withAvgDistance = allFoundImages |> Seq.map(fun img -> (img, combinedResults |> List.averageBy (fun (gd,_,_,_) -> gd img)))
        let bestPicks = withAvgDistance |> Seq.sortBy snd

        let relevantReprs = ac.Representatives |> Seq.take 1
        let knn =
            relevantReprs
            |> Seq.map findHits
            |> Seq.zip relevantReprs
            |> Seq.collect (fun (cand,hits) -> hits |> Seq.map (fun h -> {Query = cand; FoundHit = h}))
            |> Seq.groupBy (fun h -> h.FoundHit.Img)
            |> Seq.sortBy (fun (key,g) -> (g |> Seq.minBy (fun qrp -> qrp.FoundHit.Distance)).FoundHit.Distance )

        let neighbours =
            knn
            |> Seq.map (fun (img,allPatches) -> {
                Accepted = false;
                Patches = (allPatches |> Seq.map (fun qrp -> PatchId(qrp.FoundHit.Patch)) |> List.ofSeq);
                Hit = ImageId(img);
                Distance = (allPatches |> Seq.minBy (fun qrp -> qrp.FoundHit.Distance)).FoundHit.Distance
               } )
        neighbours |> List.ofSeq
        
        