using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MultiShare.ViewModel
{
	public class DevicesListViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private int _selectedDeviceIndex = -1;
		public int SelectedDeviceIndex
		{
			get { return _selectedDeviceIndex; }
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(SelectedDeviceIndex), value, "Device index cannot be less than 0");
				}

				if (_selectedDeviceIndex != value)
				{
					_selectedDeviceIndex = value;
					OnPropertyChanged();
				}
			}
		}
	}
}
