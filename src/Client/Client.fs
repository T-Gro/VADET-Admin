module Client

open Elmish
open Elmish.React

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.PowerPack.Fetch

open Thoth.Json

open Shared
open Common


open Fulma

open Fable.FontAwesome
open Fable.FontAwesome.Free
open Fulma
open Fable.Core
open Fable.Import.React
open System



[<Fable.Core.Emit("window.prompt($0,$1) ")>]
let promptDialog (headerText : string, defaultValue: string) : string = Exceptions.jsNative

[<Emit("undefined")>]
let undefined : obj = jsNative


// The model holds data that you want to keep track of while the application is running
// in this case, we are keeping track of a counter
// we mark it as optional, because initially it will not be available from the client
// the initial value will be requested from server
type Model = 
    {          
        Candidates : AttributeCandidate list;
        DynamicDbResults : AutoOfferedAttribute list;
        CurrentExpansion : AttributeExpansion option;
        PendingAjax : bool;
        ShowPatchesInModal : bool;
        SortAlphabetically : bool;
        CategoriesSwitchedToWhitelist : bool}

// The Msg type defines what events/actions can occur while the application is running
// the state of the application changes *only* in reaction to these events
type Msg =
| FreshDataArrived of Result<AttributeCandidate,exn>
| Reject of AttributeCandidate
| RejectWithReason of AttributeCandidate
| Expand  of AttributeCandidate
| AcceptTill of AttributeCandidate * Neighbor
| ExpansionArrived of Result<AttributeExpansion,exn>
| FireAjax
| AjaxArrived
| CloseModal
| InitialisationArrived of Result<InitialDisplay,exn>
| DynamicProposalsArrived of Result<DynamicDbProposals,exn>
| SkipNeighbour of Neighbor
| ToggleIgnore of string
| ToggleShowPatches
| ToggleCategorySortMode
| ToggleCategorySelectionMeaning
| AcceptOffer of AutoOfferedAttribute
| RejectOffer of AutoOfferedAttribute
| FreshOfferDataArrived of Result<OfferFreshData,exn>

module Server =
    open Fable.Remoting.Client
    open Fable.Import
    
    /// A proxy you can use to talk to server directly
    let currentPath = Browser.window.location.pathname.Substring(0,Browser.window.location.pathname.Length-1)
    let builder a b = currentPath + Route.builder a b

    let api : ICounterApi =
      Remoting.createApi()
      |> Remoting.withRouteBuilder builder
      |> Remoting.buildProxy<ICounterApi>


let ajax call arguments resultMessage =
    let serverCall = 
       Cmd.ofAsync
        call
        arguments
        (Ok >> resultMessage)
        (Error >> resultMessage)
    Cmd.batch [Cmd.ofMsg FireAjax; serverCall]

let mutable userName = ""
// defines the initial state and initial command (= side-effect) of the application
let init () : Model * Cmd<Msg> =
    userName <- Fable.Import.Browser.localStorage.getItem "Username" :?> string
    if userName = null then
        userName <- promptDialog("This is your first time around in this browser. Please provide your username for tracking your accepted and rejected attributes.","(anonymous)")
        Fable.Import.Browser.localStorage.setItem("Username",userName)


    let initialModel = {       
        Candidates = [];
        DynamicDbResults = [];
        CurrentExpansion = None;
        PendingAjax = false;
        ShowPatchesInModal = true;
        SortAlphabetically = false;
        CategoriesSwitchedToWhitelist = false}

    let cmd =  ajax  Server.api.loadDynamicDb () DynamicProposalsArrived   // ajax  Server.api.load () InitialisationArrived
    initialModel, cmd

let handleError (e:exn) origMsg currentModel =
    match e with
        | :? Fable.Remoting.Client.ProxyRequestException as ex ->
            let text = Fable.Import.JS.JSON.stringify(Fable.Import.JS.JSON.parse(ex.ResponseText),(fun k v -> if k = "ignored" || k = "handled" then undefined  else v),2)
            Fable.Import.Browser.window.alert(sprintf "The operation has produced an error and has been rolled back \n\n. %s" text)
            currentModel, Cmd.ofMsg AjaxArrived
        | _ ->
            Fable.Import.Browser.window.alert(sprintf "There was an issue with current operation %A" origMsg)
            currentModel, Cmd.ofMsg AjaxArrived

