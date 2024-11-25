using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Core;

public interface IPlayer<TEnvironment> where TEnvironment : IEnvironment
{
    public Action ChooseAction(TEnvironment environment, TimeSpan time_remaining);
}
