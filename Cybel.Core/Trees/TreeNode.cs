using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Core.Trees
{
    public class TreeNode<TData>
    {
        public ulong Hash { get; }
        public int Player { get; }
        public bool Terminal { get; }
        public IReadOnlyList<Move> Moves { get; }
        public TData? Data { get; set; }

        internal int LastVisit { get; set; } = 0;

        public TreeNode(IGame game)
        {
            Hash = game.GetHash();
            Player = game.GetCurrentPlayer();
            Terminal = game.IsTerminal();
            Moves = game.GetMoves().ToList();
        }
    }
}
