using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceLane
{
    public class DataPack
    {
        public float Temperature { get; set; }
        public float Pressure { get; set; }
        public float Humidity { get; set; }
        public float GasResistance { get; set; }

        public float Altitude { get; set; }

        public float Velocity { get; set; }



        public float Uva { get; set; }
        public float Uvb { get; set; }
        public float UvIndex { get; set; }

        public float AccelerationX { get; set; }
        public float AccelerationY { get; set; }
        public float AccelerationZ { get; set; }
        public float AccelerationNet 
        { 
            get => (float)Math.Sqrt(Math.Pow(AccelerationX, 2) + Math.Pow(AccelerationY, 2) + Math.Pow(AccelerationZ, 2));
           
        }


        public float GyroX { get; set; }
        public float GyroY { get; set; }
        public float GyroZ { get; set; }

        public float MagnetoX { get; set; }
        public float MagnetoY { get; set; }
        public float MagnetoZ { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public float TimeInMills { get; set; }

        public string HeatValues { get; set; }
    }
}
