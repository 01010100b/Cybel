using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Core
{
    public class BasicTimeManager : ITimeManager
    {
        public double TimeMod { get; set; } = 2;

        public TimeSpan GetTimeForNextMove(TimeSpan time_remaining)
        {
            TimeMod = Math.Clamp(TimeMod, 1, double.MaxValue);

            return time_remaining / TimeMod;
        }
    }
}
