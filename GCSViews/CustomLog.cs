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
using ZedGraph;

namespace MissionPlanner.GCSViews
{
    public partial class CustomLog : MyUserControl, IActivate
    {
        public static bool circle_started { get; set; }
        int tickStart = 0;

        List<RollingPointPairList> list = new List<RollingPointPairList>();



        public CustomLog()
        {
            InitializeComponent();

            CreateChart(DebugGraph1, "Debug Plot", "Time", "Data");

            circle_started = false;

        }

        public void CreateChart(ZedGraphControl zgc, string Title, string XAxis, string YAxis)
        {
            GraphPane myPane = zgc.GraphPane;

            // Set the titles and axis labels
            myPane.Title.Text = Title;
            myPane.XAxis.Title.Text = XAxis;
            myPane.YAxis.Title.Text = YAxis;

           // LineItem myCurve;

           /* for (int i=0;i<6;i++)
            {
                list.Add(new RollingPointPairList(800));
                myCurve = myPane.AddCurve("Debug"+i.ToString(), list[i], Color.Aqua, SymbolType.None);
            }*/
            


            // Show the x axis grid
            myPane.XAxis.MajorGrid.IsVisible = true;

            /*myPane.XAxis.Scale.Min = 0;
            myPane.XAxis.Scale.Max = 5;*/

            // Make the Y axis scale red
            myPane.YAxis.Scale.FontSpec.FontColor = Color.Red;
            myPane.YAxis.Title.FontSpec.FontColor = Color.Red;
            // turn off the opposite tics so the Y tics don't show up on the Y2 axis
            myPane.YAxis.MajorTic.IsOpposite = false;
            myPane.YAxis.MinorTic.IsOpposite = false;
            // Don't display the Y zero line
            myPane.YAxis.MajorGrid.IsZeroLine = true;
            // Align the Y axis labels so they are flush to the axis
            myPane.YAxis.Scale.Align = AlignP.Inside;
            // Manually set the axis range
            //myPane.YAxis.Scale.Min = -1;
            //myPane.YAxis.Scale.Max = 1;

            // Fill the axis background with a gradient
            myPane.Chart.Fill = new Fill(Color.White, Color.LightGray, 45.0f);

            // Sample at 20ms intervals
            timer2.Enabled = true;
            timer2.Start();


            // Calculate the Axis Scale Ranges
            zgc.AxisChange();

            tickStart = Environment.TickCount;


        }



        public void Activate()
        {
          
        }



