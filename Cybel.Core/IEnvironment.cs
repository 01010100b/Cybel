namespace Cybel.Core
{
    public interface IEnvironment
    {
        public string Name { get; } // environment name
        public int NumberOfAgents { get; } // number of agents in this environment

        public ulong GetStateHash(); // hash of current state
        public int GetCurrentAgent(); // current agent to take an action
        public bool IsTerminal(); // is the current state terminal?
        public double GetScore(int agent); // score of agent if in terminal state, must be between 0 and 1
        public void GenerateActions(List<Action> actions);// generate available actions for the current agent
        public void Perform(Action action); // perform a given action and transition to the next state
        public void CopyTo(IEnvironment other); // copy the current state to the other instance
        public IEnvironment Copy(); // copy this instance
    }

    public interface IEnvironment<TSelf> : IEnvironment where TSelf : IEnvironment<TSelf>
    {
        public new TSelf Copy();
        public void CopyTo(TSelf other);

        IEnvironment IEnvironment.Copy() { return Copy(); }
        void IEnvironment.CopyTo(IEnvironment other) { CopyTo((TSelf)other); }
    }
}
