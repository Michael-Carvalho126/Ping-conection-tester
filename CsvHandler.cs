using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace ISP_Ping_tester
{
    class CsvHandler
    {
        public static int[] UpdateCSVPingInformation(string IpAddress, int packetSize, string roundTripTime, bool successfulPing, int[] sessionPingArray)
        {
            //sessionPingArray [0] == Sucessful [1] == Failed
            //Vars:
            string totalPingLogsFileLocation = @"C:\Users\Tracks\source\repos\ISP_Ping_tester\TotalPingLogs.csv";
            string PingLogsFileLocation = @"C:\Users\Tracks\source\repos\ISP_Ping_tester\PingLogs.csv";
            string[] totalPingLogs = new string[2];
            string[] TotalPingsLine = new string[3];
            char[] delimiters = [':', ','];
            int PingsInt = 0;

            Directory.SetCurrentDirectory(@"C:\Users\Tracks\source\repos\ISP_Ping_tester"); //Not needed in the release!!!!!!!!!!

            //Setup log files if required:
            if (!File.Exists(totalPingLogsFileLocation))
            {
                string initializePingLogs = "Total succesful pings: 0,\r\nTotal failed pings: 0,\r\nTotal successive pings: 0,\r\n";
                
                using (StreamWriter sw = File.CreateText(totalPingLogsFileLocation))
                {
                    sw.WriteLine(initializePingLogs);
                }
                
                
            }

            if (!File.Exists(PingLogsFileLocation))
            {
                string initializePingLogs = "Auto ping logs:,\r\n";

                using (StreamWriter sw = File.CreateText(PingLogsFileLocation))
                {
                    sw.WriteLine(initializePingLogs);
                }


            }


            //Start main logging:

            if (successfulPing == true)
            {
                sessionPingArray[0] += 1;
                if (sessionPingArray[0] == 20)
                {
                    // Update the toatal csv file with +100 successful pings.
                    totalPingLogs = File.ReadAllLines(totalPingLogsFileLocation);

                    TotalPingsLine = totalPingLogs[0].Split(delimiters);
                    TotalPingsLine[1] = TotalPingsLine[1].Trim();
                    PingsInt = Convert.ToInt32(TotalPingsLine[1]);
                    PingsInt = PingsInt + 20;
                    TotalPingsLine[1] = ": " + Convert.ToString(PingsInt) + ",";
                    totalPingLogs[0] = TotalPingsLine[0] + TotalPingsLine[1] + TotalPingsLine[2];   //This might cause an issue having a [2] I am assuming that the \r\n is included in the string.
                    File.WriteAllLines(totalPingLogsFileLocation,totalPingLogs);

                    // Update the PingLogs csv file with 1 more log.
                    CsvHandler.UpdatePingLogsFile(IpAddress, packetSize, roundTripTime, successfulPing);

                    sessionPingArray[0] = 0;
                    sessionPingArray[2] = 0;
                }
                else if (sessionPingArray[0] < 20)
                {
                    CsvHandler.UpdatePingLogsFile(IpAddress, packetSize, roundTripTime, successfulPing);
                    sessionPingArray[2] = 0;
                }
            }
            else if (successfulPing == false  && sessionPingArray[2] == 0)
            {
                sessionPingArray[1] += 1;
                if (sessionPingArray[1] == 5)
                {
                    // Update the toatal csv file with +5 failed pings.
                    totalPingLogs = File.ReadAllLines(totalPingLogsFileLocation);

                    TotalPingsLine = totalPingLogs[1].Split(delimiters);
                    TotalPingsLine[1] = TotalPingsLine[1].Trim();
                    PingsInt = Convert.ToInt32(TotalPingsLine[1]);
                    PingsInt = PingsInt + 5;
                    TotalPingsLine[1] = ": " + Convert.ToString(PingsInt) + ",";
                    totalPingLogs[1] = TotalPingsLine[0] + TotalPingsLine[1] + TotalPingsLine[2];   //This might cause an issue having a [2] I am assuming that the \r\n is included in the string.
                    File.WriteAllLines(totalPingLogsFileLocation, totalPingLogs);

                    // Update the PingLogs csv file with 1 more log.
                    CsvHandler.UpdatePingLogsFile(IpAddress, packetSize, roundTripTime, successfulPing);

                    sessionPingArray[1] = 0;
                    sessionPingArray[2] = 1;
                }
                else if (sessionPingArray[1] < 5)
                {
                    roundTripTime = "N/A";

                    CsvHandler.UpdatePingLogsFile(IpAddress, packetSize, roundTripTime, successfulPing);
                    sessionPingArray[2] = 1;
                }
            }
            else if (successfulPing == false && sessionPingArray[2] == 1)
            {
                sessionPingArray[1] += 1;
                
                // Update the toatal csv file with +5 failed pings.
                totalPingLogs = File.ReadAllLines(totalPingLogsFileLocation);

                TotalPingsLine = totalPingLogs[2].Split(delimiters);
                TotalPingsLine[1] = TotalPingsLine[1].Trim();
                PingsInt = Convert.ToInt32(TotalPingsLine[1]);
                PingsInt = PingsInt + 1;
                TotalPingsLine[1] = ": " + Convert.ToString(PingsInt) + ",";
                totalPingLogs[2] = TotalPingsLine[0] + TotalPingsLine[1] + TotalPingsLine[2];   //This might cause an issue having a [2] I am assuming that the \r\n is included in the string.
                File.WriteAllLines(totalPingLogsFileLocation, totalPingLogs);

                // Update the PingLogs csv file with 1 more log.
                CsvHandler.UpdatePingLogsFile(IpAddress, packetSize, roundTripTime, successfulPing);

                if (sessionPingArray[1] == 5)
                {
                    // Update the toatal csv file with +5 failed pings.
                    totalPingLogs = File.ReadAllLines(totalPingLogsFileLocation);

                    TotalPingsLine = totalPingLogs[1].Split(delimiters);
                    TotalPingsLine[1] = TotalPingsLine[1].Trim();
                    PingsInt = Convert.ToInt32(TotalPingsLine[1]);
                    PingsInt = PingsInt + 5;
                    TotalPingsLine[1] = ": " + Convert.ToString(PingsInt) + ",";
                    totalPingLogs[1] = TotalPingsLine[0] + TotalPingsLine[1] + TotalPingsLine[2];   //This might cause an issue having a [2] I am assuming that the \r\n is included in the string.
                    File.WriteAllLines(totalPingLogsFileLocation, totalPingLogs);

                    sessionPingArray[1] = 0;
                }
            }

                return sessionPingArray;
        }

        public static void UpdatePingLogsFile(string IpAddress,int packetSize, string roundTripTime, bool successfulPing)
        {
            //Vars:
            DateTime dateTime = DateTime.Now;
            string PingLogsFileLocation = @"C:\Users\Tracks\source\repos\ISP_Ping_tester\PingLogs.csv";
            string updateLineText = dateTime.ToString() + " , " + "IP: " + IpAddress + " , " + "Packet size: " + packetSize.ToString() + " , " + "Round trip time: " + roundTripTime + " , " + "Successful ping: " + successfulPing.ToString() + "\r\n";

            File.AppendAllText(PingLogsFileLocation, updateLineText);
        }
    }
}
