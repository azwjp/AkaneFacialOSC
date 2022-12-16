/* Reference: http://grabacr.net/archives/1647 */
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Azw.FacialOsc.Properties;

namespace Azw.FacialOsc
{
    public class ResourceService : INotifyPropertyChanged
    {

        private static readonly ResourceService _current = new ResourceService();
        public static ResourceService Current
        {
            get { return _current; }
        }


        private readonly Resources _resources = new Resources();

        public Resources Resources
        {
            get { return _resources; }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ChangeCulture(CultureInfo culture)
        {
            Resources.Culture = culture;;
            CultureInfo.CurrentCulture = culture;

            RaisePropertyChanged(nameof(Resources));
        }
    }
}
