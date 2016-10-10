using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MultiShare.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

		private DevicesListViewModel _devicesListViewModel = new DevicesListViewModel();
		public DevicesListViewModel DevicesListViewModel
		{
			get
			{
				return _devicesListViewModel;
			}
			set
			{
				if (value != null && _devicesListViewModel != value)
				{
					_devicesListViewModel = value;
					OnPropertyChanged();
				}
			}
		}
    }
}
