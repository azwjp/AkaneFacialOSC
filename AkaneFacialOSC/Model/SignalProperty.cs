using System;
using System.Collections.Generic;
using System.ComponentModel;
using Azw.FacialOsc.Common;

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
                if (FaceKeyUtils.GetDataType(key) == DataType.Gaze) faceVal.ValueRange = ValueRange.MinusOneToOne;

                originalList.Add(key, faceVal);
            }
        }
    }

    public class SignalProperty : ModelBase
    {
        public SignalProperty InitRow(FaceKey key, bool isOn, float gain, float curve, bool isClipping, ValueRange center)
        {
            Key = key;
            IsSending = isOn;
            Gain = gain;
            Curve = curve;
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

        private float curce = 1;
        public float Curve
        {
            get { return curce; }
            set
            {
                curce = value;
                OnPropertyChanged(nameof(Curve));
            }
        }

        ValueRange _range = ValueRange.ZeroToOne;
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
                    ValueRange.ZeroToOne => 0.5f,
                    ValueRange.MinusOneToOne => 0,
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

}
