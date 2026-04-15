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

            ScanNetwork();
        }

        // 🔍 SKANOWANIE
        private async void ScanNetwork()
        {
            string ip = GetLocalIP();
            string subnet = ip[..ip.LastIndexOf('.') + 1];

            SemaphoreSlim sem = new SemaphoreSlim(50);

            for (int i = 1; i < 255; i++)
            {
                await sem.WaitAsync();
                string targetIp = subnet + i;

                _ = Task.Run(async () =>
                {
                    try
                    {
                        Ping ping = new Ping();
                        var reply = await ping.SendPingAsync(targetIp, 300);

                        if (reply.Status == IPStatus.Success)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (!Computers.Any(c => c.IP == targetIp))
                                {
                                    Computers.Add(new Workstation
                                    {
                                        IP = targetIp,
                                        Name = targetIp
                                    });
                                }
                            });
                        }
                    }
                    catch { }
                    finally
                    {
                        sem.Release();
                    }
                });
            }
        }

        private string GetLocalIP()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();

            return "192.168.1.1";
        }

        // 📡 WYSYŁANIE KOMEND
        private async Task SendCommand(string ip, string command)
        {
            try
            {
                using TcpClient client = new TcpClient();
                await client.ConnectAsync(ip, 5000);

                byte[] data = Encoding.UTF8.GetBytes(command);
                await client.GetStream().WriteAsync(data, 0, data.Length);
            }
            catch
            {
                MessageBox.Show($"Brak połączenia z {ip}");
            }
        }

        // 🔘 BUTTON
        private async void Manage_Click(object sender, RoutedEventArgs e)
        {
            var pc = (sender as System.Windows.Controls.Button)?.DataContext as Workstation;

            if (pc != null)
            {
                ManageWindow window = new ManageWindow(pc, SendCommand);
                window.ShowDialog();
            }
        }
    }
}
