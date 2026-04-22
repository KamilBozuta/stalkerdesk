using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace stalkerdesk
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<Workstation> Computers { get; set; }
            = new ObservableCollection<Workstation>();

        private DispatcherTimer _timer;
        private int _timeLeft;
        private Workstation _selectedPC;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            // 🔥 TIMER INIT
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;

            Loaded += async (s, e) =>
            {
                await Task.Run(ScanAllNetworks);
            };
        }

        // =========================
        // ⏱️ TIMER LOGIC
        // =========================
        private async void Timer_Tick(object sender, EventArgs e)
        {
            _timeLeft--;

            Debug.WriteLine($"Pozostało: {_timeLeft}s");

            if (_timeLeft <= 0)
            {
                _timer.Stop();

                if (_selectedPC != null)
                {
                    await SendCommand(_selectedPC.IP, "shutdown");
                    MessageBox.Show($"Komputer {_selectedPC.IP} został wyłączony.");
                }
            }
        }

        private void StartTimer_Click(object sender, RoutedEventArgs e)
        {
            var pc = (sender as FrameworkElement)?.DataContext as Workstation;

            if (pc == null)
                return;

            _selectedPC = pc;

            int minutes = 5; // 👉 możesz podpiąć textbox
            _timeLeft = minutes * 60;

            _timer.Start();

            MessageBox.Show($"Timer ustawiony na {minutes} minut dla {pc.IP}");
        }

        private void StopTimer_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            MessageBox.Show("Timer zatrzymany");
        }

        // =========================
        // SCAN NETWORK
        // =========================
        private async Task ScanAllNetworks()
        {
            try
            {
                var arpTask = Task.Run(GetArpIPs);

                string subnet = GetSubnet(GetLocalIP());

                var subnets = new[]
                {
                    subnet,
                    "10.10.10.",
                    "192.168.1.",
                    "192.168.0."
                };

                SemaphoreSlim sem = new SemaphoreSlim(50);

                var tasks = subnets
                    .Distinct()
                    .SelectMany(s =>
                        Enumerable.Range(1, 254).Select(async i =>
                        {
                            await sem.WaitAsync();
                            try
                            {
                                string ip = s + i;
                                await CheckIP(ip);
                            }
                            finally
                            {
                                sem.Release();
                            }
                        }))
                    .ToList();

                await Task.WhenAll(tasks);

                foreach (var ip in await arpTask)
                    AddComputer(ip);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Scan error: " + ex.Message);
            }
        }

        // =========================
        // ARP
        // =========================
        private System.Collections.Generic.List<string> GetArpIPs()
        {
            var list = new System.Collections.Generic.List<string>();

            try
            {
                Process p = new Process();
                p.StartInfo.FileName = "arp";
                p.StartInfo.Arguments = "-a";
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;

                p.Start();

                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();

                var matches = Regex.Matches(output, @"(\d+\.\d+\.\d+\.\d+)");

                foreach (Match m in matches)
                    list.Add(m.Value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ARP error: " + ex.Message);
            }

            return list;
        }

        // =========================
        // CHECK IP
        // =========================
        private async Task CheckIP(string ip)
        {
            bool ping = await PingIP(ip);
            bool port = await IsPortOpen(ip, 5000);

            Debug.WriteLine($"{ip} ping={ping} port={port}");

            if (ping || port)
            {
                AddComputer(ip);
            }
        }

        private async Task<bool> PingIP(string ip)
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    var r = await ping.SendPingAsync(ip, 300);
                    return r.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> IsPortOpen(string ip, int port)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    var t = client.ConnectAsync(ip, port);
                    var res = await Task.WhenAny(t, Task.Delay(300));

                    return res == t && client.Connected;
                }
            }
            catch
            {
                return false;
            }
        }

        // =========================
        // ADD DEVICE TO UI
        // =========================
        private void AddComputer(string ip)
        {
            Dispatcher.Invoke(() =>
            {
                if (!Computers.Any(c => c.IP == ip))
                {
                    Computers.Add(new Workstation
                    {
                        IP = ip,
                        Name = "Device"
                    });
                }
            });
        }

        // =========================
        // NETWORK HELPERS
        // =========================
        private string GetLocalIP()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();

            return "192.168.1.1";
        }

        private string GetSubnet(string ip)
        {
            int last = ip.LastIndexOf('.');
            return ip.Substring(0, last + 1);
        }

        // =========================
        // SEND COMMAND
        // =========================
        private async Task SendCommand(string ip, string command)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    await client.ConnectAsync(ip, 5000);

                    using (var stream = client.GetStream())
                    {
                        string msg = command + "\n";
                        byte[] data = Encoding.UTF8.GetBytes(msg);

                        await stream.WriteAsync(data, 0, data.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Brak połączenia z {ip}\n{ex.Message}");
            }
        }

        private async Task SendTimer(string ip, int minutes)
        {
            await SendCommand(ip, $"timer {minutes}");
        }

        // =========================
        // ACTIONS
        // =========================
        private async void Lock_Click(object sender, RoutedEventArgs e)
        {
            var pc = (sender as FrameworkElement)?.DataContext as Workstation;
            if (pc != null)
                await SendCommand(pc.IP, "lock");
        }

        private async void Restart_Click(object sender, RoutedEventArgs e)
        {
            var pc = (sender as FrameworkElement)?.DataContext as Workstation;
            if (pc != null)
                await SendCommand(pc.IP, "restart");
        }

        private async void Shutdown_Click(object sender, RoutedEventArgs e)
        {
            var pc = (sender as FrameworkElement)?.DataContext as Workstation;
            if (pc != null)
                await SendCommand(pc.IP, "shutdown");
        }

        // =========================
        // UI EVENTS
        // =========================

        private void Manage_Click(object sender, RoutedEventArgs e)
        {
            var pc = (sender as FrameworkElement)?.DataContext as Workstation;

            if (pc != null)
                new ManageWindow(pc, SendCommand, ShowScreen).ShowDialog();
        }


        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        public async Task ShowScreen(string ip)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    await client.ConnectAsync(ip, 5000);

                    using (NetworkStream stream = client.GetStream())
                    using (StreamWriter writer = new StreamWriter(stream) { AutoFlush = true })
                    {
                        await writer.WriteLineAsync("screen");

                        byte[] sizeBuf = new byte[4];
                        await stream.ReadAsync(sizeBuf, 0, 4);
                        int size = BitConverter.ToInt32(sizeBuf, 0);

                        byte[] img = new byte[size];
                        int read = 0;
                        while (read < size)
                            read += await stream.ReadAsync(img, read, size - read);

                        Dispatcher.Invoke(() =>
                        {
                            new ScreenViewWindow(img).Show();
                        });
                    }
                }
            }
            catch
            {
                MessageBox.Show("Błąd pobierania podglądu");
            }
        }
    }
}
