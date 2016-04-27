using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioView.ViewModels;
using Newtonsoft.Json;
using Prism.Mvvm;

namespace AudioView.Views.History
{
    public class DataGridDisplayViewModel : BindableBase
    {
        private static string settingsFile = "viewSettings.json";

        private static DataGridDisplayViewModel _instance;
        public static DataGridDisplayViewModel Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = DataGridDisplayViewModel.Load();
                }
                return _instance;
            }
        }

        public DataGridDisplayViewModel()
        {
            Task.Run(() =>
            {
                lock (Instance)
                {
                    this.PropertyChanged += (sender, args) =>
                    {
                        Save();
                    };
                }
            });
        }

        public static DataGridDisplayViewModel Load()
        {
            if (!File.Exists(settingsFile))
            {
                return new DataGridDisplayViewModel();
            }

            return JsonConvert.DeserializeObject<DataGridDisplayViewModel>(File.ReadAllText(settingsFile));
        }

        public void Save()
        {
            File.WriteAllText(settingsFile, JsonConvert.SerializeObject(this));
        }

        private bool _LAeq = true;
        public bool LAeq
        {
            get { return _LAeq; }
            set { SetProperty(ref _LAeq, value); }
        }
        private bool _LAMax = true;
        public bool LAMax
        {
            get { return _LAMax; }
            set { SetProperty(ref _LAMax, value); }
        }
        private bool _LAMin = true;
        public bool LAMin
        {
            get { return _LAMin; }
            set { SetProperty(ref _LAMin, value); }
        }
        private bool _LZMax = true;
        public bool LZMax
        {
            get { return _LZMax; }
            set { SetProperty(ref _LZMax, value); }
        }
        private bool _LZMin = true;
        public bool LZMin
        {
            get { return _LZMin; }
            set { SetProperty(ref _LZMin, value); }
        }
        private bool _OneOne_Hz6_3 = true;
        public bool OneOne_Hz6_3
        {
            get { return _OneOne_Hz6_3; }
            set { SetProperty(ref _OneOne_Hz6_3, value); }
        }
        private bool _OneOne_Hz8 = true;
        public bool OneOne_Hz8
        {
            get { return _OneOne_Hz8; }
            set { SetProperty(ref _OneOne_Hz8, value); }
        }
        private bool _OneOne_Hz10 = true;
        public bool OneOne_Hz10
        {
            get { return _OneOne_Hz10; }
            set { SetProperty(ref _OneOne_Hz10, value); }
        }
        private bool _OneOne_Hz12_5 = true;
        public bool OneOne_Hz12_5
        {
            get { return _OneOne_Hz12_5; }
            set { SetProperty(ref _OneOne_Hz12_5, value); }
        }
        private bool _OneOne_Hz16 = true;
        public bool OneOne_Hz16
        {
            get { return _OneOne_Hz16; }
            set { SetProperty(ref _OneOne_Hz16, value); }
        }
        private bool _OneOne_Hz20 = true;
        public bool OneOne_Hz20
        {
            get { return _OneOne_Hz20; }
            set { SetProperty(ref _OneOne_Hz20, value); }
        }
        private bool _OneOne_Hz25 = true;
        public bool OneOne_Hz25
        {
            get { return _OneOne_Hz25; }
            set { SetProperty(ref _OneOne_Hz25, value); }
        }
        private bool _OneOne_Hz31_5 = true;
        public bool OneOne_Hz31_5
        {
            get { return _OneOne_Hz31_5; }
            set { SetProperty(ref _OneOne_Hz31_5, value); }
        }
        private bool _OneOne_Hz40 = true;
        public bool OneOne_Hz40
        {
            get { return _OneOne_Hz40; }
            set { SetProperty(ref _OneOne_Hz40, value); }
        }
        private bool _OneOne_Hz50 = true;
        public bool OneOne_Hz50
        {
            get { return _OneOne_Hz50; }
            set { SetProperty(ref _OneOne_Hz50, value); }
        }
        private bool _OneOne_Hz63 = true;
        public bool OneOne_Hz63
        {
            get { return _OneOne_Hz63; }
            set { SetProperty(ref _OneOne_Hz63, value); }
        }
        private bool _OneOne_Hz80 = true;
        public bool OneOne_Hz80
        {
            get { return _OneOne_Hz80; }
            set { SetProperty(ref _OneOne_Hz80, value); }
        }
        private bool _OneOne_Hz100 = true;
        public bool OneOne_Hz100
        {
            get { return _OneOne_Hz100; }
            set { SetProperty(ref _OneOne_Hz100, value); }
        }
        private bool _OneOne_Hz125 = true;
        public bool OneOne_Hz125
        {
            get { return _OneOne_Hz125; }
            set { SetProperty(ref _OneOne_Hz125, value); }
        }
        private bool _OneOne_Hz160 = true;
        public bool OneOne_Hz160
        {
            get { return _OneOne_Hz160; }
            set { SetProperty(ref _OneOne_Hz160, value); }
        }
        private bool _OneOne_Hz200 = true;
        public bool OneOne_Hz200
        {
            get { return _OneOne_Hz200; }
            set { SetProperty(ref _OneOne_Hz200, value); }
        }
        private bool _OneOne_Hz250 = true;
        public bool OneOne_Hz250
        {
            get { return _OneOne_Hz250; }
            set { SetProperty(ref _OneOne_Hz250, value); }
        }
        private bool _OneOne_Hz315 = true;
        public bool OneOne_Hz315
        {
            get { return _OneOne_Hz315; }
            set { SetProperty(ref _OneOne_Hz315, value); }
        }
        private bool _OneOne_Hz400 = true;
        public bool OneOne_Hz400
        {
            get { return _OneOne_Hz400; }
            set { SetProperty(ref _OneOne_Hz400, value); }
        }
        private bool _OneOne_Hz500 = true;
        public bool OneOne_Hz500
        {
            get { return _OneOne_Hz500; }
            set { SetProperty(ref _OneOne_Hz500, value); }
        }
        private bool _OneOne_Hz630 = true;
        public bool OneOne_Hz630
        {
            get { return _OneOne_Hz630; }
            set { SetProperty(ref _OneOne_Hz630, value); }
        }
        private bool _OneOne_Hz800 = true;
        public bool OneOne_Hz800
        {
            get { return _OneOne_Hz800; }
            set { SetProperty(ref _OneOne_Hz800, value); }
        }
        private bool _OneOne_Hz1000 = true;
        public bool OneOne_Hz1000
        {
            get { return _OneOne_Hz1000; }
            set { SetProperty(ref _OneOne_Hz1000, value); }
        }
        private bool _OneOne_Hz1250 = true;
        public bool OneOne_Hz1250
        {
            get { return _OneOne_Hz1250; }
            set { SetProperty(ref _OneOne_Hz1250, value); }
        }
        private bool _OneOne_Hz1600 = true;
        public bool OneOne_Hz1600
        {
            get { return _OneOne_Hz1600; }
            set { SetProperty(ref _OneOne_Hz1600, value); }
        }
        private bool _OneOne_Hz2000 = true;
        public bool OneOne_Hz2000
        {
            get { return _OneOne_Hz2000; }
            set { SetProperty(ref _OneOne_Hz2000, value); }
        }
        private bool _OneOne_Hz2500 = true;
        public bool OneOne_Hz2500
        {
            get { return _OneOne_Hz2500; }
            set { SetProperty(ref _OneOne_Hz2500, value); }
        }
        private bool _OneOne_Hz3150 = true;
        public bool OneOne_Hz3150
        {
            get { return _OneOne_Hz3150; }
            set { SetProperty(ref _OneOne_Hz3150, value); }
        }
        private bool _OneOne_Hz4000 = true;
        public bool OneOne_Hz4000
        {
            get { return _OneOne_Hz4000; }
            set { SetProperty(ref _OneOne_Hz4000, value); }
        }
        private bool _OneOne_Hz5000 = true;
        public bool OneOne_Hz5000
        {
            get { return _OneOne_Hz5000; }
            set { SetProperty(ref _OneOne_Hz5000, value); }
        }
        private bool _OneOne_Hz6300 = true;
        public bool OneOne_Hz6300
        {
            get { return _OneOne_Hz6300; }
            set { SetProperty(ref _OneOne_Hz6300, value); }
        }
        private bool _OneOne_Hz8000 = true;
        public bool OneOne_Hz8000
        {
            get { return _OneOne_Hz8000; }
            set { SetProperty(ref _OneOne_Hz8000, value); }
        }
        private bool _OneOne_Hz10000 = true;
        public bool OneOne_Hz10000
        {
            get { return _OneOne_Hz10000; }
            set { SetProperty(ref _OneOne_Hz10000, value); }
        }
        private bool _OneOne_Hz12500 = true;
        public bool OneOne_Hz12500
        {
            get { return _OneOne_Hz12500; }
            set { SetProperty(ref _OneOne_Hz12500, value); }
        }
        private bool _OneOne_Hz16000 = true;
        public bool OneOne_Hz16000
        {
            get { return _OneOne_Hz16000; }
            set { SetProperty(ref _OneOne_Hz16000, value); }
        }
        private bool _OneOne_Hz20000 = true;
        public bool OneOne_Hz20000
        {
            get { return _OneOne_Hz20000; }
            set { SetProperty(ref _OneOne_Hz20000, value); }
        }
    }
}
