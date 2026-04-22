using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AgentWpf
{
    public partial class MainWindow : Window
    {
        private CancellationTokenSource _timerCts;
        private TcpListener _server;

        public MainWindow()
        {
            InitializeComponent();
            StartServer();
        }

        private void Log(string text)
        {
            Dispatcher.Invoke(() =>
            {
                LogBox.AppendText(text + Environment.NewLine);
                LogBox.ScrollToEnd();
            });
        }

        private void StartServer()
        {
            _server = new TcpListener(IPAddress.Any, 5000);
            _server.Start();

            Log("Agent działa...");

            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        var client = await _server.AcceptTcpClientAsync();
                        Task.Run(() => HandleClient(client));
                    }
                    catch (Exception ex)
                    {
                        Log(ex.Message);
                    }
                }
            });
        }

        private void HandleClient(TcpClient client)
        {
            try
            {
                using (var stream = client.GetStream())
                using (var reader = new System.IO.StreamReader(stream, Encoding.UTF8))
                {
                    string cmd = reader.ReadLine();
                    if (cmd != null)
                        cmd = cmd.Trim();

                    if (string.IsNullOrEmpty(cmd))
                        return;

                    Log("CMD: " + cmd);
                    Execute(cmd);
                }

                client.Close();
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }

        private void Execute(string cmd)
        {
            if (cmd.StartsWith("timer"))
            {
                var parts = cmd.Split(' ');
                int minutes;

                if (parts.Length == 2 && int.TryParse(parts[1], out minutes))
                    StartTimer(minutes);

                return;
            }

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

        private void StartTimer(int minutes)
        {
            if (_timerCts != null)
                _timerCts.Cancel();

            _timerCts = new CancellationTokenSource();
            var token = _timerCts.Token;

            Log("Timer: " + minutes + " min");

            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(minutes), token);

                    if (!token.IsCancellationRequested)
                    {
                        Log("Czas minął → LOCK");
                        Process.Start("rundll32.exe", "user32.dll,LockWorkStation");
                    }
                }
                catch (TaskCanceledException)
                {
                    Log("Timer anulowany");
                }
            });
        }
    }
}
