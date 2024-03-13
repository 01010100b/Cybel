using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Core
{
    public static class Extensions
    {
        public static T Choose<T>(this Random random, IEnumerable<KeyValuePair<T, double>> choices)
        {
            Debug.Assert(choices.Any());

            var total = choices.Sum(x => x.Value);
            var choice = random.NextDouble() * total;

            foreach (var c in choices)
            {
                choice -= c.Value;

                if (choice <= 0)
                {
                    return c.Key;
                }
            }

            return choices.Last().Key;
        }
    }
}
