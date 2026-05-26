namespace TermShower

open System

module ConsoleUi =
  let private safeSetCursor x y =
    if x >= 0 && y >= 0 && x < Console.BufferWidth && y < Console.BufferHeight then
      Console.SetCursorPosition(x, y)

  let private charWidth (c: char) =
    if int c <= 127 then 1 else 2

  let private displayWidth (text: string) =
    text |> Seq.sumBy charWidth

  let private takeByDisplayWidth (maxWidth: int) (text: string) =
    let rec loop acc width idx =
      if idx >= text.Length then
        acc |> List.rev |> Array.ofList |> String
      else
        let c = text.[idx]
        let w = charWidth c
        if width + w > maxWidth then
          acc |> List.rev |> Array.ofList |> String
        else
          loop (c :: acc) (width + w) (idx + 1)

    if maxWidth <= 0 then "" else loop [] 0 0
  
  let private writeAt x y (text: string) =
    safeSetCursor x y
    let room = max 0 (Console.WindowWidth - x - 1)
    let s = takeByDisplayWidth room text
    Console.Write s
  
  let private wrapTextByDisplayWidth (width: int) (text: string) : string list =
    if width <= 0 then []
    else
      let rec loop acc current currentWidth idx =
        if idx >= text.Length then
          let line = current |> List.rev |> Array.ofList |> String
          if line = "" then List.rev acc
          else List.rev (line :: acc)
        else
          let c = text.[idx]
          let w = charWidth c

          if currentWidth + w > width then
            let line = current |> List.rev |> Array.ofList |> String
            loop (line :: acc) [ c ] w (idx + 1)
          else
            loop acc (c :: current) (currentWidth + w) (idx + 1)

      loop [] [] 0 0

  let private wrapText (width: int) (text: string) : string list =
    if width <= 0 then []
    else
      let rec loop acc (rest: string) =
        if String.IsNullOrEmpty rest then
          List.rev acc
        elif rest.Length <= width then
          List.rev (rest :: acc)
        else
          let line = rest.Substring(0, width)
          let remain = rest.Substring(width)
          loop (line :: acc) remain

      loop [] text

  let pause () =
    Console.WriteLine()
    Console.Write "Press any key to continue..."
    Console.ReadKey(true) |> ignore

  let clear () = Console.Clear()

  let private colorOfKind = function
    | Normal -> ConsoleColor.White
    | Heal -> ConsoleColor.Green
    | Transform -> ConsoleColor.Red
    | Blink -> ConsoleColor.Gray
    | Fast -> ConsoleColor.Magenta
    | Bonus -> ConsoleColor.Yellow
    | Clear -> ConsoleColor.Cyan

  let private labelOfKind = function
    | Normal -> ""
    | Heal -> "[H] "
    | Transform -> "[T] "
    | Blink -> "[B] "
    | Fast -> "[F] "
    | Bonus -> "[$] "
    | Clear -> "[C] "

  let readMenu (title: string) (items: string list) =
    Console.CursorVisible <- false
    let rec loop selected =
      clear ()
      Console.ForegroundColor <- ConsoleColor.White
      Console.WriteLine title
      Console.WriteLine(String.replicate title.Length "=")
      Console.WriteLine()
      items
      |> List.iteri (fun i item ->
        if i = selected then
          Console.ForegroundColor <- ConsoleColor.Yellow
          Console.WriteLine($"> {item}")
        else
          Console.ForegroundColor <- ConsoleColor.Gray
          Console.WriteLine($"  {item}"))
      Console.ForegroundColor <- ConsoleColor.White
      match Console.ReadKey(true).Key with
      | ConsoleKey.UpArrow -> loop ((selected + List.length items - 1) % List.length items)
      | ConsoleKey.DownArrow -> loop ((selected + 1) % List.length items)
      | ConsoleKey.Enter -> selected
      | _ -> loop selected
    loop 0

  let showHowToPlay () =
    clear ()
    Console.ForegroundColor <- ConsoleColor.White
    Console.WriteLine "How to Play"
    Console.WriteLine "==========="
    Console.WriteLine "Type a visible term exactly and press Enter. Uppercase and lowercase are different."
    Console.WriteLine "Incorrect input does not reduce health. Health decreases only when a term reaches the bottom."
    Console.WriteLine "Special terms: [H] heal, [T] transform, [B] blink, [F] fast, [$] bonus, [C] clear."
    Console.WriteLine "During gameplay, press Esc to quit the current game."
    pause ()

  let showRankings (rankings: RankingEntry list) =
    clear ()
    Console.ForegroundColor <- ConsoleColor.White
    Console.WriteLine "Ranking"
    Console.WriteLine "======="
    match Ranking.top5 rankings with
    | [] -> Console.WriteLine "No ranking data yet."
    | xs ->
      xs
      |> List.iteri (fun i r ->
        Console.WriteLine($"{i + 1}. {r.Nickname}  Score: {r.Score}  Correct: {r.CorrectTyped}"))
    pause ()

  let drawGame (state: GameState) (inputBuffer: string) =
    Console.CursorVisible <- false
    clear ()
    let width = Console.WindowWidth
    let height = Console.WindowHeight
    let rightStart = max 50 (width * 2 / 3)
    let gameBottom = height - 4

    Console.ForegroundColor <- ConsoleColor.White
    writeAt 0 0 $"Stage: {state.Stage}   HP: {state.Health}/5   Score: {state.Score}"
    writeAt 0 1 (String.replicate (min (width - 1) (rightStart - 2)) "-")
    writeAt rightStart 0 "Meaning"
    writeAt rightStart 1 "-------"

    let panelWidth = width - rightStart - 1

    let panelWidth = max 1 (width - rightStart - 2)

    match state.LastMeaning with
    | None ->
      wrapTextByDisplayWidth panelWidth "Type a term to see its meaning."
      |> List.iteri (fun i line ->
        writeAt rightStart (3 + i) line)

    | Some t ->
      writeAt rightStart 3 t.Term

      wrapTextByDisplayWidth panelWidth t.Meaning
      |> List.truncate (max 1 (height - 8))
      |> List.iteri (fun i line ->
        writeAt rightStart (4 + i) line)

    let blinkVisible = DateTime.Now.Millisecond < 500
    state.FallingTerms
    |> List.iter (fun ft ->
      if ft.Y > 1 && ft.Y < gameBottom then
        let shown =
          match ft.Kind with
          | Blink when not blinkVisible -> labelOfKind ft.Kind + "????"
          | _ -> labelOfKind ft.Kind + ft.Entry.Term
        Console.ForegroundColor <- colorOfKind ft.Kind
        writeAt ft.X ft.Y shown)

    Console.ForegroundColor <- ConsoleColor.White
    writeAt 0 (height - 3) (String.replicate (width - 1) "-")
    writeAt 0 (height - 2) $"Input: {inputBuffer}"
    writeAt 0 (height - 1) "Enter: submit    Backspace: delete    Esc: quit"
