using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace stalkerdesk
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // ObservableCollection automatycznie odświeża widok (DataGrid) w UI
        public ObservableCollection<Workstation> Computers { get; set; }
        private DispatcherTimer _globalTimer;

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
            SetupTimer();

            // Powiązanie danych z widokiem
            this.DataContext = this;
        }

        private void LoadData()
        {
            // Przykładowe dane początkowe
            Computers = new ObservableCollection<Workstation>
            {
                new Workstation { Name = "PC-01", IP = "192.168.1.10", TimeLeftSeconds = 3600 },
                new Workstation { Name = "PC-02", IP = "192.168.1.11", TimeLeftSeconds = 1800 },
                new Workstation { Name = "PC-03", IP = "192.168.1.12", TimeLeftSeconds = 0 }
            };
        }

        private void SetupTimer()
        {
            // Timer odświeżający czas co sekundę
            _globalTimer = new DispatcherTimer();
            _globalTimer.Interval = TimeSpan.FromSeconds(1);
            _globalTimer.Tick += (s, e) =>
            {
                foreach (var computer in Computers)
                {
                    computer.Update();
                }
            };
            _globalTimer.Start();
        }

        // Metoda obsługująca przycisk z tabeli (wymaga dodania Click="SetTime_Click" w XAML)
        private void SetTime_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            var workstation = button?.DataContext as Workstation;

            if (workstation != null)
            {
                // Tutaj można dodać okienko Dialog, na razie dodajemy 30 min na sztywno
                workstation.TimeLeftSeconds += 1800;
                MessageBox.Show($"Dodano czas dla {workstation.Name}");
            }
        }
    }

    // Klasa obiektowa reprezentująca komputer w sieci
    public class Workstation : System.ComponentModel.INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string IP { get; set; }

        private int _timeLeftSeconds;
        public int TimeLeftSeconds
        {
            get => _timeLeftSeconds;
            set
            {
                _timeLeftSeconds = value;
                OnPropertyChanged(nameof(TimeLeft));
            }
        }

        // Formatowanie czasu do wyświetlenia w tabeli
        public string TimeLeft => TimeLeftSeconds > 0
            ? TimeSpan.FromSeconds(TimeLeftSeconds).ToString(@"hh\:mm\:ss")
            : "ZABLOKOWANY";

        public void Update()
        {
            if (TimeLeftSeconds > 0)
            {
                TimeLeftSeconds--;
            }
        }

        // Mechanizm powiadamiania UI o zmianie danych
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }
}