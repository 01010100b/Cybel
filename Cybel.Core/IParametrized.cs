using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Core
{
    public interface IParametrized
    {
        public void LoadParameters(IGame game);
    }

    public interface IParametrized<T> : IParametrized
    {
        public T Parameters { get; set; }
    }
}
