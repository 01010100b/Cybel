using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Core.Players
{
    public class RandomPlayer<TGame> : IPlayer<TGame> where TGame : IGame<TGame>, new()
    {
        private Random RNG { get; } = new(Guid.NewGuid().GetHashCode());

        public Move GetAction(TGame game, TimeSpan time)
        {
            var actions = game.GetMoves().ToList();

            return actions[RNG.Next(actions.Count)];
        }
    }
}
