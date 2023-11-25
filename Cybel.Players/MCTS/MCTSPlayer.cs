using Cybel.Core;
using Cybel.Players.Matchbox;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Players.MCTS
{
    public class MCTSPlayer : Player
    {
        public double C { get; set; } = 1.4;

        private Table<MCTSData> Table { get; } = new();
        private Random RNG { get; } = new(Guid.NewGuid().GetHashCode());
        private int Simulations { get; set; } = 0;

        public override Dictionary<Move, double> ScoreMoves(IGame game, TimeSpan time)
        {
            C = Math.Clamp(C, 0, double.MaxValue);

            var sw = new Stopwatch();
            sw.Start();

            var g = game.Copy();
            RunSimulation(g);

            while (sw.Elapsed < time)
            {
                game.CopyTo(g);
                RunSimulation(g);
            }

            var entry = GetTableEntry(game);
            var scores = new Dictionary<Move, double>();

            for (int i = 0; i < entry.Moves.Count; i++)
            {
                var move = entry.Moves[i];
                var score = entry.Data!.Datas[i].Visits;
                scores[move] = score;
            }

            return scores;
        }

        private void RunSimulation(IGame game)
        {
            Simulations++;
            var choices = ObjectPool.Get(() => new Dictionary<Table<MCTSData>.Entry, int>(), x => x.Clear());
            var moves = ObjectPool.Get(() => new List<Move>(), x => x.Clear());

            // selection & expansion

            while (true)
            {
                if (game.IsTerminal())
                {
                    break;
                }

                var entry = GetTableEntry(game);
                var choice = entry.Data!.Choose(C);
                choices.Add(entry, choice);
                moves.Clear();
                game.AddMoves(moves);
                game.Perform(moves[choice]);

                if (entry.Data.TotalVisits <= 0)
                {
                    break;
                }
            }

            // simulation

            while (!game.IsTerminal())
            {
                moves.Clear();
                game.AddMoves(moves);
                game.Perform(moves[RNG.Next(moves.Count)]);
            }

            // backpropagation

            foreach (var choice in choices)
            {
                if (choice.Key.Data is null)
                {
                    continue;
                }

                var entry = choice.Key;
                var move = choice.Value;
                var score = game.GetPlayerScore(entry.Player);
                var data = entry.Data.Datas[move];

                data.Score += score;
                data.Visits++;

                entry.Data.Datas[move] = data;
            }

            ObjectPool.Add(choices);
            ObjectPool.Add(moves);
        }

        private Table<MCTSData>.Entry GetTableEntry(IGame game)
        {
            var entry = Table.GetEntry(game);

            if (entry.Data is null)
            {
                entry.Data = ObjectPool.Get(() => new MCTSData());
                entry.Data.Initialize(entry.Moves.Count);
            }

            return entry;
        }

        public override string ToString()
        {
            return $"simulations: {Simulations:N0}";
        }
    }
}
