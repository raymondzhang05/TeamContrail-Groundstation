using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using MessageBox = System.Windows.Forms.MessageBox;

namespace SpaceLane
{
    public partial class formMain_V2 : Form
    {
        #region Variables Declarations 

        public static SerialPort serialPort = new SerialPort();
        static bool serialConnectionState = false;

        private Thread ReadFileThread;
        private String fileName;

        public string txt;
        delegate void SetTextCallback(string text);

        public static List<string> serialPacketLines = new List<string>();
        public static DataPack serialDp = null;


        private const float CZoomScale = 2f;
        private int FZoomLevel = 0;

        List<DataPack> dpList = new List<DataPack>();

        #endregion

        #region Delegates

        // Define a delegate type that takes two int parameters.
        private delegate void PlotValueDelegate(DataPack dp);

        // Define a delegate type that takes two int parameters.
        private delegate void PlotMapDelegate(double lat, double longitude);

        // Define a delegate type that takes a string parameter.
        private delegate void AddToMonitorDelegate(string txt);

        // Define a delegate type that takes a string parameter.
        private delegate void UpdateProgressDelegate(int value);

        private delegate void DrawHeatmapDelegate(string txt);


        #endregion

        public formMain_V2()
        {
            InitializeComponent();

            chart_Velocity.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart_Velocity.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
            chart_Velocity.MouseWheel += chart_Velocity_MouseWheel;


            chart_Pressure.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart_Pressure.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
            chart_Pressure.MouseWheel += chart_Pressure_MouseWheel;

            chart_Altitude.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart_Altitude.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
            chart_Altitude.MouseWheel += chart_Altitude_MouseWheel;

            chart_Temperature.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart_Temperature.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
            chart_Temperature.MouseWheel += chart_Temperature_MouseWheel;

            chart_Humidity.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart_Humidity.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
            chart_Humidity.MouseWheel += chart_Humidity_MouseWheel;

            chart_Gas.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart_Gas.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
            chart_Gas.MouseWheel += chart_Gas_MouseWheel;

            chart_CO2.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart_CO2.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
            chart_CO2.MouseWheel += chart_CO2_MouseWheel;

            chart_TVOC.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart_TVOC.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
            chart_TVOC.MouseWheel += chart_TVOC_MouseWheel;

            chart_CO.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart_CO.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
            chart_CO.MouseWheel += chart_CO_MouseWheel;

            try
            {
                System.Net.IPHostEntry e = System.Net.Dns.GetHostEntry("www.google.ca");
            }
            catch
            {
                mapControl.Manager.Mode = AccessMode.CacheOnly;
                //MessageBox.Show("No internet connection avaible, going to CacheOnly mode.", "GMap.NET Demo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            mapControl.MapProvider = GMapProviders.GoogleSatelliteMap; //google satellite map 
            mapControl.MinZoom = 2;  // The minimum zoom 
            mapControl.MaxZoom = 15; // The largest scale 
            mapControl.Zoom = 5;     // The current zoom 
            mapControl.ShowCenter = false; // Do not show center 10 Word point 
            mapControl.DragButton = MouseButtons.Left; // Left-click to drag the map 
            //mapControl.Position = new PointLatLng(32.064, 118.704); // Map center location: Nanjing 
            mapControl.OnMapZoomChanged += new MapZoomChanged(mapControl_OnMapZoomChanged);
            // mapControl.MouseLeftButtonDown += new MouseButtonEventHandler(mapControl_MouseLeftButtonDown);
        }

        private void formMain_V2_Shown(object sender, EventArgs e)
        {
            string message = "This software is used with Team Contrail's CanSat to aid firefighters during wildfires. To analyze a .txt file, click open file and select the file from the finder. To connect to the live data from the satellite, select a COM port and Baud rate, then click connect.";
            string title = "Welcome to Team Contrail Groundstation Software";
            MessageBoxButtons buttons = MessageBoxButtons.OK;
            DialogResult result = MessageBox.Show(message, title, buttons, MessageBoxIcon.Warning);
        }

        #region UI Event Handler
        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                fileName = openFileDialog.FileName;

                List<DataPack> DackPackList = new List<DataPack>();

                if (fileName != null)
                {

                    //reset the status
                    if (ReadFileThread != null)
                    {
                        ReadFileThread.Abort();
                        ReadFileThread = null;

                        AddToMonitor("Thread stopped");
                    }

                    resetScreen();


                    toolStripStatusLabel.Text = fileName;

                    if (ReadFileThread == null)
                    {
                        ReadFileThread = new Thread(ReadFile);
                        ReadFileThread.Priority = ThreadPriority.Normal;
                        ReadFileThread.IsBackground = true;
                        ReadFileThread.Start();

                        AddToMonitor("started reading file");
                        //btnGraph.Text = "Stop";
                    }

                }
            }
        }

        private void buttonLoadPosition_Click(object sender, EventArgs e)
        {
            try
            {
                mapControl.MapProvider = GMapProviders.GoogleSatelliteMap;
                mapControl.Position = new PointLatLng(Convert.ToDouble(txtLatitude.Text), Convert.ToDouble(txtLongitude.Text));
                mapControl.MinZoom = 2;
                mapControl.MaxZoom = 15;
                mapControl.ShowCenter = false;
                mapControl.Zoom = 5;
                mapControl.DragButton = MouseButtons.Left;

                mapControl.Overlays.Clear();
                PlotMap(Convert.ToDouble(txtLatitude.Text), Convert.ToDouble(txtLongitude.Text));
                GMapOverlay markersOverlay = new GMapOverlay("markers");
                //markersOverlay.Markers.Clear();
                GMarkerGoogle marker = new GMarkerGoogle(new PointLatLng(Convert.ToDouble(txtLatitude.Text), Convert.ToDouble(txtLongitude.Text)), GMarkerGoogleType.green);
                markersOverlay.Markers.Add(marker);
                mapControl.Overlays.Add(markersOverlay);
            }
            catch (Exception ex)
            {

            }
        }

        private void cbPortSelect_DropDown(object sender, EventArgs e)
        {
            SearchAvailableComPort();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (serialConnectionState)
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                }

                btnConnect.Text = "Connect";

                serialConnectionState = false;

