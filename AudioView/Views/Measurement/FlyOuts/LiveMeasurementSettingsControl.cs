using System;
using System.Windows.Input;
using AudioView.Views.Measurement;
using Prism.Commands;
using Prism.Mvvm;

namespace AudioView.ViewModels
{
    public class LiveMeasurementSettingsViewModel : BindableBase
    {
        private MeasurementsViewModel parentViewModel;
        private MeasurementViewModel measurementViewModel;

        public LiveMeasurementSettingsViewModel(MeasurementsViewModel parentViewModel, MeasurementViewModel measurementViewModel)
        {
            this.parentViewModel = parentViewModel;
            this.measurementViewModel = measurementViewModel;

            this._graphBoundLower = measurementViewModel.Settings.GraphLowerBound;
            this._graphBoundUpper = measurementViewModel.Settings.GraphUpperBound;
            this._MinordBLimit = measurementViewModel.Settings.MinorDBLimit;
            this._MajordBLimit = measurementViewModel.Settings.MajorDBLimit;
        }

        private int _graphBoundLower;
        public string GraphBoundLower
        {
            get { return _graphBoundLower.ToString(); }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    value = "0";
                }
                int tryParse;
                if (int.TryParse(value, out tryParse) && tryParse < _graphBoundUpper)
                {
                    SetProperty(ref _graphBoundLower, tryParse);
                }
                MinorDBLimit = MinorDBLimit;
                MajorDBLimit = MajorDBLimit;
            }
        }

        private int _graphBoundUpper;
        public string GraphBoundUpper
        {
            get { return _graphBoundUpper.ToString(); }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    value = "0";
                }
                int tryParse;
                if (int.TryParse(value, out tryParse) && tryParse > _graphBoundLower)
                {
                    SetProperty(ref _graphBoundUpper, tryParse);
                }
                MinorDBLimit = MinorDBLimit;
                MajorDBLimit = MajorDBLimit;
            }
        }

        private int _MinordBLimit;
        public string MinorDBLimit
        {
            get { return _MinordBLimit.ToString(); }
            set
            {
                int tryParse;
                if (int.TryParse(value, out tryParse) && tryParse >= 10)
                {
                    SetProperty(ref _MinordBLimit, Math.Max(_graphBoundLower, Math.Min(_graphBoundUpper, tryParse)));
                }
            }
        }

        private int _MajordBLimit;
        public string MajorDBLimit
        {
            get { return _MajordBLimit.ToString(); }
            set
            {
                int tryParse;
                if (int.TryParse(value, out tryParse) && tryParse >= 10)
                {
                    SetProperty(ref _MajordBLimit, Math.Max(_graphBoundLower, Math.Min(_graphBoundUpper, tryParse)));
                }
            }
        }

        private ICommand _saveSettings;
        public ICommand SaveSettings
        {
            get
            {
                if (_saveSettings == null)
                {
                    _saveSettings = new DelegateCommand(() =>
                    {
                        measurementViewModel.Settings.GraphLowerBound = _graphBoundLower;
                        measurementViewModel.Settings.GraphUpperBound = _graphBoundUpper;
                        measurementViewModel.Settings.MinorDBLimit = _MinordBLimit;
                        measurementViewModel.Settings.MajorDBLimit = _MajordBLimit;
                        this.parentViewModel.SettingsChanged(measurementViewModel);
                    });
                }
                return _saveSettings;
            }
        }
    }
}
