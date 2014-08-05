using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MissionPlanner.Controls;
using MissionPlanner.Utilities;
using System.IO;

namespace MissionPlanner.GCSViews
{
    public partial class CustomLog : MyUserControl, IActivate
    {
        public CustomLog()
        {
            InitializeComponent();
        }

        public void Activate()
        {
          
        }

        private void CustomLog_Load(object sender, EventArgs e)
        {

        }

        private void bindingSource1_CurrentChanged(object sender, EventArgs e)
        {

        }

        static int timercount = 0;
        static bool last_armed = true;
        private void timer1_Tick(object sender, EventArgs e)
        {

            if (MainV2.comPort.MAV.cs.armed != last_armed)
            {
                last_armed = MainV2.comPort.MAV.cs.armed;
                if(last_armed)
                {
                    label1.Text = "==========ARMED==========";
                    myGPSlog.Clear();
                }
                else
                {
                    label1.Text = "=========DISARMED=========";
                }
                return;
            }
            else if (!MainV2.comPort.MAV.cs.armed)
            {
                return;
            }
            String mystring;
            mystring = "Time:" + MainV2.comPort.MAV.cs.gpstime.ToLongTimeString();
            mystring += " Mode:" + MainV2.comPort.MAV.cs.mode + " ";
            mystring += " Latitude:" + MainV2.comPort.MAV.cs.lat.ToString() + " Longitude:" + MainV2.comPort.MAV.cs.lng.ToString();
            
            timercount++;

            myGPSlog.AppendText(mystring);
            myGPSlog.AppendText(Environment.NewLine);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog GPSlogfile = new SaveFileDialog();
            GPSlogfile.DefaultExt = "txt";
            GPSlogfile.Filter = "text files|*.txt";
            GPSlogfile.AddExtension = true;
            if(GPSlogfile.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(GPSlogfile.FileName,myGPSlog.Text);
            }
            
        }
    }
}
