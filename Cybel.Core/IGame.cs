using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Core
{
    public interface IGame<TSelf> where TSelf : IGame<TSelf>, new()
    {
        public int NumberOfPlayers { get; }

        public ulong GetHash();
        public int GetCurrentPlayer();
        public bool IsTerminal();
        public bool IsWinningPlayer(int player);

        public IEnumerable<Move> GetMoves();
        public void Perform(Move action);
        public void CopyTo(TSelf other);
        public TSelf Copy()
        {
            var t = new TSelf();
            CopyTo(t);

            return t;
        }
    }
}
