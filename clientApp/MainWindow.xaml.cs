using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace clientApp
{
    public partial class MainWindow : Window
    {
        private TcpListener server;
        private CancellationTokenSource cts;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            cts = new CancellationTokenSource();
            Task.Run(() => StartServer(cts.Token));
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            try
            {
                cts.Cancel();
                server?.Stop();
            }
            catch { }
        }

        private void StartServer(CancellationToken token)
        {
            try
            {
                server = new TcpListener(IPAddress.Any, 5000);
                server.Start();

                Dispatcher.Invoke(() =>
                {
                    StatusText.Text = "Agent działa...";
                });

                while (!token.IsCancellationRequested)
                {
                    if (!server.Pending())
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    TcpClient client = server.AcceptTcpClient();

                    Task.Run(() => HandleClient(client));
                }
            }
            catch (Exception)
            {
                Dispatcher.Invoke(() =>
                {
                    StatusText.Text = "Błąd serwera";
                });
            }
        }

        private void HandleClient(TcpClient client)
        {
            try
            {
                var stream = client.GetStream();
                byte[] buffer = new byte[1024];

                int bytes = stream.Read(buffer, 0, buffer.Length);
                string cmd = Encoding.UTF8.GetString(buffer, 0, bytes).Trim();

                Execute(cmd);

                client.Close();
            }
            catch
            {
                client.Close();
            }
        }

        private void Execute(string cmd)
        {
            switch (cmd)
            {
                case "shutdown":
                    Process.Start("shutdown", "/s /t 0");
                    break;

                case "restart":
                    Process.Start("shutdown", "/r /t 0");
                    break;

                case "lock":
                    Process.Start("rundll32.exe", "user32.dll,LockWorkStation");
                    break;
            }
        }
    }
}
 
