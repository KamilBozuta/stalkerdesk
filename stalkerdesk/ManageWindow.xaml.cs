using System;
using System.Threading.Tasks;
using System.Windows;

namespace stalkerdesk
{
    public partial class ManageWindow : Window
    {
        private Workstation _pc;
        private Func<string, string, Task> _sendCommand;
        private Func<string, Task> _showScreen;

        public ManageWindow(
            Workstation pc,
            Func<string, string, Task> sendCommand,
            Func<string, Task> showScreen)
        {
            InitializeComponent();
            _pc = pc;
            _sendCommand = sendCommand;
            _showScreen = showScreen;

            PcName.Text = pc.Name + " (" + pc.IP + ")";
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

        private async void Timer_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TimerBox.Text, out int minutes))
            {
                await _sendCommand(_pc.IP, "timer " + minutes);
                MessageBox.Show("Ustawiono timer: " + minutes + " min");
            }
            else
            {
                MessageBox.Show("Podaj poprawną liczbę");
            }
        }

        private async void Screen_Click(object sender, RoutedEventArgs e)
        {
            await _showScreen(_pc.IP);
        }
    }
}
