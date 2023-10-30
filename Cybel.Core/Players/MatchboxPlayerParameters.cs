using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Core.Players
{
    public class MatchboxPlayerParameters
    {
        public double TimeMod { get; set; } = 2;
        public double ExploreChance { get; set; } = 0.3;
        public int InitialPips { get; set; } = 3;
        public int MaxPips { get; set; } = 1000;
        public double PipChangeMod { get; set; } = -0.2;
    }
}
