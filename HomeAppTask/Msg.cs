using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HomeAppTask
{
    public partial class Msg : Form
    {
        public Msg(string lText)
        {
            InitializeComponent();
            this.label16.Text = lText;
            center();
        }

        private void center()
        {
            int x = (panel1.Width / 2) - (label16.Width / 2);
            int y = label16.Location.Y;

            label16.Location = new Point(x, y);

            

        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
