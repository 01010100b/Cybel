using Cybel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Runner
{
    internal class TicTacToe : IGame
    {
        private static int[][] WinningCombinations { get; } = new int[][]
        {
            new int[] {0, 1, 2},
            new int[] {3, 4, 5},
            new int[] {6, 7, 8},
            new int[] {0, 3, 6},
            new int[] {1, 4, 7},
            new int[] {2, 5, 8},
            new int[] {0, 4, 8},
            new int[] {2, 4, 6}
        };

        public int NumberOfPlayers => 2;

        private int[] Board { get; } = new int[9];
        private int Player { get; set; } = 0;
        private bool Terminal { get; set; } = false;
        private int Winner { get; set; } = -1;
        private Zobrist Zobrist { get; } = new(9, 9, 9);

        public void CopyTo(IGame other)
        {
            if (other is not TicTacToe o)
            {
                throw new Exception("other must be TicTacToe.");
            }

            o.Player = Player;
            o.Terminal = Terminal;
            o.Winner = Winner;

            for (int i = 0; i < Board.Length; i++)
            {
                o.Board[i] = Board[i];
            }
        }

        public IGame Copy()
        {
            var t = new TicTacToe();
            CopyTo(t);

            return t;
        }

        public IEnumerable<Move> GetMoves()
        {
            if (IsTerminal())
            {
                yield break;
            }

            for (int i = 0; i < Board.Length; i++)
            {
                if (Board[i] == 0)
                {
                    yield return new((ulong)i);
                }
            }
        }

        public int GetCurrentPlayer() => Player;

        public ulong GetHash()
        {
            Zobrist.Reset();

            for (int i = 0; i < Board.Length; i++)
            {
                var player = Board[i];
                Zobrist.Flip(player, i);
            }

            return Zobrist.Hash;
        }

        public bool IsWinningPlayer(int player) => player == Winner;

        public bool IsTerminal() => Terminal;

        public void Perform(Move move)
        {
            if (IsTerminal())
            {
                return;
            }

            Board[(int)move.Hash] = Player + 1;
            Player = 1 - Player;
            Terminal = true;

            for (int i = 0; i < Board.Length; i++)
            {
                if (Board[i] == 0)
                {
                    Terminal = false;

                    break;
                }
            }

            Winner = -1;

            foreach (var combination in WinningCombinations)
            {
                var player = Board[combination[0]];

                if (player == 0)
                {
                    continue;
                }

                var win = true;

                foreach (var c in combination)
                {
                    if (Board[c] != player)
                    {
                        win = false;

                        break;
                    }
                }

                if (win)
                {
                    Winner = player - 1;
                    Terminal = true;

                    break;
                }
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            for (int i = 0; i < Board.Length; i++)
            {
                sb.Append(Board[i].ToString());

                if (i % 3 == 2)
                {
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }
    }
}
