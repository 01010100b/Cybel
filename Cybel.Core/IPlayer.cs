using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Core
{
    public interface IPlayer<TGame> where TGame : IGame<TGame>, new()
    {
        public Move GetAction(TGame game, TimeSpan time);
    }
}
