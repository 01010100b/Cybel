using BinaryLibs.Utils;

namespace Cybel.Core;

public class RandomPlayer<TEnvironment> : IPlayer<TEnvironment> where TEnvironment : IEnvironment
{
    public Action ChooseAction(TEnvironment environment, TimeSpan time_remaining)
    {
        var actions = ObjectPool.Shared.Get(() => new List<Action>(), x => x.Clear());
        environment.GenerateActions(actions);
        var action = actions[Random.Shared.Next(actions.Count)];
        ObjectPool.Shared.Add(actions);

        return action;
    }
}
