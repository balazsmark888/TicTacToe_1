using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TicTacToe_1.Form
{
    public class Slot : Button
    {
        public int Row { get; set; }
        public int Column { get; set; }
    }
}
