using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using AudioView.ViewModels;
using AudioView.Views.History;
using MahApps.Metro.Controls;
using Prism.Commands;
using Prism.Mvvm;

namespace AudioView.Views.Measurement
{
    public class LiveGraphItemViewModel : BindableBase
    {
        private MeasurementViewModel parent;
        private string methodName;

        public LiveGraphItemViewModel(MeasurementViewModel parent, MethodInfo method)
        {
            this.parent = parent;
            methodName = method.Name;

            var name = methodName.Replace("_", ".");
            name = name.Replace("get_", "");
            if (name.Contains("Hz"))
            {
                name = name.Replace("Hz", "");
                name = name + " Hz";
            }
            Name = name;
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private ICommand _show;
        public ICommand Show
        {
            get
            {
                if (_show == null)
                {
                    _show = new DelegateCommand(() =>
                    {
                        parent.NewGraphReadingsPopUp(methodName);
                    });
                }
                return _show;
            }
        }
    }
}
