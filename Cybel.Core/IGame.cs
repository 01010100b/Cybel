using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Core
{
    public interface IGame
    {
        public ulong Id { get; }
        public int NumberOfPlayers { get; }

        public ulong GetStateHash();
        public int GetCurrentPlayer();
        public bool IsTerminal();
        public double GetPlayerScore(int player);

        public void AddMoves(List<Move> moves);
        public void Perform(Move move);
        public void CopyTo(IGame other);
        public IGame Copy();
    }
    
    public interface IGame<TSelf> : IGame where TSelf : IGame<TSelf>
    {
        public new TSelf Copy();
        public void CopyTo(TSelf other);
        
        IGame IGame.Copy() { return Copy(); }
        void IGame.CopyTo(IGame other) { CopyTo((TSelf)other); }
    }
}
