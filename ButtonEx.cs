using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IqaController
{
    public partial class ButtonEx : Button
    {
        public ButtonEx()
        {
            InitializeComponent();
            this.BackColor = SystemColors.WindowFrame;
            this.FlatStyle = FlatStyle.Flat;
            this.ForeColor = SystemColors.Window;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }
    }
}
