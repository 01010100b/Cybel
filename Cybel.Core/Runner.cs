using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Core
{
    public class Runner<TGame> where TGame : IGame<TGame>, new()
    {
        public List<IPlayer<TGame>> Play(IReadOnlyList<IPlayer<TGame>> players, TimeSpan time_bank, TimeSpan time_per_move, TGame? game = default)
        {
            game ??= new TGame();
            var g = game.Copy();

            while (!game.IsTerminal())
            {
                game.CopyTo(g);
                var player = game.GetCurrentPlayer();
                var action = players[player].GetAction(g, TimeSpan.MaxValue);
                game.Perform(action);
            }

            var winners = new List<IPlayer<TGame>>();

            for (int i = 0; i < players.Count; i++)
            {
                if (game.IsWinningPlayer(i))
                {
                    winners.Add(players[i]);
                }
            }

            return winners;
        }
    }
}
