using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.NetworkInformation;
using System.Net;
using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.IO;

namespace ISP_Ping_tester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Universtal Vars:
        bool startStop = false;
        int remainderSuccesfulPings = 0;
        int remainderFailedPings = 0;
        int remainderSuccessiveFailedPings = 0;
        int[] sessionPingArray = new int[6];    //sessionPingArray [0] == Sucessful, [1] == Failed, [2] == amount successive failed [3] == successivePing either 0 for false or 1 for true, [4] == This is for the state of the connection to work, [5] == successful ping
        //string totalPingLogsFileLocation = @"C:\Users\Tracks\source\repos\ISP_Ping_tester\TotalPingLogs.csv";

        




        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;
        private const int HTCLIENT = 1;
        private const int WM_NCHITTEST = 0x0084;
        private const int resizeBorderThickness = 8; // in pixels

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            
            base.OnSourceInitialized(e);
            var handle = (new WindowInteropHelper(this)).Handle;
            HwndSource.FromHwnd(handle)?.AddHook(WindowProc);
        }

        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_NCHITTEST)
            {
                var mousePos = GetMousePosition(lParam);
                var windowPos = this.PointToScreen(new Point(0, 0));
                var relativePos = new Point(mousePos.X - windowPos.X, mousePos.Y - windowPos.Y);

                if (relativePos.X <= resizeBorderThickness)
                {
                    if (relativePos.Y <= resizeBorderThickness)
                        return ReturnHit(HTTOPLEFT, ref handled);
                    else if (relativePos.Y >= ActualHeight - resizeBorderThickness)
                        return ReturnHit(HTBOTTOMLEFT, ref handled);
                    else
                        return ReturnHit(HTLEFT, ref handled);
                }
                else if (relativePos.X >= ActualWidth - resizeBorderThickness)
                {
                    if (relativePos.Y <= resizeBorderThickness)
                        return ReturnHit(HTTOPRIGHT, ref handled);
                    else if (relativePos.Y >= ActualHeight - resizeBorderThickness)
                        return ReturnHit(HTBOTTOMRIGHT, ref handled);
                    else
                        return ReturnHit(HTRIGHT, ref handled);
                }
                else if (relativePos.Y <= resizeBorderThickness)
                {
                    return ReturnHit(HTTOP, ref handled);
                }
                else if (relativePos.Y >= ActualHeight - resizeBorderThickness)
                {
                    return ReturnHit(HTBOTTOM, ref handled);
                }
            }

            return IntPtr.Zero;
        }

        private static IntPtr ReturnHit(int hit, ref bool handled)
        {
            handled = true;
            return new IntPtr(hit);
        }

        private static Point GetMousePosition(IntPtr lParam)
        {
            int x = (short)((lParam.ToInt32()) & 0xFFFF);
            int y = (short)((lParam.ToInt32() >> 16) & 0xFFFF);
            return new Point(x, y);
        }


        private void StartManualTest_Click(object sender, RoutedEventArgs e)
        {
            if (manualPingRepeat.Text != "")
            {
                if (manualPingAddress.Text != "")
                {
                    //Vars:
                    string[] pingFilesArray = new string[2];
                    pingFilesArray = PingFilesNamesWithTime();
                    string totalPingLogsFileLocation = pingFilesArray[0];
                    string PingLogsFileLocation = pingFilesArray[1];


                    string ipAddress = manualPingAddress.Text;
                    Ping pingSender = new Ping();
                    PingOptions options = new PingOptions();

                    options.DontFragment = true;


                    //Create a buffer of 32 bytes of data to be transmitted.
                    //Will adjust in future to be auto generated to the size
                    //of the packet size in the packet size text boxes
                    string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
                    byte[] buffer = Encoding.ASCII.GetBytes(data);
                    int timeout = 250; // Will also change to get value from a textbox
                    int manualRepeat = Convert.ToInt32(manualPingRepeat.Text);



                    for (int i = 0; i < manualRepeat; i++)
                    {
                        PingReply reply = pingSender.Send(ipAddress, timeout, buffer, options);
                        if (reply.Status == IPStatus.Success)
                        {
                            infoTextBox.Text += "Address: " + reply.Address.ToString() + "\n";
                            infoTextBox.Text += "Roundtrip time: " + reply.RoundtripTime.ToString() + "\n";
                            //infoTextBox.Text += $"Time to live: {reply.Options.Ttl}\n";
                            //infoTextBox.Text += "Don't fragment: " + reply.Options.DontFragment + "\n";
                            infoTextBox.Text += "Buffer size: " + reply.Buffer.Length + "\n--End--\n";
                            sessionPingArray = CsvHandler.UpdateCSVPingInformation(totalPingLogsFileLocation, PingLogsFileLocation,reply.Address.ToString(), reply.Buffer.Length, reply.RoundtripTime.ToString(), true, sessionPingArray);
                            Thread.Sleep(Convert.ToInt32(reply.RoundtripTime) + 80);
                        }
                        else if (reply.Status == IPStatus.DestinationHostUnreachable)
                        {
                            infoTextBox.Text += "Destination host " + ipAddress + " unreachable" + "\n--End--\n";
                            sessionPingArray = CsvHandler.UpdateCSVPingInformation(totalPingLogsFileLocation, PingLogsFileLocation,reply.Address.ToString(), reply.Buffer.Length, reply.RoundtripTime.ToString(), false, sessionPingArray);
                            Thread.Sleep(250);
                        }
                        else if (reply.Status == IPStatus.TimedOut)
                        {
                            infoTextBox.Text += "Ping round trip time exceeded the " + timeout.ToString() + " ms\n--End--\n";
                            sessionPingArray = CsvHandler.UpdateCSVPingInformation(totalPingLogsFileLocation, PingLogsFileLocation ,reply.Address.ToString(), reply.Buffer.Length, reply.RoundtripTime.ToString(), false, sessionPingArray);
                            Thread.Sleep(250);
                        }
                        
                    }
                }
                else
                {
                    MessageBox.Show("Please enter a address in the address text box.");
                }
            }
            else
            {
                MessageBox.Show("Please enter a number in the repeat text box.");
            }
        }

        private void startAuto_Click(object sender, RoutedEventArgs e)
        {
            startStop = true;

            Thread startAutoThread = new Thread(() =>
            {
                while (startStop)
                {
                    //Vars:
                    string[] pingFilesArray = new string[2];
                    pingFilesArray = PingFilesNamesWithTime();
                    string totalPingLogsFileLocation = pingFilesArray[0];
                    string PingLogsFileLocation = pingFilesArray[1];

                    string ipAddress = "www.google.co.uk";
                    //string ipAddress = "192.168.1.1";
                    //string ipAddress = "uk.yahoo.com";
                    string routerIpAddress = "192.168.1.1";
                    Ping pingSender = new Ping();
                    PingOptions options = new PingOptions();
                    Application.Current.Dispatcher.BeginInvoke(() => currentStateOfProgramTextBox.Background = Brushes.Green);
                    options.DontFragment = true;

                    //Create a buffer of 32 bytes of data to be transmitted.
                    //Will adjust in future to be auto generated to the size
                    //of the packet size in the packet size text boxes
                    string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
                    byte[] buffer = Encoding.ASCII.GetBytes(data);
                    int timeout = 250; // Will also change to get value from a textbox
                    try
                    {
                        PingReply reply = pingSender.Send(ipAddress, timeout, buffer, options);

                        if (reply.Status == IPStatus.Success)
                        {
                            Application.Current.Dispatcher.BeginInvoke(() => currentStateOfConnectionTextBox.Background = Brushes.Green);
                            Application.Current.Dispatcher.BeginInvoke(() => infoTextBox.AppendText("Address: " + reply.Address.ToString() + "\n"));
                            Application.Current.Dispatcher.BeginInvoke(() => infoTextBox.AppendText("Roundtrip time: " + reply.RoundtripTime.ToString() + " ms\n"));
                            //Application.Current.Dispatcher.BeginInvoke(() => infoTextBox.AppendText("Time to live: " + reply.Options.Ttl + " ms\n"));
                            //infoTextBox.Text += "Don't fragment: " + reply.Options.DontFragment + "\n";
                            Application.Current.Dispatcher.BeginInvoke(() => infoTextBox.AppendText("Buffer size: " + reply.Buffer.Length + "\n--End--\n"));
                            Application.Current.Dispatcher.BeginInvoke(() => infoTextBox.Focus());
                            Application.Current.Dispatcher.BeginInvoke(() => infoTextBox.CaretIndex = infoTextBox.Text.Length);
                            Application.Current.Dispatcher.BeginInvoke(() => infoTextBox.ScrollToEnd());
                            sessionPingArray = CsvHandler.UpdateCSVPingInformation(totalPingLogsFileLocation, PingLogsFileLocation, reply.Address.ToString(), reply.Buffer.Length, reply.RoundtripTime.ToString(), true, sessionPingArray);
                        }
                        else if (reply.Status == IPStatus.DestinationHostUnreachable)
                        {
                            Application.Current.Dispatcher.BeginInvoke(() => infoTextBox.AppendText("Destination host " + ipAddress + " unreachable" + "\n--End--\n"));
                            Application.Current.Dispatcher.BeginInvoke(() => infoTextBox.Focus());
                            Application.Current.Dispatcher.BeginInvoke(() => infoTextBox.CaretIndex = infoTextBox.Text.Length);
                            Application.Current.Dispatcher.BeginInvoke(() => infoTextBox.ScrollToEnd());
                            sessionPingArray = CsvHandler.UpdateCSVPingInformation(totalPingLogsFileLocation, PingLogsFileLocation, reply.Address.ToString(), reply.Buffer.Length, reply.RoundtripTime.ToString(), false, sessionPingArray);
                            if (sessionPingArray[4] == 0 && sessionPingArray[5] == 1)
                            {
                                Application.Current.Dispatcher.BeginInvoke(() => currentStateOfConnectionTextBox.Background = Brushes.OrangeRed);
                            }
                            else if (sessionPingArray[4] == 1 && sessionPingArray[5] == 1)
                            {
                                Application.Current.Dispatcher.BeginInvoke(() => currentStateOfConnectionTextBox.Background = Brushes.Red);
                            }
                            //Ping router after failed ping to Google:
                            reply = pingSender.Send(routerIpAddress, timeout, buffer, options);
                            if (reply.Status == IPStatus.Success)
                            {
                                Application.Current.Dispatcher.BeginInvoke(() => currentStateOfConnectionTextBox.Background = Brushes.Green);
                                Application.Current.Dispatcher.BeginInvoke(() => infoTextBox.AppendText("Address: " + reply.Address.ToString() + " Router after failed ping to Google.\n"));
                                Application.Current.Dispatcher.BeginInvoke(() => infoTextBox.AppendText("Roundtrip time: " + reply.RoundtripTime.ToString() + " ms\n"));
                                Application.Current.Dispatcher.BeginInvoke(() => infoTextBox.AppendText("Buffer size: " + reply.Buffer.Length + "\n--End--\n"));
                                Application.Current.Dispatcher.BeginInvoke(() => infoTextBox.Focus());
                                Application.Current.Dispatcher.BeginInvoke(() => infoTextBox.CaretIndex = infoTextBox.Text.Length);
                                Application.Current.Dispatcher.BeginInvoke(() => infoTextBox.ScrollToEnd());
                                sessionPingArray = CsvHandler.UpdateCSVPingInformation(totalPingLogsFileLocation, PingLogsFileLocation, reply.Address.ToString(), reply.Buffer.Length, reply.RoundtripTime.ToString(), true, sessionPingArray);
                            }
                            else if (reply.Status == IPStatus.DestinationHostUnreachable)
                            {
                                Application.Current.Dispatcher.BeginInvoke(() => infoTextBox.AppendText("Destination host " + ipAddress + " unreachable to Router after failed ping to Google." + "\n--End--\n"));
                                Application.Current.Dispatcher.BeginInvoke(() => infoTextBox.Focus());
                                Application.Current.Dispatcher.BeginInvoke(() => infoTextBox.CaretIndex = infoTextBox.Text.Length);
                                Application.Current.Dispatcher.BeginInvoke(() => infoTextBox.ScrollToEnd());
                                sessionPingArray = CsvHandler.UpdateCSVPingInformation(totalPingLogsFileLocation, PingLogsFileLocation, reply.Address.ToString(), reply.Buffer.Length, reply.RoundtripTime.ToString(), false, sessionPingArray);
                                if (sessionPingArray[4] == 0 && sessionPingArray[5] == 1)
                                {
                                    Application.Current.Dispatcher.BeginInvoke(() => currentStateOfConnectionTextBox.Background = Brushes.OrangeRed);
                                }
                                else if (sessionPingArray[4] == 1 && sessionPingArray[5] == 1)
                                {
                                    Application.Current.Dispatcher.BeginInvoke(() => currentStateOfConnectionTextBox.Background = Brushes.Red);
                                }
                            }
                        }
                        else if (reply.Status == IPStatus.TimedOut)
                        {
                            Application.Current.Dispatcher.BeginInvoke(() => infoTextBox.AppendText("Ping round trip time exceeded the " + timeout.ToString() + " ms\n--End--\n"));
                            Application.Current.Dispatcher.BeginInvoke(() => infoTextBox.Focus());
                            Application.Current.Dispatcher.BeginInvoke(() => infoTextBox.CaretIndex = infoTextBox.Text.Length);
                            Application.Current.Dispatcher.BeginInvoke(() => infoTextBox.ScrollToEnd());
                            sessionPingArray = CsvHandler.UpdateCSVPingInformation(totalPingLogsFileLocation, PingLogsFileLocation, reply.Address.ToString(), reply.Buffer.Length, reply.RoundtripTime.ToString(), false, sessionPingArray);
                            if (sessionPingArray[4] == 0 && sessionPingArray[5] == 1)
                            {
                                Application.Current.Dispatcher.BeginInvoke(() => currentStateOfConnectionTextBox.Background = Brushes.OrangeRed);
                            }
                            else if (sessionPingArray[4] == 1 && sessionPingArray[5] == 1)
                            {
                                Application.Current.Dispatcher.BeginInvoke(() => currentStateOfConnectionTextBox.Background = Brushes.Red);
                            }
                        }
                        else if (reply.Status == IPStatus.HardwareError)
                        {
                            MessageBox.Show("Broken");
                        }

                        Thread.Sleep(2000);
                    }
                    catch (System.Net.NetworkInformation.PingException)
                    {
                        Console.Beep(200, 2000);
                        MessageBox.Show("There is a issue with the ethernet");
                        File.AppendAllText(PingLogsFileLocation, "There was a issue with the ethernet connection.\r\n");
                        startStop = false;
                    }



                }   

                }
            );
            startAutoThread.Start();
            
        }

        private void stopAuto_Click(object sender, RoutedEventArgs e)
        {
            startStop = false;
            Application.Current.Dispatcher.BeginInvoke(() => currentStateOfProgramTextBox.Background = Brushes.Red);
            Application.Current.Dispatcher.BeginInvoke(() => currentStateOfConnectionTextBox.Background = Brushes.Gray);
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            //Vars:
            string[] pingFilesArray = new string[2];
            pingFilesArray = PingFilesNamesWithTime();
            string totalPingLogsFileLocation = pingFilesArray[0];

            startStop = false;

            //Go through all of the individual states in the array and get the modulo
            remainderSuccesfulPings = sessionPingArray[0] % 20;
            remainderFailedPings = sessionPingArray[1] % 5;
            remainderSuccessiveFailedPings = sessionPingArray[2] % 5;

            CsvHandler.UpdateTotalPingLogsFile(totalPingLogsFileLocation, 0, remainderSuccesfulPings);
            CsvHandler.UpdateTotalPingLogsFile(totalPingLogsFileLocation, 1, remainderFailedPings);
            CsvHandler.UpdateTotalPingLogsFile(totalPingLogsFileLocation, 2, remainderSuccessiveFailedPings);

            Application.Current.Shutdown();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private static string[] PingFilesNamesWithTime()
        {
            string[] pingFilesAddressWithTime = new string[2];
            char[] todayDelimiters = ['/', ' '];
            string[] todayArray = new string[4];
            string today = DateTime.Now.Date.ToString();
            todayArray = today.Split(todayDelimiters);
            today = todayArray[0] + "_" + todayArray[1] + "_" + todayArray[2];

            //string totalPingLogsFileLocation = @"C:\Users\Tracks\source\repos\ISP_Ping_tester\TotalPingLogs_" + today + ".csv";
            string totalPingLogsFileLocation = @"C:\Users\Michael_C\source\repos\C#\VS 2022\ISP_Ping_tester\TotalPingLogs_" + today + ".csv";
            //string PingLogsFileLocation = @"C:\Users\Tracks\source\repos\ISP_Ping_tester\PingLogs_" + today + ".csv";
            string PingLogsFileLocation = @"C:\Users\Michael_C\source\repos\C#\VS 2022\ISP_Ping_tester\PingLogs_" + today + ".csv";

            pingFilesAddressWithTime[0] = totalPingLogsFileLocation;
            pingFilesAddressWithTime[1] = PingLogsFileLocation;

            return pingFilesAddressWithTime;
        }
    }
}