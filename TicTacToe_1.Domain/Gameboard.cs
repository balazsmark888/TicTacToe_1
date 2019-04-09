using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacToe_1.Domain
{
    public class GameBoard : ICloneable
    {
        public int TableSize { get; set; }
        public int SlotsToWin { get; set; }
        public int[,] Table { get; set; }
        public List<KeyValuePair<int, int>> EmptySlots { get; set; }
        public int Turn { get; set; }
        public int Winner { get; set; }
        public int RowMin { get; set; }
        public int RowMax { get; set; }
        public int ColumnMin { get; set; }
        public int ColumnMax { get; set; }

        public GameBoard()
        {

        }

        public GameBoard(int tableSize, int slotsToWin)
        {
            TableSize = tableSize;
            SlotsToWin = slotsToWin;
            Table = new int[tableSize, tableSize];
            EmptySlots = new List<KeyValuePair<int, int>>();
            RowMin = int.MaxValue;
            RowMax = int.MinValue;
            ColumnMax = int.MinValue;
            ColumnMin = int.MaxValue;
            for (var i = 0; i < tableSize; i++)
            {
                for (var j = 0; j < tableSize; j++)
                {
                    EmptySlots.Add(new KeyValuePair<int, int>(i, j));
                }
            }

            Turn = -1;
        }

        public bool IsComplete()
        {
            return Winner != 0 || EmptySlots.Count == 0;
        }

        public object Clone()
        {
            return new GameBoard(TableSize, SlotsToWin)
            {
                Table = (int[,])Table.Clone(),
                EmptySlots = new List<KeyValuePair<int, int>>(EmptySlots),
                Turn = Turn,
                Winner = Winner,
                SlotsToWin = SlotsToWin,
                TableSize = TableSize
            };
        }

        public void Step(KeyValuePair<int, int> step)
        {
            Table[step.Key, step.Value] = Turn;
            
            EmptySlots.Remove(step);
            CheckForWinner(step);
            SetLimits(step);
            Turn = -Turn;

        }

        public void SetLimits(KeyValuePair<int, int> step)
        {
            if (step.Key < RowMin)
            {
                RowMin = step.Key;
            }
            if (step.Key > RowMax)
            {
                RowMax = step.Key;
            }
            if (step.Value < ColumnMin)
            {
                ColumnMin = step.Value;
            }
            if (step.Value > ColumnMax)
            {
                ColumnMax = step.Value;
            }
        }

        private void CheckForWinner(KeyValuePair<int, int> step)
        {
            CheckRowsForWinner(step);
            if (Winner != 0) return;
            CheckColumnsForWinner(step);
            if (Winner != 0) return;
            CheckMainDiagonalForWinner(step);
            if (Winner != 0) return;
            CheckSecondaryDiagonalForWinner(step);
        }

        private void CheckRowsForWinner(KeyValuePair<int, int> step)
        {
            for (var i = 0; i < TableSize - SlotsToWin + 1; i++)
            {
                var slot = Table[i, step.Value];

                for (var j = i + 1; j < i + SlotsToWin; j++)
                {
                    if (Table[j, step.Value] == slot) continue;
                    slot = 0;
                    break;
                }

                if (slot == 0) continue;
                Winner = slot;
                return;
            }
        }

        private void CheckColumnsForWinner(KeyValuePair<int, int> step)
        {
            for (var i = 0; i < TableSize - SlotsToWin + 1; i++)
            {
                var slot = Table[step.Key, i];

                for (var j = i + 1; j < i + SlotsToWin; j++)
                {
                    if (Table[step.Key, j] == slot) continue;
                    slot = 0;
                    break;
                }

                if (slot == 0) continue;
                Winner = slot;
                return;
            }
        }

        private void CheckMainDiagonalForWinner(KeyValuePair<int, int> step)
        {
            var row = step.Key;
            var column = step.Value;
            while (row != 0 && column != 0)
            {
                row--;
                column--;
            }

            for (var i = 0; i < TableSize - row - column - SlotsToWin + 1; i++)
            {
                var slot = Table[row + i, column + i];

                for (var j = i + 1; j < i + SlotsToWin; j++)
                {
                    if (Table[row + j, column + j] == slot) continue;
                    slot = 0;
                    break;
                }

                if (slot == 0) continue;
                Winner = slot;
                return;
            }

        }

        private void CheckSecondaryDiagonalForWinner(KeyValuePair<int, int> step)
        {
            var row = step.Key;
            var column = step.Value;
            while (row != 0 && column != TableSize - 1)
            {
                row--;
                column++;
            }

            for (var i = 0; i < column - row + 1 - SlotsToWin + 1; i++)
            {
                var slot = Table[row + i, column - i];

                for (var j = i + 1; j < i + SlotsToWin; j++)
                {
                    if (Table[row + j, column - j] == slot) continue;
                    slot = 0;
                    break;
                }

                if (slot == 0) continue;
                Winner = slot;
                return;
            }

        }

        public void Print()
        {
            for (var i = 0; i < TableSize; i++)
            {
                for (var j = 0; j < TableSize; j++)
                {
                    Console.Write(Table[i, j] + "\t");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }
}
