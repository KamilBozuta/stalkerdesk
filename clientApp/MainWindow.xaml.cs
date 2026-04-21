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
        private bool isRunning = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void StartStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isRunning)
            {
                StartServer();
                StartStopButton.Content = "STOP";
                StartStopButton.Background = System.Windows.Media.Brushes.Red;
                isRunning = true;
            }
            else
            {
                StopServer();
                StartStopButton.Content = "START";
                StartStopButton.Background = System.Windows.Media.Brushes.Green;
                isRunning = false;
            }
        }

        private void StartServer()
        {
            cts = new CancellationTokenSource();
            server = new TcpListener(IPAddress.Any, 5000);
            server.Start();

            Task.Run(() => ListenLoop(cts.Token));
        }

        private void StopServer()
        {
            try
            {
                cts.Cancel();
                server.Stop();
            }
            catch { }
        }

        private async Task ListenLoop(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var client = await server.AcceptTcpClientAsync();

                    _ = Task.Run(() => HandleClient(client), token);
                }
            }
            catch { }
        }

        private void HandleClient(TcpClient client)
        {
            try
            {
                var stream = client.GetStream();
                byte[] buffer = new byte[1024];

                int bytes = stream.Read(buffer, 0, buffer.Length);
                string cmd = Encoding.UTF8.GetString(buffer, 0, bytes).Trim();

                Console.WriteLine("CMD: " + cmd);
                Execute(cmd);

                client.Close();
            }
            catch { }
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