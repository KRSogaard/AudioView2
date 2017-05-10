using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;

namespace AudioView.ViewModels
{
    public class MenuViewModel<T> : BindableBase
    {
        private string _header;
        private ObservableCollection<MenuViewModel<T>> _subItems;
        private Action<T> OnClickItem;
        private T ValueItem;

        public MenuViewModel(string header,
            T item,
            Action<T> onClickItem)
            : this(header, item, new List<MenuViewModel<T>>(), onClickItem)
        {

        }
        public MenuViewModel(string header,
            List<MenuViewModel<T>> subItems)
            : this(header, default(T), subItems, obj => { })
        {
        }
        private MenuViewModel(string header,
            T item,
            List<MenuViewModel<T>> subItems,
            Action<T> onClickItem)
        {
            Header = header;
            SubItems = new ObservableCollection<MenuViewModel<T>>(subItems);
            OnClickItem = onClickItem;
            ValueItem = item;
        }

        public string Header
        {
            get { OnPropertyChanged(nameof(SubItems)); return _header;  }
            set { SetProperty(ref _header, value); }
        }

        public ObservableCollection<MenuViewModel<T>> SubItems
        {
            get { return _subItems; }
            set { SetProperty(ref _subItems, value); }
        }

        private ICommand _menuItemCommand;
        public ICommand MenuItemCommand
        {
            get
            {
                if (_menuItemCommand == null)
                {
                    _menuItemCommand = new DelegateCommand(() =>
                    {
                        OnClickItem(ValueItem);
                    });
                }
                return _menuItemCommand;
            }
        }
    }
}
