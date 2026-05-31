namespace TermShower

open System

module TermEditor =
  let private isValidTerm (term: string) =
    term.Length > 0 && term |> Seq.forall (fun c -> Char.IsLetter c || c = '-')

  let private readNonEmpty (prompt: string) =
    Console.Write prompt
    let s = Console.ReadLine().Trim()
    if s = "" then None else Some s

  let private wait (message: string) =
    Console.WriteLine message
    ConsoleUi.pause ()

  let private confirm (message: string) =
    Console.Write $"{message} (y/n): "

    let rec loop () =
      match Console.ReadKey(true).Key with
      | ConsoleKey.Y ->
        Console.WriteLine "y"
        true
      | ConsoleKey.N ->
        Console.WriteLine "n"
        false
      | _ -> loop ()

    loop ()

  let private addTerm (terms: TermEntry list) =
    ConsoleUi.clear ()

    match readNonEmpty "New term: " with
    | None ->
      wait "Canceled. No changes were saved."
      terms
    | Some term ->
      match readNonEmpty "Meaning: " with
      | None ->
        wait "Canceled. No changes were saved."
        terms
      | Some meaning ->
        if not (isValidTerm term) then
          wait "Only alphabets and hyphen are allowed."
          terms
        elif terms |> List.exists (fun t -> t.Term = term) then
          wait "The term already exists."
          terms
        elif confirm "Save this new term?" then
          let updated = { Term = term; Meaning = meaning } :: terms
          Storage.saveTerms updated
          wait "Term added."
          updated
        else
          wait "Canceled. No changes were saved."
          terms

  let private editTerm (terms: TermEntry list) =
    ConsoleUi.clear ()
    Console.WriteLine "Current terms:"

    terms
    |> List.sortBy (fun t -> t.Term)
    |> List.iter (fun t -> Console.WriteLine($"- {t.Term}"))

    Console.WriteLine()

    match readNonEmpty "Term to edit: " with
    | None ->
      wait "Canceled. No changes were saved."
      terms
    | Some target ->
      match terms |> List.tryFind (fun t -> t.Term = target) with
      | None ->
        wait "Term not found."
        terms
      | Some oldTerm ->
        Console.Write $"New term (Enter to keep '{oldTerm.Term}'): "
        let newTermRaw = Console.ReadLine().Trim()

        Console.Write "New meaning (Enter to keep old meaning): "
        let newMeaningRaw = Console.ReadLine().Trim()

        let newTerm =
          if newTermRaw = "" then oldTerm.Term else newTermRaw

        let newMeaning =
          if newMeaningRaw = "" then oldTerm.Meaning else newMeaningRaw

        if not (isValidTerm newTerm) then
          wait "Only alphabets and hyphen are allowed."
          terms
        elif newTerm <> oldTerm.Term && terms |> List.exists (fun t -> t.Term = newTerm) then
          wait "The new term already exists."
          terms
        else
          Console.WriteLine()
          Console.WriteLine "Before:"
          Console.WriteLine($"  Term: {oldTerm.Term}")
          Console.WriteLine($"  Meaning: {oldTerm.Meaning}")
          Console.WriteLine()
          Console.WriteLine "After:"
          Console.WriteLine($"  Term: {newTerm}")
          Console.WriteLine($"  Meaning: {newMeaning}")
          Console.WriteLine()

          if confirm "Save these changes?" then
            let updated =
              terms
              |> List.map (fun t ->
                if t.Term = oldTerm.Term then
                  { Term = newTerm; Meaning = newMeaning }
                else t)
            Storage.saveTerms updated
            wait "Term edited."
            updated
          else
            wait "Canceled. No changes were saved."
            terms

  let private deleteTerm (terms: TermEntry list) =
    ConsoleUi.clear ()
    if List.length terms <= Storage.MinimumTermCount then
      wait $"At least {Storage.MinimumTermCount} terms must remain."
      terms
    else
      terms |> List.sortBy (fun t -> t.Term) |> List.iter (fun t -> Console.WriteLine($"- {t.Term}"))
      Console.WriteLine()
      match readNonEmpty "Term to delete: " with
      | None -> wait "Empty term is not allowed."; terms
      | Some target ->
        if terms |> List.exists (fun t -> t.Term = target) then
          let updated = terms |> List.filter (fun t -> t.Term <> target)
          Storage.saveTerms updated
          wait "Term deleted."
          updated
        else
          wait "Term not found."
          terms

  let rec run (terms: TermEntry list) =
    let items = [ "Add Term"; "Edit Term"; "Delete Term"; "Back to Main Menu" ]
    match ConsoleUi.readMenu "Edit Terms" items with
    | 0 -> addTerm terms |> run
    | 1 -> editTerm terms |> run
    | 2 -> deleteTerm terms |> run
    | _ -> Storage.loadTerms ()
