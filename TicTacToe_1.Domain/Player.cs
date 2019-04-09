using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacToe_1.Domain
{
    public class Player
    {
        public bool IsBot { get; set; }
        public int Turn { get; set; }

        public Player(int turn, bool isBot)
        {
            IsBot = isBot;
            Turn = turn;
        }

        public KeyValuePair<int, int> NextStep(GameBoard gameBoard)
        {
            KeyValuePair<int, int> step;
            if (IsBot)
            {
                if (gameBoard.TableSize > 3)
                {
                    if (gameBoard.TableSize >= 8)
                    {
                        var maxDepth = GetMaxDepth(gameBoard.EmptySlots.Count);
                        var bestOption = BestOptionOfSubtreeWithCutsAndRestriction(gameBoard, maxDepth, 1, Turn * double.MaxValue);
                        step = bestOption.Value;
                    }
                    else
                    {
                        var maxDepth = GetMaxDepth(gameBoard.EmptySlots.Count);
                        var bestOption = BestOptionOfSubtreeWithCuts(gameBoard, maxDepth, 1, Turn * double.MaxValue);
                        step = bestOption.Value;
                    }
                }
                else
                {
                    var bestOption = BestOptionOfSubtree(gameBoard);
                    step = bestOption.Value;
                }
                //gameBoard.Step(step);
            }
            else
            {
                step = GetPlayerInput(gameBoard);
                //gameBoard.Step(step);
            }
            return step;
        }

        private static KeyValuePair<int, KeyValuePair<int, int>> BestOptionOfSubtree(GameBoard gameBoard)
        {
            var bestOption = new KeyValuePair<int, KeyValuePair<int, int>>(-2, new KeyValuePair<int, int>());
            foreach (var emptySlot in gameBoard.EmptySlots)
            {
                var tempBoard = (GameBoard)gameBoard.Clone();
                tempBoard.Step(emptySlot);
                if (tempBoard.IsComplete())
                {
                    return new KeyValuePair<int, KeyValuePair<int, int>>(tempBoard.Winner, emptySlot);
                }

                var option = BestOptionOfSubtree(tempBoard).Key;
                if (option == gameBoard.Turn)
                {
                    return new KeyValuePair<int, KeyValuePair<int, int>>(option, emptySlot);
                }

                if (bestOption.Key == -gameBoard.Turn || bestOption.Key == -2)
                {
                    bestOption = new KeyValuePair<int, KeyValuePair<int, int>>(option, emptySlot);
                }

            }

            return bestOption;
        }

        private KeyValuePair<double, KeyValuePair<int, int>> BestOptionOfSubtreeWithCuts(GameBoard gameBoard, int maxDepth, int currentDepth, double parentBest)
        {
            var currentMultiplier = 1.0 / currentDepth;
            var ownBest = -gameBoard.Turn * (double.MaxValue - 1) * currentMultiplier;
            var bestSlot = new KeyValuePair<int, int>();

            if (gameBoard.EmptySlots.Count != 0)
            {
                bestSlot = gameBoard.EmptySlots[0];
            }

            if (gameBoard.IsComplete() || currentDepth == maxDepth)
            {
                var evaluation = EvaluateGameBoard(gameBoard) * currentMultiplier;
                if (gameBoard.Turn * ownBest < gameBoard.Turn * evaluation)
                {
                    ownBest = evaluation;
                }
            }
            else
            {
                foreach (var emptySlot in gameBoard.EmptySlots)
                {
                    var tempBoard = (GameBoard)gameBoard.Clone();
                    tempBoard.Step(emptySlot);
                    var evaluation = BestOptionOfSubtreeWithCuts(tempBoard, maxDepth, currentDepth + 1, ownBest);
                    if (gameBoard.Turn * ownBest < gameBoard.Turn * evaluation.Key)
                    {
                        ownBest = evaluation.Key;
                        bestSlot = emptySlot;
                    }
                    if (gameBoard.Turn * ownBest >= gameBoard.Turn * parentBest)
                    {
                        return new KeyValuePair<double, KeyValuePair<int, int>>(ownBest, bestSlot);
                    }

                }
            }

            return new KeyValuePair<double, KeyValuePair<int, int>>(ownBest, bestSlot);
        }

        private KeyValuePair<double, KeyValuePair<int, int>> BestOptionOfSubtreeWithCutsAndRestriction(GameBoard gameBoard, int maxDepth, int currentDepth, double parentBest)
        {
            var currentMultiplier = 1.0 / currentDepth;
            var ownBest = -gameBoard.Turn * (double.MaxValue - 1) * currentMultiplier;
            var bestSlot = new KeyValuePair<int, int>();

            if(gameBoard.EmptySlots.Count != 0)
            {
                bestSlot = gameBoard.EmptySlots[0];
            }

            if (gameBoard.IsComplete() || currentDepth == maxDepth)
            {
                var evaluation = EvaluateGameBoardWithRestriction(gameBoard) * currentMultiplier;
                if (gameBoard.Turn * ownBest < gameBoard.Turn * evaluation)
                {
                    ownBest = evaluation;
                }
            }
            else
            {
                foreach (var emptySlot in gameBoard.EmptySlots)
                {
                    if (!IsSlotInBounds(emptySlot, gameBoard))
                    {
                        continue;
                    }
                    var tempBoard = (GameBoard)gameBoard.Clone();
                    tempBoard.Step(emptySlot);
                    var evaluation = BestOptionOfSubtreeWithCutsAndRestriction(tempBoard, maxDepth, currentDepth + 1, ownBest);
                    if (gameBoard.Turn * ownBest < gameBoard.Turn * evaluation.Key)
                    {
                        ownBest = evaluation.Key;
                        bestSlot = emptySlot;
                    }
                    if (gameBoard.Turn * ownBest >= gameBoard.Turn * parentBest)
                    {
                        return new KeyValuePair<double, KeyValuePair<int, int>>(ownBest, bestSlot);
                    }

                }
            }

            return new KeyValuePair<double, KeyValuePair<int, int>>(ownBest, bestSlot);
        }

        public bool IsSlotInBounds(KeyValuePair<int, int> slot, GameBoard gameBoard)
        {
            if (slot.Key < gameBoard.RowMin - gameBoard.SlotsToWin || slot.Key > gameBoard.RowMax + gameBoard.SlotsToWin || slot.Value < gameBoard.ColumnMin - gameBoard.SlotsToWin || slot.Value > gameBoard.ColumnMax + gameBoard.SlotsToWin)
            {
                return false;
            }
            return true;
        }

        public static double EvaluateGameBoard(GameBoard gameBoard)
        {
            var evaluationScore = 0.0;
            evaluationScore += EvaluateRows(gameBoard);
            evaluationScore += EvaluateColumns(gameBoard);
            evaluationScore += EvaluateMainDiagonals(gameBoard);
            evaluationScore += EvaluateSecondaryDiagonals(gameBoard);
            return evaluationScore;
        }

        public static double EvaluateGameBoardWithRestriction(GameBoard gameBoard)
        {
            var evaluationScore = 0.0;
            evaluationScore += EvaluateRowsWithRestriction(gameBoard);
            evaluationScore += EvaluateColumnsWithRestriction(gameBoard);
            evaluationScore += EvaluateMainDiagonalsWithRestriction(gameBoard);
            evaluationScore += EvaluateSecondaryDiagonalsWithRestriction(gameBoard);
            return evaluationScore;
        }

        public static double EvaluateRows(GameBoard gameBoard)
        {
            var evaluationScore = 0.0;
            for (var i = 0; i < gameBoard.TableSize; i++)
            {
                evaluationScore += EvaluateRow(gameBoard, i);
            }

            return evaluationScore;
        }

        public static double EvaluateRowsWithRestriction(GameBoard gameBoard)
        {
            var evaluationScore = 0.0;
            for (var i = Math.Max(0, gameBoard.RowMin - gameBoard.SlotsToWin); i < Math.Min(gameBoard.TableSize, gameBoard.RowMax + gameBoard.SlotsToWin); i++)
            {
                evaluationScore += EvaluateRowWithRestriction(gameBoard, i);
            }

            return evaluationScore;
        }

        public static double EvaluateColumns(GameBoard gameBoard)
        {
            var evaluationScore = 0.0;
            for (var i = 0; i < gameBoard.TableSize; i++)
            {
                evaluationScore += EvaluateColumn(gameBoard, i);
            }

            return evaluationScore;
        }

        public static double EvaluateColumnsWithRestriction(GameBoard gameBoard)
        {
            var evaluationScore = 0.0;
            for (var i = Math.Max(0, gameBoard.ColumnMin - gameBoard.SlotsToWin); i < Math.Min(gameBoard.TableSize, gameBoard.ColumnMax + gameBoard.SlotsToWin); i++)
            {
                evaluationScore += EvaluateColumnWithRestriction(gameBoard, i);
            }

            return evaluationScore;
        }

        public static double EvaluateRow(GameBoard gameBoard, int rowIndex)
        {
            var evaluationScore = 0.0;
            for (var i = 0; i < gameBoard.TableSize - gameBoard.SlotsToWin + 1; i++)
            {
                var ownCounter = 0;
                var zeroCounter = 0;
                var enemyCounter = 0;
                for (var j = i; j < i + gameBoard.SlotsToWin; j++)
                {
                    if (gameBoard.Table[rowIndex, j] == 1)
                    {
                        ownCounter++;
                    }
                    else if (gameBoard.Table[rowIndex, j] == 0)
                    {
                        zeroCounter++;
                    }
                    else
                    {
                        enemyCounter++;
                    }
                }

                if (enemyCounter == 0 && zeroCounter != gameBoard.SlotsToWin)
                {
                    evaluationScore += Math.Pow(10.0, ownCounter);
                }
                else if (ownCounter == 0 && zeroCounter != gameBoard.SlotsToWin)
                {
                    evaluationScore -= Math.Pow(10.0, enemyCounter);
                }
            }

            return evaluationScore;
        }

        public static double EvaluateRowWithRestriction(GameBoard gameBoard, int rowIndex)
        {
            var evaluationScore = 0.0;
            for (var i = Math.Max(0, gameBoard.ColumnMin - gameBoard.SlotsToWin); i < Math.Min(gameBoard.TableSize - gameBoard.SlotsToWin + 1, gameBoard.ColumnMax + gameBoard.SlotsToWin); i++)
            {
                var ownCounter = 0;
                var zeroCounter = 0;
                var enemyCounter = 0;
                for (var j = i; j < i + gameBoard.SlotsToWin; j++)
                {
                    if (gameBoard.Table[rowIndex, j] == 1)
                    {
                        ownCounter++;
                    }
                    else if (gameBoard.Table[rowIndex, j] == 0)
                    {
                        zeroCounter++;
                    }
                    else
                    {
                        enemyCounter++;
                    }
                }

                if (enemyCounter == 0 && zeroCounter != gameBoard.SlotsToWin)
                {
                    evaluationScore += Math.Pow(10.0, ownCounter);
                }
                else if (ownCounter == 0 && zeroCounter != gameBoard.SlotsToWin)
                {
                    evaluationScore -= Math.Pow(10.0, enemyCounter);
                }
            }

            return evaluationScore;
        }

        public static double EvaluateColumn(GameBoard gameBoard, int columnIndex)
        {
            var evaluationScore = 0.0;
            for (var i = 0; i < gameBoard.TableSize - gameBoard.SlotsToWin + 1; i++)
            {
                var ownCounter = 0;
                var zeroCounter = 0;
                var enemyCounter = 0;
                for (var j = i; j < i + gameBoard.SlotsToWin; j++)
                {
                    if (gameBoard.Table[j, columnIndex] == 1)
                    {
                        ownCounter++;
                    }
                    else if (gameBoard.Table[j, columnIndex] == 0)
                    {
                        zeroCounter++;
                    }
                    else
                    {
                        enemyCounter++;
                    }
                }

                if (enemyCounter == 0 && zeroCounter != gameBoard.SlotsToWin)
                {
                    evaluationScore += Math.Pow(10.0, ownCounter);
                }
                else if (ownCounter == 0 && zeroCounter != gameBoard.SlotsToWin)
                {
                    evaluationScore -= Math.Pow(10.0, enemyCounter);
                }
            }

            return evaluationScore;
        }

        public static double EvaluateColumnWithRestriction(GameBoard gameBoard, int columnIndex)
        {
            var evaluationScore = 0.0;
            for (var i = Math.Max(0, gameBoard.RowMin - gameBoard.SlotsToWin); i < Math.Min(gameBoard.TableSize - gameBoard.SlotsToWin + 1, gameBoard.RowMax + gameBoard.SlotsToWin); i++)
            {
                var ownCounter = 0;
                var zeroCounter = 0;
                var enemyCounter = 0;
                for (var j = i; j < i + gameBoard.SlotsToWin; j++)
                {
                    if (gameBoard.Table[j, columnIndex] == 1)
                    {
                        ownCounter++;
                    }
                    else if (gameBoard.Table[j, columnIndex] == 0)
                    {
                        zeroCounter++;
                    }
                    else
                    {
                        enemyCounter++;
                    }
                }

                if (enemyCounter == 0 && zeroCounter != gameBoard.SlotsToWin)
                {
                    evaluationScore += Math.Pow(10.0, ownCounter);
                }
                else if (ownCounter == 0 && zeroCounter != gameBoard.SlotsToWin)
                {
                    evaluationScore -= Math.Pow(10.0, enemyCounter);
                }
            }

            return evaluationScore;
        }

        public static double EvaluateMainDiagonals(GameBoard gameBoard)
        {
            var evaluationScore = 0.0;
            for (var row = gameBoard.TableSize - gameBoard.SlotsToWin; row >= 0; row--)
            {
                for (var j = 0; j < gameBoard.TableSize - row - gameBoard.SlotsToWin + 1; j++)
                {
                    var ownCounter = 0;
                    var zeroCounter = 0;
                    var enemyCounter = 0;
                    for (var k = j; k < j + gameBoard.SlotsToWin; k++)
                    {
                        if (gameBoard.Table[row + k, k] == 1)
                        {
                            ownCounter++;
                        }
                        else if (gameBoard.Table[row + k, k] == 0)
                        {
                            zeroCounter++;
                        }
                        else
                        {
                            enemyCounter++;
                        }
                    }
                    if (enemyCounter == 0 && zeroCounter != gameBoard.SlotsToWin)
                    {
                        evaluationScore += Math.Pow(10.0, ownCounter);
                    }
                    else if (ownCounter == 0 && zeroCounter != gameBoard.SlotsToWin)
                    {
                        evaluationScore -= Math.Pow(10.0, enemyCounter);
                    }
                }
            }

            for (var column = 1; column < gameBoard.TableSize - gameBoard.SlotsToWin; column++)
            {
                for (var j = 0; j < gameBoard.TableSize - column - gameBoard.SlotsToWin + 1; j++)
                {
                    var ownCounter = 0;
                    var zeroCounter = 0;
                    var enemyCounter = 0;
                    for (var k = j; k < j + gameBoard.SlotsToWin; k++)
                    {
                        if (gameBoard.Table[k, column + k] == 1)
                        {
                            ownCounter++;
                        }
                        else if (gameBoard.Table[k, column + k] == 0)
                        {
                            zeroCounter++;
                        }
                        else
                        {
                            enemyCounter++;
                        }
                    }
                    if (enemyCounter == 0 && zeroCounter != gameBoard.SlotsToWin)
                    {
                        evaluationScore += Math.Pow(10.0, ownCounter);
                    }
                    else if (ownCounter == 0 && zeroCounter != gameBoard.SlotsToWin)
                    {
                        evaluationScore -= Math.Pow(10.0, enemyCounter);
                    }
                }
            }

            return evaluationScore;
        }

        public static double EvaluateMainDiagonalsWithRestriction(GameBoard gameBoard)
        {
            var evaluationScore = 0.0;
            for (var row = Math.Min(gameBoard.TableSize - gameBoard.SlotsToWin, gameBoard.RowMax + gameBoard.SlotsToWin); row >= Math.Max(0, gameBoard.RowMin - gameBoard.SlotsToWin); row--)
            {
                for (var j = Math.Max(0, gameBoard.ColumnMin - gameBoard.SlotsToWin); j < Math.Min(gameBoard.TableSize - row - gameBoard.SlotsToWin + 1, gameBoard.ColumnMax + gameBoard.SlotsToWin); j++)
                {
                    var ownCounter = 0;
                    var zeroCounter = 0;
                    var enemyCounter = 0;
                    for (var k = j; k < j + gameBoard.SlotsToWin; k++)
                    {
                        if (gameBoard.Table[row + k, k] == 1)
                        {
                            ownCounter++;
                        }
                        else if (gameBoard.Table[row + k, k] == 0)
                        {
                            zeroCounter++;
                        }
                        else
                        {
                            enemyCounter++;
                        }
                    }
                    if (enemyCounter == 0 && zeroCounter != gameBoard.SlotsToWin)
                    {
                        evaluationScore += Math.Pow(10.0, ownCounter);
                    }
                    else if (ownCounter == 0 && zeroCounter != gameBoard.SlotsToWin)
                    {
                        evaluationScore -= Math.Pow(10.0, enemyCounter);
                    }
                }
            }

            for (var column = Math.Max(1, gameBoard.ColumnMin - gameBoard.SlotsToWin); column < Math.Min(gameBoard.TableSize - gameBoard.SlotsToWin, gameBoard.ColumnMax + gameBoard.SlotsToWin); column++)
            {
                for (var j = Math.Max(0, gameBoard.ColumnMin - gameBoard.SlotsToWin); j < Math.Min(gameBoard.TableSize - column - gameBoard.SlotsToWin + 1, gameBoard.ColumnMax + gameBoard.SlotsToWin); j++)
                {
                    var ownCounter = 0;
                    var zeroCounter = 0;
                    var enemyCounter = 0;
                    for (var k = j; k < j + gameBoard.SlotsToWin; k++)
                    {
                        if (gameBoard.Table[k, column + k] == 1)
                        {
                            ownCounter++;
                        }
                        else if (gameBoard.Table[k, column + k] == 0)
                        {
                            zeroCounter++;
                        }
                        else
                        {
                            enemyCounter++;
                        }
                    }
                    if (enemyCounter == 0 && zeroCounter != gameBoard.SlotsToWin)
                    {
                        evaluationScore += Math.Pow(10.0, ownCounter);
                    }
                    else if (ownCounter == 0 && zeroCounter != gameBoard.SlotsToWin)
                    {
                        evaluationScore -= Math.Pow(10.0, enemyCounter);
                    }
                }
            }

            return evaluationScore;
        }

        public static double EvaluateSecondaryDiagonals(GameBoard gameBoard)
        {
            var evaluationScore = 0.0;
            for (var row = gameBoard.TableSize - gameBoard.SlotsToWin; row >= 0; row--)
            {
                for (var j = 0; j < gameBoard.TableSize - row - gameBoard.SlotsToWin + 1; j++)
                {
                    var ownCounter = 0;
                    var zeroCounter = 0;
                    var enemyCounter = 0;
                    for (var k = j; k < j + gameBoard.SlotsToWin; k++)
                    {
                        if (gameBoard.Table[row + k, gameBoard.TableSize - 1 - k] == 1)
                        {
                            ownCounter++;
                        }
                        else if (gameBoard.Table[row + k, gameBoard.TableSize - 1 - k] == 0)
                        {
                            zeroCounter++;
                        }
                        else
                        {
                            enemyCounter++;
                        }
                    }
                    if (enemyCounter == 0 && zeroCounter != gameBoard.SlotsToWin)
                    {
                        evaluationScore += Math.Pow(10.0, ownCounter);
                    }
                    else if (ownCounter == 0 && zeroCounter != gameBoard.SlotsToWin)
                    {
                        evaluationScore -= Math.Pow(10.0, enemyCounter);
                    }
                }
            }

            for (var column = 1; column < gameBoard.TableSize - gameBoard.SlotsToWin; column++)
            {
                for (var j = 0; j < gameBoard.TableSize - column - gameBoard.SlotsToWin + 1; j++)
                {
                    var ownCounter = 0;
                    var zeroCounter = 0;
                    var enemyCounter = 0;
                    for (var k = j; k < j + gameBoard.SlotsToWin; k++)
                    {
                        if (gameBoard.Table[k, column + k] == 1)
                        {
                            ownCounter++;
                        }
                        else if (gameBoard.Table[k, column + k] == 0)
                        {
                            zeroCounter++;
                        }
                        else
                        {
                            enemyCounter++;
                        }
                    }
                    if (enemyCounter == 0 && zeroCounter != gameBoard.SlotsToWin)
                    {
                        evaluationScore += Math.Pow(10.0, ownCounter);
                    }
                    else if (ownCounter == 0 && zeroCounter != gameBoard.SlotsToWin)
                    {
                        evaluationScore -= Math.Pow(10.0, enemyCounter);
                    }
                }
            }

            return evaluationScore;
        }

        public static double EvaluateSecondaryDiagonalsWithRestriction(GameBoard gameBoard)
        {
            var evaluationScore = 0.0;
            for (var row = Math.Min(gameBoard.TableSize - gameBoard.SlotsToWin, gameBoard.RowMax + gameBoard.SlotsToWin); row >= Math.Max(0, gameBoard.RowMin - gameBoard.SlotsToWin); row--)
            {
                for (var j = Math.Max(0, gameBoard.ColumnMin - gameBoard.SlotsToWin); j < Math.Min(gameBoard.TableSize - row - gameBoard.SlotsToWin + 1, gameBoard.ColumnMax + gameBoard.SlotsToWin); j++)
                {
                    var ownCounter = 0;
                    var zeroCounter = 0;
                    var enemyCounter = 0;
                    for (var k = j; k < j + gameBoard.SlotsToWin; k++)
                    {
                        if (gameBoard.Table[row + k, gameBoard.TableSize - 1 - k] == 1)
                        {
                            ownCounter++;
                        }
                        else if (gameBoard.Table[row + k, gameBoard.TableSize - 1 - k] == 0)
                        {
                            zeroCounter++;
                        }
                        else
                        {
                            enemyCounter++;
                        }
                    }
                    if (enemyCounter == 0 && zeroCounter != gameBoard.SlotsToWin)
                    {
                        evaluationScore += Math.Pow(10.0, ownCounter);
                    }
                    else if (ownCounter == 0 && zeroCounter != gameBoard.SlotsToWin)
                    {
                        evaluationScore -= Math.Pow(10.0, enemyCounter);
                    }
                }
            }

            for (var column = Math.Max(1, gameBoard.ColumnMin - gameBoard.SlotsToWin); column < Math.Min(gameBoard.TableSize - gameBoard.SlotsToWin, gameBoard.ColumnMax + gameBoard.SlotsToWin); column++)
            {
                for (var j = Math.Max(0, gameBoard.ColumnMin - gameBoard.SlotsToWin); j < Math.Min(gameBoard.TableSize - column - gameBoard.SlotsToWin + 1, gameBoard.ColumnMax + gameBoard.SlotsToWin); j++)
                {
                    var ownCounter = 0;
                    var zeroCounter = 0;
                    var enemyCounter = 0;
                    for (var k = j; k < j + gameBoard.SlotsToWin; k++)
                    {
                        if (gameBoard.Table[k, column + k] == 1)
                        {
                            ownCounter++;
                        }
                        else if (gameBoard.Table[k, column + k] == 0)
                        {
                            zeroCounter++;
                        }
                        else
                        {
                            enemyCounter++;
                        }
                    }
                    if (enemyCounter == 0 && zeroCounter != gameBoard.SlotsToWin)
                    {
                        evaluationScore += Math.Pow(10.0, ownCounter);
                    }
                    else if (ownCounter == 0 && zeroCounter != gameBoard.SlotsToWin)
                    {
                        evaluationScore -= Math.Pow(10.0, enemyCounter);
                    }
                }
            }

            return evaluationScore;
        }

        private KeyValuePair<int, int> GetPlayerInput(GameBoard gameBoard)
        {
            int row, column;
            do
            {
                Console.WriteLine("Enter coordinates:");
                row = int.Parse(Console.ReadLine());
                column = int.Parse(Console.ReadLine());
            }
            while (!gameBoard.EmptySlots.Contains(new KeyValuePair<int, int>(row, column)));
            return new KeyValuePair<int, int>(row, column);
        }

        private int GetMaxDepth(int numberOfEmptySlots)
        {
            var treeSpread = 1;
            var depth = 0;
            while (treeSpread < 400000 && numberOfEmptySlots - depth > 0)
            {
                treeSpread *= (numberOfEmptySlots - depth);
                depth++;
            }

            return depth;
        }
    }
}
