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
        int tickStart = 0;

        List<RollingPointPairList> list = new List<RollingPointPairList>();

        public CustomLog()
        {
            InitializeComponent();

            CreateChart(DebugGraph1, "Debug Plot", "Time", "Data");

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
        private void timer1_Tick(object sender, EventArgs e)
        {
            timercount++;
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


    }
}
