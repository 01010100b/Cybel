using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Core
{
    public abstract class Player
    {
        public virtual Move ChooseMove(IGame game, TimeSpan time)
        {
            var scores = ScoreMoves(game, time);
            var moves = new List<Move>();
            game.AddMoves(moves);

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
                throw new Exception("No available moves.");
            }

            return best.Value;
        }

        public abstract Dictionary<Move, double> ScoreMoves(IGame game, TimeSpan time);
    }
}
    