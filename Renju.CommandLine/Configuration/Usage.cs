namespace Renju.CommandLine.Configuration;

public static class Usage
{
    public const string Text =
        """
        Renju console game

        usage: renju [options]

        options:
          --black <spec>   black player (default: human)
          --white <spec>   white player (default: graph)
          --board <n>      board size, 5-19 (default: 19)
          --config <path>  config file (default: ./renju.json if present)
          --arena [N]      play N AI-vs-AI games, print totals (N may come from config)
          --seed <n>       arena: master seed for a reproducible series
          --no-alternate   arena: do not swap colors between games
          --help, -h       this help

        player spec:  human | plain[:opt=val,...] | graph[:opt=val,...]
          graph only:  depth=1..8 (default 3)          prediction depth, plies
                       topK=1..50 (default 10)         candidate moves per node
          any AI:      timeout=0|50..60000 (default 0) ms cap per move, 0 = off
                       minDelay=0..10000 (default 0)   ms floor per move

        examples:
          renju --white graph:depth=4,topK=8,minDelay=300
          renju --black plain --white graph:depth=5 --board 15
          renju --arena 100 --black plain --white graph --seed 42

        config file (renju.json), same settings, CLI overrides it per value:
          {
            "board": 19,
            "players": {
              "black": { "type": "human" },
              "white": { "type": "graph", "depth": 3, "topK": 10, "minDelay": 300 }
            },
            "arena": { "games": 100, "seed": 42, "alternate": true }
          }
        """;
}
