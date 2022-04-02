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

namespace AZW.FacialOSC.Model
{
    public class Rows
    {
        public IDictionary<FaceKey, SignalRow> originalList = new Dictionary<FaceKey, SignalRow>();

        public Rows()
        {
            foreach (FaceKey key in Enum.GetValues(typeof(FaceKey)))
            {
                var faceVal = new SignalRow();
                faceVal.Key = key;
                if (FaceKeyUtils.GetDataType(key) == DataType.Gaze) faceVal.ValueRange = Range.ZeroCentered;

                originalList.Add(key, faceVal);
            }
        }
    }

    public class SignalRow : ModelBase
    {
        public SignalRow InitRow(FaceKey key, bool isOn, double gain, bool isClipping, Range center)
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
                base.OnPropertyChanged(nameof(_isSending));
            }
        }

        private bool _isClipping = true;
        public bool IsClipping
        {
            get { return _isClipping; }
            set
            {
                _isClipping = value;
                base.OnPropertyChanged(nameof(_isClipping));
            }
        }

        private double _value = 0;
        public double Value
        {
            get { return _value; }
            set
            {
                _value = value;
                base.OnPropertyChanged(nameof(_value));
            }
        }


        private double gain = 1;
        public double Gain
        {
            get { return gain; }
            set
            {
                gain = value;
                OnPropertyChanged(nameof(Gain));
            }
        }

        Range _range = Range.Fixed;
        public Range ValueRange
        {
            get { return _range; }
            set
            {
                _range = value;
                OnPropertyChanged(nameof(ValueRange));
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
    public enum Range
    {
        Fixed, ZeroCentered, HalfCentered,
    }
}
