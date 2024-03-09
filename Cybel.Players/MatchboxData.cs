using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Players
{
    public class MatchboxData
    {
        public List<double> Pips { get; } = new();

        public void Initialize(int moves, int pips)
        {
            Pips.Clear();

            for (int i = 0; i < moves; i++)
            {
                Pips.Add(pips);
            }
        }
    }
}