// The update function computes the next state of the application based on the current state and the incoming events/messages
// It can also run side-effects (encoded as commands) like calling the server via Http.
// these commands in turn, can dispatch messages to which the update function will react.
let mutable lastReason = "Not meaningful as an attribute"

let filteredNeighbours (exp: AttributeExpansion) (md : Model) =
    let blackwhitelist = if md.CategoriesSwitchedToWhitelist then id else not
    let neighbours =
        exp.Neighbors
        |> List.filter (fun n -> exp.IgnoredCategories |> List.exists (fun ic -> n.Categories |> List.contains ic) |> blackwhitelist)
    neighbours


let update (msg : Msg) (currentModel : Model) : Model * Cmd<Msg> =
    match currentModel.CurrentExpansion, msg with
    | _ ,FireAjax ->
        {currentModel with PendingAjax = true}, Cmd.none
    | _ , AjaxArrived ->
        {currentModel with PendingAjax = false}, Cmd.none
    | _, CloseModal ->
        {currentModel with CurrentExpansion = None}, Cmd.none
    | _, InitialisationArrived (Ok(init)) ->
        {currentModel with Candidates = init.Candidates}, Cmd.ofMsg AjaxArrived
    | _, DynamicProposalsArrived (Ok(results)) ->
        {currentModel with DynamicDbResults = results.ProductAttributePairs |> List.sortBy (fun x -> x.OldId)}, Cmd.ofMsg AjaxArrived
    | _, FreshDataArrived (Ok(cand)) ->
        {currentModel with Candidates = currentModel.Candidates |> List.map (fun c -> if c.Id = cand.Id then cand else c); CurrentExpansion = None  }, Cmd.ofMsg AjaxArrived
    | _, FreshOfferDataArrived(Ok(off)) ->
        let newResults =
            currentModel.DynamicDbResults
            |> List.map (fun c -> if c.NewImage = off.NewImage && c.OldId = off.OldId then {c with Status = off.Status} else c)
        {currentModel with DynamicDbResults = newResults; CurrentExpansion = None  }, Cmd.ofMsg AjaxArrived
    | _, ExpansionArrived (Ok(expansion)) ->
        {currentModel with CurrentExpansion = Some expansion}, Cmd.ofMsg AjaxArrived
    | _, RejectWithReason(cand) ->
        let reason = promptDialog("Please provide reason for rejection",lastReason)
        if reason <> null  then         
            lastReason <- reason
            currentModel, ajax Server.api.rejectOfferedAttribute  {Subject = cand; Reason = reason; Username = userName} FreshDataArrived
        else
            currentModel, Cmd.none 
    | _, AcceptOffer(off) ->
        let payload = {Reaction = AcceptingOffer; DistanceToAttribute = off.DistanceToAttribute; OldId = off.OldId; NewImage = off.NewImage; Username = userName}
        currentModel, ajax Server.api.reactOnOffer payload  FreshOfferDataArrived
    | _, RejectOffer(off) ->
        let payload = {Reaction = RejectingOffer; DistanceToAttribute = off.DistanceToAttribute; OldId = off.OldId; NewImage = off.NewImage; Username = userName}
        currentModel, ajax Server.api.reactOnOffer payload  FreshOfferDataArrived
    | _, Reject(cand) ->
        //let reason = promptDialog("Please provide reason for rejection","Not meaningful as an attribute")
        currentModel, ajax Server.api.rejectOfferedAttribute  {Subject = cand; Reason = "Not meaningful"; Username = userName} FreshDataArrived
    | _, SkipNeighbour(n) ->
        let emptyN = {Patches = []; Accepted = false; Distance = 100.0f; Hit = ImageId("rejected.png?orig=" + extractImgId n.Hit); Categories = n.Categories}
        let exp = currentModel.CurrentExpansion |> Option.map (fun exp -> {exp with Neighbors = exp.Neighbors |> List.map (fun nn -> if nn <> n then nn else emptyN)})
        {currentModel with CurrentExpansion =  exp }, Cmd.none
     | _, Expand(cand) ->       
        currentModel, ajax Server.api.expandCandidate  cand ExpansionArrived
    | Some(exp), ToggleIgnore(cat) ->
        let ignoreSet = if exp.IgnoredCategories|> List.contains cat then exp.IgnoredCategories |> List.filter (fun ic -> ic <> cat) else cat :: exp.IgnoredCategories
        {currentModel with CurrentExpansion = Some {exp with IgnoredCategories = ignoreSet}}, Cmd.none
    | Some(exp), AcceptTill(cand,pickedN)   ->
        let newName = promptDialog("Please provide a new name for discovered visual attribute","Flower pattern")
        let quality = promptDialog("Please provide a subjective numerical ranking 0-10 of the attribute's quality","-1")
        if newName <> null && quality <> null then
            let filteredKnn =
                filteredNeighbours exp currentModel            
                |> List.takeWhile (fun nn -> nn <> pickedN)
                |> List.append [pickedN]
                |> List.map (fun nn -> {nn with Accepted = true})
                
            {currentModel with CurrentExpansion = Some {exp with Neighbors = filteredKnn}}, ajax Server.api.acceptNewAttribute  {
                Candidate = cand;
                NewName = newName;
                AcceptedMatches = filteredKnn;
                IgnoredCategories = exp.IgnoredCategories;
                Quality = quality;
                Username = userName
            } FreshDataArrived
        else
            currentModel, Cmd.none
    | _, InitialisationArrived(Error(e)) -> handleError e msg currentModel
    | _, DynamicProposalsArrived(Error(e)) -> handleError e msg currentModel
    | _, ExpansionArrived(Error(e)) -> handleError e msg currentModel
    | _, FreshDataArrived(Error(e)) -> handleError e msg currentModel
    | _, FreshOfferDataArrived(Error(e)) -> handleError e msg currentModel
    | _, ToggleShowPatches -> {currentModel with ShowPatchesInModal = not currentModel.ShowPatchesInModal}, Cmd.none
    | _, ToggleCategorySortMode -> {currentModel with SortAlphabetically = not currentModel.SortAlphabetically}, Cmd.none
    | _, ToggleCategorySelectionMeaning -> {currentModel with CategoriesSwitchedToWhitelist = not currentModel.CategoriesSwitchedToWhitelist}, Cmd.none
    | _ -> currentModel, Cmd.none


