using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Core.Players
{
    public class RandomPlayer : Player
    {
        protected override ITimeManager TimeManager => new BasicTimeManager();

        private Random RNG { get; } = new(Guid.NewGuid().GetHashCode());

        public override Dictionary<Move, double> ScoreMoves(IGame game, TimeSpan time)
        {
            var scores = new Dictionary<Move, double>();
            var moves = game.GetMoves().ToList();

            foreach (var move in moves)
            {
                scores.Add(move, RNG.NextDouble());
            }

            return scores;
        }
    }
}
