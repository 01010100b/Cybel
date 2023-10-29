namespace Cybel.Core
{
    public readonly struct Move
    {
        public readonly ulong Hash;

        public Move(ulong hash) 
        {
            Hash = hash;
        }
    }
}