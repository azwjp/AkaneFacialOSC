using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Text.Json.Serialization;

namespace Azw.FacialOsc.Model
{
    public class Rows
    {
        public IDictionary<FaceKey, SignalProperty> originalList = new Dictionary<FaceKey, SignalProperty>();

        public Rows()
        {
            foreach (FaceKey key in Enum.GetValues(typeof(FaceKey)))
            {
                var faceVal = new SignalProperty();
                faceVal.Key = key;
                if (FaceKeyUtils.GetDataType(key) == DataType.Gaze) faceVal.ValueRange = ValueRange.ZeroCentered;

                originalList.Add(key, faceVal);
            }
        }
    }

    public class SignalProperty : ModelBase
    {
        public SignalProperty InitRow(FaceKey key, bool isOn, float gain, bool isClipping, ValueRange center)
        {
            Key = key;
            IsSending = isOn;
            Gain = gain;
            IsClipping = isClipping;
            ValueRange = center;
            return this;
        }

        public FaceKey Key { get; set; }

        private bool _isSending = true;
        public bool IsSending
        {
            get { return _isSending; }
            set
            {
                _isSending = value;
                base.OnPropertyChanged(nameof(IsSending));
            }
        }

        private bool _isClipping = true;
        public bool IsClipping
        {
            get { return _isClipping; }
            set
            {
                _isClipping = value;
                base.OnPropertyChanged(nameof(IsClipping));
            }
        }

        private double _value = 0;
        public double Value
        {
            get { return _value; }
            set
            {
                _value = value;
                base.OnPropertyChanged(nameof(Value));
            }
        }


        private float gain = 1;
        public float Gain
        {
            get { return gain; }
            set
            {
                gain = value;
                OnPropertyChanged(nameof(Gain));
            }
        }

        ValueRange _range = ValueRange.Fixed;
        public ValueRange ValueRange
        {
            get { return _range; }
            set
            {
                _range = value;
                OnPropertyChanged(nameof(ValueRange));
            }
        }

        public float CenterValue
        {
            get
            {
                return ValueRange switch
                {
                    ValueRange.Fixed => 0.5f,
                    ValueRange.HalfCentered => 0.5f,
                    ValueRange.ZeroCentered => 0,
                    _ => throw new UnexpectedEnumValueException(ValueRange),
                };
            }
        }
    }
    public class ModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
    public enum ValueRange
    {
        Fixed, ZeroCentered, HalfCentered
    }
}
