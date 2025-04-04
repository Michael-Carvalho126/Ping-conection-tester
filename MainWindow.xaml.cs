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

namespace ISP_Ping_tester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Universtal Vars:
        bool startStop = false;
        int currentSessionSuccesfulPings = 0;
        int currentSessionFailedPings = 0;
        int[] sessionPingArray = new int[3];    //sessionPingArray [0] == Sucessful [1] == Failed [2] == successivePing either 0 for false or 1 for true

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartManualTest_Click(object sender, RoutedEventArgs e)
        {
            if (manualPingRepeat.Text != "")
            {
                if (manualPingAddress.Text != "")
                {
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
                            sessionPingArray = CsvHandler.UpdateCSVPingInformation(reply.Address.ToString(), reply.Buffer.Length, reply.RoundtripTime.ToString(), true, sessionPingArray);
                            Thread.Sleep(Convert.ToInt32(reply.RoundtripTime) + 80);
                        }
                        else if (reply.Status == IPStatus.DestinationHostUnreachable)
                        {
                            infoTextBox.Text += "Destination host " + ipAddress + " unreachable" + "\n--End--\n";
                            sessionPingArray = CsvHandler.UpdateCSVPingInformation(reply.Address.ToString(), reply.Buffer.Length, reply.RoundtripTime.ToString(), false, sessionPingArray);
                            Thread.Sleep(250);
                        }
                        else if (reply.Status == IPStatus.TimedOut)
                        {
                            infoTextBox.Text += "Ping round trip time exceeded the " + timeout.ToString() + " ms\n--End--\n";
                            sessionPingArray = CsvHandler.UpdateCSVPingInformation(reply.Address.ToString(), reply.Buffer.Length, reply.RoundtripTime.ToString(), false, sessionPingArray);
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
                    string ipAddress = "www.google.co.uk";
                    //string ipAddress = "192.168.1.1";
                    //string ipAddress = "uk.yahoo.com";
                    Ping pingSender = new Ping();
                    PingOptions options = new PingOptions();

                    options.DontFragment = true;

                    //Create a buffer of 32 bytes of data to be transmitted.
                    //Will adjust in future to be auto generated to the size
                    //of the packet size in the packet size text boxes
                    string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
                    byte[] buffer = Encoding.ASCII.GetBytes(data);
                    int timeout = 250; // Will also change to get value from a textbox
                    PingReply reply = pingSender.Send(ipAddress, timeout, buffer, options);

                    if (reply.Status == IPStatus.Success)
                    {
                        Application.Current.Dispatcher.BeginInvoke(() => infoTextBox.AppendText("Address: " + reply.Address.ToString() + "\n"));
                        Application.Current.Dispatcher.BeginInvoke(() => infoTextBox.AppendText("Roundtrip time: " + reply.RoundtripTime.ToString() + " ms\n"));
                        //Application.Current.Dispatcher.BeginInvoke(() => infoTextBox.AppendText("Time to live: " + reply.Options.Ttl + " ms\n"));
                        //infoTextBox.Text += "Don't fragment: " + reply.Options.DontFragment + "\n";
                        Application.Current.Dispatcher.BeginInvoke(() => infoTextBox.AppendText("Buffer size: " + reply.Buffer.Length + "\n--End--\n"));
                        Application.Current.Dispatcher.BeginInvoke(() => infoTextBox.Focus());
                        Application.Current.Dispatcher.BeginInvoke(() => infoTextBox.CaretIndex = infoTextBox.Text.Length);
                        Application.Current.Dispatcher.BeginInvoke(() => infoTextBox.ScrollToEnd());
                        sessionPingArray = CsvHandler.UpdateCSVPingInformation(reply.Address.ToString(), reply.Buffer.Length, reply.RoundtripTime.ToString(), true, sessionPingArray);
                    }
                    else if (reply.Status == IPStatus.DestinationHostUnreachable)
                    {
                        Application.Current.Dispatcher.BeginInvoke(() => infoTextBox.AppendText("Destination host " + ipAddress + " unreachable" + "\n--End--\n"));
                        sessionPingArray = CsvHandler.UpdateCSVPingInformation(reply.Address.ToString(), reply.Buffer.Length, reply.RoundtripTime.ToString(), false, sessionPingArray);
                    }
                    else if (reply.Status == IPStatus.TimedOut)
                    {
                        Application.Current.Dispatcher.BeginInvoke(() => infoTextBox.AppendText("Ping round trip time exceeded the " + timeout.ToString() + " ms\n--End--\n"));
                        sessionPingArray = CsvHandler.UpdateCSVPingInformation(reply.Address.ToString(), reply.Buffer.Length, reply.RoundtripTime.ToString(), false, sessionPingArray);
                    }

                    Thread.Sleep(1000);
                }
            }
            );
            startAutoThread.Start();
            
        }

        private void stopAuto_Click(object sender, RoutedEventArgs e)
        {
            startStop = false;
        }
    }
}