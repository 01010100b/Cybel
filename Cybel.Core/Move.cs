namespace Cybel.Core
{
    public readonly struct Move(ulong hash)
    {
        public readonly ulong Hash = hash;
    }
}