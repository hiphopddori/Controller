using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IqaController
{
    public partial class ucLoadingBar : UserControl
    {
        public ucLoadingBar()
        {
            InitializeComponent();
        }

        public void On(string msg)
        {
            this.Invoke(new Action(delegate ()
            {
                this.Show();
                lblTitle.Text = msg;

            }));
        }
        public void Off()
        {
            this.Invoke(new Action(delegate ()
            {
                this.Hide();
                lblTitle.Text = "";

            }));
        }
    }
}
