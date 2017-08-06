using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using AudioView.Annotations;
using AudioView.Common.Data;
using AudioView.Common.Engine;
using AudioView.UserControls.CountDown.ClockItems;
using AudioView.UserControls.Graphs;
using NLog;
using Prism.Commands;
using Prism.Mvvm;

namespace AudioView.UserControls.CountDown
{
    public class AudioViewCountDownViewModel : BindableBase, IMeterListener
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        protected bool isMajor;
        private ClockItem mainItem;
        private ClockItem secondItem;
        private int limitDb;
        private bool isPopOut;
        private MeasurementItemViewModel mainItemViewModel;
        private MeasurementItemViewModel secondaryItemViewModel;

        public SolidColorBrush BarBrush { get; set; }
        public SolidColorBrush BarOverBrush { get; set; }


        public AudioViewCountDownViewModel(bool isMajor, TimeSpan interval, int limitDb, Type mainItem, Type secondItem, bool isPopOut = false)
        {
            Interval = interval;
            this.isMajor = isMajor;
            this.limitDb = limitDb;
            this.mainItem = ClockItemsFactory.AllClockItems.First(x => x.GetType() == mainItem);
            this.secondItem = ClockItemsFactory.AllClockItems.First(x => x.GetType() == secondItem);
            this.isPopOut = isPopOut;

            mainItemViewModel = new MeasurementItemViewModel();
            secondaryItemViewModel = new MeasurementItemViewModel();

            ClockSelections = new ObservableCollection<ClockSelectionViewModel>();
            foreach (var clockItem in ClockItemsFactory.AllClockItems)
            {
                ClockSelections.Add(new ClockSelectionViewModel(this, clockItem));
            }
        }
        
        public TimeSpan Interval { get; set; }
        public DateTime NextReadingTime { get; set; }
        public DateTime LastReadingTime { get; set; }

        public ReadingData LastReading { get; set; }
        public ReadingData LastBuildingInterval { get; set; }
        public ReadingData LastInterval { get; set; }

        public string DisplayValue { get; set; }

        public ObservableCollection<ClockSelectionViewModel> ClockSelections { get; set; }

        private double _angle;
        public double Angle
        {
            get { return 360 - _angle; }
            set
            {
                SetProperty(ref _angle, value);
                OnPropertyChanged();
                UpdateValues();
                UpdateTextColor();
            }
        }

        private void UpdateTextColor()
        {
            if (LastReading == null)
            {
                SetProperty(ref _textColor, BarBrush, nameof(TextColor));
                return;
            }
            
            SetProperty(ref _textColor, mainItem.IsReadingOverLimit(limitDb) ? BarOverBrush : BarBrush, nameof(TextColor));
            SetProperty(ref _textColorSecondary, secondItem.IsReadingOverLimit(limitDb) ? BarOverBrush : BarBrush, nameof(TextColorSecondary));
        }

        private void UpdateValues()
        {
            var data = new ClockItemData()
            {
                LastReading = LastReading,
                LastInterval = LastInterval,
                NextReadingTime = NextReadingTime,
                LastBuilding = LastBuildingInterval
            };
            mainItem.SetValues(mainItemViewModel, data);
            secondItem.SetValues(secondaryItemViewModel, data);

            mainItemViewModel.TextColor = TextColor;
            secondaryItemViewModel.TextColor = TextColorSecondary;
        }

        public bool ShowArch
        {
            // They do not want the countdown on the pop outs
            get { return !isPopOut; }
        }

        private int _arcThickness;
        public int ArcThickness
        {
            get
            {
                return _arcThickness;
            }
             set
            {
                _arcThickness = value; OnPropertyChanged();
            }
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; OnPropertyChanged(); }
        }

        public MeasurementItemViewModel MainIteam
        {
            get { return mainItemViewModel; }
        }

        public MeasurementItemViewModel SecondIteam
        {
            get { return secondaryItemViewModel; }
        }

        private Brush _textColor;
        public Brush TextColor => _textColor;

        private Brush _textColorSecondary;
        public Brush TextColorSecondary => _textColorSecondary;

        public void OnNext(DateTime time)
        {
            LastReadingTime = time.AddMilliseconds(-Interval.TotalMilliseconds);
            NextReadingTime = time;
        }

        #region IMeterListener
        public Task OnMinor(DateTime time, DateTime starTime, ReadingData data)
        {
            return Task.Run(() =>
            {
                if (!isMajor)
                    LastInterval = data;
            });
        }

        public Task OnMajor(DateTime time, DateTime starTime, ReadingData data)
        {
            return Task.Run(() =>
            {
                if (isMajor)
                    LastInterval = data;
            });
        }

        public Task OnSecond(DateTime time, DateTime starTime, ReadingData data, ReadingData minorData, ReadingData majorData)
        {
            return Task.Run(() =>
            {
                LastReading = data;
                LastBuildingInterval = isMajor ? majorData : minorData;
            });
        }

        public Task NextMinor(DateTime time)
        {
            return Task.Run(() =>
            {
                if(!isMajor)
                    OnNext(time);
            });
        }

        public Task NextMajor(DateTime time)
        {
            return Task.Run(() =>
            {
                logger.Info("Next interval: " + time + " Major: " + isMajor);
                if (isMajor)
                    OnNext(time);
            });
        }

        public Task StopListener()
        {
            return Task.FromResult<object>(null);
        }

        #endregion

        public void ChangeMainDisplayItem(ClockItem clodItem)
        {
            mainItem = clodItem;
        }
        public void ChangeSecondayDisplayItem(ClockItem clodItem)
        {
            secondItem = clodItem;
        }
        public void ChangeLimitDb(int limitDb)
        {
            this.limitDb = limitDb;
        }
    }

    public class ClockSelectionViewModel : BindableBase
    {
        private AudioViewCountDownViewModel parent;
        private ClockItem clockItem;

        public ClockSelectionViewModel(AudioViewCountDownViewModel parent, ClockItem clockItem)
        {
            this.parent = parent;
            this.clockItem = clockItem;
        }

        public string Name => "Set to " + clockItem.Name;

        private ICommand _switchCommand;
        public ICommand SwitchCommand
        {
            get
            {
                if (_switchCommand == null)
                {
                    _switchCommand = new DelegateCommand(() =>
                    {
                        parent.ChangeMainDisplayItem(clockItem);
                    });
                }
                return _switchCommand;
            }
        }

        private ICommand _switchSecondayCommand;
        public ICommand SwitchSecondayCommand
        {
            get
            {
                if (_switchSecondayCommand == null)
                {
                    _switchSecondayCommand = new DelegateCommand(() =>
                    {
                        parent.ChangeSecondayDisplayItem(clockItem);
                    });
                }
                return _switchSecondayCommand;
            }
        }

        public ObservableCollection<ClockSelectionViewModel> SubItems { get; set; }
    }
}
