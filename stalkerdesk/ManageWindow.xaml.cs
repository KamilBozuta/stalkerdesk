using System;
using System.Threading.Tasks;
using System.Windows;

namespace stalkerdesk
{
    public partial class ManageWindow : Window
    {
        private Workstation _pc;
        private Func<string, string, Task> _sendCommand;

        public ManageWindow(Workstation pc, Func<string, string, Task> sendCommand)
        {
            InitializeComponent();
            _pc = pc;
            _sendCommand = sendCommand;

            PcName.Text = $"{pc.Name} ({pc.IP})";
        }

        private async void Shutdown_Click(object sender, RoutedEventArgs e)
        {
            await _sendCommand(_pc.IP, "shutdown");
        }

        private async void Restart_Click(object sender, RoutedEventArgs e)
        {
            await _sendCommand(_pc.IP, "restart");
        }

        private async void Lock_Click(object sender, RoutedEventArgs e)
        {
            await _sendCommand(_pc.IP, "lock");
        }
    }
}