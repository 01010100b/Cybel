using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Core
{
    public interface ITimeManager
    {
        public TimeSpan GetTimeForNextMove(TimeSpan time_remaining);
    }
}
