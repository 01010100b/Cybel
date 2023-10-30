using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Cybel.Core.Tables
{
    public class Table<TData> where TData : class
    {
        public class Entry
        {
            public ulong Hash { get; }
            public int Player { get; }
            public bool Terminal { get; }
            public IReadOnlyList<Move> Moves { get; }
            public TData? Data { get; set; }

            internal int LastVisit { get; set; }

            internal Entry(IGame game)
            {
                Hash = game.GetHash();
                Player = game.GetCurrentPlayer();
                Terminal = game.IsTerminal();
                Moves = game.GetMoves().ToList();
                Data = null;
                LastVisit = 0;
            }
        }

        public int MaxSize { get; set; } = 1000000;

        private Dictionary<ulong, Entry> Entries { get; } = new();
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

            if (Entries.TryGetValue(game.GetHash(), out var entry))
            {
                entry.LastVisit = Visit;

                return entry;
            }
            else
            {
                entry = new(game) { LastVisit = Visit };
                Entries.Add(entry.Hash, entry);

                return entry;
            }
        }

        private void Trim()
        {
            var count = Entries.Count / 10;

            if (count <= 0)
            {
                return;
            }

            var trims = new List<Entry>();

            foreach (var entry in Entries.Values.OrderBy(x => x.LastVisit))
            {
                trims.Add(entry);

                if (trims.Count >= count)
                {
                    break;
                }
            }

            foreach (var trim in trims)
            {
                Entries.Remove(trim.Hash);
            }
        }
    }
}
