using Cybel.Core;
using Cybel.Core.Players;
using Cybel.Games;
using System.Diagnostics;

namespace Cybel.Runner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var game = MNK.GetTicTacToe();
            RunTournament(game.Copy(), 1, true);
            RunTournament(game.Copy(), 100);
        }

        private static void RunTournament(IGame game, int games, bool verbose = false)
        {
            Console.WriteLine($"Running tournament of {games} games...");
            var sw = new Stopwatch();
            sw.Start();
            var runner = new Core.Runner() { WriteToConsole = verbose };
            var players = new List<Player>()
            {
                new RandomPlayer(),
                new MatchboxPlayer()
            };

            var results = runner.Play(game, players, games);

            for (int i = 0; i < players.Count; i++)
            {
                var player = players[i];
                var score = results[player] / games;

                Console.WriteLine($"{i} {player.GetType().Name} score {score:P0}");
            }

            Console.WriteLine($"Tournament took {sw.Elapsed}");
            Console.WriteLine();
        }
    }
}