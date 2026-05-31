# Term Shower

F# terminal typing game for CS-20200 Programming Principles.

## Run

```bash
dotnet run
```

The project targets `net10.0` as written in the proposal. If your local SDK is older, change `net10.0` in `TermShower.fsproj` to your installed target, such as `net8.0`.

## Data files

The game creates these files automatically under `/Data` when it runs:

- `terms.tsv`: term and meaning data
- `rankings.tsv`: local top 5 ranking data

They are plain tab-separated text files.


## Recommended terminal setting:
- Window size: at least 100 columns x 30 rows
- Font size: 12pt or 13pt
- Encoding: UTF-8

## Main menu controls

Use the arrow keys to move through the main menu.

- Up Arrow: move to the previous menu item
- Down Arrow: move to the next menu item
- Enter: select the current menu item

The main menu provides the following options:

- Start Game: start a new game
- How to Play: show the gameplay instructions
- Ranking: show the local top 5 ranking
- Edit Terms: add, edit, or delete term data
- Exit: quit the program

## Term editing

Select `Edit Terms` from the main menu to manage the term list.

The term editor provides the following operations:

- Add Term: add a new term and its meaning
- Edit Term: change an existing term or meaning
- Delete Term: remove an existing term

When adding a term, enter the new term and its meaning. Before the term is saved, the program asks:

```text
Save this new term? (y/n):