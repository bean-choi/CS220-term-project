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
    [ { Term = "recursion"; Meaning = "함수가 자기 자신을 호출하여 더 작은 문제를 해결하는 방식." }
      { Term = "closure"; Meaning = "함수와 그 함수가 정의될 때의 lexical scope에 있던 변수들이 함께 저장된 값." }
      { Term = "pattern-matching"; Meaning = "데이터의 형태에 따라 경우를 나누어 처리하는 간결한 방식." }
      { Term = "module"; Meaning = "관련된 타입, 값, 함수들을 하나로 묶은 코드 단위." }
      { Term = "namespace"; Meaning = "코드를 정리하고 이름 충돌을 피하기 위해 사용하는 이름 공간." }
      { Term = "higher-order"; Meaning = "함수를 인자로 받거나 함수를 결과로 반환하는 함수." }
      { Term = "fold"; Meaning = "컬렉션의 원소를 차례로 방문하면서 누적 결과를 만드는 함수." }
      { Term = "map"; Meaning = "컬렉션의 모든 원소에 함수를 적용하여 새 컬렉션을 만드는 함수." }
      { Term = "filter"; Meaning = "조건을 만족하는 원소만 남기는 함수." }
      { Term = "list"; Meaning = "값들이 순서대로 연결된 유한한 시퀀스." }
      { Term = "option"; Meaning = "값이 있는 경우 Some, 없는 경우 None으로 표현하는 타입." }
      { Term = "record"; Meaning = "이름이 붙은 여러 필드를 묶어서 표현하는 데이터 타입." }
      { Term = "tuple"; Meaning = "정해진 개수의 값을 순서대로 묶은 데이터 구조." }
      { Term = "union"; Meaning = "여러 가능한 case 중 하나의 값을 가질 수 있는 타입." }
      { Term = "interface"; Meaning = "어떤 타입이 구현해야 하는 멤버들의 형식을 정한 약속." }
      { Term = "polymorphism"; Meaning = "하나의 인터페이스로 여러 타입의 값을 다룰 수 있는 성질." }
      { Term = "lazy"; Meaning = "값이 실제로 필요할 때까지 계산을 미루는 방식." }
      { Term = "sequence"; Meaning = "필요한 원소를 그때그때 계산하는 지연 컬렉션." }
      { Term = "async"; Meaning = "메인 흐름과 독립적으로 실행될 수 있는 비동기 계산." }
      { Term = "monad"; Meaning = "특정 계산 문맥 안에서 계산들을 연결하기 위한 패턴." }
      { Term = "immutable"; Meaning = "한 번 만들어진 뒤에는 값이 바뀌지 않는 성질." }
      { Term = "mutable"; Meaning = "저장된 값을 나중에 바꿀 수 있는 위치나 변수." }
      { Term = "pipeline"; Meaning = "|> 연산자를 사용해 값을 여러 함수에 차례로 전달하는 방식." }
      { Term = "active-pattern"; Meaning = "match 표현식에서 사용할 수 있도록 직접 정의하는 사용자 지정 패턴." } ]

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
