using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
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
            string[] totalPingLogs = new string[3];
            string[] TotalPingsLine = new string[3];
            char[] delimiters = [':', ','];

            Directory.SetCurrentDirectory(@"C:\Users\Tracks\source\repos\ISP_Ping_tester"); //Not needed in the release!!!!!!!!!!

            //Setup log files if required:
            if (!File.Exists(totalPingLogsFileLocation))
            {
                string initializePingLogs = "Total succesful pings: 0,\r\nTotal single failed pings: 0,\r\nTotal successive pings: 0,";
                
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
                    // l = 0
                    // Update the toatal csv file with +100 successful pings.
                    UpdateTotalPingLogsFile(totalPingLogsFileLocation, 0, 20);

                    // Update the PingLogs csv file with 1 more log.
                    CsvHandler.UpdatePingLogsFile(IpAddress, packetSize, roundTripTime, successfulPing);

                    sessionPingArray[0] = 0;
                    
                }
                else if (sessionPingArray[0] < 20)
                {
                    CsvHandler.UpdatePingLogsFile(IpAddress, packetSize, roundTripTime, successfulPing);
                }
                sessionPingArray[3] = 0; // Bool for successive pings
                sessionPingArray[4] = 0;
                sessionPingArray[5] = 0;
            }
            else if (successfulPing == false  && sessionPingArray[3] == 0)
            {
                sessionPingArray[1] += 1;
                
                if (sessionPingArray[1] == 5)
                {
                    // l = 1
                    // Update the toatal csv file with +5 failed pings.
                    UpdateTotalPingLogsFile(totalPingLogsFileLocation, 1, 5);

                    // Update the PingLogs csv file with 1 more log.
                    CsvHandler.UpdatePingLogsFile(IpAddress, packetSize, roundTripTime, successfulPing);

                    sessionPingArray[1] = 0;
                }
                else if (sessionPingArray[1] < 5)
                {
                    roundTripTime = "N/A";

                    CsvHandler.UpdatePingLogsFile(IpAddress, packetSize, roundTripTime, successfulPing);
                }
                sessionPingArray[3] = 1; // Bool for successive pings
                sessionPingArray[4] = 0;
                sessionPingArray[5] = 1;
            }
            else if (successfulPing == false && sessionPingArray[3] == 1)
            {
                sessionPingArray[2] += 1;

                if (sessionPingArray[2] == 5)
                {
                    //L = 2
                    // Update the toatal csv file with +5 failed pings.
                    UpdateTotalPingLogsFile(totalPingLogsFileLocation, 2 , 5);

                    sessionPingArray[2] = 0;
                }

                UpdatePingLogsFile(IpAddress, packetSize, roundTripTime, successfulPing);
                sessionPingArray[4] = 1;
                sessionPingArray[5] = 1;
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

        public static void UpdateTotalPingLogsFile(string totalPingLogsFileLocation, int lineToEdit, int pingCount)
        {
            //Vars:
            string[] totalPingLogs = new string[3];
            string[] TotalPingsLine = new string[3];
            char[] delimiters = [':', ','];
            int pingsInt = 0;


            totalPingLogs = File.ReadAllLines(totalPingLogsFileLocation);

            TotalPingsLine = totalPingLogs[lineToEdit].Split(delimiters);
            TotalPingsLine[1] = TotalPingsLine[1].Trim();
            pingsInt = Convert.ToInt32(TotalPingsLine[1]);
            pingsInt = pingsInt + pingCount;
            TotalPingsLine[1] = ": " + Convert.ToString(pingsInt) + ",";
            totalPingLogs[lineToEdit] = TotalPingsLine[0] + TotalPingsLine[1] + TotalPingsLine[2];

            File.WriteAllLines(totalPingLogsFileLocation, totalPingLogs);
        }
    }
}