let navBrand =
    Navbar.navbar [ Navbar.Color IsWhite ]
        [ Container.container [ ]
            [ Navbar.Brand.div [ ]
                [ Navbar.Item.a [ Navbar.Item.CustomClass "brand-text" ]
                      [ str "e-Shop admin" ] ]
              Navbar.menu [ ]
                  [ Navbar.Start.div [ ]
                      [ Navbar.Item.a [ ]
                            [ str "Experiment" ]
                        Navbar.Item.a [ ]
                            [ str "Paper" ]                       
                        Navbar.Item.a [ ]
                            [ str "Results" ] ] ] ] ]


let breadcrump =
    Breadcrumb.breadcrumb [ ]
        [ Breadcrumb.item [ ]
              [ a [ ] [ str "e-Shop" ] ]
          Breadcrumb.item [ ]
              [ a [ ] [ str "Attributes" ] ]
          Breadcrumb.item [ ]
              [ a [ ] [ str "Visual" ] ]
          Breadcrumb.item [ Breadcrumb.Item.IsActive true ]
              [ a [ ] [ str "New candidates" ] ] ]

let hero =
    Hero.hero [ Hero.Color IsInfo             
                Hero.CustomClass "welcome" ]
        [ Hero.body [ ]
            [ Container.container [ Container.IsFluid  ]
                [
                    Heading.h3 [ ]  [ str "Hello, Admin." ]
                    p [] [str "Expand an attribute to see how it applies to the data-set."]
                    p [] [str "After expansion, right-click to reject a selected neighbour, and left-click to apply as an attribute to all items up to the selected one."]
                   ] ] ]

let shortStatusName  = function
    | Offered -> str("Offered")
    | Accepted(_,name) -> span [] [str("Accepted");br[];str(name)]
    | Rejected(_,name) -> span [] [str("Rejected");br[];str(name)]
    | AutoOffered -> str("Calculated")
    | OfferedButBlacklisted -> str("Blacklisted")
    | OfferedButNotWhitelisted -> str("Not on Whitelist")

let statusColor  = function
    | Offered -> [ClassName "has-background-grey"]
    | Accepted(_,_) -> [ ClassName "has-background-success"]
    | Rejected(_,_) -> [ ClassName "has-background-danger" ]
    | AutoOffered -> [ ClassName "has-background-success"]
    | OfferedButBlacklisted -> [ ClassName "has-background-danger"]
    | OfferedButNotWhitelisted -> [ ClassName "has-background-danger"]

