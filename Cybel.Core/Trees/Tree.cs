using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Core.Trees
{
    public class Tree<TData>
    {
        public int MaxSize { get; set; } = 1000000;

        private Dictionary<ulong, TreeNode<TData>> Nodes { get; } = new();
        private int Visit { get; set; } = 0;

        public TreeNode<TData> GetNode(IGame game)
        {
            if (Nodes.Count >= MaxSize)
            {
                Trim();
            }

            Visit++;

            if (Visit >= int.MaxValue)
            {
                Visit = 0;

                foreach (var n in Nodes.Values)
                {
                    n.LastVisit = Visit;
                }
            }

            if (Nodes.TryGetValue(game.GetHash(), out var node))
            {
                node.LastVisit = Visit;

                return node;
            }
            else
            {
                node = new(game) { LastVisit = Visit };
                Nodes.Add(node.Hash, node);

                return node;
            }
        }

        private void Trim()
        {
            var count = Nodes.Count / 10;

            if (count <= 0)
            {
                return;
            }

            var trims = new List<TreeNode<TData>>();

            foreach (var node in Nodes.Values.OrderBy(x => x.LastVisit))
            {
                trims.Add(node);

                if (trims.Count >= count)
                {
                    break;
                }
            }

            foreach (var trim in trims)
            {
                Nodes.Remove(trim.Hash);
            }
        }
    }
}
