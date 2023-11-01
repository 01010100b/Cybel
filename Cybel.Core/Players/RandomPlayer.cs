using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Core.Players
{
    public class RandomPlayer : Player
    {
        private Random RNG { get; } = new(Guid.NewGuid().GetHashCode());

        public override Dictionary<Move, double> ScoreMoves(IGame game, TimeSpan time)
        {
            var scores = new Dictionary<Move, double>();

            foreach (var move in game.GetMoves())
            {
                scores.Add(move, 0);
            }

            scores[scores.ElementAt(RNG.Next(scores.Count)).Key] = 1;

            return scores;
        }
    }
}
