using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static void Main()
    {
        TcpListener server = new TcpListener(IPAddress.Any, 5000);
        server.Start();

        Console.WriteLine("Agent działa...");

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();

            Task.Run(() =>
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
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
        }
    }

    static void Execute(string cmd)
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