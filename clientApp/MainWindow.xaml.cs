using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using System.Windows.Shapes;
using System.Drawing;
using System.Drawing.Imaging;




namespace AgentWpf
{
    public partial class MainWindow : Window
    {
        private TcpListener _server;
        private CancellationTokenSource _timerCts;

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

            Log("Agent uruchomiony");

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
                using (client)
                using (var stream = client.GetStream())
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    string cmd = reader.ReadLine();

                    if (string.IsNullOrWhiteSpace(cmd))
                        return;

                    Log("CMD: " + cmd);

                    if (cmd == "screen")
                    {
                        SendScreenshot(stream);
                        return;
                    }

                    Execute(cmd);
                }
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
                string[] p = cmd.Split(' ');
                if (p.Length == 2 && int.TryParse(p[1], out int min))
                    StartTimer(min);
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
            _timerCts?.Cancel();
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
                        Process.Start("rundll32.exe", "user32.dll,LockWorkStation");
                    }
                }
                catch
                {
                    Log("Timer anulowany");
                }
            });
        }

        // ✅ SCREENSHOT (WPF sposób – bez System.Drawing)

        private void SendScreenshot(NetworkStream stream)
        {
            System.Drawing.Rectangle bounds = Screen.PrimaryScreen.Bounds;

            using (Bitmap bmp = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size);
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    bmp.Save(ms, ImageFormat.Jpeg);
                    byte[] img = ms.ToArray();

                    byte[] size = BitConverter.GetBytes(img.Length);
                    stream.Write(size, 0, size.Length);
                    stream.Write(img, 0, img.Length);
                }
            }

            Log("Wysłano screenshot");
        }



    }
}
