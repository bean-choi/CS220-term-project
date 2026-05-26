namespace TermShower

open System

type TermEntry = {
  Term: string
  Meaning: string
}

type SpecialKind =
  | Normal
  | Heal
  | Transform
  | Blink
  | Fast
  | Bonus
  | Clear

type FallingTerm = {
  Id: int
  Entry: TermEntry
  X: int
  Y: int
  Kind: SpecialKind
  ScoreMultiplier: int
  LastMove: DateTime
}

type GameState = {
  Health: int
  Score: int
  Stage: int
  CorrectTyped: int
  LastMeaning: TermEntry option
  FallingTerms: FallingTerm list
  StartedAt: DateTime
  NextSpawnAt: DateTime
  NextId: int
}

type RankingEntry = {
  Nickname: string
  Score: int
  CorrectTyped: int
}
