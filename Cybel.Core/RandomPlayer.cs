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

        public override IEnumerable<KeyValuePair<Move, double>> ScoreMoves(IGame game, TimeSpan time)
        {
            foreach (var move in game.GetMoves())
            {
                yield return new(move, RNG.NextDouble());
            }
        }
    }
}
