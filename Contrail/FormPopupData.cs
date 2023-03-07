using SpaceLane;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Contrail
{
    public partial class FormPopupData : System.Windows.Forms.Form
    {
        public FormPopupData()
        {
            InitializeComponent();
            this.Text = "Analyzed Gas Concentration Data Window";
        }


        /*private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            {
                StreamWriter fileLogger = new StreamWriter("C:/Users/raymo/Downloads/log.txt");
                fileLogger.WriteLine(txt);
                fileLogger.Flush();
                fileLogger.Close();
            }
            catch (Exception)
            {

            }

            DataPack dp = null;

            if (!txt.StartsWith("-----"))
            {
                portPacketLines.Add(txt);
            }
            else
            {
                try
                {
                    dp = DataPackReader.ReadDataPackCom(portPacketLines, dp);

                    PlotValue(dp);

                }
                catch (Exception e1)
                {
                    portPacketLines.Clear();
                }
            }

            mapControl.CanDragMap = true;

        }



        // Read the file
        private void ReadFile()
        {
            List<string> packetLines = new List<string>();
            DataPack dp = null;

            dpList.Clear();

            //mapControl.CanDragMap = false;

            using (StreamReader reader = new StreamReader(fileName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    AddToMonitor(line);

                    if (!line.StartsWith("-----"))
                    {
                        packetLines.Add(line);
                    }
                    else
                    {
                        try
                        {
                            dp = DataPackReader.ReadDataPackCom(packetLines, dp);

                            PlotValue(dp);

                            //show the location on the map
                            PlotMap(dp.Latitude, dp.Longitude);

                            //Generate the Heatmap if dp.HeatValues is not empty
                            if (!string.IsNullOrEmpty(dp.HeatValues))
                                DrawHeatmap(dp.HeatValues);

                            dpList.Add(dp);
                            packetLines.Clear();

                            Thread.Sleep(50);
                        }
                        catch (Exception e)
                        {
                            packetLines.Clear();
                        }
                    }
                }

                UpdateProgress(100);

                UpdateProgress(0);


                AddToMonitor("Finished reading file.");

                try
                {
                    ReadFileThread.Abort();
                    ReadFileThread = null;
                }
                catch (Exception ex)
                {
                    ReadFileThread = null;
                }
            }

            mapControl.CanDragMap = true;
        }

        //Textbox values
        private void PlotValue(DataPack dp)
        {
            if (this.InvokeRequired)
            {
                // Make arguments for the delegate.
                object[] args = new object[] { dp };

                // Make the delegate.
                PlotValueDelegate plot_value_delegate = PlotValue;

                // Invoke the delegate on the main UI thread.
                this.Invoke(plot_value_delegate, args);

                // We're done.
                return;
            }

            
        }*/
    }
}