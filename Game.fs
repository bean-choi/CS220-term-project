namespace TermShower

open System
open System.Threading

module Game =
  let private maxHealth = 5
  let private initialHealth = 3

  let private randomEntry (rng: Random) (terms: TermEntry list) =
    terms |> List.item (rng.Next(List.length terms))

  let private differentEntry (rng: Random) (terms: TermEntry list) (oldTerm: string) =
    let candidates = terms |> List.filter (fun t -> t.Term <> oldTerm)
    if List.isEmpty candidates then randomEntry rng terms else randomEntry rng candidates

  let private labelLengthOfKind = function
    | Normal -> 0
    | _ -> 4

  let private shownLength (ft: FallingTerm) =
    labelLengthOfKind ft.Kind + ft.Entry.Term.Length

  let private hasOverlap x len (others: FallingTerm list) =
    let overlaps x len otherX otherLen =
      let leftA = x
      let rightA = x + len - 1
      let leftB = otherX
      let rightB = otherX + otherLen - 1
      not (rightA + 2 < leftB || rightB + 2 < leftA)
    others
    |> List.exists (fun ft -> overlaps x len ft.X (shownLength ft))
  
  let private spawnClear (rng: Random) (terms: TermEntry list) (width: int) (nextId: int) =
    let entry = randomEntry rng terms
    let labelLength = 4
    let maxX = max 1 (min 48 width - entry.Term.Length - labelLength - 1)

    { Id = nextId
      Entry = entry
      X = rng.Next(0, maxX)
      Y = 2
      Kind = Clear
      ScoreMultiplier = Stage.scoreMultiplier Clear
      LastMove = DateTime.Now }
  
  let private spawnOne
    (rng: Random)
    (terms: TermEntry list)
    (width: int)
    (nextId: int)
    (stage: int)
    (sameRowTerms: FallingTerm list)
    =
    let entry = randomEntry rng terms
    let kind = Stage.chooseSpecialKind rng stage
    let labelLength = labelLengthOfKind kind
    let textLength = labelLength + entry.Term.Length
    let gameWidth = min 48 width
    let maxX = max 0 (gameWidth - textLength - 1)

    let rec pickX tries =
      let x = rng.Next(0, maxX + 1)
      if tries <= 0 || not (hasOverlap x textLength sameRowTerms) then x
      else pickX (tries - 1)

    { Id = nextId
      Entry = entry
      X = pickX 30
      Y = 2
      Kind = kind
      ScoreMultiplier = Stage.scoreMultiplier kind
      LastMove = DateTime.Now }

  let private spawnTerms (rng: Random) (terms: TermEntry list) (width: int) (state: GameState) =
    let count = Stage.spawnCount rng state.Stage

    let rec loop n acc nextId =
      if n <= 0 then acc, nextId
      else
        let sameRowTerms =
          state.FallingTerms
          |> List.filter (fun ft -> ft.Y = 2)
          |> List.append acc

        let spawned = spawnOne rng terms width nextId state.Stage sameRowTerms
        loop (n - 1) (spawned :: acc) (nextId + 1)

    let spawned, nextId = loop count [] state.NextId

    { state with
        FallingTerms = spawned @ state.FallingTerms
        NextId = nextId
        NextSpawnAt = Stage.nextSpawnTime rng state.Stage }

  let private moveTerms (bottom: int) (state: GameState) =
    let now = DateTime.Now
    let moved =
      state.FallingTerms
      |> List.map (fun ft ->
        let interval = Stage.fallIntervalMs state.Stage ft.Kind
        if (now - ft.LastMove).TotalMilliseconds >= float interval then
          { ft with Y = ft.Y + 1; LastMove = now }
        else ft)
    let fallen, alive = moved |> List.partition (fun ft -> ft.Y >= bottom)
    { state with Health = state.Health - List.length fallen; FallingTerms = alive }

  let private scoreFor (state: GameState) (ft: FallingTerm) =
    ((Stage.baseScore state.Stage) + (String.length ft.Entry.Term)) * ft.ScoreMultiplier

  let private findLowestMatchingTerm (text: string) (fallingTerms: FallingTerm list) : FallingTerm option =
    fallingTerms
    |> List.filter (fun ft -> ft.Entry.Term = text)
    |> List.sortByDescending (fun ft -> ft.Y, -ft.Id)
    |> List.tryHead

  let private handleSubmit (rng: Random) (terms: TermEntry list) (input: string) (state: GameState) =
    let text = input.Trim()
    if text = "" then state
    else
      let matched: FallingTerm option =
        findLowestMatchingTerm text state.FallingTerms
      match matched with
      | None -> state
      | Some ft ->
        match ft.Kind with
        | Transform ->
          let replacement = differentEntry rng terms ft.Entry.Term
          let updated =
            state.FallingTerms
            |> List.map (fun x ->
              if x.Id = ft.Id then { x with Entry = replacement; Kind = Normal; ScoreMultiplier = 2 }
              else x)
          { state with CorrectTyped = state.CorrectTyped + 1; LastMeaning = Some ft.Entry; FallingTerms = updated }
        | Clear ->
          let others = state.FallingTerms |> List.filter (fun x -> x.Id <> ft.Id)
          let gained = others |> List.sumBy (scoreFor state)
          { state with
              Score = state.Score + gained
              CorrectTyped = state.CorrectTyped + 1
              LastMeaning = Some ft.Entry
              FallingTerms = [] }
        | Heal ->
          let gained = scoreFor state ft
          { state with
              Health = min maxHealth (state.Health + 1)
              Score = state.Score + gained
              CorrectTyped = state.CorrectTyped + 1
              LastMeaning = Some ft.Entry
              FallingTerms = state.FallingTerms |> List.filter (fun x -> x.Id <> ft.Id) }
        | _ ->
          let gained = scoreFor state ft
          { state with
              Score = state.Score + gained
              CorrectTyped = state.CorrectTyped + 1
              LastMeaning = Some ft.Entry
              FallingTerms = state.FallingTerms |> List.filter (fun x -> x.Id <> ft.Id) }

  let private saveRankingIfNeeded (state: GameState) =
    ConsoleUi.clear ()
    Console.CursorVisible <- true
    Console.ForegroundColor <- ConsoleColor.White
    Console.WriteLine "Game Over"
    Console.WriteLine "========="
    Console.WriteLine($"Final Score: {state.Score}")
    Console.WriteLine($"Final Stage: {state.Stage}")
    Console.WriteLine($"Correctly Typed Words: {state.CorrectTyped}")
    Console.WriteLine()

    let rankings = Storage.loadRankings ()
    let finalRankings =
      if Ranking.qualifies state.Score rankings then
        Console.Write "New Top 5 score! Enter nickname: "
        let nickname =
          let n = Console.ReadLine().Trim()
          if n = "" then "Player" else n
        let updated = Ranking.add { Nickname = nickname; Score = state.Score; CorrectTyped = state.CorrectTyped } rankings
        Storage.saveRankings updated
        updated
      else rankings
    Console.WriteLine()
    Console.WriteLine "Top 5 Ranking"
    Ranking.top5 finalRankings
    |> List.iteri (fun i r -> Console.WriteLine($"{i + 1}. {r.Nickname}  Score: {r.Score}  Correct: {r.CorrectTyped}"))
    ConsoleUi.pause ()

  let run (terms: TermEntry list) =
    ConsoleUi.configureTerminal ()

    if not (ConsoleUi.isTerminalSizeEnough ()) then
      ConsoleUi.showTerminalSizeWarning ()
    
    let rng = Random()
    let started = DateTime.Now

    let initialWidth = Console.WindowWidth
    let initialHeight = Console.WindowHeight

    let mutable state =
      { Health = initialHealth
        Score = 0
        Stage = 1
        CorrectTyped = 0
        LastMeaning = None
        FallingTerms = []
        StartedAt = started
        NextSpawnAt = DateTime.Now.AddMilliseconds(500.0)
        NextId = 1 }
    let mutable input = ""
    let mutable quit = false
    let mutable terminalResized = false

    while state.Health > 0 && not quit && not terminalResized do
      let width = Console.WindowWidth
      let height = Console.WindowHeight
      let bottom = height - 3

      if width <> initialWidth || height <> initialHeight then
        terminalResized <- true
      else
        while Console.KeyAvailable do
          let key = Console.ReadKey(true)
          match key.Key with
          | ConsoleKey.Escape -> quit <- true
          | ConsoleKey.Enter ->
            state <- handleSubmit rng terms input state
            input <- ""
          | ConsoleKey.Backspace ->
            if input.Length > 0 then input <- input.Substring(0, input.Length - 1)
          | _ ->
            if not (Char.IsControl key.KeyChar) then input <- input + string key.KeyChar

        let oldStage = state.Stage
        let newStage = Stage.currentStage state.StartedAt

        state <- { state with Stage = newStage }

        if Stage.shouldSpawnClear oldStage newStage then
          let clearTerm = spawnClear rng terms width state.NextId
          state <-
            { state with
                FallingTerms = clearTerm :: state.FallingTerms
                NextId = state.NextId + 1 }

        state <- moveTerms bottom state

        if DateTime.Now >= state.NextSpawnAt then
          state <- spawnTerms rng terms width state

        ConsoleUi.drawGame state input
        Thread.Sleep 40
    
    let rankings = Storage.loadRankings ()

    if terminalResized || quit then
      if Ranking.qualifies state.Score rankings then
        saveRankingIfNeeded state
      else
        ()
    else
      saveRankingIfNeeded state
