using Cybel.Core.Tables;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Core.Players
{
    public class MatchboxPlayer : ScoringPlayerBase
    {
        private class Matchbox
        {
            public List<int> Pips { get; set; } = new();
        }

        public double TimeMod { get; set; } = 2;
        public double ExploreChance { get; set; } = 0.3;
        public int InitialPips { get; set; } = 3;
        public int MaxPips { get; set; } = 1000;
        public double PipChangeMod { get; set; } = -0.2;

        private Table<Matchbox> Table { get; } = new();
        private Random RNG { get; } = new Random(Guid.NewGuid().GetHashCode());
        private int Simulations { get; set; } = 0;

        public override Dictionary<Move, double> ScoreMoves(IGame game, TimeSpan time)
        {
            RunSimulations(game.Copy(), time / Math.Max(1.1, TimeMod));

            var entry = Table.GetEntry(game);
            var pips = entry.Data!.Pips;
            var scores = new Dictionary<Move, double>();

            for (int i = 0; i < entry.Moves.Count; i++)
            {
                var move = entry.Moves[i];
                var score = (double)pips[i];
                scores.Add(move, score);
            }

            return scores;
        }

        private void RunSimulations(IGame game, TimeSpan time)
        {
            var g = game.Copy();
            var choices = new List<KeyValuePair<Table<Matchbox>.Entry, int>>();
            var winners = new List<int>();
            var sw = new Stopwatch();
            sw.Start();

            do
            {
                game.CopyTo(g);
                choices.Clear();
                winners.Clear();

                while (!g.IsTerminal())
                {
                    var entry = Table.GetEntry(g);
                    
                    if (entry.Data is null)
                    {
                        var data = new Matchbox();

                        for (int i = 0; i < entry.Moves.Count; i++)
                        {
                            data.Pips.Add(InitialPips);
                        }

                        entry.Data = data;
                    }

                    var choice = RNG.NextDouble() < ExploreChance ? RNG.Next(entry.Moves.Count) : Choose(entry.Data);
                    var move = entry.Moves[choice];

                    g.Perform(move);
                    choices.Add(new(entry, choice));
                }

                for (int i = 0; i < g.NumberOfPlayers; i++)
                {
                    if (g.IsWinningPlayer(i))
                    {
                        winners.Add(i);
                    }
                }

                if (winners.Count > 0)
                {
                    for (int i = 0; i < choices.Count; i++)
                    {
                        var left = Math.Max(1, choices.Count - i);
                        var chance = Math.Pow(left, PipChangeMod);

                        if (RNG.NextDouble() > chance)
                        {
                            continue;
                        }

                        var choice = choices[i];
                        var pips = choice.Key.Data!.Pips;

                        if (winners.Contains(choice.Key.Player))
                        {
                            pips[choice.Value] = Math.Min(MaxPips, pips[choice.Value] + 1);
                        }
                        else
                        {
                            pips[choice.Value] = Math.Max(0, pips[choice.Value] - 1);
                        }
                    }
                }

                Simulations++;

            } while (sw.Elapsed < time);
        }

        private int Choose(Matchbox matchbox)
        {
            var total = matchbox.Pips.Sum();

            if (total > 0)
            {
                var goal = RNG.Next(total);
                var acc = 0;

                for (int i = 0; i < matchbox.Pips.Count; i++)
                {
                    acc += matchbox.Pips[i];

                    if (acc > goal)
                    {
                        return i;
                    }
                }
            }

            return RNG.Next(matchbox.Pips.Count);
        }

        public override string ToString()
        {
            return $"simulations: {Simulations:N0}";
        }
    }
}
