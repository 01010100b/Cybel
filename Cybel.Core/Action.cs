namespace Cybel.Core
{
    public readonly struct Action(ulong hash)
    {
        public ulong Hash { get; } = hash;
    }
}