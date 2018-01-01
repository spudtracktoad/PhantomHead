using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.System;

namespace PhantomHead
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {        
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void btnRestart_Click(object sender, RoutedEventArgs e)
        {
            //Restart Device
            ShutdownManager.BeginShutdown(ShutdownKind.Restart, TimeSpan.FromSeconds(3));
        }

        private void btnShutodown_Click(object sender, RoutedEventArgs e)
        {
            //shutdown the device
            ShutdownManager.BeginShutdown(ShutdownKind.Shutdown, TimeSpan.FromSeconds(2));

        }

        private void btnOpenAd5360Tests_Click(object sender, RoutedEventArgs e)
        {

            AD5360_TestPanel testPanel;// = new AD5360_TestPanel();
            this.Frame.Navigate(typeof(AD5360_TestPanel));//, testPanel);

        }
    }
}
