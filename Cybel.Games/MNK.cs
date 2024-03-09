using Cybel.Core;
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
        public static MNK GetConnectFour() => new(7, 6, 4, 2, true);

        public ulong Id => GetId();
        public int NumberOfPlayers => Players;

        public int Columns { get; private set; }
        public int Rows { get; private set; }
        public int Connected { get; private set; }
        public int Players { get; private set; }
        public bool Drops { get; private set; } // whether placed pieces drop down (as in connect-four)
        private List<int> Dirs { get; }

        private int[] Board { get; set; }
        private int Player { get; set; }
        private bool Terminal { get; set; }
        private int Winner { get; set; }
        private Zobrist Zobrist { get; }

        public MNK(int columns, int rows, int connected, int players, bool drops = false) 
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(columns, 2);
            ArgumentOutOfRangeException.ThrowIfLessThan(rows, 2);
            ArgumentOutOfRangeException.ThrowIfLessThan(connected, 2);
            ArgumentOutOfRangeException.ThrowIfLessThan(players, 1);

            Columns = columns;
            Rows = rows;
            Connected = connected;
            Players = players;
            Drops = drops;
            Dirs =
            [
                -1, 1, 
                Columns, -Columns,
                Columns - 1, -Columns + 1,
                -Columns - 1, Columns + 1
            ];

            Board = new int[Columns * Rows];
            Array.Fill(Board, -1);
            Player = 0;
            Terminal = false;
            Winner = -1;

            var dimensions = new int[players + 1];
            Array.Fill(dimensions, Board.Length);
            Zobrist = new(dimensions);
        }

        public MNK Copy()
        {
            var mnk = new MNK(Columns, Rows, Connected, Players, Drops);
            CopyTo(mnk);

            return mnk;
        }

        public void CopyTo(MNK other)
        {
            other.Columns = Columns;
            other.Rows = Rows;
            other.Connected = Connected;
            other.Players = Players;
            other.Drops = Drops;
            other.Dirs.Clear();
            other.Dirs.AddRange(Dirs);
            
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
                var player = Board[i] + 1;
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

            if (Drops)
            {
                for (int column = 0; column < Columns; column++)
                {
                    var block = Rows;

                    for (int row = 0; row < Rows; row++)
                    {
                        var index = (row * Columns) + column;

                        if (Board[index] != -1)
                        {
                            block = row;

                            break;
                        }
                    }

                    if (block > 0)
                    {
                        var index = ((block - 1) * Columns) + column;
                        yield return new((ulong)index);
                    }
                }
            }
            else
            {
                for (int i = 0; i < Board.Length; i++)
                {
                    if (Board[i] == -1)
                    {
                        yield return new((ulong)i);
                    }
                }
            }
        }

        public void AddMoves(List<Move> moves)
        {
            if (IsTerminal())
            {
                return;
            }

            if (Drops)
            {
                for (int column = 0; column < Columns; column++)
                {
                    var block = Rows;

                    for (int row = 0; row < Rows; row++)
                    {
                        var index = (row * Columns) + column;

                        if (Board[index] != -1)
                        {
                            block = row;

                            break;
                        }
                    }

                    if (block > 0)
                    {
                        var index = ((block - 1) * Columns) + column;
                        moves.Add(new((ulong)index));
                    }
                }
            }
            else
            {
                for (int i = 0; i < Board.Length; i++)
                {
                    if (Board[i] == -1)
                    {
                        moves.Add(new((ulong)i));
                    }
                }
            }
        }

        public bool IsTerminal() => Terminal;

        public double GetPlayerScore(int player)
        {
            if (Winner == -1)
            {
                return 1d / NumberOfPlayers;
            }
            else if (Winner == player)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public void Perform(Move move)
        {
            if (IsTerminal())
            {
                return;
            }

            var pos = (int)move.Hash;
            Board[pos] = Player;
            Player = (Player + 1) % NumberOfPlayers;
            Terminal = true;

            for (int i = 0; i < Board.Length; i++)
            {
                if (Board[i] == -1)
                {
                    Terminal = false;

                    break;
                }
            }

            Winner = -1;

            if (HasWon(pos))
            {
                Winner = Board[pos];
                Terminal = true;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            for (int i = 0; i < Board.Length; i++)
            {
                var ch = "-";

                if (Board[i] >= 0)
                {
                    ch = Board[i].ToString();
                }

                sb.Append(ch);

                if (i % Columns == Columns - 1)
                {
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        private ulong GetId()
        {
            var id = Zobrist.GetHash($"{GetType().Name} {Columns}.{Rows}.{Connected}.{Players}.{Drops}");

            return id;
        }

        private bool HasWon(int pos)
        {
            var color = Board[pos];

            for (int dir = 0; dir < Dirs.Count; dir += 2)
            {
                var count = 1;

                for (int part = 0; part < 2; part++)
                {
                    var offset = Dirs[dir + part];
                    var side = (offset + Columns + Columns) % Columns;
                    var next = pos;

                    while (true)
                    {
                        next += offset;

                        if (next < 0 || next >= Board.Length)
                        {
                            // moved off the board vertically
                            break;
                        }
                        else if (side == Columns - 1 && next % Columns == Columns - 1)
                        {
                            // wrapped around the board to the left
                            break;
                        }
                        else if (side == 1 && next % Columns == 0)
                        {
                            // wrapped around the board to the right
                            break;
                        }
                        else if (Board[next] != color)
                        {
                            break;
                        }

                        count++;
                    }

                    if (count >= Connected)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
