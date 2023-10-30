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

        public IReadOnlyDictionary<IPlayer, int> Play(IGame game, IReadOnlyList<IPlayer> players, int games)
        {
            var results = new Dictionary<IPlayer, int>(players
                .Select(x => new KeyValuePair<IPlayer, int>(x, 0)));

            for (int i = 0; i < games; i++)
            {
                var winners = PlayGame(players.OrderBy(x => Random.Shared.Next()).ToList(), game.Copy());

                foreach (var winner in winners)
                {
                    results[winner]++;
                }
            }

            return results;
        }

        private List<IPlayer> PlayGame(IReadOnlyList<IPlayer> players, IGame game)
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
            var g = game.Copy();
            var sw = new Stopwatch();

            while (!game.IsTerminal())
            {
                var player = game.GetCurrentPlayer();
                times[player] += TimePerMove;
                game.CopyTo(g);
                sw.Restart();
                var move = players[player].GetMove(g, times[player]);
                sw.Stop();
                times[player] -= sw.Elapsed;

                if (!game.GetMoves().Contains(move))
                {
                    move = game.GetMoves().First();
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

            var winners = new List<IPlayer>();

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
