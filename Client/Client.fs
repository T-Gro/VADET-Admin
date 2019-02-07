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

[<Fable.Core.Emit("window.prompt($0,$1) ")>]
let promptDialog (headerText : string, defaultValue: string) : string = Exceptions.jsNative

// The model holds data that you want to keep track of while the application is running
// in this case, we are keeping track of a counter
// we mark it as optional, because initially it will not be available from the client
// the initial value will be requested from server
type Model = 
    { 
        TableItems : int ;
        Rename : Rename option;
        Candidates : AttributeCandidate list;
        CurrentExpansion : AttributeExpansion option;
        PendingAjax : bool}

// The Msg type defines what events/actions can occur while the application is running
// the state of the application changes *only* in reaction to these events
type Msg =
| Delete of int
| Rename of int
| RenameLoaded of Result<Rename,exn>
| FreshDataArrived of Result<AttributeCandidate,exn>
| Reject of AttributeCandidate
| Expand  of AttributeCandidate
| AcceptTill of AttributeCandidate * Neighbor
| ExpansionArrived of AttributeExpansion
| FireAjax
| AjaxArrived

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
    let initialModel = { TableItems = 10; Rename = None; Candidates = [] }
    let loadCountCmd =
        Cmd.ofAsync
            Server.api.rename
            3
            (Ok >> RenameLoaded)
            (Error >> RenameLoaded)
    initialModel, loadCountCmd



// The update function computes the next state of the application based on the current state and the incoming events/messages
// It can also run side-effects (encoded as commands) like calling the server via Http.
// these commands in turn, can dispatch messages to which the update function will react.
let update (msg : Msg) (currentModel : Model) : Model * Cmd<Msg> =
    match currentModel, msg with
    | _ ,FireAjax ->
        {currentModel with PendingAjax = true}, Cmd.None
    | _ , AjaxArrived ->
        {currentModel with PendingAjax = false}, Cmd.None
    | _, RenameLoaded (Ok newName) ->
        {currentModel with Rename = Some newName }, Cmd.ofMsg AjaxArrived
    | _, Rename idx ->
        let serverCallCmd =
            Cmd.ofAsync
                Server.api.rename
                idx
                (Ok >> RenameLoaded)
                (Error >> RenameLoaded)
        currentModel, Cmd.batch  [Cmd.ofMsg FireAjax; serverCallCmd]
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




let columns (model : Model) (dispatch : Msg -> unit) =
            let namingFunc idx =
                match model.Rename with                
                | Some(record) when record.Id = idx -> record.NewName
                | _ -> "Lorem ipsum g"

            Card.card [ CustomClass "list-card" ]
                [ Card.header [ ]
                    [ Card.Header.title [ ]
                        [ str "New candidates" ]
                    ]
                  div [ Class "card-table" ]
                      [ Content.content [ ]
                          [ Table.table
                              [ Table.IsFullWidth
                                Table.IsStriped ]
                              [ tbody [ ]
                                  [ for idx in 1..model.TableItems ->
                                      tr [ ]
                                          [ td [ ]
                                                [ str ( namingFunc idx) ]
                                            td [ ]
                                                [ Button.a
                                                    [ Button.Size IsSmall
                                                      Button.Color IsPrimary
                                                      Button.OnClick (fun _ -> dispatch (Rename idx)) ]
                                                    [ str "Random name" ] ] ] ] ] ];
                            div [] [str "Blax"];
                            div [] [str "Blux"];
                            Table.table
                              [ Table.IsFullWidth
                                Table.IsStriped ]
                              [ tbody [ ]
                                  [ for idx in 1..model.TableItems ->
                                      tr [ ]
                                          [ td [ ]
                                                [ str ( namingFunc idx) ]
                                            td [ ]
                                                [ Button.a
                                                    [ Button.Size IsSmall
                                                      Button.Color IsPrimary
                                                      Button.OnClick (fun _ -> dispatch (Rename idx)) ]
                                                    [ str "Random name" ] ] ] ] ] 
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
                    Column.column [ Column.Width (Screen.All, Column.Is12) ]
                      [ breadcrump
                        hero                      
                        columns model dispatch ] ] ] ]

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
