using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Learning
{
    public class Parameter(string name, double min, double max, double @default)
    {
        public string Name { get; } = name;
        public double Min { get; } = min;
        public double Max { get; } = max;
        public double Default { get; } = Math.Clamp(@default, min, max);
    }
}
