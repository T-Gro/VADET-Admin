module PatchProposals 
    open KnnResults.Domain
    open KnnResults.Domain.Models
    open System.Configuration
    open Shared
    open System
    open System.Text.RegularExpressions
    open System.Globalization
    open FSharp.Text.RegexProvider

    type NewProductCodeRegex = Regex< @"^(?<GridSize>\dx\d)_(?<ImageName>.*)\-(?<PatchId>\d+)$" >

    let Loaded = AllResults.Load(ConfigurationManager.AppSettings.["FilteredPatchesBin"])    
    let NameMap = Loaded.ImageEncoding |> Seq.map (fun kvp -> (kvp.Value,kvp.Key)) |> Map.ofSeq
    let PatchMap = Loaded.PatchEncoding |> Seq.map (fun kvp -> (kvp.Value,kvp.Key)) |> Map.ofSeq
    let IdMap = Loaded.Rows |> Seq.mapi (fun i x -> (x.Query, i)) |> dict

    let SimilaritiesToNewProducts = SimilarityGraph.Load(ConfigurationManager.AppSettings.["NewProductSimilarities"])
    let CurrentLayer = ConfigurationManager.AppSettings.["AlexnetLayer"]

    let inline DefaultIfEmpty d l = System.Linq.Enumerable.DefaultIfEmpty(l, d)

    let loadNewProductMatches() =
        use dbCtx = new VADETContext()
        let existingAttrs =
            query {
                for a in dbCtx.VisualAttributeDefinition do
                    where (a.AttributeSource.Contains(CurrentLayer))
                    select a
            } |> Seq.toList

        let labelsForNewProducts =
            dbCtx.Query<NewProductLabels>()
            |> Seq.map (fun npl -> (npl.ProductId, npl))
            |> dict

        let imagePatches =
            query {
                for coa in dbCtx.Query<CandidatesOfAttributes>() do
                    join attr in dbCtx.VisualAttributeDefinition  on (coa.Id = attr.Id)
                    where (attr.AttributeSource.Contains(CurrentLayer))
                    select coa
            } |> Seq.groupBy (fun x -> x.Id) |> dict



        let parseNewId s =
            let parts = NewProductCodeRegex().TypedMatch(s)
            (ImageId(parts.ImageName.Value),PatchId(parts.GridSize.Value + "@" + parts.PatchId.Value))

        let newProducts =
            SimilaritiesToNewProducts.ResultsForNewImages
            |> Seq.map (fun kv -> (parseNewId kv.Key, kv.Value) )
            |> Seq.filter (fun ((ImageId i,p),d) -> not(String.IsNullOrWhiteSpace(i)))
            |> Seq.groupBy (fst >> fst)
            |> Map.ofSeq

        let proposals =
            seq{
                for p in newProducts do
                for ea in existingAttrs do
                let patches = imagePatches.[ea.Id] |> Seq.map (fun x -> x.PatchName) 
                let minDistances =
                    patches
                    |> Seq.map (
                        fun oldPatch ->
                            p.Value
                            |> Seq.collect (fun x -> snd x)
                            |> Seq.filter (fun x -> x.OldPatchName = oldPatch )
                            |> Seq.map (fun x -> x.Distance)
                            |> DefaultIfEmpty (SimilaritiesToNewProducts.OldNameTreshold512.[oldPatch] * 1.1f)                                                   
                            |> Seq.min)
                let avg = minDistances |> Seq.average |> float
                if (avg) <= ea.DistanceTreshold.Value then
                    yield {
                        OldId = ea.Id;
                        Name = ea.Name;
                        NewImage = p.Key;
                        OriginalTreshold = ea.DistanceTreshold.Value;
                        DistanceToAttribute = avg;
                        OriginalBlacklist = ea.DiscardedCategories;
                        OriginalWhitelist = ea.WhitelistedCategories;
                        Status = AttributeStatus.AutoOffered}    
            } |> Seq.toList

        {ProductAttributePairs = proposals }




    let loadInitialDisplay() =
        let acceptedSoFar = EventStore.loadExistingApprovals()
        let rejectedSoFar = EventStore.loadExistingRejections()
        let createCandidate (r : ResultsRow) =
            let imgs =
                [yield r.Query; yield! (r.Hits |> Seq.map (fun x -> x.Hit))]
                |> List.map (fun p -> (ImageId(NameMap.[p.ImageId]),PatchId(PatchMap.[p.PatchId])))
            let id = IdMap.[r.Query]
            let status =
                let (isRej,rej) = rejectedSoFar.TryGetValue id
                let (isAcc,acc) = acceptedSoFar.TryGetValue id
                match (isRej,isAcc) with
                    | (true,false) -> Rejected(rej |> snd, rej |> fst) 
                    | (_,true) -> Accepted(fst acc, snd acc)
                    | _ -> Offered       

            {Representatives = imgs; Status = status; Id = id}

        let r = new Random()
        let firstItems =
            Loaded.Rows
            |> Seq.rev
            |> Seq.map createCandidate
            |> Seq.filter (fun c -> c.Status = Offered)
            |> Seq.sortBy (fun c -> r.Next())
            |> Seq.truncate 75
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
            |> Seq.truncate 256
            |> Seq.map (fun (img,dist) -> {
                Accepted = false;
                Patches = findAllPatches img;
                Hit = ImageId(img);
                Distance = dist;
                Categories = ZootLabels.Get(img).AllCategories |> List.ofArray
            })
    
        bestPicks |> List.ofSeq
        
        