let statusOrder  = function
    | Offered -> 1
    | Accepted(_,_) -> 0
    | Rejected(_,_) -> 2
    | AutoOffered -> 3
    | OfferedButBlacklisted -> 4
    | OfferedButNotWhitelisted -> 5   



let renderImageWithPatches image patches   =
    div [ClassName("img-container")] [
        yield img [ Src ("http://herkules.ms.mff.cuni.cz/vadet-merged/images-cropped/images-cropped/"+ extractImgId image ) ]
        for p in patches |> Seq.distinct do
        let patchId = extractPatchId p
        let coordParts = patchId.Split([|'_';'@'|])
        let position = int32 coordParts.[1]
        if coordParts.[0] = "6x8" then
            let row = float(position / 6) * (100.0/8.0)
            let column = float(position % 6) * (100.0/6.0)
            let asPercent = sprintf "%.2f%%"
            yield span [ClassName("is6x8 marker"); Style[CSSProp.Top(row |> asPercent); CSSProp.Left(column |> asPercent)]] []
        if coordParts.[0] = "3x4" then
            let row = float(position / 3) * (100.0/4.0)
            let column = float(position % 3) * (100.0/3.0)
            let asPercent = sprintf "%.2f%%"
            yield span [ClassName("is3x4 marker"); Style[CSSProp.Top(row |> asPercent); CSSProp.Left(column |> asPercent)]] []
    ]  

let expandedModal (model: Model) (dispatch: Msg -> unit) =
    match model.CurrentExpansion with
    | None -> br[]
    | Some(expansion) ->
        let sortList list =
            if model.SortAlphabetically then
                list |> List.sortBy (fun o -> (string (fst o)).ToUpper().Trim() )
            else list|> List.sortByDescending snd;

        let allCategories =
            expansion.Neighbors
            |> List.collect (fun x -> x.Categories)
            |> List.groupBy id
            |> List.map (fun (key, g) -> (key, g |> List.length ) )
            |> sortList
            |> List.map (fun (cat,count) ->
                Button.button
                    [Button.Color (if expansion.IgnoredCategories |> List.contains cat then IsWarning else IsSuccess); Button.OnClick (fun _ -> dispatch (ToggleIgnore(cat)) ); Button.Size IsSmall ]
                    [str (sprintf "%s (%ix)" cat count)])
        let renderNeighbour (n: Neighbor) =
                a
                    [
                        yield OnClick (fun _ -> dispatch (AcceptTill(expansion.Candidate,n)))
                        yield OnContextMenu (fun me -> dispatch (SkipNeighbour(n)); me.preventDefault())                      
                        yield Title (sprintf "Distance = %f. Left-click to accept as an attribute assigned to all objects until this neighbour. Right-click to skip a selected neighbour.\n Categories = %A" n.Distance n.Categories)
                        if n.Accepted then yield ClassName "blink"
                    ]
                    [
                        Image.image [ Image.Is128x128 ] [renderImageWithPatches n.Hit (n.Patches |> Seq.filter(fun _ -> model.ShowPatchesInModal))]                    
                    ]

        Modal.modal
            [Modal.IsActive true]
            [
                Modal.background [ Props [ OnClick (fun _ -> dispatch CloseModal) ] ] [ ]
                Modal.content [  GenericOption.Props [Style[ Width "85%"]]  ]
                    [
                        Box.box'
                            []
                            [
                                yield! allCategories;
                                yield (br []);
                                yield Button.button
                                        [Button.Color (if model.ShowPatchesInModal then IsInfo  else IsWarning); Button.OnClick (fun _ -> dispatch (ToggleShowPatches) ); Button.Size IsSmall ]
                                        [str (if model.ShowPatchesInModal then "Click to hide borders of patches" else "Clcik to show borders of patches")]
                                yield Button.button
                                        [Button.Color (if model.SortAlphabetically then IsWarning  else IsInfo); Button.OnClick (fun _ -> dispatch (ToggleCategorySortMode) ); Button.Size IsSmall ]
                                        [str (if model.SortAlphabetically then "Categories sorted alphabetically (click to change to count)" else "Categories are sorted by count (click to sort alphabetically")]
                                yield Button.button
                                        [Button.Color (if model.CategoriesSwitchedToWhitelist then IsWarning  else IsInfo); Button.OnClick (fun _ -> dispatch (ToggleCategorySelectionMeaning) ); Button.Size IsSmall ]
                                        [str (if model.CategoriesSwitchedToWhitelist then "Clicking categories whitelists them (click to change to blacklist)" else "Selected categories are blacklisted (click to change to whitelist)")]
                                
                                yield (br []);
                                yield! (
                                    filteredNeighbours expansion model                                   
                                    |> List.map renderNeighbour);
                            ]
                        
                    ]
                Modal.close
                    [
                        Modal.Close.Size IsLarge
                        Modal.Close.OnClick (fun _ -> dispatch CloseModal)
                    ][ ]
            ]    

