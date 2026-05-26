namespace TermShower

open System

module Stage =
  let stageEvery = TimeSpan.FromSeconds 25.0

  let currentStage (startedAt: DateTime) =
    let elapsed = DateTime.Now - startedAt
    1 + int (elapsed.TotalMilliseconds / stageEvery.TotalMilliseconds)

  let baseScore stage = 10 + (stage - 1) * 3

  let fallIntervalMs stage kind =
    let baseInterval = max 120 (650 - (stage - 1) * 35)
    match kind with
    | Fast -> max 70 (baseInterval / 2)
    | _ -> baseInterval

  let spawnIntervalRangeMs stage =
    let minInterval = max 450 (1800 - (stage - 1) * 90)
    let maxInterval = max 900 (3200 - (stage - 1) * 130)
    minInterval, maxInterval

  let nextSpawnTime (rng: Random) stage =
    let minMs, maxMs = spawnIntervalRangeMs stage
    DateTime.Now.AddMilliseconds(float (rng.Next(minMs, maxMs + 1)))

  let spawnCount (rng: Random) stage =
    let p2 = min 45 (8 + stage * 3)
    let p3 = min 20 (stage)
    let roll = rng.Next(100)
    if roll < p3 then 3
    elif roll < p3 + p2 then 2
    else 1

  let chooseSpecialKind (rng: Random) stage =
    let specialChance = min 42 (10 + stage * 2)
    if rng.Next(100) >= specialChance then Normal
    else
      let harmfulWeight = 6 + stage
      let beneficialWeight = 7 + stage / 2
      let candidates =
        [ Heal, beneficialWeight
          Bonus, beneficialWeight
          Clear, 1
          Transform, harmfulWeight
          Blink, harmfulWeight
          Fast, harmfulWeight ]
      let total = candidates |> List.sumBy snd
      let roll = rng.Next(total)
      let rec pick acc = function
        | [] -> Normal
        | (kind, weight) :: tl ->
          let next = acc + weight
          if roll < next then kind else pick next tl
      pick 0 candidates

  let scoreMultiplier = function
    | Normal -> 1
    | Heal -> 1
    | Transform -> 2
    | Blink -> 2
    | Fast -> 2
    | Bonus -> 4
    | Clear -> 0
