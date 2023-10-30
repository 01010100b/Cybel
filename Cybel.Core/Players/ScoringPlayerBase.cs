using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Core.Players
{
    public abstract class ScoringPlayerBase : IPlayer
    {
        public Move ChooseMove(IGame game, TimeSpan time)
        {
            var scores = ScoreMoves(game, time);

            Move? best = null;
            var score = double.MinValue;

            foreach (var move in game.GetMoves())
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
