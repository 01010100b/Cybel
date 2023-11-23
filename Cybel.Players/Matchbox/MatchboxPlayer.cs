using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cybel.Core;

namespace Cybel.Players.Matchbox
{
    public class MatchboxPlayer : Player, IParametrized<MatchboxPlayerParameters>
    {
        public MatchboxPlayerParameters Parameters { get; set; } = new();

        private Table<MatchboxData> Table { get; } = new();
        private Random RNG { get; } = new Random(Guid.NewGuid().GetHashCode());
        private int Simulations { get; set; } = 0;

        public override Dictionary<Move, double> ScoreMoves(IGame game, TimeSpan time)
        {
            Parameters.Validate();
            RunSimulations(game.Copy(), time / Parameters.TimeMod);

            var entry = GetTableEntry(game);
            var scores = new Dictionary<Move, double>();

            for (int i = 0; i < entry.Moves.Count; i++)
            {
                var move = entry.Moves[i];
                var score = (double)entry.Data!.Pips[i];
                scores.Add(move, score);
            }

            return scores;
        }

        private void RunSimulations(IGame game, TimeSpan time)
        {
            var g = game.Copy();
            var choices = new List<KeyValuePair<Table<MatchboxData>.Entry, int>>();
            var scores = new List<double>();
            var sw = new Stopwatch();
            sw.Start();

            while (sw.Elapsed < time)
            {
                Simulations++;
                game.CopyTo(g);
                choices.Clear();
                scores.Clear();

                while (!g.IsTerminal())
                {
                    var entry = GetTableEntry(g);
                    var choice = RNG.NextDouble() < Parameters.ExploreChance ? RNG.Next(entry.Moves.Count) : Choose(entry.Data!);
                    var move = entry.Moves[choice];
                    choices.Add(new(entry, choice));
                    g.Perform(move);
                }

                for (int i = 0; i < g.NumberOfPlayers; i++)
                {
                    scores.Add(Math.Clamp(g.GetPlayerScore(i), 0, 1));
                }

                for (int i = 0; i < choices.Count; i++)
                {
                    var choice = choices[i];
                    var pips = choice.Key.Data!.Pips;
                    var score = scores[choice.Key.Player] * 2 - 1;
                    var depth = choices.Count - i;
                    score *= Math.Pow(depth, Parameters.PipChangeMod);

                    score += pips[choice.Value];
                    score = Math.Clamp(score, 0, Parameters.MaxPips);
                    pips[choice.Value] = score;
                }
            }
        }

        private int Choose(MatchboxData matchbox)
        {
            var total = matchbox.Pips.Sum();

            if (total > 0)
            {
                var goal = RNG.NextDouble() * total;
                var acc = 0d;

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

        private Table<MatchboxData>.Entry GetTableEntry(IGame game)
        {
            var entry = Table.GetEntry(game);

            if (entry.Data is null)
            {
                entry.Data = ObjectPool.Get(() => new MatchboxData());
                entry.Data.Initialize(entry.Moves.Count, Parameters.InitialPips);
            }

            return entry;
        }

        public override string ToString()
        {
            return $"simulations: {Simulations:N0}";
        }
    }
}
