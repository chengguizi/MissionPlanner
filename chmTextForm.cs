using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MissionPlanner
{
    public partial class chmTextForm : Form
    {
        public chmTextForm()
        {
            InitializeComponent();
        }

        private void chmTextForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        private void But_AddText_Click(object sender, EventArgs e)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"D:\SkyDrive\Y3S1\MDP\MPlogfile.txt", true))
            {
                textBox1.AppendText("/////" + DateTime.Now.ToLongTimeString() + ": " + textBox_AddText.Text + Environment.NewLine);
                file.WriteLine("/////" + DateTime.Now.ToLongTimeString() + ": " + textBox_AddText.Text);
                textBox_AddText.Clear();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (MainV2.comPort.BaseStream.IsOpen)
                label_txbuff.Text = MainV2.comPort.MAV.cs.txbuffer.ToString();
        }
    }
}
