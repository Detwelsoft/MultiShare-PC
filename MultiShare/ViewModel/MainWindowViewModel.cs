using MultiShare.Model;
using MultiShare.Server;
using MultiShare.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

		private MultiShareServer Server = new MultiShareServer();

		private int _selectedDeviceIndex = -1;
		/// <summary>
		/// Номер выбранного устройства (номер первого = 0).
		/// </summary>
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

		private ObservableCollection<Device> _devices = new ObservableCollection<Device>();
		/// <summary>
		/// Список подключённых устройств-клиентов.
		/// </summary>
		public ObservableCollection<Device> Devices
		{
			get { return _devices; }
		}

		public SimpleCommand RunServer { get; set; }

		public MainWindowViewModel()
		{
			RunServer = new SimpleCommand(async () =>
			{
				await Server.StartAsync();
			},
			() =>
			{
				return true;
			});
		}
	}
}
