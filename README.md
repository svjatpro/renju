# Renju

Console Renju (five-in-a-row with renju rules) with a fast, math-model AI.
C# / .NET 8. The engine (`Renju.Core`) is strictly separated from clients; `Renju.CommandLine` is the console client.

## Build & run

```
dotnet build Renju.sln
dotnet run --project Renju.CommandLine            # human (black) vs Graph AI, board 19
```

In game: arrows move the cursor, space puts a stone, `n` starts a new game (colors swap), `l` switches layout, `esc` exits.

## Players & AI types

Each color is configured independently as `human` or one of the AI types:

- **plain** — evaluates only the current board state; fast baseline.
- **graph** — keeps a predicted game graph between moves and picks the best path through it; the stronger, default AI.

## Command line

```
renju [options]

--black <spec>   black player (default: human)
--white <spec>   white player (default: graph)
--board <n>      board size, 5-19 (default: 19)
--config <path>  config file (default: ./renju.json if present)
--help, -h       help
```

Player spec: `human` | `plain[:opt=val,...]` | `graph[:opt=val,...]`

| Option | Applies to | Default | Range | Meaning |
|---|---|---|---|---|
| `depth` | graph | 3 | 1–8 | prediction depth, plies |
| `topK` | graph | 10 | 1–50 | candidate moves per node |
| `timeout` | any AI | 0 (off) | 50–60000 | ms cap on thinking per move; on expiry plays the best move found so far |
| `minDelay` | any AI | 0 | 0–10000 | ms floor per move, keeps fast AIs watchable |

Examples:

```
renju --white graph:depth=4,topK=8,minDelay=300      # tuned AI opponent
renju --black graph --white human                    # play white
renju --black plain --white graph:minDelay=500       # AI vs AI, watchable
renju --board 15
```

## Arena

AI-vs-AI series runner — the measuring tool for AI changes: `renju --arena <N> --black <spec> --white <spec>`.

- Colors alternate every game (`--no-alternate` to disable); results are reported per player, split by color.
- `--seed <n>` makes the whole series reproducible; without it a random master seed is picked and printed, so any run can be replayed.
- Both players must be AI; `minDelay` is ignored (runs at full speed), `timeout` is honored.
- A game that exceeds board² moves or where a player fails to move counts as "stuck", not as a result.

```
renju --arena 100 --black plain --white graph:depth=4 --seed 42

arena: plain (P1) vs graph:depth=4 (P2) - 100 games, board 19, seed 42

P1  plain                   0 wins  (0 as black, 0 as white)
P2  graph:depth=4         100 wins  (50 as black, 50 as white)
draws 0, avg moves/game 36

move time ms (p50/p95/max):  P1 0/0/19   P2 228/319/497
total: 416.2 s
```

## Config file

Same settings as the CLI; JSON. Default `renju.json` in the current directory, or `--config <path>`.
Precedence: defaults < config file < command line, merged per value (a CLI option overrides only the value it names;
if the CLI changes a player's *type*, that player's file options are discarded).

```json
{
  "board": 19,
  "players": {
    "black": { "type": "human" },
    "white": { "type": "graph", "depth": 3, "topK": 10, "timeout": 0, "minDelay": 300 }
  },
  "arena": { "games": 100, "seed": 42, "alternate": true }
}
```

## Projects

- `Renju.Core` — engine: board, referee (renju rules), AI analysers. No console/file I/O.
- `Renju.CommandLine` — console client: rendering, input, CLI/config parsing.
- `Renju.Core.Tests`, `Renju.CommandLine.Tests` — NUnit test suites (`dotnet test`).
