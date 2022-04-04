using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Azw.FacialOsc.View;
using ControlzEx.Theming;

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
