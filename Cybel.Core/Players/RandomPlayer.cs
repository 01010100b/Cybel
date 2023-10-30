using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Core.Players
{
    public class RandomPlayer : IPlayer
    {
        private Random RNG { get; } = new(Guid.NewGuid().GetHashCode());

        public Move ChooseMove(IGame game, TimeSpan time)
        {
            var moves = game.GetMoves().ToList();

            return moves[RNG.Next(moves.Count)];
        }
    }
}
