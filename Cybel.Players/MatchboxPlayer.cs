using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cybel.Core;
using Cybel.Learning;
using Microsoft.VisualBasic;

namespace Cybel.Players
{
    public class MatchboxPlayer : LearningPlayer
    {
        protected override ITimeManager TimeManager => new BasicTimeManager();

        private int MaxPips { get; set; } = 10000; // Math.Clamp(max_pips, 0, int.MaxValue);
        private int InitialPips { get; set; } = 1000; // Math.Clamp(initial_pips, 0, int.MaxValue);
        private double ExploreChance { get; set; } = 0.7; // Math.Clamp(explore_chance, 0, 1);
        private double PipChangeMod { get; set; } = -0.3; // Math.Clamp(pip_change_mod, double.MinValue, 0);

        private Table<MatchboxData> Table { get; } = new();
        private Random RNG { get; } = new Random(Guid.NewGuid().GetHashCode());
        private int Simulations { get; set; } = 0;

        public override IEnumerable<Parameter> GetParameters()
        {
            yield return new("MaxPips", 0, int.MaxValue, 10000);
            yield return new("InitialPips", 0, int.MaxValue, 1000);
            yield return new("ExploreChance", 0, 1, 0.7);
            yield return new("PipChangeMod", double.MinValue, 0, -0.3);
            yield return new("TimeMod", 2, 100, 2);
        }

        protected override void OnParameterChanged(string name)
        {
            if (name == "TimeMod")
            {
                ((BasicTimeManager)TimeManager).TimeMod = GetParameter("TimeMod");
            }
        }

        public override Dictionary<Move, double> ScoreMoves(IGame game, TimeSpan time)
        {
            RunSimulations(game.Copy(), time);

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
                    var choice = RNG.NextDouble() < ExploreChance ? RNG.Next(entry.Moves.Count) : Choose(entry.Data!);
                    var move = entry.Moves[choice];
                    choices.Add(new(entry, choice));
                    g.Perform(move);
                }

                for (int i = 0; i < g.NumberOfPlayers; i++)
                {
                    scores.Add(g.GetPlayerScore(i));
                }

                for (int i = 0; i < choices.Count; i++)
                {
                    var choice = choices[i];
                    var pips = choice.Key.Data!.Pips;
                    var score = scores[choice.Key.Player] * 2 - 1;
                    var depth = choices.Count - i;
                    score *= Math.Pow(depth, PipChangeMod);

                    score += pips[choice.Value];
                    score = Math.Clamp(score, 0, MaxPips);
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
                entry.Data.Initialize(entry.Moves.Count, InitialPips);
            }

            return entry;
        }

        public override string ToString()
        {
            return $"simulations: {Simulations:N0}";
        }
    }
}
