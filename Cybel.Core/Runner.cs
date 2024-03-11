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
        public TimeSpan TimeBank { get; set; } = TimeSpan.FromSeconds(1);
        public TimeSpan TimePerMove { get; set; } = TimeSpan.FromSeconds(0.1);

        public Dictionary<Player, double> Play(IGame game, IReadOnlyList<Player> players, int games)
        {
            var results = new Dictionary<Player, double>();

            foreach (var player in players)
            {
                results.Add(player, 0);
            }

            var g = ObjectPool.Get(game.Copy, game.CopyTo);

            foreach (var match in GetMatches(players, game.NumberOfPlayers))
            {
                for (int i = 0; i < games; i++)
                {
                    game.CopyTo(g);
                    match.Sort((a, b) => Random.Shared.Next().CompareTo(Random.Shared.Next()));

                    foreach (var score in PlayGame(match, g))
                    {
                        results[score.Key] += score.Value;
                    }
                }
            }

            ObjectPool.Add(g);

            return results;
        }

        private IEnumerable<KeyValuePair<Player, double>> PlayGame(IReadOnlyList<Player> players, IGame game)
        {
            var times = ObjectPool.Get(() => new List<TimeSpan>(), x => x.Clear());
            times.AddRange(players.Select(x => TimeBank));
            var moves = ObjectPool.Get(() => new List<Move>(), x => x.Clear());
            var g = ObjectPool.Get(game.Copy, game.CopyTo);
            var sw = Stopwatch.StartNew();

            while (!game.IsTerminal())
            {
                var player = game.GetCurrentPlayer();
                moves.Clear();
                moves.AddRange(game.GetMoves());

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
            }

            ObjectPool.Add(times);
            ObjectPool.Add(moves);
            ObjectPool.Add(g);

            for (int i = 0; i < players.Count; i++)
            {
                yield return new(players[i], game.GetPlayerScore(i));
            }
        }

        private List<List<Player>> GetMatches(IReadOnlyList<Player> players, int count)
        {
            if (count <= 1)
            {
                return players.Select(x => new List<Player>() { x }).ToList();
            }
            else if (count > 2)
            {
                throw new NotImplementedException();
            }

            var matches = new List<List<Player>>();

            for (int i = 0; i < players.Count - 1; i++)
            {
                for (int j = i + 1; j < players.Count; j++)
                {
                    var match = new List<Player>
                    {
                        players[i],
                        players[j]
                    };
                    matches.Add(match);
                }
            }

            return matches;
        }
    }
}
