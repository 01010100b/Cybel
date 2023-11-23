using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Players.MCTS
{
    public class MCTSPlayerParameters
    {
        public double TimeMod { get; set; } = 2;
        public double C { get; set; } = 1.4;

        public void Validate()
        {
            TimeMod = Math.Clamp(TimeMod, 1, double.MaxValue);
            C = Math.Clamp(C, 0, double.MaxValue);
        }
    }
}
