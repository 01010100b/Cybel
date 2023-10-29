using Cybel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Runner
{
    internal class TicTacToe : IGame<TicTacToe>
    {
        public int NumberOfPlayers => throw new NotImplementedException();

        public void CopyTo(TicTacToe other)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Move> GetMoves()
        {
            throw new NotImplementedException();
        }

        public int GetCurrentPlayer()
        {
            throw new NotImplementedException();
        }

        public ulong GetHash()
        {
            throw new NotImplementedException();
        }

        public bool IsWinningPlayer(int player)
        {
            throw new NotImplementedException();
        }

        public bool IsTerminal()
        {
            throw new NotImplementedException();
        }

        public void Perform(Move move)
        {
            throw new NotImplementedException();
        }
    }
}
