using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Core
{
    public abstract class Player
    {
        public double TimeMod { get; set; } = 2;
        public Dictionary<ulong, ulong>? Openings { get; set; } = new();

        public virtual Move ChooseMove(IGame game, TimeSpan time)
        {
            TimeMod = Math.Clamp(TimeMod, 1, double.MaxValue);

            var moves = new List<Move>();
            game.AddMoves(moves);

            if (Openings is not null)
            {
                if (Openings.TryGetValue(game.GetStateHash(), out var hash))
                {
                    foreach (var move in moves)
                    {
                        if (move.Hash == hash)
                        {
                            return move;
                        }
                    }
                }
            }

            var scores = ScoreMoves(game, time / TimeMod);
            Move? best = null;
            var score = double.MinValue;

            foreach (var move in moves)
            {
                if (scores.TryGetValue(move, out var s))
                {
                    if (s > score)
                    {
                        best = move;
                        score = s;
                    }
                }
            }

            if (best is null)
            {
                throw new Exception("No available move.");
            }

            return best.Value;
        }

        public abstract Dictionary<Move, double> ScoreMoves(IGame game, TimeSpan time);
    }
}
    