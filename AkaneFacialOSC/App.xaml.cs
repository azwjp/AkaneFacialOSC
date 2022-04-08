using System.Windows;

namespace Azw.FacialOsc
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            _ = Controller.Instance.InitApp();
        }
    }
}
