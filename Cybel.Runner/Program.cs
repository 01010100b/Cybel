using Cybel.Core;
using Cybel.Core.Players;
using System.Diagnostics;

namespace Cybel.Runner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var tictactoe = new TicTacToe();
            RunTournament(tictactoe, 1, true);
            RunTournament(tictactoe, 100);
        }

        private static void RunTournament(IGame game, int games, bool console = false)
        {
            Console.WriteLine($"running tournament of {games} games...");
            var sw = new Stopwatch();
            sw.Start();
            var runner = new Core.Runner() { WriteToConsole = console };
            var players = new List<IPlayer>()
            {
                new RandomPlayer(),
                new MatchboxPlayer()
            };

            var results = runner.Play(game, players, games);

            for (int i = 0; i < players.Count; i++)
            {
                var player = players[i];
                var wins = results[player];
                var perc = wins / (double)games;

                Console.WriteLine($"{i} {player.GetType().Name} wins {wins:N0} ({perc:P0})");
            }

            Console.WriteLine($"tournament took {sw.Elapsed}");
        }
    }
}