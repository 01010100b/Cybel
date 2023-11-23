using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Players.Matchbox
{
    public class MatchboxPlayerParameters
    {
        public double TimeMod { get; set; } = 2;
        public double ExploreChance { get; set; } = 0.7;
        public int InitialPips { get; set; } = 1000;
        public int MaxPips { get; set; } = int.MaxValue;
        public double PipChangeMod { get; set; } = -0.2;

        public void Validate()
        {
            TimeMod = Math.Clamp(TimeMod, 1, double.MaxValue);
            ExploreChance = Math.Clamp(ExploreChance, 0, 1);
            InitialPips = Math.Clamp(InitialPips, 0, int.MaxValue);
            MaxPips = Math.Clamp(MaxPips, 0, int.MaxValue);
            PipChangeMod = Math.Clamp(PipChangeMod, double.MinValue, 0);
        }
    }
}
