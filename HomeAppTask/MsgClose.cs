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
    public partial class MsgClose : Form
    {
        public MsgClose()
        {
            InitializeComponent();
            
        }



        private void MsgClose_Load(object sender, EventArgs e)
        {
            //// Make button1's dialog result OK.
            //button2.DialogResult = DialogResult.Yes;
            //// Make button2's dialog result Cancel.
            //button2.DialogResult = DialogResult.No;

            //this.AcceptButton = button1;
            //this.CancelButton = button2;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
