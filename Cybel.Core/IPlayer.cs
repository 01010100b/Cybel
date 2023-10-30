using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Core
{
    public interface IPlayer
    {
        public Move GetMove(IGame game, TimeSpan time);
    }
}
