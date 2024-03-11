using Cybel.Core;
using Cybel.Core.Players;
using Cybel.Games;
using Cybel.Players;
using System.Diagnostics;
using System.Text;

namespace Cybel.Runner
{
    internal class Program
    {
        public static string Folder => AppDomain.CurrentDomain.BaseDirectory;

        public static string GetBookFile(IGame game)
        {
            var id = Convert.ToHexString(Encoding.UTF8.GetBytes(game.Name));

            return Path.Combine(Folder, $"book-{id}.json");
        }

        static void Main(string[] args)
        {
            var game = MNK.GetConnectFour();
            var book = GenerateBook(game.Copy(), () => new MCTSPlayer());
            Console.WriteLine($"Book with {book.Count:N0} positions");
            List<Player> players = [new MCTSPlayer(), new MCTSPlayer()];
            
            foreach (var position in book)
            {
                players[1].Openings.Add(position.Key, position.Value);
            }

            RunTournament(game.Copy(), players, 100);
        }

        private static void RunTournament(IGame game, List<Player> players, int games)
        {
            Console.WriteLine($"Running tournament of {games} games...");
            var sw = Stopwatch.StartNew();
            var runner = new Core.Runner();

            var results = runner.Play(game, players, games);

            for (int i = 0; i < players.Count; i++)
            {
                var player = players[i];
                Console.WriteLine($"{i} {player.GetType().Name} {results[player]:N2}");
            }

            Console.WriteLine($"Tournament took {sw.Elapsed}");
            Console.WriteLine();
        }

        private static Dictionary<ulong, ulong> GenerateBook(IGame start_position, Func<Player> player_factory)
        {
            var time_per_position = TimeSpan.FromSeconds(20);
            var file = GetBookFile(start_position);
            var openings = new OpeningsGenerator(start_position, player_factory);

            var debug_file = Path.Combine(Folder, "debug.txt");
            var last_debug = DateTime.UtcNow;

            if (File.Exists(debug_file))
            {
                File.Delete(debug_file);
            }

            File.WriteAllLines(debug_file, [$"debug start {DateTime.Now}"]);

            if (File.Exists(file))
            {
                using var stream = File.OpenRead(file);
                openings.Load(stream);
            }
            
            openings.Start(time_per_position);
            var sw = Stopwatch.StartNew();

            Console.WriteLine("Generating book, press any key to stop...");

            while (!Console.KeyAvailable)
            {
                if (DateTime.UtcNow > last_debug + TimeSpan.FromHours(1))
                {
                    File.AppendAllLines(debug_file, [$"checkpoint {DateTime.Now}"]);
                    last_debug = DateTime.UtcNow;
                }

                if (sw.Elapsed < TimeSpan.FromMinutes(1))
                {
                    Thread.Sleep(100);
                }
                else
                {
                    File.Delete(file);
                    using var stream = File.OpenWrite(file);
                    openings.Save(stream);

                    sw.Restart();
                }
            }

            Console.WriteLine($"Stopping... (may take {time_per_position})");

            openings.Stop();
            File.Delete(file);
            openings.Save(File.OpenWrite(file));

            return openings.GetOpenings();
        }
    }
}