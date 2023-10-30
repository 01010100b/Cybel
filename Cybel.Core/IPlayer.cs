using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Core
{
    public interface IPlayer
    {
        public Move ChooseMove(IGame game, TimeSpan time);
    }
}
