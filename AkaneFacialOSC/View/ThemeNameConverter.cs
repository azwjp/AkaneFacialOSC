using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Azw.FacialOsc.Properties;
using MaterialDesignThemes.Wpf;

namespace Azw.FacialOsc.View
{
    [ValueConversion(typeof(AkaneThemes.Themes), typeof(string))]
    public class ThemeNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is not AkaneThemes.Themes theme) return DependencyProperty.UnsetValue;

            return theme switch
            {
                AkaneThemes.Themes.AkaneTheme => Resources.ThemeAkane,
                AkaneThemes.Themes.SimpleTheme => Resources.ThemeSimple,
                _ => throw new UnexpectedEnumValueException(theme),
            };
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is not string str) return DependencyProperty.UnsetValue;

            return Enum.Parse(typeof(AkaneThemes.Themes), str, true);
        }
    }
}
