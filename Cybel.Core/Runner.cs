using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Core
{
    public class Runner
    {
        public bool WriteToConsole { get; set; } = false;
        public TimeSpan TimeBank { get; set; } = TimeSpan.FromSeconds(1);
        public TimeSpan TimePerMove { get; set; } = TimeSpan.FromSeconds(0.1);

        public Dictionary<Player, double> Play(IGame game, IReadOnlyList<Player> players, int games)
        {
            if (players.Count != game.NumberOfPlayers)
            {
                throw new Exception("Not the correct number of players.");
            }

            var results = new Dictionary<Player, double>();

            foreach (var player in players)
            {
                results.Add(player, 0);
            }

            for (int i = 0; i < games; i++)
            {
                var scores = PlayGame(players.OrderBy(x => Random.Shared.Next()).ToList(), game.Copy());

                foreach (var score in scores)
                {
                    results[score.Key] += score.Value;
                }
            }

            return results;
        }

        private Dictionary<Player, double> PlayGame(IReadOnlyList<Player> players, IGame game)
        {
            if (WriteToConsole)
            {
                Console.WriteLine(game);
            }

            var times = players.Select(x => TimeBank).ToList();
            var moves = new List<Move>();
            var g = game.Copy();
            var sw = new Stopwatch();

            while (!game.IsTerminal())
            {
                var player = game.GetCurrentPlayer();
                moves.Clear();
                game.AddMoves(moves);

                if (times[player] > TimeSpan.Zero)
                {
                    times[player] += TimePerMove;
                    game.CopyTo(g);

                    sw.Restart();
                    var move = players[player].ChooseMove(g, times[player]);
                    sw.Stop();
                    times[player] -= sw.Elapsed;

                    if (!moves.Contains(move))
                    {
                        move = moves.First();
                    }

                    game.Perform(move);
                }
                else
                {
                    game.Perform(moves.First());
                }

                if (WriteToConsole)
                {
                    for (int i = 0; i < players.Count; i++)
                    {
                        var p = players[i];
                        Console.WriteLine($"{i} {p.GetType().Name} {p}");
                    }

                    Console.WriteLine(game);
                }
            }

            var scores = new Dictionary<Player, double>();

            for (int i = 0; i < players.Count; i++)
            {
                scores.Add(players[i], Math.Clamp(game.GetPlayerScore(i), 0, 1));
            }

            return scores;
        }
    }
}
