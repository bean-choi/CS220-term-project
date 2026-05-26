namespace TermShower

open System
open System.IO

module Storage =
  [<Literal>]
  let MinimumTermCount = 20

  let private dataDir = Path.Combine(AppContext.BaseDirectory, "Data")
  let termsPath = Path.Combine(dataDir, "terms.tsv")
  let rankingsPath = Path.Combine(dataDir, "rankings.tsv")

  let private sanitize (s: string) =
    s.Replace("\t", " ").Replace("\r", " ").Replace("\n", " ").Trim()

  let defaultTerms : TermEntry list =
    [ { Term = "recursion"; Meaning = "A function calls itself to solve a smaller version of a problem." }
      { Term = "closure"; Meaning = "A function value that remembers variables from its lexical scope." }
      { Term = "pattern-matching"; Meaning = "A concise way to branch by the shape of data." }
      { Term = "module"; Meaning = "A group of related types, values, and functions." }
      { Term = "namespace"; Meaning = "A named scope used to organize code and avoid name collisions." }
      { Term = "higher-order"; Meaning = "A function that takes or returns another function." }
      { Term = "fold"; Meaning = "A function that accumulates a result by visiting elements." }
      { Term = "map"; Meaning = "A function that transforms every element in a collection." }
      { Term = "filter"; Meaning = "A function that keeps only elements satisfying a predicate." }
      { Term = "list"; Meaning = "A finite sequence of values." }
      { Term = "option"; Meaning = "A type representing Some value or None." }
      { Term = "record"; Meaning = "A data type with named fields." }
      { Term = "tuple"; Meaning = "A fixed-size grouping of values." }
      { Term = "union"; Meaning = "A type with several possible cases." }
      { Term = "interface"; Meaning = "A contract describing members that a type must implement." }
      { Term = "polymorphism"; Meaning = "Using one interface for values of different types." }
      { Term = "lazy"; Meaning = "A computation delayed until its value is needed." }
      { Term = "sequence"; Meaning = "A stream-like collection evaluated on demand." }
      { Term = "async"; Meaning = "A computation that can run independently from the main flow." }
      { Term = "monad"; Meaning = "A pattern for composing context-sensitive computations." }
      { Term = "immutable"; Meaning = "A value that cannot be changed after creation." }
      { Term = "mutable"; Meaning = "A location whose stored value can be changed." }
      { Term = "pipeline"; Meaning = "Using |> to pass a value through functions." }
      { Term = "active-pattern"; Meaning = "A custom pattern used inside match expressions." } ]

  let ensureDataFiles () =
    Directory.CreateDirectory(dataDir) |> ignore
    if not (File.Exists termsPath) then
      let lines : string array =
        defaultTerms
        |> List.map (fun t -> $"{sanitize t.Term}\t{sanitize t.Meaning}")
        |> List.toArray
      File.WriteAllLines(termsPath, lines)
    if not (File.Exists rankingsPath) then
      File.WriteAllText(rankingsPath, "")

  let private parseTermLine (line: string) : TermEntry option =
    match line.Split('\t', 2) with
    | [| term; meaning |] when term.Trim() <> "" -> Some { Term = term.Trim(); Meaning = meaning.Trim() }
    | _ -> None

  let loadTerms () : TermEntry list =
    ensureDataFiles ()
    File.ReadAllLines termsPath
    |> Array.choose parseTermLine
    |> Array.toList
    |> fun terms -> if List.length terms < MinimumTermCount then defaultTerms else terms

  let saveTerms (terms: TermEntry list) =
    ensureDataFiles ()
    let lines : string array =
      terms
      |> List.sortBy (fun t -> t.Term)
      |> List.map (fun t -> $"{sanitize t.Term}\t{sanitize t.Meaning}")
      |> List.toArray
    File.WriteAllLines(termsPath, lines)

  let private parseRankingLine (line: string) : RankingEntry option =
    match line.Split('\t') with
    | [| nickname; score; correct |] ->
      match Int32.TryParse score, Int32.TryParse correct with
      | (true, s), (true, c) -> Some { Nickname = nickname.Trim(); Score = s; CorrectTyped = c }
      | _ -> None
    | _ -> None

  let loadRankings () : RankingEntry list =
    ensureDataFiles ()
    File.ReadAllLines rankingsPath
    |> Array.choose parseRankingLine
    |> Array.toList

  let saveRankings (rankings: RankingEntry list) =
    ensureDataFiles ()
    let lines : string array =
      rankings
      |> List.sortByDescending (fun r -> r.Score)
      |> List.truncate 5
      |> List.map (fun r -> $"{sanitize r.Nickname}\t{r.Score}\t{r.CorrectTyped}")
      |> List.toArray
    File.WriteAllLines(rankingsPath, lines)
