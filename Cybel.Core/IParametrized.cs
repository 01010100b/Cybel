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
        public void SaveParameters(IGame game);
    }

    public interface IParametrized<T> : IParametrized where T : new()
    {
        public T Parameters { get; set; }

        void IParametrized.LoadParameters(IGame game)
        {
            Parameters = ParameterStorage.Load<T>(game);
        }

        void IParametrized.SaveParameters(IGame game)
        {
            ParameterStorage.Save(game, Parameters);
        }
    }
}
