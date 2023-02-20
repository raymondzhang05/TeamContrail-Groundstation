using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace SpaceLane
{
    public partial class HeatmapForm : Form
    {

        // Creates standard thermal image color palette (blue=cold, red=hot)
        private static int[] paletteR = new int[256];
        private static int[] paletteG = new int[256];
        private static int[] paletteB = new int[256];

        static void CreateThermalImageColorPalette(int min, int max)
        {
            // The palette is gnuplot's PM3D palette.
            // See here for details: https://stackoverflow.com/questions/28495390/thermal-imaging-palette


            for (int x = 0; x <= 255; x++)
            {
                int range = 256;
                paletteR[x] = System.Convert.ToByte(range * Math.Sqrt(x / range));
                paletteG[x] = System.Convert.ToByte(range * Math.Pow(x / range, 3));
                if (Math.Sin(2 * Math.PI * (x / range)) >= 0)
                {
                    paletteB[x] = System.Convert.ToByte(range * Math.Sin(2 * Math.PI * (x / range)));
                }
                else
                {
                    paletteB[x] = 0;
                }
            }
        }

        public HeatmapForm()
        {
            InitializeComponent();
        }

        private void HeatmapForm_Load(object sender, EventArgs e)
        {
            

            CreateImage();
        }

        private Color HeatMapColor(decimal value, decimal min, decimal max)
        {
            
            float correctionFactor = (float)( (value - min) / (max - min));

            /*
            Color tempColor =  Color.FromArgb
            (
                255,
                 Convert.ToByte(255 * correctionFactor),
                0,
                Convert.ToByte(255 * (1 - correctionFactor))                
            );

            float red = (float)tempColor.R;
            float green = (float)tempColor.G;
            float blue = (float)tempColor.B;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (255 - red) * 0 + red;
                green = (255 - green) * 0 + green;
                blue = (255 - blue) * 0 + blue;
            }

            return Color.FromArgb(tempColor.A, (int)red, (int)green, (int)blue);
            */


            List<Color> baseColors = new List<Color>();  // create a color list
            
            baseColors.Add(Color.Cyan);
            baseColors.Add(Color.Blue);
            baseColors.Add(Color.DarkBlue);
            baseColors.Add(Color.Orange);
            baseColors.Add(Color.Red);
            baseColors.Add(Color.DarkRed);
            List<Color> colors = interpolateColors(baseColors, 200);

           

            return colors[Convert.ToInt16(Math.Abs(correctionFactor * 200 -1))];

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




        private void CreateImage()
        {
            

            int width = 32;
            int height = 24;

            Bitmap bmp = new Bitmap(width , height );

            string strValue = "28.92,28.35,28.35,27.36,26.20,26.04,25.94,25.98,26.67,26.12,27.41,27.05,26.17,25.85,25.41,24.72,24.31,24.35,24.11,24.15,23.35,23.38,23.46,22.71,22.60,23.07,23.10,22.92,22.12,23.31,23.34,23.14,28.58,28.26,27.57,27.54,25.76,26.02,26.57,26.37,26.06,26.10,26.61,27.02,25.61,25.28,24.87,24.73,23.97,24.73,23.57,23.98,23.39,23.42,22.91,23.34,22.45,22.71,22.73,22.96,22.88,22.66,22.17,23.91,28.15,27.86,27.43,26.97,25.70,25.12,26.70,26.51,26.38,26.22,26.33,25.46,25.04,24.56,24.69,24.56,23.81,23.85,23.61,23.66,23.05,23.28,23.17,22.83,23.11,22.55,22.80,22.59,22.72,22.99,22.50,22.79,28.60,27.33,26.95,26.71,25.71,25.75,26.08,26.48,26.56,26.02,25.62,25.82,24.70,24.57,24.70,24.57,23.83,23.69,23.82,24.04,23.45,23.30,23.02,23.24,22.77,22.98,22.85,23.05,22.78,22.18,23.02,22.61,27.26,27.03,26.42,25.99,25.87,25.71,26.21,26.25,25.59,25.27,25.57,24.91,24.52,24.22,23.99,24.05,23.34,23.73,23.66,23.51,22.94,22.98,23.07,22.72,22.83,22.31,22.55,22.74,22.82,22.66,23.30,23.57,27.71,26.54,26.20,25.98,25.48,25.90,26.40,26.61,25.41,25.45,25.40,24.92,24.37,24.40,24.01,24.07,23.19,23.58,23.17,24.05,23.14,23.00,23.09,23.66,22.68,22.53,22.77,23.17,22.66,22.70,23.34,25.12,26.45,26.48,25.92,25.97,26.59,26.81,26.35,25.64,24.51,24.91,24.70,24.39,24.19,24.41,24.18,23.74,23.36,23.41,23.52,22.71,23.13,23.39,23.09,23.31,22.68,22.58,22.79,22.44,22.69,22.91,27.73,31.89,26.90,26.25,25.50,26.17,26.40,27.17,25.98,25.82,25.40,24.57,24.36,24.74,23.88,23.76,23.68,23.76,23.38,23.59,23.03,23.58,23.50,23.07,22.76,23.15,22.88,22.78,22.44,23.03,22.92,23.33,32.24,37.95,26.19,25.56,25.67,25.52,25.23,25.46,24.86,24.91,24.69,24.05,24.53,24.23,23.40,23.76,23.88,23.58,23.55,22.95,23.20,22.89,23.02,23.06,23.49,22.99,22.74,22.77,23.55,22.27,22.75,22.98,40.22,42.68,26.21,25.77,25.27,25.92,25.43,25.28,24.87,24.91,24.20,24.07,23.86,24.40,23.41,23.45,23.57,23.76,23.74,23.13,23.39,23.42,22.87,22.74,22.64,22.83,22.41,22.43,22.64,22.66,22.78,25.29,40.35,43.77,25.74,25.56,25.47,25.71,24.67,24.72,24.50,24.19,23.85,23.71,24.02,24.07,23.23,23.12,23.71,23.42,23.24,22.95,23.37,22.76,23.04,22.58,22.78,22.99,22.57,22.28,23.02,22.87,23.14,25.47,37.65,40.14,26.18,25.78,25.88,25.91,25.25,24.73,24.33,24.38,23.69,23.55,23.86,24.08,23.57,23.46,22.89,23.61,23.09,23.13,23.22,23.11,22.89,22.93,22.62,23.17,22.24,22.48,22.48,22.88,22.78,24.72,32.27,34.28,25.72,25.76,25.87,25.31,24.48,24.36,24.33,24.21,23.49,23.55,24.18,23.56,23.71,23.27,23.54,23.42,22.92,22.78,23.22,22.75,23.05,22.42,22.28,22.68,22.23,22.42,22.23,22.85,22.53,23.37,26.66,25.92,26.39,25.97,25.27,25.51,24.68,24.19,24.16,24.39,23.68,23.39,23.68,23.91,23.23,23.28,23.21,23.44,23.10,22.96,22.90,23.10,22.73,22.44,22.31,22.87,21.90,22.26,22.62,22.67,22.75,23.00,24.65,25.10,25.96,25.12,25.46,24.91,23.90,24.53,23.95,23.81,23.99,23.70,23.84,23.89,23.21,23.60,23.55,22.94,22.73,22.76,23.00,22.92,22.69,22.41,22.60,22.44,22.57,22.03,22.58,22.83,22.70,22.78,23.20,23.05,25.98,25.35,25.06,25.31,24.48,24.54,24.32,24.36,23.47,23.36,23.34,23.56,23.05,22.95,22.91,23.44,23.07,22.94,23.18,23.26,22.37,22.42,22.26,22.62,22.06,22.23,22.21,23.03,22.53,21.82,22.39,23.47,25.75,24.90,25.26,24.49,23.89,23.95,23.56,23.44,23.13,23.17,23.65,23.38,23.53,23.58,23.34,23.07,22.88,22.73,22.65,22.52,22.28,22.30,22.89,22.23,22.33,22.37,22.57,22.20,22.11,22.56,22.54,22.57,24.85,25.36,24.64,24.90,24.09,23.96,23.75,24.36,23.31,23.35,23.15,23.38,23.20,23.08,23.36,23.07,22.73,22.40,22.50,22.87,22.29,22.31,22.55,22.78,22.53,22.39,22.59,22.40,22.13,22.38,22.15,23.22,24.35,24.90,24.83,24.47,23.84,23.74,24.08,23.42,23.25,23.34,24.14,23.14,22.63,22.86,22.95,23.52,22.47,22.35,22.75,22.80,22.42,22.44,22.49,22.18,22.03,22.13,22.69,22.38,22.44,21.91,22.68,22.31,24.61,24.68,25.06,24.69,23.85,23.54,23.90,23.80,23.44,23.16,23.44,23.50,22.98,23.21,23.13,22.83,22.48,22.53,22.58,22.99,22.09,22.80,22.69,23.09,22.61,22.32,22.51,22.97,22.47,22.73,22.27,23.40,25.05,24.66,25.26,24.24,24.21,23.70,23.63,24.32,22.98,23.42,23.75,23.09,23.23,22.95,23.42,23.12,22.73,22.45,22.70,22.75,22.49,22.56,23.35,22.87,22.94,22.78,23.03,23.31,22.37,23.06,23.27,23.57,24.59,24.66,24.62,24.90,23.60,23.89,23.64,24.32,23.18,23.42,23.02,23.46,22.69,23.14,22.53,22.94,22.92,22.98,22.70,22.93,22.68,22.93,22.80,23.24,22.77,22.21,22.84,23.31,22.80,22.86,22.84,23.57,24.55,24.90,25.05,24.21,24.17,23.82,23.79,23.66,23.47,23.54,23.69,22.81,22.65,22.89,23.36,23.05,22.67,22.73,22.79,22.47,22.78,23.01,22.90,22.55,22.27,22.72,23.39,22.83,23.13,22.55,23.44,23.75,25.07,24.40,24.11,24.66,23.74,23.61,23.79,23.86,23.47,23.53,23.11,23.39,22.84,22.70,22.81,23.05,23.04,22.54,22.99,23.04,22.60,22.64,23.10,23.35,22.88,22.93,22.78,23.66,22.50,23.20,23.46,24.20,";

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

                //CreateThermalImageColorPalette(Decimal.ToInt16(minValue * 100), Decimal.ToInt16(maxValue * 100));

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int i = ((y * width) + x);

                        float correctionFactor = (float)((valueArr[i] - minValue) / (maxValue - minValue));
                        int color = Decimal.ToInt16((decimal)(correctionFactor * 255));

                       // bmp.SetPixel(x, y, Color.FromArgb(paletteR[color], paletteG[color], paletteB[color]));

                        bmp.SetPixel(x, y, HeatMapColor(valueArr[i], minValue, maxValue));

                        /*
                        bmp.SetPixel(x+1, y, HeatMapColor(valueArr[i], minValue, maxValue));

                        bmp.SetPixel(x+2, y, HeatMapColor(valueArr[i], minValue, maxValue));

                        bmp.SetPixel(x, y + 1, HeatMapColor(valueArr[i], minValue, maxValue));

                        bmp.SetPixel(x, y + 2, HeatMapColor(valueArr[i], minValue, maxValue));

                        bmp.SetPixel(x + 1, y + 1, HeatMapColor(valueArr[i], minValue, maxValue));
                        bmp.SetPixel(x + 1, y + 2, HeatMapColor(valueArr[i], minValue, maxValue));

                        bmp.SetPixel(x + 2, y + 1, HeatMapColor(valueArr[i], minValue, maxValue));
                        bmp.SetPixel(x + 2, y + 2, HeatMapColor(valueArr[i], minValue, maxValue));
                        */
                    }
                }



                Bitmap originalImage = bmp;
                Bitmap adjustedImage = bmp;
                float brightness = 1.01f; // no change in brightness
                float contrast = 1.1f; // twice the contrast
                float gamma = 2.0f; // no change in gamma

                float adjustedBrightness = brightness - 1.0f;
                // create matrix that will brighten and contrast the image
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





               


                pictureBox1.Image = adjustedImage;

                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;

              //  pictureBox1.Width = 32 * 15;
              //  pictureBox1.Height = 24 * 15;

                pictureBox1.Refresh();


            }

           

            List<byte> bytes = new List<byte>(); // this list should be filled with values
                 

            

            
        }
    }
}
