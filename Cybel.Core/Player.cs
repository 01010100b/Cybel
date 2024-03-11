using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Core
{
    public abstract class Player
    {
        public Dictionary<ulong, ulong> Openings { get; } = [];

        protected abstract ITimeManager TimeManager { get; }

        public virtual Move ChooseMove(IGame game, TimeSpan time_remaining)
        {
            var moves = ObjectPool.Get(() => new List<Move>(), x => x.Clear());
            moves.AddRange(game.GetMoves());

            if (Openings.TryGetValue(game.GetStateHash(), out var opening))
            {
                foreach (var move in moves)
                {
                    if (move.Hash == opening)
                    {
                        return move;
                    }
                }
            }

            var scores = ObjectPool.Get(() => new Dictionary<Move, double>(), x => x.Clear());
            foreach (var score in ScoreMoves(game, TimeManager.GetTimeForNextMove(time_remaining)))
            {
                scores.Add(score.Key, score.Value);
            }

            Move? best_move = null;
            var best_score = double.MinValue;

            foreach (var move in moves)
            {
                if (scores.TryGetValue(move, out var score))
                {
                    if (score > best_score)
                    {
                        best_move = move;
                        best_score = score;
                    }
                }
            }

            if (best_move is null)
            {
                throw new Exception("No available move.");
            }

            ObjectPool.Add(moves);
            ObjectPool.Add(scores);

            return best_move.Value;
        }

        public abstract IEnumerable<KeyValuePair<Move, double>> ScoreMoves(IGame game, TimeSpan time);
    }
}
    