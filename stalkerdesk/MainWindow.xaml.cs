using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace stalkerdesk
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<Workstation> Computers { get; set; }
            = new ObservableCollection<Workstation>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            Loaded += async (s, e) => await ScanNetwork();
        }

        private async Task ScanNetwork()
        {
            string ip = GetLocalIP();

            int lastDot = ip.LastIndexOf('.');
            string subnet = lastDot > 0 ? ip.Substring(0, lastDot + 1) : "192.168.1.";

            SemaphoreSlim sem = new SemaphoreSlim(50);
            Task[] tasks = new Task[254];

            for (int i = 1; i < 255; i++)
            {
                await sem.WaitAsync();
                string targetIp = subnet + i;

                tasks[i - 1] = Task.Run(async () =>
                {
                    try
                    {
                        using (Ping ping = new Ping())
                        {
                            var reply = await ping.SendPingAsync(targetIp, 300);

                            if (reply.Status == IPStatus.Success)
                            {
                                string hostName = GetHostName(targetIp);

                                await Dispatcher.InvokeAsync(() =>
                                {
                                    if (!Computers.Any(c => c.IP == targetIp))
                                    {
                                        Computers.Add(new Workstation
                                        {
                                            IP = targetIp,
                                            Name = hostName
                                        });
                                    }
                                });
                            }
                        }
                    }
                    catch { }
                    finally
                    {
                        sem.Release();
                    }
                });
            }

            await Task.WhenAll(tasks);
        }

        private string GetLocalIP()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();

            return "192.168.1.1";
        }

        private string GetHostName(string ip)
        {
            try
            {
                var entry = Dns.GetHostEntry(ip);
                return entry.HostName;
            }
            catch
            {
                return "Unknown device";
            }
        }

        private async Task SendCommand(string ip, string command)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    await client.ConnectAsync(ip, 5000);

                    var stream = client.GetStream();
                    byte[] data = Encoding.UTF8.GetBytes(command);

                    await stream.WriteAsync(data, 0, data.Length);
                    await stream.FlushAsync();
                }
            }
            catch
            {
                MessageBox.Show($"Brak połączenia z {ip}");
            }
        }

        private void Manage_Click(object sender, RoutedEventArgs e)
        {
            var pc = (sender as FrameworkElement)?.DataContext as Workstation;

            if (pc != null)
            {
                ManageWindow window = new ManageWindow(pc, SendCommand);
                window.ShowDialog();
            }
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
