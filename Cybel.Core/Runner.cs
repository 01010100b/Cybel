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

        public IReadOnlyDictionary<Player, int> Play(IGame game, IReadOnlyList<Player> players, int games)
        {
            var results = new Dictionary<Player, int>();

            foreach (var player in players)
            {
                results.Add(player, 0);
                
                if (player is IParametrized p)
                {
                    p.LoadParameters(game);
                }
            }

            for (int i = 0; i < games; i++)
            {
                var winners = PlayGame(players.OrderBy(x => Random.Shared.Next()).ToList(), game.Copy());

                foreach (var winner in winners)
                {
                    results[winner]++;
                }
            }

            foreach (var player in players.OfType<IParametrized>())
            {
                player.SaveParameters(game);
            }

            return results;
        }

        private List<Player> PlayGame(IReadOnlyList<Player> players, IGame game)
        {
            if (players.Count != game.NumberOfPlayers)
            {
                throw new Exception("Not the correct number of players.");
            }

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
                times[player] += TimePerMove;
                game.CopyTo(g);

                sw.Restart();
                var move = players[player].ChooseMove(g, times[player]);
                sw.Stop();
                times[player] -= sw.Elapsed;
                moves.Clear();
                game.AddMoves(moves);

                if (!moves.Contains(move))
                {
                    move = moves.First();
                }

                game.Perform(move);

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

            var winners = new List<Player>();

            for (int i = 0; i < players.Count; i++)
            {
                if (game.IsWinningPlayer(i))
                {
                    winners.Add(players[i]);
                }
            }

            return winners;
        }
    }
}
