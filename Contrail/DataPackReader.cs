using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceLane
{
    internal class DataPackReader
    {
        public static DataPack ReadDataPackCom(List<String> packetLines)
        {
            //validate data

            DataPack dp = new DataPack();

            for (int i = 0; i < packetLines.Count; i++)
            {
                if (packetLines[i].Contains("BME680:"))
                {
                    int startIndex = packetLines[i].IndexOf("BME680:");
                    int endIndex = packetLines[i].IndexOf("/z");
                    String BmeLine = packetLines[i].Substring(startIndex + "BME680:".Length, endIndex - startIndex - "BME680:".Length);

                    String[] bmeArray = BmeLine.Split(';');
                    dp.Temperature = float.Parse(bmeArray[0]);
                    dp.Pressure = float.Parse(bmeArray[1]);
                    dp.Humidity = float.Parse(bmeArray[2]);
                    dp.GasResistance = float.Parse(bmeArray[3]);
                    dp.Altitude = float.Parse(bmeArray[4]);
                }
                else if (packetLines[i].Contains("VEML6075:"))
                {
                    int startIndex = packetLines[i].IndexOf("VEML6075:");
                    int endIndex = packetLines[i].IndexOf("/z");
                    String UvLine = packetLines[i].Substring(startIndex + "VEML6075:".Length, endIndex - startIndex - "VEML6075:".Length);

                    String[] uvArray = UvLine.Split(';');
                    dp.Uva = float.Parse(uvArray[0]);
                    dp.Uvb = float.Parse(uvArray[1]);
                    dp.UvIndex = float.Parse(uvArray[2]);
                }
                else if (packetLines[i].Contains("SGP30:"))
                {
                    int startIndex = packetLines[i].IndexOf("SGP30:");
                    int endIndex = packetLines[i].IndexOf("/z");
                    String SGPline = packetLines[i].Substring(startIndex + "SGP30:".Length, endIndex - startIndex - "SGP30:".Length);

                    String[] SGPArray = SGPline.Split(';');
                    dp.CO2 = float.Parse(SGPArray[0]);
                    dp.TVOC = float.Parse(SGPArray[1]);
                }
                else if (packetLines[i].Contains("MQ:"))
                {
                    int startIndex = packetLines[i].IndexOf("MQ:");
                    int endIndex = packetLines[i].IndexOf("/z");
                    String MQline = packetLines[i].Substring(startIndex + "MQ:".Length, endIndex - startIndex - "MQ:".Length);

                    String[] MQArray = MQline.Split(';');
                    dp.CO = float.Parse(MQArray[0]);
                }
                else if (packetLines[i].Contains("MPU9250(Acceleration):"))
                {
                    int startIndex = packetLines[i].IndexOf("MPU9250(Acceleration):");
                    int endIndex = packetLines[i].IndexOf("/z");
                    String AccelerationLine = packetLines[i].Substring(startIndex + "MPU9250(Acceleration):".Length, endIndex - startIndex - "MPU9250(Acceleration):".Length);

                    String[] accelerationArray = AccelerationLine.Split(';');
                    dp.AccelerationX = float.Parse(accelerationArray[0]);
                    dp.AccelerationY = float.Parse(accelerationArray[1]);
                    dp.AccelerationZ = float.Parse(accelerationArray[2]);
                }
                else if (packetLines[i].Contains("MPU9250(Gyroscope):"))
                {
                    int startIndex = packetLines[i].IndexOf("MPU9250(Gyroscope):");
                    int endIndex = packetLines[i].IndexOf("/z");
                    String GyroLine = packetLines[i].Substring(startIndex + "MPU9250(Gyroscope):".Length, endIndex - startIndex - "MPU9250(Gyroscope):".Length);

                    String[] gyroArray = GyroLine.Split(';');
                    dp.GyroX = float.Parse(gyroArray[0]);
                    dp.GyroY = float.Parse(gyroArray[1]);
                    dp.GyroZ = float.Parse(gyroArray[2]);
                }
                else if (packetLines[i].Contains("MPU9250(Magnetometer):"))
                {
                    int startIndex = packetLines[i].IndexOf("MPU9250(Magnetometer):");
                    int endIndex = packetLines[i].IndexOf("/z");
                    String MagnetoLine = packetLines[i].Substring(startIndex + "MPU9250(Magnetometer):".Length, endIndex - startIndex - "MPU9250(Magnetometer):".Length);

                    String[] magnetoArray = MagnetoLine.Split(';');
                    dp.MagnetoX = float.Parse(magnetoArray[0]);
                    dp.MagnetoY = float.Parse(magnetoArray[1]);
                    dp.MagnetoZ = float.Parse(magnetoArray[2]);
                }
                else if (packetLines[i].Contains("Coordinates:"))
                {
                    int startIndex = packetLines[i].IndexOf("Coordinates:");
                    int endIndex = packetLines[i].IndexOf("/z");
                    String CoordinatesLine = packetLines[i].Substring(startIndex + "Coordinates:".Length, endIndex - startIndex - "Coordinates:".Length);

                    String[] coordinatesArray = CoordinatesLine.Split(';');
                    double Latitude = double.Parse(coordinatesArray[0]);
                    double Longitude = double.Parse(coordinatesArray[1]);

                    dp.Latitude = Latitude;
                    dp.Longitude = Longitude;

                }
                else if (packetLines[i].Contains("Time:"))
                {
                    int startIndex = packetLines[i].IndexOf("Time:");
                    int endIndex = packetLines[i].IndexOf("/z");
                    String TimeLine = packetLines[i].Substring(startIndex + "Time:".Length, endIndex - startIndex - "Time:".Length);

                    dp.TimeInMills = float.Parse(TimeLine);
                }
                else if (packetLines[i].Contains("IR:"))
                {
                    int startIndex = packetLines[i].IndexOf("IR:");
                    int endIndex = packetLines[i].IndexOf("/z");
                    dp.HeatValues = packetLines[i].Substring(startIndex + "IR:".Length, endIndex - startIndex - "IR:".Length);
                }
                else
                {

                }
            }

            return dp;
        }



        public static DataPack ReadDataPackCom(List<String> packetLines, DataPack previousDP)
        {
            DataPack dp = ReadDataPackCom(packetLines);

            if (previousDP == null)
            {
                dp.Velocity = 0;

                return dp;
            }

            if (previousDP.Altitude == 0 || previousDP.TimeInMills == 0)
            {
                dp.Velocity = 0;

                return dp;
            }

            if (previousDP.TimeInMills >= dp.TimeInMills)
            {
                dp.Velocity = 0;
                return dp;
            }

            dp.Velocity = (dp.Altitude - previousDP.Altitude) * 1000 / (dp.TimeInMills - previousDP.TimeInMills);


            return dp;
        }





        public static DataPack ReadDataPack(List<String> packetLines)
        {
            //validate data

            DataPack dp = new DataPack();

            for (int i = 0; i < packetLines.Count; i++)
            {
                if (packetLines[i].StartsWith("BME680:"))
                {
                    String BmeLine = packetLines[i].Replace("BME680:", "").Replace("/z", "");
                    String[] bmeArray = BmeLine.Split(';');
                    dp.Temperature = float.Parse(bmeArray[0]);
                    dp.Pressure = float.Parse(bmeArray[1]);
                    dp.Humidity = float.Parse(bmeArray[2]);
                    dp.GasResistance = float.Parse(bmeArray[3]);
                    dp.Altitude = float.Parse(bmeArray[4]);
                }
                else if (packetLines[i].StartsWith("VEML6075:"))
                {
                    String UvLine = packetLines[i].Replace("VEML6075:", "").Replace("/z", "");
                    String[] uvArray = UvLine.Split(';');
                    dp.Uva = float.Parse(uvArray[0]);
                    dp.Uvb = float.Parse(uvArray[1]);
                    dp.UvIndex = float.Parse(uvArray[2]);
                }
                else if (packetLines[i].Contains("SGP30:"))
                {
                    int startIndex = packetLines[i].IndexOf("SGP30:");
                    int endIndex = packetLines[i].IndexOf("/z");
                    String SGPline = packetLines[i].Substring(startIndex + "SGP30:".Length, endIndex - startIndex - "SGP30:".Length);

                    String[] SGPArray = SGPline.Split(';');
                    dp.CO2 = float.Parse(SGPArray[0]);
                    dp.TVOC = float.Parse(SGPArray[1]);
                }
                else if (packetLines[i].Contains("MQ:"))
                {
                    int startIndex = packetLines[i].IndexOf("MQ:");
                    int endIndex = packetLines[i].IndexOf("/z");
                    String MQline = packetLines[i].Substring(startIndex + "MQ:".Length, endIndex - startIndex - "MQ:".Length);

                    String[] MQArray = MQline.Split(';');
                    dp.CO = float.Parse(MQArray[0]);
                }
                else if (packetLines[i].StartsWith("MPU9250(Acceleration)"))
                {
                    String AccelerationLine = packetLines[i].Replace("MPU9250(Acceleration):", "").Replace("/z", "");
                    String[] accelerationArray = AccelerationLine.Split(';');
                    dp.AccelerationX = float.Parse(accelerationArray[0]);
                    dp.AccelerationY = float.Parse(accelerationArray[1]);
                    dp.AccelerationZ = float.Parse(accelerationArray[2]);
                }
                else if (packetLines[i].StartsWith("MPU9250(Gyroscope)"))
                {
                    String GyroLine = packetLines[i].Replace("MPU9250(Gyroscope):", "").Replace("/z", "");
                    String[] gyroArray = GyroLine.Split(';');
                    dp.GyroX = float.Parse(gyroArray[0]);
                    dp.GyroY = float.Parse(gyroArray[1]);
                    dp.GyroZ = float.Parse(gyroArray[2]);
                }
                else if (packetLines[i].StartsWith("MPU9250(Magnetometer)"))
                {
                    String MagnetoLine = packetLines[i].Replace("MPU9250(Magnetometer):", "").Replace("/z", "");
                    String[] magnetoArray = MagnetoLine.Split(';');
                    dp.MagnetoX = float.Parse(magnetoArray[0]);
                    dp.MagnetoY = float.Parse(magnetoArray[1]);
                    dp.MagnetoZ = float.Parse(magnetoArray[2]);
                }
                else if (packetLines[i].StartsWith("Coordinates"))
                {
                    String CoordinatesLine = packetLines[i].Replace("Coordinates:", "").Replace("/z", "");
                    String[] coordinatesArray = CoordinatesLine.Split(';');
                    double Latitude = double.Parse(coordinatesArray[0]);
                    double Longitude = double.Parse(coordinatesArray[1]);

                    dp.Latitude = Latitude;
                    dp.Longitude = Longitude;

                }
                else
                {
                    String TimeLine = packetLines[i];
                    dp.TimeInMills = float.Parse(TimeLine);
                }
            }

            return dp;
        }



        public static DataPack ReadDataPack(List<String> packetLines, DataPack previousDP)
        {
            DataPack dp = ReadDataPack(packetLines);

            if (previousDP == null)
            {
                dp.Velocity = 0;

                return dp;
            }

            if (previousDP.Altitude == 0 || previousDP.TimeInMills == 0)
            {
                dp.Velocity = 0;

                return dp;
            }

            if (previousDP.TimeInMills >= dp.TimeInMills)
            {
                dp.Velocity = 0;
                return dp;
            }

            dp.Velocity = (dp.Altitude - previousDP.Altitude) * 1000 / (dp.TimeInMills - previousDP.TimeInMills);


            return dp;
        }



        public static DataPack ReadDataPackOld(List<String> packetLines)
        {
            //validate data

            DataPack dp = new DataPack();

            for (int i = 0; i < packetLines.Count; i++)
            {
                if (packetLines[i].StartsWith("BME680:"))
                {
                    String BmeLine = packetLines[i].Replace("BME680:", "").Replace("/z", "");
                    String[] bmeArray = BmeLine.Split(';');
                    dp.Temperature = float.Parse(bmeArray[0]);
                    dp.Pressure = float.Parse(bmeArray[1]);
                    dp.Humidity = float.Parse(bmeArray[2]);
                    //dp.GasResistance = float.Parse(bmeArray[3]);
                    dp.Altitude = float.Parse(bmeArray[3]);
                }
                else if (packetLines[i].StartsWith("VEML6075:"))
                {
                    String UvLine = packetLines[i].Replace("VEML6075:", "").Replace("/z", "");
                    String[] uvArray = UvLine.Split(';');
                    dp.Uva = float.Parse(uvArray[0]);
                    dp.Uvb = float.Parse(uvArray[1]);
                    dp.UvIndex = float.Parse(uvArray[2]);
                }
                else if (packetLines[i].StartsWith("MPU9250(Acceleration)"))
                {
                    String AccelerationLine = packetLines[i].Replace("MPU9250(Acceleration):", "").Replace("/z", "");
                    String[] accelerationArray = AccelerationLine.Split(';');
                    dp.AccelerationX = float.Parse(accelerationArray[0]);
                    dp.AccelerationY = float.Parse(accelerationArray[1]);
                    dp.AccelerationZ = float.Parse(accelerationArray[2]);
                }
                else if (packetLines[i].Contains("SGP30:"))
                {
                    int startIndex = packetLines[i].IndexOf("SGP30:");
                    int endIndex = packetLines[i].IndexOf("/z");
                    String SGPline = packetLines[i].Substring(startIndex + "SGP30:".Length, endIndex - startIndex - "SGP30:".Length);

                    String[] SGPArray = SGPline.Split(';');
                    dp.CO2 = float.Parse(SGPArray[0]);
                    dp.TVOC = float.Parse(SGPArray[1]);
                }
                else if (packetLines[i].Contains("MQ:"))
                {
                    int startIndex = packetLines[i].IndexOf("MQ:");
                    int endIndex = packetLines[i].IndexOf("/z");
                    String MQline = packetLines[i].Substring(startIndex + "MQ:".Length, endIndex - startIndex - "MQ:".Length);

                    String[] MQArray = MQline.Split(';');
                    dp.CO = float.Parse(MQArray[0]);
                }
                else if (packetLines[i].StartsWith("MPU9250(Gyroscope)"))
                {
                    String GyroLine = packetLines[i].Replace("MPU9250(Gyroscope):", "").Replace("/z", "");
                    String[] gyroArray = GyroLine.Split(';');
                    dp.GyroX = float.Parse(gyroArray[0]);
                    dp.GyroY = float.Parse(gyroArray[1]);
                    dp.GyroZ = float.Parse(gyroArray[2]);
                }
                else if (packetLines[i].StartsWith("MPU9250(Magnetometer)"))
                {
                    String MagnetoLine = packetLines[i].Replace("MPU9250(Magnetometer):", "").Replace("/z", "");
                    String[] magnetoArray = MagnetoLine.Split(';');
                    dp.MagnetoX = float.Parse(magnetoArray[0]);
                    dp.MagnetoY = float.Parse(magnetoArray[1]);
                    dp.MagnetoZ = float.Parse(magnetoArray[2]);
                }

                else if (packetLines[i].StartsWith("Coordinates"))
                {
                    String CoordinatesLine = packetLines[i].Replace("Coordinates:", "").Replace("/z", "");
                    String[] coordinatesArray = CoordinatesLine.Split(';');
                    double Latitude = double.Parse(coordinatesArray[0]);
                    double Longitude = double.Parse(coordinatesArray[1]);

                    dp.Latitude = Latitude;
                    dp.Longitude = Longitude;

                }
                else
                {
                    String TimeLine = packetLines[i];
                    dp.TimeInMills = float.Parse(TimeLine);
                }
            }

            return dp;
        }



        public static DataPack ReadDataPackOld(List<String> packetLines, DataPack previousDP)
        {
            DataPack dp = ReadDataPackOld(packetLines);

            if (previousDP == null)
            {
                dp.Velocity = 0;

                return dp;
            }

            if (previousDP.Altitude == 0 || previousDP.TimeInMills == 0)
            {
                dp.Velocity = 0;

                return dp;
            }


            dp.TimeInMills = dp.TimeInMills + previousDP.TimeInMills;

            dp.Velocity = (dp.Altitude - previousDP.Altitude) * 1000 / (dp.TimeInMills - previousDP.TimeInMills);


            return dp;
        }





    }
}
