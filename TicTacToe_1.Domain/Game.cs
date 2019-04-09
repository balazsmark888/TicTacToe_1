using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacToe_1.Domain
{
    public class Game
    {
        public GameBoard GameBoard { get; set; }
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }

        public Game(int tableSize, int slotsToWin,Random random)
        {
            GameBoard = new GameBoard(tableSize, slotsToWin);
            
            
            Player1 = new Player(random.Next() % 2, true);

            if (Player1.Turn == 0)
            {
                Player1.Turn = -1;
            }

            Player2 = new Player(-Player1.Turn, false);
        }

        public void StartGame()
        {
            GameBoard.Print();
            while (!GameBoard.IsComplete())
            {
                if (Player1.Turn == GameBoard.Turn)
                {
                    Player1.NextStep(GameBoard);
                }
                else
                {
                    Player2.NextStep(GameBoard);
                }
                GameBoard.Print();
            }

        }

    }
}
