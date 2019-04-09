using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TicTacToe_1.Domain;

namespace TicTacToe_1.Form
{
    public partial class Form1 : System.Windows.Forms.Form
    {

        public List<Slot> Slots { get; set; }
        public Game Game { get; set; }



        public Form1()
        {
            InitializeComponent();
            Game = new Game(10, 5, new Random());

            Slots = new List<Slot>();

            tableLayoutPanel.ColumnCount = Game.GameBoard.TableSize;
            tableLayoutPanel.RowCount = Game.GameBoard.TableSize;

            for (var i = 0; i < Game.GameBoard.TableSize; i++)
            {
                for (var j = 0; j < Game.GameBoard.TableSize; j++)
                {
                    var slot = new Slot()
                    {
                        Column = j,
                        Row = i
                    };
                    slot.Click += new EventHandler(SlotHandler);
                    Slots.Add(slot);
                    tableLayoutPanel.Controls.Add(slot);
                }
            }


        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                while (!Game.GameBoard.IsComplete())
                {
                    if (Game.Player1.IsBot && Game.GameBoard.Turn == Game.Player1.Turn)
                    {
                        StepOnBoard(Game.Player1.NextStep(Game.GameBoard));
                    }
                    if (Game.Player2.IsBot && Game.GameBoard.Turn == Game.Player2.Turn)
                    {
                        StepOnBoard(Game.Player1.NextStep(Game.GameBoard));
                    }
                }
            });

        }

        public void SlotHandler(object sender, EventArgs e)
        {
            var slot = sender as Slot;
            if (Game.Player1.Turn == Game.GameBoard.Turn && !Game.Player1.IsBot)
            {
                if (Game.GameBoard.EmptySlots.Contains(new KeyValuePair<int, int>(slot.Row, slot.Column)))
                {
                    StepOnBoard(new KeyValuePair<int, int>(slot.Row, slot.Column));
                }
                else
                {
                    MessageBox.Show("Invalid slot.");
                }
            }
            if (Game.Player2.Turn == Game.GameBoard.Turn && !Game.Player2.IsBot)
            {
                if (Game.GameBoard.EmptySlots.Contains(new KeyValuePair<int, int>(slot.Row, slot.Column)))
                {
                    StepOnBoard(new KeyValuePair<int, int>(slot.Row, slot.Column));
                }
                else
                {
                    MessageBox.Show("Invalid slot.");
                }
            }
        }

        public void StepOnBoard(KeyValuePair<int, int> step)
        {
            if (Game.GameBoard.IsComplete())
            {
                return;
            }
            Game.GameBoard.Step(step);
            var currentSlot = Array.Find(tableLayoutPanel.Controls.Cast<Control>().ToArray(), s =>
            {
                var slot = (Slot)s;
                return slot.Column == step.Value && slot.Row == step.Key;
            });
            Invoke((MethodInvoker)delegate
            {
                if (Game.GameBoard.Table[step.Key, step.Value] == 1)
                {
                    currentSlot.BackColor = Color.Red;
                    currentSlot.Text = "X";
                }
                else
                {
                    currentSlot.BackColor = Color.Blue;
                    currentSlot.Text = "O";
                }
                if (Game.GameBoard.IsComplete())
                {
                    switch (Game.GameBoard.Winner)
                    {
                        case -1:
                            {
                                MessageBox.Show("Blue won!");
                                break;
                            }
                        case 1:
                            {
                                MessageBox.Show("Red won!");
                                break;
                            }
                        case 0:
                            {
                                MessageBox.Show("Draw!");
                                break;
                            }
                    }
                    Dispose(true);
                }
            });
        }
    }
}
