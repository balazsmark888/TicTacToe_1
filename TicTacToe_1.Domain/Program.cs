using System;

namespace TicTacToe_1.Domain
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var random = new Random();
            var game = new Game(3, 3, random);
            game.StartGame();
            Console.ReadKey();
        }
    }
}