        int timercount = 0;
        bool last_armed = true;
        bool last_circle_started = false;
        private void timer1_Tick(object sender, EventArgs e)
        {
            
            
                ///MainV2.speechEngine.SpeakAsync("Speak test");
            ///
            
            var _mydebug = MainV2.comPort.MAV.cs.mydebug;
           // if (_mydebug != null && _mydebug.Count > 0)
               // Debuglabel.Text = _mydebug[0].name + " - " + _mydebug[0].value.ToString();
            ///
            if (MainV2.comPort.MAV.cs.armed != last_armed)
            {
                last_armed = MainV2.comPort.MAV.cs.armed;
                if(last_armed)
                {
                    label1.Text = "==========ARMED==========";
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"D:\SkyDrive\Y3S1\MDP\GPSlog.txt", true))
                    {
                        file.WriteLine("<<<<<<<<<<<<<<<< New Session at " + DateTime.Now.ToShortDateString() + DateTime.Now.ToLongTimeString() + " <<<<<<<<<<<<<<<<");
                    }
                    myGPSlog.Clear();
                }
                else
                {
                    label1.Text = "=========DISARMED=========";
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"D:\SkyDrive\Y3S1\MDP\GPSlog.txt", true))
                    {
                        file.WriteLine(">>>>>>>>>>>>>>>> Session Closed at " + DateTime.Now.ToShortDateString() + DateTime.Now.ToLongTimeString() + " >>>>>>>>>>>>>>>>");
                    }
                }
                timercount = 0;
                return;
            }
            else if (!MainV2.comPort.MAV.cs.armed)
            {
                timercount = 0;
                return;
            }

            String curr_mode;
            switch (MainV2.comPort.MAV.cs.mode)
            {
                case "Auto":
                    curr_mode = "0";
                    break;
                case "Guided":
                    curr_mode = "1";
                    break;
                case "RTL":
                    curr_mode = "2";
                    break;
                default:
                    return;
            }

            if (circle_started == true)
            {
                if (curr_mode == "0")
                    curr_mode = "3";
                else
                    circle_started = false;
            }

            if (circle_started != last_circle_started)
            {
                last_circle_started = circle_started;
                timercount = 0;
            }
                

            timercount++;
            String mystring;
            mystring = timercount.ToString();
            mystring += " " + MainV2.comPort.MAV.cs.lat.ToString("F9") + " " + MainV2.comPort.MAV.cs.lng.ToString("F9") + " " + curr_mode;


            // 0:Stabilize,1:Acro,2:AltHold,3:Auto,4:Guided,5:Loiter,6:RTL,7:Circle,9:Land,10:OF_Loiter,11:Drift,13:Sport
            
            

            myGPSlog.AppendText(mystring + Environment.NewLine);

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"D:\SkyDrive\Y3S1\MDP\GPSlog.txt", true))
            {
                file.WriteLine(mystring);
            }
            
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

        
        private void timer2_Tick_1(object sender, EventArgs e)
        {
            double time = (Environment.TickCount - tickStart) / 1000.0;

            // Make sure that the curvelist has at least one curve
            if (DebugGraph1.GraphPane == null || DebugGraph1.GraphPane.CurveList.Count <= 0)
                return;

            // Get the first CurveItem in the graph
            LineItem curve = DebugGraph1.GraphPane.CurveList[0] as LineItem;
            if (curve == null)
                return;

            // Get the PointPairList
            IPointListEdit list = curve.Points as IPointListEdit;
            // If this is null, it means the reference at curve.Points does not
            // support IPointListEdit, so we won't be able to modify it
            if (list == null)
                return;

            // Time is measured in seconds
            //double time = (Environment.TickCount - tickStart) / 1000.0;

            // Keep the X scale at a rolling 30 second interval, with one
            // major step between the max X value and the end of the axis
            Scale xScale = DebugGraph1.GraphPane.XAxis.Scale;
            if (time > xScale.Max - xScale.MajorStep)
            {
                xScale.Max = time + xScale.MajorStep;
                xScale.Min = xScale.Max - 10.0;
            }

            // Make sure the Y axis is rescaled to accommodate actual data
            try
            {
                DebugGraph1.AxisChange();
            }
            catch { }
            // Force a redraw
            DebugGraph1.Invalidate();




        }

        List<string> displayname = new List<string>();

        Color[] colours = new Color[] {      
            Color.Red, 
           Color.Green, 
           Color.Blue, 
           Color.Pink, 
           Color.Yellow, 
           Color.Orange, 
           Color.Violet, 
           Color.Wheat, 
           Color.Teal, 
           Color.Silver };

        private void timer3_Tick(object sender, EventArgs e)
        {
            var _mydebug = MainV2.comPort.MAV.cs.mydebug;

            if (!MainV2.comPort.BaseStream.IsOpen && !MainV2.comPort.logreadmode)
                return;

            //Console.WriteLine(DateTime.Now.Millisecond + " timer2 serial");
            try
            {
                MainV2.comPort.MAV.cs.UpdateCurrentSettings(currentStateBindingSource);
            }
            catch { }



            double time = (Environment.TickCount - tickStart) / 1000.0;


            if (_mydebug != null && _mydebug.Count > 0)
            {
                if (displayname.Count > 0)
                {
                    for (int i=0;i<displayname.Count;i++)
                    {
                        if(displayname[i].Equals(_mydebug[0].name))
                        {
                            if(checkedListBox1.GetItemChecked(i))
                            {
                                list[i].Add(time, _mydebug[0].value);
                                _mydebug.RemoveAt(0);
                            }
                            else
                            {
                                list[i].Clear();
                                _mydebug.RemoveAt(0);
                            }
                            return;
                            
                        }
                    }
                }
                    checkedListBox1.Items.Add(_mydebug[0].name, true);
                    displayname.Add(_mydebug[0].name);
                    list.Add(new RollingPointPairList(800));
                    list[list.Count-1].Add(time, _mydebug[0].value);
                    DebugGraph1.GraphPane.AddCurve(_mydebug[0].name, list[list.Count - 1], colours[(list.Count - 1) % colours.Length], SymbolType.None);
                    //LineItem ccurve = (LineItem)(DebugGraph1.GraphPane.CurveList[list.Count - 1]);
                    //ccurve.Label.Text = _mydebug[0].name;
                          

                _mydebug.RemoveAt(0);   

            }
                
        }

        int _last_yaw = 1000;

        public int warp180(int a, int b)
        {
            int c = Math.Abs(a - b);
            if (c>180)
            {
                c = 360 - c;
            }
            return c;
        }


        private void checkheading_Tick(object sender, EventArgs e)
        {
            int yaw = (int)MainV2.comPort.MAV.cs.yaw;

                if (warp180(yaw, _last_yaw) < 5 && _last_yaw <=360)
                    yaw = _last_yaw;

                if (yaw <22.5 || yaw > 337.5)
                {
                    MainV2.speechEngine.SpeakAsync("north");
                }
                else if (yaw > 22.5 && yaw < 67.5)
                {
                    MainV2.speechEngine.SpeakAsync("north east");
                }
                else if (yaw > 67.5 && yaw < 112.5)
                {
                    MainV2.speechEngine.SpeakAsync("east");
                }
                else if (yaw > 112.5 && yaw < 157.5)
                {
                    MainV2.speechEngine.SpeakAsync("south east");
                }
                else if (yaw > 157.5 && yaw < 202.5)
                {
                    MainV2.speechEngine.SpeakAsync("south");
                }
                else if (yaw > 202.5 && yaw < 247.5)
                {
                    MainV2.speechEngine.SpeakAsync("south west");
                }
                else if (yaw >247.5 && yaw < 292.5)
                {
                    MainV2.speechEngine.SpeakAsync("west");
                }
                else if (yaw > 292.5&& yaw < 337.5)
                {
                    MainV2.speechEngine.SpeakAsync("north west");
                }
                else
                {
                    MainV2.speechEngine.SpeakAsync("error heading");
                }


                _last_yaw = yaw;


           /* if (_last_yaw > 360)
            {
                _last_yaw = yaw;
            }*/
                
        }

        private void tim_ch6_Tick(object sender, EventArgs e)
        {
            ////////////////////
            /// for heading check
            /// 
            float ch6in = MainV2.comPort.MAV.cs.ch6in;
            if (MainV2.comPort.BaseStream.IsOpen)
            {

                txt_ch6.Text = "Channel 6 pwm = " + ch6in.ToString();
                if(ch6in>1500.0 && !checkheading.Enabled)
                    checkheading.Start();
                else if (ch6in<=1500.0)
                    checkheading.Stop();
            }
            else
            {
                txt_ch6.Text = "Link lost";
                checkheading.Stop();
            }
        }

        public String myTextBox { set { myGPSlog.AppendText(value); } }

        private void But_ShowConsole_Click(object sender, EventArgs e)
        {
            MainV2.TextForm.Show();
        }


    }
}
