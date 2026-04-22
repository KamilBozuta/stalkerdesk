using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace stalkerdesk
{
    public partial class ScreenViewWindow : Window
    {
        public ScreenViewWindow(byte[] image)
        {
            InitializeComponent();

            using (MemoryStream ms = new MemoryStream(image))
            {
                BitmapImage bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = ms;
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();

                ScreenImage.Source = bmp;
            }
        }
    }
}
