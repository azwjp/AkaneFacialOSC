using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using ViveSR.anipal.Eye;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AZW.FacialOSC
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        bool isDirty = false;
        Rows rows = new Rows();

        public MainWindow()
        {
            InitializeComponent();
            var a =Preferences.Load();

            SignalList.DataContext = new ObservableCollection<SignalRow>(rows.originalList);

        }

        void GainValue_Inputted(object sender, RoutedEventArgs e)
        {
            var ui = sender as TextBox;
            var row = ui.DataContext as SignalRow;
        }

        private void Slider_Gain_ValueChanged(object sender, RoutedEventArgs e)
        {
            var ui = sender as Slider;
            var row = ui.DataContext as SignalRow;
        }
        public ObservableCollection<SignalRow> Items { get; private set; }
            = new ObservableCollection<SignalRow>();

        private void Button_ContextCanceled(UIElement sender, RoutedEventArgs args)
        {

        }
        private void centerButton_Click(object sender, RoutedEventArgs e)
        {
            var ui = sender as Button;
            var row = ui.DataContext as SignalRow;

            switch (row.Center)
            {
                case Center.Fixed:
                    return;
                case Center.Half:
                    row.Center = Center.Zero;
                    break;
                case Center.Zero:
                    row.Center = Center.Half;
                    break;
                default:
                    throw new NotImplementedException();
            }

            //ui.Content = row.Center;
        }
        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            ViveSR.anipal.SRanipal_API.Initial(SRanipal_Eye.ANIPAL_TYPE_EYE_V2, IntPtr.Zero);
            myButton.Content = "Clicked";
        }

        private void myButton_Click3(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
    public sealed class AccessibleButton : Control
    {
        public AccessibleButton()
        {
            DefaultStyleKey = typeof(AccessibleButton);
        }

        public static DependencyProperty LabelProperty = DependencyProperty.Register(
            "Label", typeof(string), typeof(AccessibleButton),
            PropertyMetadata.Create(string.Empty));

        public string Label
        {
            set { SetValue(LabelProperty, value); }
            get { return (string)GetValue(LabelProperty); }
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            Click?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Click;
    }
}
