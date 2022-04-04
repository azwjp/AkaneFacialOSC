using System.Windows.Media;

namespace Azw.FacialOsc.View
{
    internal class AkaneColors
    {
        #region AkaneTheme
        public static readonly Color AkanePink = Color.FromRgb(0xE0, 0x5A, 0x8D);
        public static readonly SolidColorBrush AkanePinkBrush = new SolidColorBrush() { Color = AkanePink };
        public static readonly Color AkanePurple = Color.FromRgb(0x81, 0x2A, 0x5D);
        public static readonly SolidColorBrush AkanePurpleBrush = new SolidColorBrush() { Color = AkanePurple };
        public static readonly Color AkaneGreen = Color.FromRgb(0xAB, 0xCA, 0x79);
        public static readonly SolidColorBrush AkaneGreenBrush = new SolidColorBrush() { Color = AkaneGreen };
        public static readonly Color WhiteText = Color.FromRgb(0xDD, 0xFF, 0xFF);
        #endregion

        #region SimpleTheme
        public static readonly SolidColorBrush BlackBrush = new SolidColorBrush() { Color = Colors.Black };
        public static readonly Color BlueAccent = Color.FromRgb(0x00, 0x78, 0xD7);
        public static readonly SolidColorBrush BlueAccentBrush = new SolidColorBrush() { Color = BlueAccent };
        public static readonly Color GrayAccent = Color.FromRgb(0x80, 0x80, 0x80);
        public static readonly SolidColorBrush GrayAccentBrush = new SolidColorBrush() { Color = GrayAccent };
        #endregion

        #region StatusColor
        public static readonly Color Unavailable = Color.FromRgb(253, 251, 159);
        public static readonly Color Disbled = Color.FromRgb(238, 62, 91);
        public static readonly Color Starting = Color.FromRgb(160, 165, 250);
        public static readonly Color Running = Color.FromRgb(139, 235, 161);
        public static readonly Color Stopping = Color.FromRgb(160, 165, 250);

        public static readonly SolidColorBrush UnavailableBrush = new SolidColorBrush() { Color = Unavailable };
        public static readonly SolidColorBrush DisbledBrush = new SolidColorBrush() { Color = Disbled };
        public static readonly SolidColorBrush StartingBrush = new SolidColorBrush() { Color = Starting };
        public static readonly SolidColorBrush RunningBrush = new SolidColorBrush() { Color = Running };
        public static readonly SolidColorBrush StoppingBrush = new SolidColorBrush() { Color = Stopping };
        #endregion
    }
}
