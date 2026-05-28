module TermShower.Program

open System
open TermShower

[<EntryPoint>]
let main _ =
  ConsoleUi.configureTerminal ()
  Storage.ensureDataFiles ()
  let rec loop terms =
    let items = [ "Start Game"; "How to Play"; "Ranking"; "Edit Terms"; "Exit" ]
    match ConsoleUi.readMenu "Term Shower" items with
    | 0 -> Game.run terms; loop (Storage.loadTerms ())
    | 1 -> ConsoleUi.showHowToPlay (); loop terms
    | 2 -> Storage.loadRankings () |> ConsoleUi.showRankings; loop terms
    | 3 -> TermEditor.run terms |> loop
    | _ -> 0
  try
    loop (Storage.loadTerms ())
  finally
    Console.CursorVisible <- true
    Console.ResetColor()
