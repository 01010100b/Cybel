using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cybel.Core
{
    public class OpeningsGenerator(IGame start_position, Func<Player> player_factory)
    {
        private IGame StartPosition { get; } = start_position;
        private Func<Player> PlayerFactory { get; } = player_factory;
        private TimeSpan TimePerPosition { get; set; }
        private double TimeDecay { get; set; }
        private double ScoreCutoff { get; set; }
        private int ScoreCutoffDepth { get; set; }

        private Dictionary<ulong, Dictionary<ulong, double>> Book { get; } = [];
        private HashSet<ulong> Reserved { get; } = [];
        private List<Thread> Threads { get; } = [];
        private volatile bool Stopping = false;

        public void Start(TimeSpan time_per_position, double time_decay = 0.9, double score_cutoff = 0.5, int threads = 1)
        {
            Stop();

            lock (Threads)
            {
                TimePerPosition = time_per_position;
                TimeDecay = time_decay;
                ScoreCutoff = score_cutoff;

                while (Threads.Count < threads)
                {
                    var thread = new Thread(Run);
                    thread.Start();
                    Threads.Add(thread);
                }
            }
        }

        public void Stop()
        {
            Stopping = true;

            lock (Threads)
            {
                foreach (var thread in Threads)
                {
                    thread.Join();
                }

                Reserved.Clear();
                Threads.Clear();
            }

            Stopping = false;
        }

        public void Load(Stream stream)
        {
            lock (Threads)
            {
                if (Threads.Count > 0)
                {
                    throw new Exception("Can not load when running.");
                }

                lock (Book)
                {
                    Book.Clear();
                    try
                    {
                        var book = JsonSerializer.Deserialize<Dictionary<ulong, Dictionary<ulong, double>>>(stream)!;

                        foreach (var position in book)
                        {
                            Book.Add(position.Key, position.Value);
                        }
                    }
                    catch
                    {

                    }
                }
            }
        }

        public void Save(Stream stream)
        {
            lock (Book)
            {
                var options = new JsonSerializerOptions() { WriteIndented = true };
                JsonSerializer.Serialize(stream, Book, options);
                stream.Flush();
            }
        }

        public Dictionary<ulong, ulong> GetOpenings()
        {
            var book = new Dictionary<ulong, ulong>();

            lock (Book)
            {
                foreach (var position in Book)
                {
                    ulong? best_move = null;
                    double best_score = double.MinValue;

                    foreach (var move in position.Value)
                    {
                        if (move.Value > best_score)
                        {
                            best_move = move.Key;
                            best_score = move.Value;
                        }
                    }

                    Debug.Assert(best_move is not null);

                    book.Add(position.Key, best_move.Value);
                }
            }

            return book;
        }

        private void Run()
        {
            IGame game;
            Player player;

            lock (StartPosition)
            {
                game = StartPosition.Copy();
            }

            lock (PlayerFactory)
            {
                player = PlayerFactory();
            }

            while (!Stopping)
            {
                var got_position = TryReservePosition(game, out var depth);

                while (!got_position)
                {
                    if (Stopping)
                    {
                        break;
                    }

                    Thread.Sleep(1000);
                    got_position = TryReservePosition(game, out depth);
                }

                if (Stopping)
                {
                    break;
                }

                var time = TimePerPosition * Math.Pow(TimeDecay, depth);

                if (time < TimeSpan.FromSeconds(1))
                {
                    time = TimeSpan.FromSeconds(1);
                }

                var cutoff = ScoreCutoff;

                if (ScoreCutoffDepth > 0 && depth < ScoreCutoffDepth)
                {
                    cutoff *= depth / (double)ScoreCutoffDepth;
                }

                var hash = game.GetStateHash();
                var moves = new Dictionary<ulong, double>();
                var max_score = double.MinValue;

                foreach (var score in player.ScoreMoves(game, time))
                {
                    moves.Add(score.Key.Hash, score.Value);
                    max_score = Math.Max(max_score, score.Value);
                }

                foreach (var move in moves.ToList())
                {
                    if (move.Value < max_score * cutoff)
                    {
                        moves.Remove(move.Key);
                    }
                }

                lock (Book)
                {
                    Book.Add(hash, moves);
                }

                lock (Reserved)
                {
                    Reserved.Remove(hash);
                }
            }
        }

        private bool TryReservePosition(IGame game, out int depth)
        {
            depth = 0;

            lock (StartPosition)
            {
                StartPosition.CopyTo(game);
            }

            while (true)
            {
                lock (Book)
                {
                    if (Book.TryGetValue(game.GetStateHash(), out var moves))
                    {
                        var choice = GetRandom(moves);
                        var move = game.GetMoves().Single(x => x.Hash == choice);
                        game.Perform(move);
                        depth++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (game.IsTerminal())
            {
                return false;
            }

            lock (Reserved)
            {
                if (Reserved.Contains(game.GetStateHash()))
                {
                    return false;
                }
                else
                {
                    Reserved.Add(game.GetStateHash());

                    return true;
                }
            }
        }

        private static T GetRandom<T>(IEnumerable<KeyValuePair<T, double>> choices)
        {
            Debug.Assert(choices.Any());

            var total = choices.Sum(x => x.Value);
            var choice = Random.Shared.NextDouble() * total;

            foreach (var c in choices)
            {
                choice -= c.Value;

                if (choice <= 0)
                {
                    return c.Key;
                }
            }

            return choices.Last().Key;
        }
    }
}
