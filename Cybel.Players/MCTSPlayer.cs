using Cybel.Core;
using Cybel.Learning;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Players
{
    public class MCTSPlayer : LearningPlayer
    {
        protected override ITimeManager TimeManager => new BasicTimeManager();

        private double C => GetParameter("C");
        private Table<MCTSData> Table { get; } = new();
        private Random RNG { get; } = new(Guid.NewGuid().GetHashCode());
        private int Simulations { get; set; } = 0;

        public override IEnumerable<Parameter> GetParameters()
        {
            yield return new("C", 0, 10, 1.4);
            yield return new("TimeMod", 2, 100, 2);
        }

        protected override void OnParameterChanged(string name)
        {
            if (name == "TimeMod")
            {
                ((BasicTimeManager)TimeManager).TimeMod = GetParameter("TimeMod");
            }
        }

        public override IEnumerable<KeyValuePair<Move, double>> ScoreMoves(IGame game, TimeSpan time)
        {
            var sw = Stopwatch.StartNew();
            var g = ObjectPool.Get(game.Copy, game.CopyTo);
            RunSimulation(g);

            while (sw.Elapsed < time)
            {
                game.CopyTo(g);
                RunSimulation(g);
            }

            var entry = GetTableEntry(game);

            for (int i = 0; i < entry.Moves.Count; i++)
            {
                var move = entry.Moves[i];
                var score = entry.Data!.Datas[i].Visits;

                yield return new(move, score);
            }

            ObjectPool.Add(g);
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
                moves.AddRange(game.GetMoves());
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
                moves.AddRange(game.GetMoves());
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
