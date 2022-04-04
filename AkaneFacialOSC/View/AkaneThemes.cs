using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using MahApps.Metro.Controls;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;

namespace Azw.FacialOsc.View
{
    public class AkaneThemes
    {

        public static readonly ITheme AkaneTheme = Theme.Create(Theme.Dark, AkaneColors.AkanePink, AkaneColors.AkanePink);
        public static readonly ITheme SimpleTheme = Theme.Create(Theme.Light, Colors.Black, Colors.DodgerBlue);

        public enum Themes {
            AkaneTheme, SimpleTheme,
        }

        static AkaneThemes() {
            AkaneTheme.PrimaryMid = new ColorPair(AkaneColors.AkanePink, AkaneColors.WhiteText);
            AkaneTheme.Selection = AkaneColors.WhiteText;
        }

        public static void Use(Themes themes)
        {
            switch (themes)
            {
                case Themes.AkaneTheme: UseAkaneTheme(); break;
                case Themes.SimpleTheme: UseSimpleTheme(); break;
                default: throw new UnexpectedEnumValueException(themes);
            }
        }

        public static void UseAkaneTheme()
        {
            var paletteHelper = new PaletteHelper();
            paletteHelper.SetTheme(AkaneTheme);
            Application.Current.Resources["WindowTitleBrush"] = AkaneColors.AkanePinkBrush;
            Application.Current.Resources["NonActiveWindowTitleBrush"] = AkaneColors.AkanePurpleBrush;
            Application.Current.Resources["WindowBorderBrush"] = AkaneColors.AkaneGreenBrush;
            foreach (Window window in Application.Current.Windows)
            {
                if (window is MetroWindow metro)
                {
                    metro.WindowTitleBrush = AkaneColors.AkanePinkBrush;
                    metro.NonActiveWindowTitleBrush = AkaneColors.AkanePurpleBrush;
                    metro.BorderBrush = AkaneColors.AkaneGreenBrush;
                }
            }
        }
        public static void UseSimpleTheme()
        {
            var paletteHelper = new PaletteHelper();
            paletteHelper.SetTheme(SimpleTheme);
            Application.Current.Resources["WindowTitleBrush"] = AkaneColors.BlackBrush;
            Application.Current.Resources["NonActiveWindowTitleBrush"] = AkaneColors.GrayAccentBrush;
            Application.Current.Resources["WindowBorderBrush"] = AkaneColors.BlueAccentBrush;
            foreach (Window window in Application.Current.Windows)
            {
                if (window is MetroWindow metro)
                {
                    metro.WindowTitleBrush = AkaneColors.BlackBrush;
                    metro.NonActiveWindowTitleBrush = AkaneColors.GrayAccentBrush;
                    metro.BorderBrush = AkaneColors.BlueAccentBrush;
                }
            }
        }
    }
}
