module Client

open Elmish
open Elmish.React

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.PowerPack.Fetch

open Thoth.Json

open Shared


open Fulma

open Fable.FontAwesome
open Fable.FontAwesome.Free
open Fulma
open Fable.Core
open Fable.Import.React


[<Fable.Core.Emit("window.prompt($0,$1) ")>]
let promptDialog (headerText : string, defaultValue: string) : string = Exceptions.jsNative

// The model holds data that you want to keep track of while the application is running
// in this case, we are keeping track of a counter
// we mark it as optional, because initially it will not be available from the client
// the initial value will be requested from server
type Model = 
    {          
        Candidates : AttributeCandidate list;
        CurrentExpansion : AttributeExpansion option;
        PendingAjax : bool}

// The Msg type defines what events/actions can occur while the application is running
// the state of the application changes *only* in reaction to these events
type Msg =
| FreshDataArrived of Result<AttributeCandidate,exn>
| Reject of AttributeCandidate
| Expand  of AttributeCandidate
| AcceptTill of AttributeCandidate * Neighbor
| ExpansionArrived of Result<AttributeExpansion,exn>
| FireAjax
| AjaxArrived
| CloseModal

module Server =

    open Shared
    open Fable.Remoting.Client

    /// A proxy you can use to talk to server directly
    let api : ICounterApi =
      Remoting.createApi()
      |> Remoting.withRouteBuilder Route.builder
      |> Remoting.buildProxy<ICounterApi>

 
// defines the initial state and initial command (= side-effect) of the application
let init () : Model * Cmd<Msg> =
    let initialModel = {       
        Candidates = List.init 10 (fun i -> {Id = i; Status = Offered; Representatives = []});
        CurrentExpansion = None;
        PendingAjax = false }

    initialModel, Cmd.none



let ajax call arguments resultMessage =
    let serverCall = 
       Cmd.ofAsync
        call
        arguments
        (Ok >> resultMessage)
        (Error >> resultMessage)
    Cmd.batch [Cmd.ofMsg FireAjax; serverCall]

// The update function computes the next state of the application based on the current state and the incoming events/messages
// It can also run side-effects (encoded as commands) like calling the server via Http.
// these commands in turn, can dispatch messages to which the update function will react.
let update (msg : Msg) (currentModel : Model) : Model * Cmd<Msg> =
    match currentModel.CurrentExpansion, msg with
    | _ ,FireAjax ->
        {currentModel with PendingAjax = true}, Cmd.none
    | _ , AjaxArrived ->
        {currentModel with PendingAjax = false}, Cmd.none
    | _, CloseModal ->
        {currentModel with CurrentExpansion = None}, Cmd.none
    | _, FreshDataArrived (Ok(cand)) ->
        {currentModel with Candidates = currentModel.Candidates |> List.map (fun c -> if c.Id = cand.Id then cand else c); CurrentExpansion = None  }, Cmd.ofMsg AjaxArrived
    | _, ExpansionArrived (Ok(expansion)) ->
        {currentModel with CurrentExpansion = Some expansion}, Cmd.ofMsg AjaxArrived
    | _, Reject(cand) ->
        let reason = promptDialog("Please provide reason for rejection","Not meaningful as an attribute")
        currentModel, ajax Server.api.rejectOfferedAttribute  {Subject = cand; Reason = reason} FreshDataArrived
     | _, Expand(cand) ->       
        currentModel, ajax Server.api.expandCandidate  cand ExpansionArrived
    | Some(exp), AcceptTill(cand,pickedN)   ->
        let newName = promptDialog("Please provide a new name for discovered visual attribute","Flower pattern")
        let filteredKnn =
            exp.Neighbors
            |> List.takeWhile (fun nn -> nn <> pickedN)
            |> List.append [pickedN]
            |> List.map (fun nn -> {nn with Accepted = true})
        {currentModel with CurrentExpansion = Some {exp with Neighbors = filteredKnn}}, ajax Server.api.acceptNewAttribute  {
            Candidate = cand;
            NewName = newName;
            AcceptedMatches = filteredKnn
        } FreshDataArrived
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
                [ Heading.h3 [ ]
                      [ str "Hello, Admin." ]
                   ] ] ]

let shortStatusName  = function
    | Offered -> str("Offered")
    | Accepted(_,name) -> span [] [str("Accepted");br[];str(name)]
    | Rejected(_,_) -> str("Rejected")

let statusColor  = function
    | Offered -> []
    | Accepted(_,_) -> [ ClassName "has-background-success"]
    | Rejected(_,_) -> [ ClassName "has-background-grey" ]

let statusOrder  = function
    | Offered -> 1
    | Accepted(_,_) -> 0
    | Rejected(_,_) -> 2



let expandedModal (model: Model) (dispatch: Msg -> unit) =


    match model.CurrentExpansion with
    | None -> br[]
    | Some(expansion) ->
        let renderNeighbour (n: Neighbor) =
                a
                    [
                        OnClick (fun _ -> dispatch (AcceptTill(expansion.Candidate,n)))
                        Title (sprintf "Distance = %f. Click to accept as an attribute assigned to all objects until this neighbour." n.Distance)
                    ]
                    [                        
                        img [
                                yield Src "https://dummyimage.com/128x128/7a7a7a/fff";
                                if n.Accepted then yield ClassName "blink";
                            ]
                    ]

        Modal.modal
            [Modal.IsActive true]
            [
                Modal.background [ Props [ OnClick (fun _ -> dispatch CloseModal) ] ] [ ]
                Modal.content [  GenericOption.Props [Style[ Width "85%"]]  ]
                    [
                        Box.box'
                            []
                            (expansion.Neighbors |> List.map renderNeighbour)
                        
                    ]
                Modal.close
                    [
                        Modal.Close.Size IsLarge
                        Modal.Close.OnClick (fun _ -> dispatch CloseModal)
                    ][ ]
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
                                  [ for c in model.Candidates |> Seq.sortBy (fun c -> c.Status |> statusOrder) ->
                                      tr (statusColor(c.Status) |> Seq.cast<IHTMLProp>)
                                          [ 
                                            td [ ]                                                
                                                [
                                                    yield shortStatusName(c.Status);
                                                    yield br [];
                                                    yield smallButton "Expand" (Expand(c) ) IsPrimary;
                                                    yield br [];                                                   
                                                    if c.Status = Offered then
                                                        yield smallButton "Reject" (Reject(c)) IsDanger
                                                ] 
                                            td [ ] [Image.image [ Image.Is128x128 ] [ img [ Src "https://dummyimage.com/128x128/7a7a7a/fff" ] ]]
                                            td [ ] [Image.image [ Image.Is128x128 ] [ img [ Src "https://dummyimage.com/128x128/7a7a7a/fff" ] ]]
                                            td [ ] [Image.image [ Image.Is128x128 ] [ img [ Src "https://dummyimage.com/128x128/7a7a7a/fff" ] ]]                                               
                                            td [ ] [Image.image [ Image.Is128x128 ] [ img [ Src "https://dummyimage.com/128x128/7a7a7a/fff" ] ]]
                                            td [ ] [Image.image [ Image.Is128x128 ] [ img [ Src "https://dummyimage.com/128x128/7a7a7a/fff" ] ]]                                           

                                           ] ] ] ] 
                      ]
                  Card.footer [ ]
                      [ Card.Footer.div [ ]
                          [ str "View All" ] ] ]
          
            

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
