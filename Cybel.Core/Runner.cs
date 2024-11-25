using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Core;

public class Runner<TEnvironment> where TEnvironment : IEnvironment
{
    public void Run(TEnvironment environment, IPlayer<TEnvironment> player1, IPlayer<TEnvironment> player2)
    {

    }
}
