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
        let findAllPatches img =
            combinedResults
            |> List.map (fun (_,bil,_,_) -> bil.TryFind img)
            |> List.choose id
            |> Seq.collect id
            |> Seq.map (fun x -> PatchId x.Patch)
            |> Seq.distinct
            |> List.ofSeq

        let bestPicks =
            withAvgDistance
            |> Seq.sortBy snd
            |> Seq.truncate 512
            |> Seq.map (fun (img,dist) -> {
                Accepted = false;
                Patches = findAllPatches img;
                Hit = ImageId(img);
                Distance = dist
            })
    
        bestPicks |> List.ofSeq
        
        