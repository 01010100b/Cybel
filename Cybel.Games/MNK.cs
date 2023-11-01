using Cybel.Core;
using Cybel.Games.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Games
{
    public class MNK : IGame<MNK>
    {
        public static MNK GetTicTacToe() => new(3, 3, 3, 2);

        public ulong Id => GetId();
        public int NumberOfPlayers => Players;

        public int Columns { get; private set; }
        public int Rows { get; private set; }
        public int Connected { get; private set; }
        public int Players { get; private set; }

        private int[] Board { get; set; }
        private int Player { get; set; }
        private bool Terminal { get; set; }
        private int Winner { get; set; }
        private Zobrist Zobrist { get; }

        public MNK(int columns, int rows, int connected, int players) 
        {
            Columns = Math.Max(1, columns);
            Rows = Math.Max(1, rows);
            Connected = Math.Max(1, connected);
            Players = Math.Max(1, players);
            Board = new int[Columns * Rows];
            Player = 0;
            Terminal = false;
            Winner = -1;

            var dimensions = new int[players + 1];
            Array.Fill(dimensions, Board.Length);
            Zobrist = new(dimensions);
        }

        public MNK Copy()
        {
            var mnk = new MNK(Columns, Rows, Connected, Players);
            CopyTo(mnk);

            return mnk;
        }

        public void CopyTo(MNK other)
        {
            other.Columns = Columns;
            other.Rows = Rows;
            other.Connected = Connected;
            other.Players = Players;
            
            other.Player = Player;
            other.Terminal = Terminal;
            other.Winner = Winner;

            if (other.Board.Length != Board.Length)
            {
                other.Board = new int[Board.Length];
            }

            Array.Copy(Board, other.Board, Board.Length);
        }

        public int GetCurrentPlayer() => Player;

        public ulong GetStateHash()
        {
            Zobrist.Reset();

            for (int i = 0; i < Board.Length; i++)
            {
                var player = Board[i];
                Zobrist.Flip(player, i);
            }

            return Zobrist.Hash;
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

        public bool IsTerminal() => Terminal;

        public bool IsWinningPlayer(int player) => Winner == player;

        public void Perform(Move move)
        {
            if (IsTerminal())
            {
                return;
            }

            var pos = (int)move.Hash;
            Board[pos] = Player + 1;
            Player = (Player + 1) % NumberOfPlayers;
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

            if (HasWon(pos))
            {
                Winner = Board[pos] - 1;
                Terminal = true;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            for (int i = 0; i < Board.Length; i++)
            {
                sb.Append(Board[i]);

                if (i % Columns == Columns - 1)
                {
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        private ulong GetId()
        {
            var id = Zobrist.GetHash(GetType());

            id ^= (ulong)Columns.GetHashCode() << 30;
            id ^= (ulong)Rows.GetHashCode() << 20;
            id ^= (ulong)Connected.GetHashCode() << 10;
            id ^= (ulong)Players.GetHashCode();

            return id;
        }

        private bool HasWon(int pos)
        {
            var dirs = new List<(int, int)>()
            {
                (-1, 1),
                (Columns, -Columns),
                (Columns - 1, -Columns + 1),
                (-Columns - 1, Columns + 1)
            };

            var parts = new List<int>();
            var color = Board[pos];

            foreach (var dir in dirs)
            {
                parts.Clear();
                parts.Add(dir.Item1);
                parts.Add(dir.Item2);

                var count = 1;

                foreach (var part in parts)
                {
                    var side = (part + Columns + Columns) % Columns;
                    var next = pos;

                    while (true)
                    {
                        next += part;

                        if (next < 0 || next >= Board.Length)
                        {
                            break;
                        }

                        if (side == Columns - 1 && next % Columns == Columns - 1)
                        {
                            break;
                        }
                        else if (side == 1 && next % Columns == 0)
                        {
                            break;
                        }

                        if (Board[next] != color)
                        {
                            break;
                        }

                        count++;
                    }
                }

                if (count >= Connected)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