let renderDynamicDbResult (d: AutoOfferedAttribute) (smallButton) =
    tr (statusColor(d.Status) |> Seq.cast<IHTMLProp>)
        [
            yield td [] [shortStatusName(d.Status)]
            if d.Status = AutoOffered then
                yield smallButton "Accept" (AcceptOffer(d) ) IsPrimary
                yield smallButton "Reject" (RejectOffer(d)) IsDanger
            yield td [] [str(sprintf "%d : %s" d.OldId d.Name)]
            yield td [] [str(sprintf "TS: %.3f" d.OriginalTreshold)]
            yield td [] [str(sprintf "Dist: %.3f" d.DistanceToAttribute)]
            yield td [] [yield Image.image [ Image.Is128x128 ] [ div [ClassName("img-container")] [ img [ Src ("http://herkules.ms.mff.cuni.cz/vadet-merged/images-cropped/images-2019/"+ extractImgId d.NewImage + ".jpg") ]]]]
            if not(String.IsNullOrWhiteSpace(d.OriginalBlacklist)) then yield td [] [str(sprintf "Blacklist : %s" d.OriginalBlacklist)]
            if not(String.IsNullOrWhiteSpace(d.OriginalWhitelist)) then yield td [] [str(sprintf "Whitelist : %s" d.OriginalWhitelist)]
        ]

let renderCandidate (c:AttributeCandidate) (smallButton) =
  tr (statusColor(c.Status) |> Seq.cast<IHTMLProp>)
    [ 
    yield td [ ]                                                
        [
            yield shortStatusName(c.Status);
            yield br [];
            yield smallButton "Expand" (Expand(c) ) IsPrimary;
            yield br [];                                                   
            if c.Status = Offered then
                yield smallButton "Reject" (Reject(c)) IsDanger
                yield smallButton "Reject w/ reason" (RejectWithReason(c)) IsWarning
        ];
    for (i,patches) in c.Representatives |> List.groupBy fst do                                                
        yield td [ ] [
            Image.image [ Image.Is128x128 ] [ renderImageWithPatches i (patches |> Seq.map snd) ]];

    ]
     
let columns (model : Model) (dispatch : Msg -> unit) =
            let smallButton name message color =
                Button.a [
                            Button.Size IsSmall
                            Button.Color color
                            Button.OnClick (fun _ -> dispatch message)
                            Button.IsLoading model.PendingAjax
                          ]
                          [str name]


            Card.card [ CustomClass "list-card" ]
                [ Card.header [ ]
                    [ Card.Header.title [ ]
                        [ str "New candidates" ]
                    ]
                  div [ Class "card-table" ]
                      [ Content.content [ ]
                          [ Table.table
                              [ Table.IsBordered
                                Table.IsNarrow
                                Table.IsStriped ]
                              [ tbody [ ]
                                  [ for c in model.DynamicDbResults  ->  //.Candidates  ->
                                      renderDynamicDbResult c smallButton  ] ] ] 
                      ]
                  Card.footer [ ]
                      [ Card.Footer.div [ ]
                          [ ] ] ]
          
            

let view (model : Model) (dispatch : Msg -> unit) =
    div [ ]
        [ navBrand
          Container.container [ Container.IsFluid ]
              [ Columns.columns [ ]
                  [ 
                    Column.column [ Column.Width ( Screen.All , Column.Is12) ]
                      [ breadcrump
                        hero                      
                        columns model dispatch
                        expandedModal model dispatch] ] ] ]

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
|> Program.withHMR
#endif
|> Program.withReact "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
