using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Core.Players
{
    public class MatchboxPlayer : IScoringPlayer, IParametrized<MatchboxPlayerParameters>
    {
        private class Matchbox
        {
            public List<int> Pips { get; } = new();

            public Matchbox(int moves, int pips)
            {
                for (int i = 0; i < moves; i++)
                {
                    Pips.Add(pips);
                }
            }
        }

        public MatchboxPlayerParameters Parameters { get; set; } = new();

        private Table<Matchbox> Table { get; } = new();
        private Random RNG { get; } = new Random(Guid.NewGuid().GetHashCode());
        private int Simulations { get; set; } = 0;

        public void LoadParameters(IGame game)
        {
            Parameters = new();
        }

        public Dictionary<Move, double> ScoreMoves(IGame game, TimeSpan time)
        {
            RunSimulations(game.Copy(), time / Math.Max(1.1, Parameters.TimeMod));

            var entry = Table.GetEntry(game);
            entry.Data ??= new(entry.Moves.Count, Parameters.InitialPips);
            var scores = new Dictionary<Move, double>();

            for (int i = 0; i < entry.Moves.Count; i++)
            {
                var move = entry.Moves[i];
                var score = (double)entry.Data.Pips[i];
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

            while (sw.Elapsed < time)
            {
                Simulations++;
                game.CopyTo(g);
                choices.Clear();
                winners.Clear();

                while (!g.IsTerminal())
                {
                    var entry = Table.GetEntry(g);
                    entry.Data ??= new(entry.Moves.Count, Parameters.InitialPips);
                    var choice = RNG.NextDouble() < Parameters.ExploreChance ? RNG.Next(entry.Moves.Count) : Choose(entry.Data);
                    var move = entry.Moves[choice];
                    choices.Add(new(entry, choice));
                    g.Perform(move);
                }

                for (int i = 0; i < g.NumberOfPlayers; i++)
                {
                    if (g.IsWinningPlayer(i))
                    {
                        winners.Add(i);
                    }
                }

                if (winners.Count == 0)
                {
                    continue;
                }

                for (int i = 0; i < choices.Count; i++)
                {
                    var depth = Math.Max(1, choices.Count - i);
                    var chance = Math.Pow(depth, Parameters.PipChangeMod);

                    if (RNG.NextDouble() > chance)
                    {
                        continue;
                    }

                    var choice = choices[i];
                    var pips = choice.Key.Data!.Pips;

                    if (winners.Contains(choice.Key.Player))
                    {
                        pips[choice.Value] = Math.Min(Parameters.MaxPips, pips[choice.Value] + 1);
                    }
                    else
                    {
                        pips[choice.Value] = Math.Max(0, pips[choice.Value] - 1);
                    }
                }
            }
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
