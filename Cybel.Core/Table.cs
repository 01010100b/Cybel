using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Cybel.Core
{
    public class Table<TData> where TData : class
    {
        // LRU cache

        public class Entry
        {
            public ulong Hash { get; private set; }
            public int Player { get; private set; }
            public bool Terminal { get; private set; }
            public IReadOnlyList<Move> Moves { get; }
            public TData? Data { get; set; }

            internal int LastVisit { get; set; }

            internal Entry()
            {
                Moves = new List<Move>();
            }

            internal void Initialize(IGame game)
            {
                Hash = game.GetStateHash();
                Player = game.GetCurrentPlayer();
                Terminal = game.IsTerminal();
                Data = null;
                LastVisit = 0;

                var moves = (List<Move>)Moves;
                moves.Clear();
                moves.AddRange(game.GetMoves());
            }
        }

        public int MaxSize { get; set; } = 100_000;

        private Dictionary<ulong, Entry> Entries { get; } = [];
        private int Visit { get; set; } = 0;

        public Entry GetEntry(IGame game)
        {
            if (Entries.Count >= MaxSize)
            {
                Trim();
            }

            Visit++;

            if (Visit >= int.MaxValue)
            {
                Visit = 0;

                foreach (var e in Entries.Values)
                {
                    e.LastVisit = Visit;
                }
            }

            if (Entries.TryGetValue(game.GetStateHash(), out var entry))
            {
                entry.LastVisit = Visit;

                return entry;
            }
            else
            {
                entry = ObjectPool.Get(() => new Entry());
                entry.Initialize(game);
                entry.LastVisit = Visit;
                Entries.Add(entry.Hash, entry);

                return entry;
            }
        }

        private void Trim()
        {
            var count = Entries.Count / 4;

            if (count <= 0)
            {
                return;
            }

            var trims = ObjectPool.Get(() => new List<Entry>(), x => x.Clear());

            foreach (var entry in Entries.Values.OrderBy(x => x.LastVisit))
            {
                trims.Add(entry);

                if (trims.Count >= count)
                {
                    break;
                }
            }

            foreach (var entry in trims)
            {
                Entries.Remove(entry.Hash);
                
                if (entry.Data is not null)
                {
                    ObjectPool.Add(entry.Data);
                    entry.Data = null;
                }

                ObjectPool.Add(entry);
            }

            ObjectPool.Add(trims);
        }
    }
}
