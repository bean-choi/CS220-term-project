namespace TermShower

module Ranking =
  let top5 (rankings: RankingEntry list) =
    rankings
    |> List.sortByDescending (fun (r: RankingEntry) -> r.Score)
    |> List.truncate 5

  let qualifies (score: int) (rankings: RankingEntry list) =
    let current = top5 rankings
    List.length current < 5 || current |> List.exists (fun (r: RankingEntry) -> score > r.Score)

  let add (entry: RankingEntry) (rankings: RankingEntry list) =
    entry :: rankings |> top5
