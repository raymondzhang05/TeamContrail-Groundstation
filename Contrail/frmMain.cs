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
using System.Windows.Forms.DataVisualization.Charting;

namespace SpaceLane
{
    public partial class frmMain : Form
    {
        public static SerialPort serialPort = new SerialPort();
        static bool connectionState = false;


        public frmMain()
        {
            InitializeComponent();
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog.FileName;

                List<DataPack> DackPackList = new List<DataPack>();

                if (fileName != null)
                {
                    List<string> packetLines = new List<string>();
                    using (StreamReader reader = new StreamReader(fileName))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (!line.EndsWith("eop")) {
                                packetLines.Add(line);
                            }
                            else
                            {
                                packetLines.Add(line);
                                DataPack dp = DataPackReader.ReadDataPack(packetLines);

                                //process monitor stuff
                                tbTime.Text = dp.TimeInMills.ToString();
                                tbBMETemperature.Text = dp.Temperature.ToString();
                                tbBMEPressure.Text = dp.Pressure.ToString();
                                tbBMEHumidity.Text = dp.Humidity.ToString();
                                tbBMEVOCGas.Text = dp.GasResistance.ToString();
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

                                DackPackList.Add(dp);

                                packetLines.Clear();
                            }
                        }
                    }


                    //show demo temperature chart
                    chart_Temperature.DataSource = DackPackList;
                    Series TemperatureSerie = chart_Temperature.Series[0];

                    TemperatureSerie.ChartType = SeriesChartType.Line;
                    TemperatureSerie.Color = Color.Red;
                    TemperatureSerie.BorderWidth = 1;

                    //TemperatureSerie.XValueMember = "TimeInMills";
                    TemperatureSerie.YValueMembers = "Temperature";

                    chart_Temperature.ChartAreas[0].AxisY.Maximum = 30;
                    chart_Temperature.ChartAreas[0].AxisY.Minimum = 0;

                    chart_Temperature.DataBind();


                    //show demo pressure chart
                    chart_Pressure.DataSource = DackPackList;
                    Series PressureSerie = chart_Pressure.Series[0];

                    PressureSerie.ChartType = SeriesChartType.Line;
                    PressureSerie.Color = Color.Red;
                    PressureSerie.BorderWidth = 1;

                    //PressureSerie.XValueMember = "TimeInMills";
                    PressureSerie.YValueMembers = "Pressure";

                    chart_Pressure.ChartAreas[0].AxisY.Maximum = 1070;
                    chart_Pressure.ChartAreas[0].AxisY.Minimum = 930;

                    chart_Pressure.DataBind();


                    //show demo humidity chart
                    chart_Humidity.DataSource = DackPackList;
                    Series HumiditySerie = chart_Humidity.Series[0];

                    HumiditySerie.ChartType = SeriesChartType.Line;
                    HumiditySerie.Color = Color.Red;
                    HumiditySerie.BorderWidth= 1;

                    //HumiditySerie.XValueMember = "TimeInMills";
                    HumiditySerie.YValueMembers = "Humidity";

                    //chart_Humidity.ChartAreas[0].AxisY.Maximum = 45;
                    //chart_Humidity.ChartAreas[0].AxisY.Minimum = 35;

                    chart_Humidity.DataBind();

                    //show demo gas resistance chart
                    chart_Gas.DataSource = DackPackList;
                    Series GasSerie = chart_Gas.Series[0];

                    GasSerie.ChartType = SeriesChartType.Line;
                    GasSerie.Color = Color.Red;
                    GasSerie.BorderWidth= 1;

                    //GasSerie.XValueMember = "TimeInMills";
                    GasSerie.YValueMembers = "GasResistance";

                    //chart_Gas.ChartAreas[0].AxisY.Maximum = 32;
                    //chart_Gas.ChartAreas[0].AxisY.Minimum = 4;

                    chart_Gas.DataBind();

                    //show demo acceleration (net) chart
                    chart_Acceleration_Net.DataSource = DackPackList;
                    Series AccelerationSerie = chart_Acceleration_Net.Series[0];

                    AccelerationSerie.ChartType = SeriesChartType.Line;
                    AccelerationSerie.Color = Color.Red;
                    AccelerationSerie.BorderWidth= 1;

                    //AccelerationSerie.XValueMember = "TimeInMills";
                    AccelerationSerie.YValueMembers = "AccelerationZ";

                    //chart_Acceleration_Axis_Z.ChartAreas[0].AxisX.Maximum = 10;
                    //chart_Acceleration_Axis_Z.ChartAreas[0].AxisX.Minimum = -10;

                    chart_Acceleration_Net.DataBind();
                }
            }
        }

        private void cbPortSelect_DropDown(object sender, EventArgs e)
        {
            SearchAvailableComPort();
        }

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

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (!connectionState)
            {
                connectionState = true;

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
                }
                catch (Exception ex)
                {

                }


            }
        }

        public string txt;

        delegate void SetTextCallback(string text);

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


        private void port_DataReceived(object sender,
                                 SerialDataReceivedEventArgs e)
        {

            txt = serialPort.ReadExisting().ToString();
            SetText(txt.ToString());

        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            if (connectionState)
            {
                connectionState=false;

                if (serialPort.IsOpen);
                {
                    serialPort.Close();
                }
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {

        }
    }
}
