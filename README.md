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