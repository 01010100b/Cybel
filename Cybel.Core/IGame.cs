using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Core
{
    public interface IGame
    {
        public int NumberOfPlayers { get; }

        public ulong GetHash();
        public int GetCurrentPlayer();
        public bool IsTerminal();
        public bool IsWinningPlayer(int player);

        public IEnumerable<Move> GetMoves();
        public void Perform(Move move);
        public void CopyTo(IGame other);
        public IGame Copy();
    }
}