                toolStripStatusLabel.Text = "Serial port disconnected";
            }
            else
            {
                try
                {
                    serialPort.PortName = cbPortSelect.Text;
                    serialPort.BaudRate = Convert.ToInt32(tbBaudRate.Text);

                    // Attach a method to be called when there
                    // is data waiting in the port's buffer
                    serialPort.DataReceived += new
                       SerialDataReceivedEventHandler(port_DataReceived);

                    // Begin communications
                    serialPort.Open();

                    btnConnect.Text = "Disconnect";
                    toolStripStatusLabel.Text = "Serial port connected";
                    serialConnectionState = true;
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void chart_Velocity_MouseEnter(object sender, EventArgs e)
        {
            if (!chart_Velocity.Focused)
                chart_Velocity.Focus();
        }

        private void chart_Velocity_MouseLeave(object sender, EventArgs e)
        {
            if (chart_Velocity.Focused)
                chart_Velocity.Parent.Focus();
        }

        private void chart_Velocity_MouseWheel(object sender, MouseEventArgs e)
        {
            try
            {
                Axis xAxis = chart_Velocity.ChartAreas[0].AxisX;
                double xMin = xAxis.ScaleView.ViewMinimum;
                double xMax = xAxis.ScaleView.ViewMaximum;
                double xPixelPos = xAxis.PixelPositionToValue(e.Location.X);

                if (e.Delta < 0 && FZoomLevel > 0)
                {
                    // Scrolled down, meaning zoom out
                    if (--FZoomLevel <= 0)
                    {
                        FZoomLevel = 0;
                        xAxis.ScaleView.ZoomReset();
                    }
                    else
                    {
                        double xStartPos = Math.Max(xPixelPos - (xPixelPos - xMin) * CZoomScale, 0);
                        double xEndPos = Math.Min(xStartPos + (xMax - xMin) * CZoomScale, xAxis.Maximum);
                        xAxis.ScaleView.Zoom(xStartPos, xEndPos);
                    }
                }
                else if (e.Delta > 0)
                {
                    // Scrolled up, meaning zoom in
                    double xStartPos = Math.Max(xPixelPos - (xPixelPos - xMin) / CZoomScale, 0);
                    double xEndPos = Math.Min(xStartPos + (xMax - xMin) / CZoomScale, xAxis.Maximum);
                    xAxis.ScaleView.Zoom(xStartPos, xEndPos);
                    FZoomLevel++;
                }
            }
            catch { }
        }

        private void chart_Gas_MouseEnter(object sender, EventArgs e)
        {
            if (!chart_Gas.Focused)
                chart_Gas.Focus();
        }

        private void chart_Gas_MouseLeave(object sender, EventArgs e)
        {
            if (chart_Gas.Focused)
                chart_Gas.Parent.Focus();
        }

        private void chart_Gas_MouseWheel(object sender, MouseEventArgs e)
        {
            try
            {
                Axis xAxis = chart_Gas.ChartAreas[0].AxisX;
                double xMin = xAxis.ScaleView.ViewMinimum;
                double xMax = xAxis.ScaleView.ViewMaximum;
                double xPixelPos = xAxis.PixelPositionToValue(e.Location.X);

                if (e.Delta < 0 && FZoomLevel > 0)
                {
                    // Scrolled down, meaning zoom out
                    if (--FZoomLevel <= 0)
                    {
                        FZoomLevel = 0;
                        xAxis.ScaleView.ZoomReset();
                    }
                    else
                    {
                        double xStartPos = Math.Max(xPixelPos - (xPixelPos - xMin) * CZoomScale, 0);
                        double xEndPos = Math.Min(xStartPos + (xMax - xMin) * CZoomScale, xAxis.Maximum);
                        xAxis.ScaleView.Zoom(xStartPos, xEndPos);
                    }
                }
                else if (e.Delta > 0)
                {
                    // Scrolled up, meaning zoom in
                    double xStartPos = Math.Max(xPixelPos - (xPixelPos - xMin) / CZoomScale, 0);
                    double xEndPos = Math.Min(xStartPos + (xMax - xMin) / CZoomScale, xAxis.Maximum);
                    xAxis.ScaleView.Zoom(xStartPos, xEndPos);
                    FZoomLevel++;
                }
            }
            catch { }
        }


        private void chart_Humidity_MouseEnter(object sender, EventArgs e)
        {
            if (!chart_Humidity.Focused)
                chart_Humidity.Focus();
        }

        private void chart_Humidity_MouseLeave(object sender, EventArgs e)
        {
            if (chart_Humidity.Focused)
                chart_Humidity.Parent.Focus();
        }

        private void chart_Humidity_MouseWheel(object sender, MouseEventArgs e)
        {
            try
            {
                Axis xAxis = chart_Humidity.ChartAreas[0].AxisX;
                double xMin = xAxis.ScaleView.ViewMinimum;
                double xMax = xAxis.ScaleView.ViewMaximum;
                double xPixelPos = xAxis.PixelPositionToValue(e.Location.X);

                if (e.Delta < 0 && FZoomLevel > 0)
                {
                    // Scrolled down, meaning zoom out
                    if (--FZoomLevel <= 0)
                    {
                        FZoomLevel = 0;
                        xAxis.ScaleView.ZoomReset();
                    }
                    else
                    {
                        double xStartPos = Math.Max(xPixelPos - (xPixelPos - xMin) * CZoomScale, 0);
                        double xEndPos = Math.Min(xStartPos + (xMax - xMin) * CZoomScale, xAxis.Maximum);
                        xAxis.ScaleView.Zoom(xStartPos, xEndPos);
                    }
                }
                else if (e.Delta > 0)
                {
                    // Scrolled up, meaning zoom in
                    double xStartPos = Math.Max(xPixelPos - (xPixelPos - xMin) / CZoomScale, 0);
                    double xEndPos = Math.Min(xStartPos + (xMax - xMin) / CZoomScale, xAxis.Maximum);
                    xAxis.ScaleView.Zoom(xStartPos, xEndPos);
                    FZoomLevel++;
                }
            }
            catch { }
        }

        private void chart_Altitude_MouseEnter(object sender, EventArgs e)
        {
            if (!chart_Altitude.Focused)
                chart_Altitude.Focus();
        }

        private void chart_Altitude_MouseLeave(object sender, EventArgs e)
        {
            if (chart_Altitude.Focused)
                chart_Altitude.Parent.Focus();
        }

        private void chart_Altitude_MouseWheel(object sender, MouseEventArgs e)
        {
            try
            {
                Axis xAxis = chart_Altitude.ChartAreas[0].AxisX;
                double xMin = xAxis.ScaleView.ViewMinimum;
                double xMax = xAxis.ScaleView.ViewMaximum;
                double xPixelPos = xAxis.PixelPositionToValue(e.Location.X);

                if (e.Delta < 0 && FZoomLevel > 0)
                {
                    // Scrolled down, meaning zoom out
                    if (--FZoomLevel <= 0)
                    {
                        FZoomLevel = 0;
                        xAxis.ScaleView.ZoomReset();
                    }
                    else
                    {
                        double xStartPos = Math.Max(xPixelPos - (xPixelPos - xMin) * CZoomScale, 0);
                        double xEndPos = Math.Min(xStartPos + (xMax - xMin) * CZoomScale, xAxis.Maximum);
                        xAxis.ScaleView.Zoom(xStartPos, xEndPos);
                    }
                }
                else if (e.Delta > 0)
                {
                    // Scrolled up, meaning zoom in
                    double xStartPos = Math.Max(xPixelPos - (xPixelPos - xMin) / CZoomScale, 0);
                    double xEndPos = Math.Min(xStartPos + (xMax - xMin) / CZoomScale, xAxis.Maximum);
                    xAxis.ScaleView.Zoom(xStartPos, xEndPos);
                    FZoomLevel++;
                }
            }
            catch { }
        }

        private void chart_Pressure_MouseEnter(object sender, EventArgs e)
        {
            if (!chart_Pressure.Focused)
                chart_Pressure.Focus();
        }

        private void chart_Pressure_MouseLeave(object sender, EventArgs e)
        {
            if (chart_Pressure.Focused)
                chart_Pressure.Parent.Focus();
        }

        private void chart_Pressure_MouseWheel(object sender, MouseEventArgs e)
        {
            try
            {
                Axis xAxis = chart_Pressure.ChartAreas[0].AxisX;
                double xMin = xAxis.ScaleView.ViewMinimum;
                double xMax = xAxis.ScaleView.ViewMaximum;
                double xPixelPos = xAxis.PixelPositionToValue(e.Location.X);

                if (e.Delta < 0 && FZoomLevel > 0)
                {
                    // Scrolled down, meaning zoom out
                    if (--FZoomLevel <= 0)
                    {
                        FZoomLevel = 0;
                        xAxis.ScaleView.ZoomReset();
                    }
                    else
                    {
                        double xStartPos = Math.Max(xPixelPos - (xPixelPos - xMin) * CZoomScale, 0);
                        double xEndPos = Math.Min(xStartPos + (xMax - xMin) * CZoomScale, xAxis.Maximum);
                        xAxis.ScaleView.Zoom(xStartPos, xEndPos);
                    }
                }
                else if (e.Delta > 0)
                {
                    // Scrolled up, meaning zoom in
                    double xStartPos = Math.Max(xPixelPos - (xPixelPos - xMin) / CZoomScale, 0);
                    double xEndPos = Math.Min(xStartPos + (xMax - xMin) / CZoomScale, xAxis.Maximum);
                    xAxis.ScaleView.Zoom(xStartPos, xEndPos);
                    FZoomLevel++;
                }
            }
            catch { }
        }

        private void chart_Temperature_MouseEnter(object sender, EventArgs e)
        {
            if (!chart_Temperature.Focused)
                chart_Temperature.Focus();
        }

        private void chart_Temperature_MouseLeave(object sender, EventArgs e)
        {
            if (chart_Temperature.Focused)
                chart_Temperature.Parent.Focus();
        }

        private void chart_Temperature_MouseWheel(object sender, MouseEventArgs e)
        {
            try
            {
                Axis xAxis = chart_Temperature.ChartAreas[0].AxisX;
                double xMin = xAxis.ScaleView.ViewMinimum;
                double xMax = xAxis.ScaleView.ViewMaximum;
                double xPixelPos = xAxis.PixelPositionToValue(e.Location.X);

                if (e.Delta < 0 && FZoomLevel > 0)
                {
                    // Scrolled down, meaning zoom out
                    if (--FZoomLevel <= 0)
                    {
                        FZoomLevel = 0;
                        xAxis.ScaleView.ZoomReset();
                    }
                    else
                    {
                        double xStartPos = Math.Max(xPixelPos - (xPixelPos - xMin) * CZoomScale, 0);
                        double xEndPos = Math.Min(xStartPos + (xMax - xMin) * CZoomScale, xAxis.Maximum);
                        xAxis.ScaleView.Zoom(xStartPos, xEndPos);
                    }
                }
                else if (e.Delta > 0)
                {
                    // Scrolled up, meaning zoom in
                    double xStartPos = Math.Max(xPixelPos - (xPixelPos - xMin) / CZoomScale, 0);
                    double xEndPos = Math.Min(xStartPos + (xMax - xMin) / CZoomScale, xAxis.Maximum);
                    xAxis.ScaleView.Zoom(xStartPos, xEndPos);
                    FZoomLevel++;
                }
            }
            catch { }
        }

        private void chart_CO2_MouseEnter(object sender, EventArgs e)
        {
            if (!chart_CO2.Focused)
                chart_CO2.Focus();
        }

        private void chart_CO2_MouseLeave(object sender, EventArgs e)
        {
            if (chart_CO2.Focused)
                chart_CO2.Parent.Focus();
        }

        private void chart_CO2_MouseWheel(object sender, MouseEventArgs e)
        {
            try
            {
                Axis xAxis = chart_CO2.ChartAreas[0].AxisX;
                double xMin = xAxis.ScaleView.ViewMinimum;
                double xMax = xAxis.ScaleView.ViewMaximum;
                double xPixelPos = xAxis.PixelPositionToValue(e.Location.X);

                if (e.Delta < 0 && FZoomLevel > 0)
                {
                    // Scrolled down, meaning zoom out
                    if (--FZoomLevel <= 0)
                    {
                        FZoomLevel = 0;
                        xAxis.ScaleView.ZoomReset();
                    }
                    else
                    {
                        double xStartPos = Math.Max(xPixelPos - (xPixelPos - xMin) * CZoomScale, 0);
                        double xEndPos = Math.Min(xStartPos + (xMax - xMin) * CZoomScale, xAxis.Maximum);
                        xAxis.ScaleView.Zoom(xStartPos, xEndPos);
                    }
                }
                else if (e.Delta > 0)
                {
                    // Scrolled up, meaning zoom in
                    double xStartPos = Math.Max(xPixelPos - (xPixelPos - xMin) / CZoomScale, 0);
                    double xEndPos = Math.Min(xStartPos + (xMax - xMin) / CZoomScale, xAxis.Maximum);
                    xAxis.ScaleView.Zoom(xStartPos, xEndPos);
                    FZoomLevel++;
                }
            }
            catch { }
        }

        private void chart_TVOC_MouseEnter(object sender, EventArgs e)
        {
            if (!chart_TVOC.Focused)
                chart_TVOC.Focus();
        }

        private void chart_TVOC_MouseLeave(object sender, EventArgs e)
        {
            if (chart_TVOC.Focused)
                chart_TVOC.Parent.Focus();
        }

        private void chart_TVOC_MouseWheel(object sender, MouseEventArgs e)
        {
            try
            {
                Axis xAxis = chart_TVOC.ChartAreas[0].AxisX;
                double xMin = xAxis.ScaleView.ViewMinimum;
                double xMax = xAxis.ScaleView.ViewMaximum;
                double xPixelPos = xAxis.PixelPositionToValue(e.Location.X);

                if (e.Delta < 0 && FZoomLevel > 0)
                {
                    // Scrolled down, meaning zoom out
                    if (--FZoomLevel <= 0)
                    {
                        FZoomLevel = 0;
                        xAxis.ScaleView.ZoomReset();
                    }
                    else
                    {
                        double xStartPos = Math.Max(xPixelPos - (xPixelPos - xMin) * CZoomScale, 0);
                        double xEndPos = Math.Min(xStartPos + (xMax - xMin) * CZoomScale, xAxis.Maximum);
                        xAxis.ScaleView.Zoom(xStartPos, xEndPos);
                    }
                }
                else if (e.Delta > 0)
                {
                    // Scrolled up, meaning zoom in
                    double xStartPos = Math.Max(xPixelPos - (xPixelPos - xMin) / CZoomScale, 0);
                    double xEndPos = Math.Min(xStartPos + (xMax - xMin) / CZoomScale, xAxis.Maximum);
                    xAxis.ScaleView.Zoom(xStartPos, xEndPos);
                    FZoomLevel++;
                }
            }
            catch { }
        }

        private void chart_CO_MouseEnter(object sender, EventArgs e)
        {
            if (!chart_CO.Focused)
                chart_CO.Focus();
        }

        private void chart_CO_MouseLeave(object sender, EventArgs e)
        {
            if (chart_CO.Focused)
                chart_CO.Parent.Focus();
        }

        private void chart_CO_MouseWheel(object sender, MouseEventArgs e)
        {
            try
            {
                Axis xAxis = chart_CO.ChartAreas[0].AxisX;
                double xMin = xAxis.ScaleView.ViewMinimum;
                double xMax = xAxis.ScaleView.ViewMaximum;
                double xPixelPos = xAxis.PixelPositionToValue(e.Location.X);

                if (e.Delta < 0 && FZoomLevel > 0)
                {
                    // Scrolled down, meaning zoom out
                    if (--FZoomLevel <= 0)
                    {
                        FZoomLevel = 0;
                        xAxis.ScaleView.ZoomReset();
                    }
                    else
                    {
                        double xStartPos = Math.Max(xPixelPos - (xPixelPos - xMin) * CZoomScale, 0);
                        double xEndPos = Math.Min(xStartPos + (xMax - xMin) * CZoomScale, xAxis.Maximum);
                        xAxis.ScaleView.Zoom(xStartPos, xEndPos);
                    }
                }
                else if (e.Delta > 0)
                {
                    // Scrolled up, meaning zoom in
                    double xStartPos = Math.Max(xPixelPos - (xPixelPos - xMin) / CZoomScale, 0);
                    double xEndPos = Math.Min(xStartPos + (xMax - xMin) / CZoomScale, xAxis.Maximum);
                    xAxis.ScaleView.Zoom(xStartPos, xEndPos);
                    FZoomLevel++;
                }
            }
            catch { }
        }

        private void chart_Acceleraion_Net_MouseEnter(object sender, EventArgs e)
        {
            if (!chart_Velocity.Focused)
                chart_Velocity.Focus();
        }

        private void chart_Acceleration_Net_MouseLeave(object sender, EventArgs e)
        {
            if (chart_Velocity.Focused)
                chart_Velocity.Parent.Focus();
        }

        private void chart_Acceleration_Net_MouseWheel(object sender, MouseEventArgs e)
        {
            try
            {

            }
            catch { }
        }

        private void btnClearGraph_Click(object sender, EventArgs e)
        {
            resetScreen();
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            richTextBoxSerialMonitor.Clear();
        }

        private void Checkbox_LockGPSMap_CheckedChanged(object sender, EventArgs e)
        {
            if (!Checkbox_LockGPSMap.Checked)
            {
                mapControl.DragButton = MouseButtons.Left;
            }
            else
            {
                mapControl.DragButton = MouseButtons.None;
            }
        }

        #endregion



        #region Helper Methods

        private void SearchAvailableComPort()
        {
            cbPortSelect.Text = "";
            cbPortSelect.Items.Clear();

            foreach (string _string in SerialPort.GetPortNames())
            {
                cbPortSelect.Items.Add(_string);
                cbPortSelect.Text = _string;
            }
        }


        private void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (richTextBoxSerialMonitor.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.richTextBoxSerialMonitor.AppendText(text);
            }
        }


        List<string> portPacketLines = new List<string>();


        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            mapControl.CanDragMap = false;

            txt = serialPort.ReadLine().ToString();
            SetText(txt.ToString() + "\n\r");

            try
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

                    //show the location on the map
                    PlotMap(dp.Latitude, dp.Longitude);

                    //Generate the Heatmap if dp.HeatValues is not empty
                    if (!string.IsNullOrEmpty(dp.HeatValues))
                        DrawHeatmap(dp.HeatValues);

                    dpList.Add(dp);

                    portPacketLines.Clear();
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

        double lastLatitude, LastLongitude;

        private void PlotMap(double Latitude, double Longitude)
        {
            if (Latitude != lastLatitude && Longitude != LastLongitude)
            {
                // See if we're on the worker thread and thus
                // need to invoke the main UI thread.
                if (this.InvokeRequired)
                {
                    // Make arguments for the delegate.
                    object[] args = new object[] { Latitude, Longitude };

                    // Make the delegate.
                    PlotMapDelegate plot_map_delegate = PlotMap;

                    // Invoke the delegate on the main UI thread.
                    this.Invoke(plot_map_delegate, args);

                    // We're done.
                    return;
                }

                lastLatitude = Latitude;
                LastLongitude = Longitude;

                //show the location on the map
                mapControl.Position = new PointLatLng(Latitude, Longitude);

                GMapOverlay markersOverlay = new GMapOverlay("markers");
                GMarkerGoogle marker = new GMarkerGoogle(new PointLatLng(Latitude, Longitude), GMarkerGoogleType.green);
                markersOverlay.Markers.Add(marker);
                mapControl.Overlays.Add(markersOverlay);

                mapControl.Refresh();
                Thread.Sleep(50);
            }
        }

        // Plot a new value.
        private void PlotValue(DataPack dp)
        {
            // See if we're on the worker thread and thus
            // need to invoke the main UI thread.
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

            if (toolStripProgressBar1.Value == 100)
                toolStripProgressBar1.Value = 0;

            toolStripProgressBar1.Value = toolStripProgressBar1.Value + 1;

            //process monitor stuff
            tbTime.Text = String.Format("{0:0.##}", dp.TimeInMills / 1000);
            tbBMETemperature.Text = dp.Temperature.ToString();
            tbBMEPressure.Text = dp.Pressure.ToString();
            tbBMEHumidity.Text = dp.Humidity.ToString();
            tbBMEVOCGas.Text = dp.GasResistance.ToString();
            tbAltitude.Text = dp.Altitude.ToString();
            tbVelocity.Text = String.Format("{0:0.##}", dp.Velocity);
            tbLattitude.Text = dp.Latitude.ToString();
            tbLongitude.Text = dp.Longitude.ToString();
            tbCO2.Text = dp.CO2.ToString();
            tbTVOC.Text = dp.TVOC.ToString();
            tbCO.Text = dp.CO.ToString();





            /*
            tbUVA.Text = dp.Uva.ToString();
            tbUVB.Text = dp.Uvb.ToString();
            tbUVIndex.Text = dp.UvIndex.ToString();
            tbAccelerationX.Text = dp.AccelerationX.ToString();
            tbAccelerationY.Text = dp.AccelerationY.ToString();
            tbAccelerationZ.Text = dp.AccelerationZ.ToString();
            tbGyroX.Text = dp.GyroX.ToString();
            tbGyroY.Text = dp.GyroY.ToString();
            tbGyroZ.Text = dp.GyroZ.ToString();
            tbMagnetometerX.Text = dp.MagnetoX.ToString();
            tbMagnetometerY.Text = dp.MagnetoY.ToString();
            tbMagnetometerZ.Text = dp.MagnetoZ.ToString();
            */




            //Pressure Chart           
            try
            {
                Series PressureSerie = chart_Pressure.Series[0];
                PressureSerie.Points.AddXY(Math.Round(dp.TimeInMills / 1000, 2), dp.Pressure);

                if (dp.Pressure > Constants.PressureMax)
                    Constants.PressureMax = (float)(dp.Pressure * 1.5);

                if (dp.Pressure < Constants.PressureMin)
                    Constants.PressureMin = (float)(dp.Pressure * 0.5);


                chart_Pressure.ChartAreas[0].AxisY.Maximum = Constants.PressureMax;
                chart_Pressure.ChartAreas[0].AxisY.Minimum = Constants.PressureMin;

                chart_Pressure.Update();
            }
            catch
            {

            }


            //chart_Altitude 
            try
            {
                Series AltitudeSerie = chart_Altitude.Series[0];
                AltitudeSerie.Points.AddXY(Math.Round(dp.TimeInMills / 1000, 2), dp.Altitude);

                if (dp.Altitude > Constants.AltitudeMax)
                    Constants.AltitudeMax = (float)(dp.Altitude * 1.5);

                if (dp.Altitude < Constants.AltitudeMin)
                    Constants.AltitudeMin = (float)(dp.Altitude * 0.5);

                chart_Altitude.ChartAreas[0].AxisY.Maximum = Constants.AltitudeMax;
                chart_Altitude.ChartAreas[0].AxisY.Minimum = Constants.AltitudeMin;

                chart_Altitude.Update();
            }
            catch
            {

            }

            //chart_Temperature 
            try
            {
                Series TemperatureSerie = chart_Temperature.Series[0];
                TemperatureSerie.Points.AddXY(Math.Round(dp.TimeInMills / 1000, 2), dp.Temperature);

                if (dp.Temperature > Constants.TemperatureMax)
                    Constants.TemperatureMax = (float)(dp.Temperature * 1.5);

                if (dp.Temperature < Constants.TemperatureMin)
                    Constants.TemperatureMin = (float)(dp.Temperature * 0.5);

                chart_Temperature.ChartAreas[0].AxisY.Maximum = Constants.TemperatureMax;
                chart_Temperature.ChartAreas[0].AxisY.Minimum = Constants.TemperatureMin;

                chart_Temperature.Update();
            }
            catch
            {

            }

            //chart_Humidity
            try
            {
                Series HumiditySerie = chart_Humidity.Series[0];
                HumiditySerie.Points.AddXY(Math.Round(dp.TimeInMills / 1000, 2), dp.Humidity);

                if (dp.Humidity > Constants.HumidityMax)
                    Constants.HumidityMax = (float)(dp.Humidity * 1.5);

                if (dp.Humidity < Constants.HumidityMin)
                    Constants.HumidityMin = (float)(dp.Humidity * 0.5);

                chart_Humidity.ChartAreas[0].AxisY.Maximum = Constants.HumidityMax;
                chart_Humidity.ChartAreas[0].AxisY.Minimum = Constants.HumidityMin;

                chart_Humidity.Update();
            }
            catch
            {

            }

            //chart_Gas
            try
            {
                Series GasSerie = chart_Gas.Series[0];
                GasSerie.Points.AddXY(Math.Round(dp.TimeInMills / 1000, 2), dp.GasResistance);

                if (dp.GasResistance > Constants.GasMax)
                    Constants.GasMax = (float)(dp.GasResistance * 1.5);

                if (dp.GasResistance < Constants.GasMin)
                    Constants.GasMin = (float)(dp.GasResistance * 0.5);

                chart_Gas.ChartAreas[0].AxisY.Maximum = Constants.GasMax;
                chart_Gas.ChartAreas[0].AxisY.Minimum = Constants.GasMin;

                chart_Gas.Update();
            }
            catch
            {

            }

            //chart_CO2
            try
            {
                Series CO2Serie = chart_CO2.Series[0];
                CO2Serie.Points.AddXY(Math.Round(dp.TimeInMills / 1000, 2), dp.CO2);

                if (dp.CO2 > Constants.CO2Max)
                    Constants.CO2Max = (float)(dp.CO2 * 1.5);

                if (dp.CO2 < Constants.GasMin)
                    Constants.GasMin = (float)(dp.CO2 * 0.5);

                chart_CO2.ChartAreas[0].AxisY.Maximum = Constants.CO2Max;
                chart_CO2.ChartAreas[0].AxisY.Minimum = Constants.CO2Min;

                chart_CO2.Update();
            }
            catch
            {

            }

            //chart_TVOC
            try
            {
                Series TVOCSerie = chart_TVOC.Series[0];
                TVOCSerie.Points.AddXY(Math.Round(dp.TimeInMills / 1000, 2), dp.TVOC);

                if (dp.TVOC > Constants.TVOCMax)
                    Constants.TVOCMax = (float)(dp.TVOC * 1.5);

                if (dp.TVOC < Constants.TVOCMin)
                    Constants.TVOCMin = (float)(dp.TVOC * 0.5);

                chart_TVOC.ChartAreas[0].AxisY.Maximum = Constants.TVOCMax;
                chart_TVOC.ChartAreas[0].AxisY.Minimum = Constants.TVOCMin;

                chart_TVOC.Update();
            }
            catch
            {

            }

            //chart_CO
            try
            {
                Series COSerie = chart_CO.Series[0];
                COSerie.Points.AddXY(Math.Round(dp.TimeInMills / 1000, 2), dp.CO);

                if (dp.CO > Constants.COMax)
                    Constants.COMax = (float)(dp.CO * 1.5);

                if (dp.CO < Constants.COMin)
                    Constants.COMin = (float)(dp.CO * 0.5);

                chart_CO.ChartAreas[0].AxisY.Maximum = Constants.COMax;
                chart_CO.ChartAreas[0].AxisY.Minimum = Constants.COMin;

                chart_CO.Update();
            }
            catch
            {

            }

            //chart_Velocity
            try
            {
                Series VelocitySerie = chart_Velocity.Series[0];
                VelocitySerie.Points.AddXY(Math.Round(dp.TimeInMills / 1000, 2), dp.Velocity);

                if (dp.Velocity >= Constants.VelocityMax)
                    Constants.VelocityMax = (float)(dp.Velocity * 1.5);

                if (dp.Velocity < Constants.VelocityMin)
                {
                    if (dp.Velocity < 0)
                    {
                        Constants.VelocityMin = (float)(dp.Velocity * 1.1);
                    }
                    else
                    {
                        Constants.VelocityMin = (float)(dp.Velocity * 0.9);
                    }
                }

                chart_Velocity.ChartAreas[0].AxisY.Maximum = Constants.VelocityMax;
                chart_Velocity.ChartAreas[0].AxisY.Minimum = Constants.VelocityMin;

                chart_Velocity.Update();
            }
            catch
            {

            }
        }


        private static Color HeatMapColor(decimal value, decimal min, decimal max)
        {
            float correctionFactor = (float)((value - min) / (max - min));

            List<Color> baseColors = new List<Color>();  // create a color list

            baseColors.Add(Color.Cyan);
            baseColors.Add(Color.Blue);
            baseColors.Add(Color.DarkBlue);
            baseColors.Add(Color.Orange);
            baseColors.Add(Color.Red);
            baseColors.Add(Color.DarkRed);
            List<Color> colors = interpolateColors(baseColors, 200);

            return colors[Convert.ToInt16(Math.Abs(correctionFactor * 200 - 1))];
        }

        public static List<Color> interpolateColors(List<Color> stopColors, int count)
        {
            SortedDictionary<float, Color> gradient = new SortedDictionary<float, Color>();
            for (int i = 0; i < stopColors.Count; i++)
                gradient.Add(1f * i / (stopColors.Count - 1), stopColors[i]);
            List<Color> ColorList = new List<Color>();

            using (Bitmap bmp = new Bitmap(count, 1))
            using (Graphics G = Graphics.FromImage(bmp))
            {
                Rectangle bmpCRect = new Rectangle(Point.Empty, bmp.Size);
                LinearGradientBrush br = new LinearGradientBrush
                                        (bmpCRect, Color.Empty, Color.Empty, 0, false);
                ColorBlend cb = new ColorBlend();
                cb.Positions = new float[gradient.Count];
                for (int i = 0; i < gradient.Count; i++)
                    cb.Positions[i] = gradient.ElementAt(i).Key;
                cb.Colors = gradient.Values.ToArray();
                br.InterpolationColors = cb;
                G.FillRectangle(br, bmpCRect);
                for (int i = 0; i < count; i++) ColorList.Add(bmp.GetPixel(i, 0));
                br.Dispose();
            }
            return ColorList;
        }

        private static Bitmap CreateImage(string strValue)
        {
            int width = 32;
            int height = 24;

            Bitmap bmp = new Bitmap(width, height);

            if (strValue.EndsWith(","))
                strValue = strValue.Substring(0, strValue.Length - 1);

            string[] tempArr = strValue.Split(",".ToCharArray());

            decimal d;
            decimal[] valueArr;

            if (tempArr.All(number => Decimal.TryParse(number, out d)))
            {
                valueArr = Array.ConvertAll<string, decimal>(tempArr, Convert.ToDecimal);

                decimal maxValue = valueArr.Max();
                decimal minValue = valueArr.Min();

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int i = ((y * width) + x);

                        float correctionFactor = (float)((valueArr[i] - minValue) / (maxValue - minValue));
                        int color = Decimal.ToInt16((decimal)(correctionFactor * 255));

                        bmp.SetPixel(x, y, HeatMapColor(valueArr[i], minValue, maxValue));
                    }
                }

                //adjust brightness/contrast etc.
                Bitmap originalImage = bmp;
                Bitmap adjustedImage = bmp;
                float brightness = 1.01f;
                float contrast = 1.1f;
                float gamma = 1.0f;

                float adjustedBrightness = brightness - 1.0f;
                float[][] ptsArray ={
                    new float[] {contrast, 0, 0, 0, 0}, // scale red
                    new float[] {0, contrast, 0, 0, 0}, // scale green
                    new float[] {0, 0, contrast, 0, 0}, // scale blue
                    new float[] {0, 0, 0, 1.0f, 0}, // don't scale alpha
                    new float[] {adjustedBrightness, adjustedBrightness, adjustedBrightness, 0, 1}};

                ImageAttributes imageAttributes = new ImageAttributes();
                imageAttributes.ClearColorMatrix();
                imageAttributes.SetColorMatrix(new ColorMatrix(ptsArray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                imageAttributes.SetGamma(gamma, ColorAdjustType.Bitmap);
                Graphics g = Graphics.FromImage(adjustedImage);
                g.DrawImage(originalImage, new Rectangle(0, 0, adjustedImage.Width, adjustedImage.Height)
                    , 0, 0, originalImage.Width, originalImage.Height,
                    GraphicsUnit.Pixel, imageAttributes);

                return adjustedImage;
            }

            return null;
        }

        private void DrawHeatmap(string heatmapValue)
        {
            // See if we're on the worker thread and thus
            // need to invoke the main UI thread.
            if (this.InvokeRequired)
            {
                // Make arguments for the delegate.
                object[] args = new object[] { heatmapValue };

                // Make the delegate.
                DrawHeatmapDelegate draw_heatmap_delegate = DrawHeatmap;

                // Invoke the delegate on the main UI thread.
                this.Invoke(draw_heatmap_delegate, args);

                // We're done.
                return;
            }

            if (toolStripProgressBar1.Value == 100)
                toolStripProgressBar1.Value = 0;

            toolStripProgressBar1.Value = toolStripProgressBar1.Value + 1;

            try
            {
                //draw heat map
                //Bitmap adjustedImage = CreateImage(heatmapValue);
                //pictureBox2.Image = adjustedImage;
                //pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
                //pictureBox2.Refresh();
            }
            catch (Exception e)
            {

            }
        }

        // Add a status string to txtStatus.
        private void AddToMonitor(string txt)
        {
            // See if we're on the worker thread and thus
            // need to invoke the main UI thread.
            if (this.InvokeRequired)
            {
                // Make arguments for the delegate.
                object[] args = new object[] { txt };

                // Make the delegate.
                AddToMonitorDelegate add_status_delegate = AddToMonitor;

                // Invoke the delegate on the main UI thread.
                this.Invoke(add_status_delegate, args);

                // We're done.
                return;
            }

            // No Invoke required. Just display the message.
            this.richTextBoxSerialMonitor.AppendText("\r\n" + txt);
            this.richTextBoxSerialMonitor.Select(this.richTextBoxSerialMonitor.Text.Length, 0);
            this.richTextBoxSerialMonitor.ScrollToCaret();
        }



        // Add a status string to txtStatus.
        private void UpdateProgress(int value)
        {
            // See if we're on the worker thread and thus
            // need to invoke the main UI thread.
            if (this.InvokeRequired)
            {
                // Make arguments for the delegate.
                object[] args = new object[] { value };

                // Make the delegate.
                UpdateProgressDelegate add_status_delegate = UpdateProgress;

                // Invoke the delegate on the main UI thread.
                this.Invoke(add_status_delegate, args);

                // We're done.
                return;
            }

            // No Invoke required. Just display the message.
            this.toolStripProgressBar1.Value = value;
        }

        private void resetScreen()
        {

            //reset all graph
            chart_Pressure.Series[0].Points.Clear();
            chart_Altitude.Series[0].Points.Clear();
            chart_Temperature.Series[0].Points.Clear();
            chart_Humidity.Series[0].Points.Clear();
            chart_Gas.Series[0].Points.Clear();
            chart_Velocity.Series[0].Points.Clear();
            chart_CO2.Series[0].Points.Clear();
            chart_TVOC.Series[0].Points.Clear();
            chart_CO.Series[0].Points.Clear();
        }

        #endregion

        private float GetDataPointValue(DataPack dp, String serialName)
        {
            switch (serialName)
            {
                case "Altitude":
                    return dp.Altitude;
                case "Velocity":
                    return dp.Velocity;
                case "Pressure":
                    return dp.Pressure;
                case "Temperature":
                    return dp.Temperature;
                case "Humidity":
                    return dp.Humidity;
                case "Gas":
                    return dp.GasResistance;
                case "CO2":
                    return dp.CO2;
                case "TVOC":
                    return dp.TVOC;
                case "CO":
                    return dp.CO;

                default: return 0f;
            }
        }

        private void SetChartAxisY2Scale(Chart sourceChart, String serialName)
        {
            switch (serialName)
            {
                case "Altitude":
                    sourceChart.ChartAreas[0].AxisY2.Maximum = Constants.AltitudeMax;
                    sourceChart.ChartAreas[0].AxisY2.Minimum = Constants.AltitudeMin;
                    break;

                case "Velocity":
                    sourceChart.ChartAreas[0].AxisY2.Maximum = Constants.VelocityMax;
                    sourceChart.ChartAreas[0].AxisY2.Minimum = Constants.VelocityMin;
                    break;

                case "Pressure":
                    sourceChart.ChartAreas[0].AxisY2.Maximum = Constants.PressureMax;
                    sourceChart.ChartAreas[0].AxisY2.Minimum = Constants.PressureMin;
                    break;

                case "Temperature":
                    sourceChart.ChartAreas[0].AxisY2.Maximum = Constants.TemperatureMax;
                    sourceChart.ChartAreas[0].AxisY2.Minimum = Constants.TemperatureMin;
                    break;

                case "Humidity":
                    sourceChart.ChartAreas[0].AxisY2.Maximum = Constants.HumidityMax;
                    sourceChart.ChartAreas[0].AxisY2.Minimum = Constants.HumidityMin;
                    break;

                case "Gas":
                    sourceChart.ChartAreas[0].AxisY2.Maximum = Constants.GasMax;
                    sourceChart.ChartAreas[0].AxisY2.Minimum = Constants.GasMin;
                    break;

                case "CO2":
                    sourceChart.ChartAreas[0].AxisY2.Maximum = Constants.CO2Max;
                    sourceChart.ChartAreas[0].AxisY2.Minimum = Constants.CO2Min;
                    break;

                case "TVOC":
                    sourceChart.ChartAreas[0].AxisY2.Maximum = Constants.TVOCMax;
                    sourceChart.ChartAreas[0].AxisY2.Minimum = Constants.TVOCMin;
                    break;

                case "CO":
                    sourceChart.ChartAreas[0].AxisY2.Maximum = Constants.COMax;
                    sourceChart.ChartAreas[0].AxisY2.Minimum = Constants.COMin;
                    break;

                case "Acceleration":
                    sourceChart.ChartAreas[0].AxisY2.Maximum = Constants.AccelerationMax;
                    sourceChart.ChartAreas[0].AxisY2.Minimum = Constants.AccelerationMin;
                    break;

            }
        }



        private Chart GetSourceChart(String selectedTab)
        {
            Chart sourceChart = null;

            if (selectedTab.Equals("Altitude"))
            {
                sourceChart = chart_Altitude;
            }
            else if (selectedTab.Equals("Velocity"))
            {
                sourceChart = chart_Velocity;
            }
            else if (selectedTab.Equals("Pressure"))
            {
                sourceChart = chart_Pressure;
            }
            else if (selectedTab.Equals("Temperature"))
            {
                sourceChart = chart_Temperature;
            }
            else if (selectedTab.Equals("Humidity"))
            {
                sourceChart = chart_Humidity;
            }
            else if (selectedTab.Equals("Gas"))
            {
                sourceChart = chart_Gas;
            }
            else if (selectedTab.Equals("CO2"))
            {
                sourceChart = chart_CO2;
            }
            else if (selectedTab.Equals("TVOC"))
            {
                sourceChart = chart_TVOC;
            }
            else if (selectedTab.Equals("CO"))
            {
                sourceChart = chart_CO;
            }

            return sourceChart;
        }


        private void cbOtherSerial_SelectedIndexChanged(object sender, EventArgs e)
        {
            string serialName = cbOtherSerial.SelectedItem.ToString();
            string selectedTab = tabControlCharts.SelectedTab.Text;


            Chart sourceChart = GetSourceChart(selectedTab);

            if (sourceChart.Series.Count > 1)
            {
                //we need to remove all the other series
                sourceChart.Series.RemoveAt(1);
            }

            sourceChart.Series.Add(new Series(serialName));
            Series newSerial = sourceChart.Series[1];
            newSerial.ChartType = SeriesChartType.Line;

            foreach (DataPack dp in dpList)
            {
                float serialValue = GetDataPointValue(dp, serialName);
                newSerial.Points.AddXY(Math.Round(dp.TimeInMills / 1000, 2), serialValue);
            }

            ChartArea ca = sourceChart.ChartAreas[0];

            Series s1 = sourceChart.Series[0];
            Series s2 = sourceChart.Series[1];

            ca.AxisY2.Enabled = AxisEnabled.True;
            s1.YAxisType = AxisType.Primary;
            s2.YAxisType = AxisType.Secondary;

            //ca.RecalculateAxesScale();
            //
            SetChartAxisY2Scale(sourceChart, serialName);

            sourceChart.Update();
        }

        private void tabControlCharts_SelectedIndexChanged(object sender, EventArgs e)
        {
            cbOtherSerial.Items.Clear();

            foreach (TabPage tp in tabControlCharts.TabPages)
            {
                if (!tp.Text.Equals(tabControlCharts.SelectedTab.Text))
                {
                    cbOtherSerial.Items.Add(tp.Text);
                }
            }

            Chart sourceChart = GetSourceChart(tabControlCharts.SelectedTab.Text);

            if (sourceChart.Series.Count > 1)
            {
                //we need to remove all the other series
                sourceChart.Series.RemoveAt(1);
            }
        }

        private void mapControl_OnMapZoomChanged()
        {

        }

        private void mapControl_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                fileName = openFileDialog.FileName;

                List<DataPack> DackPackList = new List<DataPack>();

                if (fileName != null)
                {

                    //reset the status
                    if (ReadFileThread != null)
                    {
                        ReadFileThread.Abort();
                        ReadFileThread = null;

                        AddToMonitor("Thread stopped");
                    }

                    resetScreen();



                    toolStripStatusLabel.Text = fileName;

                    if (ReadFileThread == null)
                    {
                        ReadFileThread = new Thread(ReadFileOld);
                        ReadFileThread.Priority = ThreadPriority.Normal;
                        ReadFileThread.IsBackground = true;
                        ReadFileThread.Start();

                        AddToMonitor("started reading file");
                        //btnGraph.Text = "Stop";
                    }

                }
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ReadFileOld()
        {
            List<string> packetLines = new List<string>();
            DataPack dp = null;

            dpList.Clear();

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
                        dp = DataPackReader.ReadDataPackOld(packetLines, dp);
                        PlotValue(dp);



                        //show the location on the map
                        PlotMap(dp.Latitude, dp.Longitude);


                        GMapOverlay markersOverlay = new GMapOverlay("markers");
                        GMarkerGoogle marker = new GMarkerGoogle(new PointLatLng(dp.Latitude, dp.Longitude), GMarkerGoogleType.green);
                        markersOverlay.Markers.Add(marker);
                        mapControl.Overlays.Add(markersOverlay);




                        dpList.Add(dp);
                        packetLines.Clear();
                    }
                }

                UpdateProgress(100);
                Thread.Sleep(100);
                UpdateProgress(0);

                ////show the location on the map
                //PlotMap(43.8828, -79.4403);


                //GMapOverlay markersOverlay = new GMapOverlay("markers");
                //GMarkerGoogle marker = new GMarkerGoogle(new PointLatLng(43.8828, -79.4403), GMarkerGoogleType.green);
                //markersOverlay.Markers.Add(marker);
                //mapControl.Overlays.Add(markersOverlay);



                AddToMonitor("Finished reading file.");

                try
                {
                    ReadFileThread.Abort();
                    ReadFileThread = null;
                }
                catch (Exception ex)
                {

                }
            }


        }
    }
}
