using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AZW.FacialOSC
{
    public class Rows
    {
        public ICollection<SignalRow> originalList = new List<SignalRow>();

        public Rows()
        {
            foreach (FaceKey key in Enum.GetValues(typeof(FaceKey)))
            {
                var faceVal = new SignalRow();
                faceVal.Key = key;
                if (FaceKeyUtils.GetDataType(key) == DataType.Gaze) faceVal.Center = Center.Zero;

                originalList.Add(faceVal);
            }
        }
    }

    public class SignalRow : ModelBase
    {
        public void InitRow(FaceKey key, bool isOn, float gain, bool isClipping, Center center)
        {
            Key = key;
            IsSending = isOn;
            Gain = gain;
            IsClipping = isClipping;
            Center = center;
        }

        public FaceKey Key { get; set; }

        private double gain = 1;

        public bool IsSending { get; set; } = true;
        public bool IsClipping = true;

        public double Gain
        {
            get { return gain; }
            set { 
                gain = value;
                OnPropertyChanged(nameof(Gain));
            }
        }

        Center center = Center.Fixed;
        public Center Center
        {
            get { return center; }
            set {
                center = value;
                OnPropertyChanged(nameof(Center));
                switch (value){
                    case Center.Fixed:
                        CenterValue = 0.5;
                        return;
                    case Center.Zero:
                        CenterValue = 0;
                        return;
                    case Center.Half:
                        CenterValue = 0.5;
                        return;
                    default:
                        throw new NotImplementedException(nameof(value));
                }
            }
        }
        public double CenterValue = 0.5;
    }
    public class ModelBase : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
    }
    public enum Center
    {
        Fixed, Zero, Half,
    }

    public class CenterToLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not Center center) return DependencyProperty.UnsetValue;

            switch (center)
            {
                case Center.Fixed:
                case Center.Half:
                    return "0..1";
                case Center.Zero:
                    return "-1..1";
                default:
                    throw new NotImplementedException(nameof(value));
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is not string str) return DependencyProperty.UnsetValue;

            return Enum.Parse<Center>(str);
        }
    }
    public class CenterToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not Center center) return DependencyProperty.UnsetValue;

            switch (center)
            {
                case Center.Fixed:
                    return Visibility.Collapsed;
                case Center.Half:
                case Center.Zero:
                    return Visibility.Visible;
                default:
                    throw new NotImplementedException(nameof(value));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is not Visibility v) return DependencyProperty.UnsetValue;

            return v == Visibility.Collapsed ? Center.Fixed : Center.Half;
        }
    }
}
