using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Core
{
    public abstract class Player
    {
        protected abstract ITimeManager TimeManager { get; }

        public virtual Move ChooseMove(IGame game, TimeSpan time_remaining)
        {
            var moves = game.GetMoves().ToList();
            var scores = ScoreMoves(game, TimeManager.GetTimeForNextMove(time_remaining));
            Move? best = null;
            var score = double.MinValue;

            foreach (var move in moves)
            {
                if (scores.TryGetValue(move, out var s))
                {
                    if (s > score)
                    {
                        best = move;
                        score = s;
                    }
                }
            }

            if (best is null)
            {
                throw new Exception("No available move.");
            }

            return best.Value;
        }

        public abstract Dictionary<Move, double> ScoreMoves(IGame game, TimeSpan time);
    }
}
    