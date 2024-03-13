using Cybel.Core;
using Cybel.Core.Players;
using Cybel.Games;
using Cybel.Learning;
using Cybel.Players;
using System.Diagnostics;
using System.Globalization;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;

namespace Cybel.Runner
{
    internal class Program
    {
        public static string Folder => AppDomain.CurrentDomain.BaseDirectory;

        public static string GetBookFile(IGame game)
        {
            var id = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(game.Name)));

            return Path.Combine(Folder, $"book-{id}.json");
        }

        public static string GetParametersFile(LearningPlayer player, IGame game)
        {
            var id = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(player.GetType().Name + game.Name)));

            return Path.Combine(Folder, $"parameters-{id}.json");
        }

        static void Main(string[] args)
        {
            var game = MNK.GetConnectFour();
            var player = new MCTSPlayer();

            var solution = Optimize(game.Copy(), () => new MCTSPlayer());

            foreach (var parameter in solution.Parameters)
            {
                player.SetParameter(parameter.Key, parameter.Value);
            }
            /*
            var openings = GenerateOpenings(game.Copy(), () =>
            {
                var player = new MCTSPlayer();

                foreach (var parameter in solution.Parameters)
                {
                    player.SetParameter(parameter.Key, parameter.Value);
                }

                return player;
            });

            Console.WriteLine($"Book with {openings.Count:N0} positions");
            
            foreach (var position in openings)
            {
                player.Openings.Add(position.Key, position.Value);
            }
            */
            RunTournament(game.Copy(), [player, new MCTSPlayer()], 100);
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

        private static Solution Optimize(IGame game, Func<LearningPlayer> player_factory)
        {
            Console.WriteLine("Optimizing...");

            var ga = new GeneticAlgorithm(16, 20, 0.5, player_factory);
            var file = GetParametersFile(player_factory(), game);
            var stopping = false;

            if (File.Exists(file))
            {
                using var stream = File.OpenRead(file);
                ga.Load(stream);
            }

            var thread = new Thread(() =>
            {
                var g = game.Copy();

                while (!stopping)
                {
                    game.CopyTo(g);
                    ga.Step(g);
                }
            })
            {
                IsBackground = true
            };
            thread.Start();

            return Run(() => Save(file, ga.Save), () => { stopping = true; thread.Join(); }, ga.GetBest);
        }

        private static Dictionary<ulong, ulong> GenerateOpenings(IGame start_position, Func<Player> player_factory)
        {
            Console.WriteLine("Generating openings...");

            var time_per_position = TimeSpan.FromSeconds(30);
            var generator = new OpeningsGenerator(start_position, player_factory);
            var file = GetBookFile(start_position);

            if (File.Exists(file))
            {
                using var stream = File.OpenRead(file);
                generator.Load(stream);
            }
            
            generator.Start(time_per_position, threads: Environment.ProcessorCount);

            return Run(() => Save(file, generator.Save), generator.Stop, generator.GetOpenings);
        }

        private static T Run<T>(Action save, Action stop, Func<T> result)
        {
            Console.WriteLine("Press any key to stop.");

            var debug_file = Path.Combine(Folder, "debug.txt");
            var last_debug = DateTime.UtcNow;

            if (File.Exists(debug_file))
            {
                File.Delete(debug_file);
            }

            File.WriteAllLines(debug_file, [$"debug start {DateTime.Now}"]);

            var sw = Stopwatch.StartNew();

            while (!Console.KeyAvailable)
            {
                if (DateTime.UtcNow > last_debug + TimeSpan.FromHours(1))
                {
                    File.AppendAllLines(debug_file, [$"checkpoint {DateTime.Now}"]);
                    last_debug = DateTime.UtcNow;
                }

                if (sw.Elapsed < TimeSpan.FromMinutes(10))
                {
                    Thread.Sleep(100);
                }
                else
                {
                    save();
                    sw.Restart();
                }
            }

            Console.WriteLine("Stopping...");
            stop();
            save();

            return result();
        }

        private static void Save(string file, Action<Stream> save)
        {
            File.Delete(file);
            using var stream = File.OpenWrite(file);
            save(stream);
        }
    }
}