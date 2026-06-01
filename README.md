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

## Gameplay controls

During the game, the player types the falling term shown on the screen.

* Type letters, numbers, and hyphen characters as the input term.
* Press Backspace to delete the last typed character.
* Press Enter to submit the current input.
* Press Esc to quit the current game and return to the main menu or ranking flow.
* If the typed input exactly matches a falling term, that term is removed.

When several falling terms have the same text, the game removes the lowest matching term first.

## Gameplay rules

The game starts with 3 HP.

A term falls from the top of the terminal screen toward the bottom. If a normal term reaches the bottom before the player removes it, the player loses 1 HP. The game ends when HP becomes 0.

During gameplay, the screen shows the current stage, HP, score, falling terms, term meanings, and the input box.

The game becomes harder as the stage increases. Higher stages make terms fall faster and increase the chance that multiple terms appear during one spawn event.

## Special terms

Some terms have special effects.

* Heal term: increases HP by 1, up to the maximum HP limit.
* Transform term: appears in red and changes its displayed text while falling.
* Blink term: periodically disappears and reappears.
* Fast term: falls faster than normal terms.
* Bonus term: gives an additional score bonus when removed.
* Clear term: removes other falling terms. The clear term itself does not give its own score; score is awarded only for the other terms removed by the clear effect.

## Score and ranking

The score increases when the player correctly removes terms. A longer term gives more score. The score reward can increase as the stage increases.

After the game ends, the program checks whether the score belongs in the local top 5 ranking. If it does, the player can enter a nickname. The ranking stores at least the following information:

* nickname
* final score
* number of correctly typed terms

The ranking is stored locally in `Data/rankings.tsv`.

If the player quits during gameplay with Esc and the current score belongs in the local top 5 ranking, the nickname input screen is shown before returning to the main menu.

## Requirement changes

The final implementation follows the submitted proposal. No major gameplay requirement was intentionally removed.

The implementation uses a terminal-based interface, so the game is designed for a sufficiently large terminal window rather than a graphical window. The recommended terminal size is described above.

If the terminal size changes during gameplay, the current game is stopped to avoid broken screen rendering. If the current score belongs in the ranking, the nickname input screen is shown; otherwise, the program returns to the main menu.

## LLM usage

I used an LLM during development of this project. The LLM was used for implementation assistance, debugging F# compiler errors and organizing code structure.

Some outputs from the LLM required manual changes. In particular, I had to manually check whether the implementation matched the proposed game behavior, adjust the ranking flow, fix terminal rendering behavior, verify term removal order, and make sure the in-game screen did not display unnecessary log messages.

The main point that the LLM did not correctly handle at first was preserving all project-specific requirements consistently across different files. Therefore, I manually reviewed and modified the final implementation so that the behavior matched the proposal and the README.
