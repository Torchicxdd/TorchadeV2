using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace TorchadeV2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TorchadeHwndHost? dxHost;
        private ScreenCapture _screenCapture = new ScreenCapture();

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void StartCaptureButton_Click(object sender, RoutedEventArgs e)
        {
            bool canScreenCapture = _screenCapture.OnInitialization();
            if (canScreenCapture)
            {
                await _screenCapture.StartCaptureAsync(this);
            }
            else
            {
                hostContainer.Children.Remove(dxHost);
                dxHost?.Dispose();
                dxHost = null;

                // Update button
                startCaptureButton.Content = "Start Capture";
            }
        }

        private void CreateHwndHost()
        {
            if (dxHost == null)
            {
                dxHost = new TorchadeHwndHost
                {
                    Width = hostContainer.Width,
                    Height = hostContainer.Height
                };

                hostContainer.Children.Add(dxHost);
                startCaptureButton.Content = "Stop Capture";
            }
        }
    }
}