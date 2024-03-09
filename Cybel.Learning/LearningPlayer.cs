using Cybel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Learning
{
    public abstract class LearningPlayer : Player
    {
        private Dictionary<string, double> Parameters { get; } = [];

        public abstract IEnumerable<Parameter> GetParameters();

        public double GetParameter(string name)
        {
            if (Parameters.TryGetValue(name, out var value))
            {
                return value;
            }
            else
            {
                value = GetParameters().Single(x => x.Name == name).Default;
                SetParameter(name, value);

                return value;
            }
        }

        public void SetParameter(string name, double value)
        {
            Parameters[name] = value;
            OnParameterChanged(name);
        }

        protected abstract void OnParameterChanged(string name);
        
    }
}
