using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Games
{
    public class Zobrist
    {
        public static ulong GetHash(string s)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(s));
            Debug.Assert(bytes.Length >= sizeof(ulong));

            return BitConverter.ToUInt64(bytes);
        }

        public ulong Hash { get; private set; }

        private ulong[][] Hashes { get; }

        public Zobrist(params int[] dimensions)
        {
            Hashes = new ulong[dimensions.Length][];

            for (int i = 0; i < dimensions.Length; i++)
            {
                Hashes[i] = new ulong[dimensions[i]];
            }

            CalculateHashes(863471);
            Reset();
        }

        public void Reset()
        {
            Hash = 0;
        }

        public void Flip(int x, int y)
        {
            Hash ^= Hashes[x][y];
        }

        private void CalculateHashes(int seed)
        {
            var rng = new Random(seed);
            var bytes = new byte[sizeof(ulong)];

            for (int i = 0; i < Hashes.Length; i++)
            {
                var hashes = Hashes[i];

                for (int j = 0; j < hashes.Length; j++)
                {
                    rng.NextBytes(bytes);
                    hashes[j] = BitConverter.ToUInt64(bytes);
                }
            }
        }
    }
}
