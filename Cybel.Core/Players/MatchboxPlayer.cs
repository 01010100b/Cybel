using Cybel.Core.Trees;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Core.Players
{
    public class MatchboxPlayer : IPlayer
    {
        private class Matchbox
        {
            public List<int> Pips { get; set; } = new();
        }

        public double TimeMod { get; set; } = 2;
        public double ExploreChance { get; set; } = 0.3;
        public int InitialPips { get; set; } = 3;
        public int MaxPips { get; set; } = 100;

        private Tree<Matchbox> Tree { get; } = new();
        private Random RNG { get; } = new Random(Guid.NewGuid().GetHashCode());
        private int Simulations { get; set; } = 0;

        public Move GetMove(IGame game, TimeSpan time)
        {
            RunSimulations(game.Copy(), time / Math.Max(1.1, TimeMod));
            
            var node = Tree.GetNode(game);
            var choice = RNG.Next(node.Moves.Count);

            if (node.Data is not null)
            {
                choice = Choose(node.Data);
            }

            var move = node.Moves[choice];

            return move;
        }

        private void RunSimulations(IGame game, TimeSpan time)
        {
            var g = game.Copy();
            var choices = new List<KeyValuePair<TreeNode<Matchbox>, int>>();
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
                    var node = Tree.GetNode(g);
                    
                    if (node.Data is null)
                    {
                        var data = new Matchbox();

                        for (int i = 0; i < node.Moves.Count; i++)
                        {
                            data.Pips.Add(InitialPips);
                        }

                        node.Data = data;
                    }

                    var choice = RNG.NextDouble() < ExploreChance ? RNG.Next(node.Moves.Count) : Choose(node.Data);
                    var move = node.Moves[choice];

                    g.Perform(move);
                    choices.Add(new(node, choice));
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
                    foreach (var choice in choices)
                    {
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
