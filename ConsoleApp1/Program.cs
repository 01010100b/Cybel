using Cybel.Core;
using Cybel.Games;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            var runner = new Runner<MNK>();
            var player1 = new RandomPlayer<MNK>();
            var player2 = new MNKPlayer();
            var mnk = MNK.GetTicTacToe();
            runner.Run(mnk, player1, player2);
        }
    }
}
