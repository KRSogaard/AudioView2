using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using AudioView.Annotations;
using Prism.Mvvm;

namespace AudioView.UserControls.CountDown
{
    public class MeasurementItemViewModel : BindableBase
    {
        private string value;
        private string unit;
        private string measurement;
        private Brush textColor;

        private bool _hasUnit;
        public bool HasUnit
        {
            get { return _hasUnit; }
            set { SetProperty(ref _hasUnit, value); }
        }

        private bool _hasMeasurement;
        public bool HasMeasurement
        {
            get { return _hasMeasurement; }
            set { SetProperty(ref _hasMeasurement, value); }
        }

        public string Value
        {
            set
            {
                this.SetProperty(ref this.value, value);
            }
            get { return value; }
        }

        public string Unit
        {
            set
            {
                this.SetProperty(ref unit, value);
                HasUnit = !String.IsNullOrWhiteSpace(unit);
            }
            get { return unit; }
        }

        public string Measurement
        {
            set
            {
                SetProperty(ref measurement, value);
                HasMeasurement = !String.IsNullOrWhiteSpace(measurement);
            }
            get { return measurement; }
        }

        public Brush TextColor
        {
            set { SetProperty(ref textColor, value); }
            get { return textColor; }
        }

        public void Clear()
        {
            Value = "";
            Unit = "";
            Measurement = "";
        }

        public void NoValue()
        {
            Value = "N/A";
            Unit = "";
            Measurement = "";
        }
    }
}
