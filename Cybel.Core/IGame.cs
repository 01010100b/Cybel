using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Core
{
    public interface IGame
    {
        public ulong Id { get; } // unique ID for the game
        public int NumberOfPlayers { get; } // number of players for game

        public ulong GetStateHash(); // hash of current state
        public int GetCurrentPlayer(); // current player to move
        public bool IsTerminal(); // is the current state terminal?
        public double GetPlayerScore(int player); // score of player in terminal state

        public void AddMoves(List<Move> moves); // generate moves for current state
        public void Perform(Move move); // perform a given move and transition to the next state
        public void CopyTo(IGame other); // copy the current state to the other instance
        public IGame Copy(); // copy this instance
    }
    
    public interface IGame<TSelf> : IGame where TSelf : IGame<TSelf>
    {
        public new TSelf Copy();
        public void CopyTo(TSelf other);
        
        IGame IGame.Copy() { return Copy(); }
        void IGame.CopyTo(IGame other) { CopyTo((TSelf)other); }
    }
}
