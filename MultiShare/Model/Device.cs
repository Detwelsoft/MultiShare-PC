﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MultiShare.Model
{
	public class Device : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private IPEndPoint _address;
		/// <summary>
		/// Сетевой адрес устройства-клиента.
		/// </summary>
		public IPEndPoint Address
		{
			get { return _address; }
			set
			{
				if (_address != value)
				{
					_address = value;
					OnPropertyChanged();
				}
			}
		}

		private PhysicalAddress _mac;
		/// <summary>
		/// Физический адрес устройства-клиента.
		/// </summary>
		public PhysicalAddress MAC
		{
			get	{ return _mac; }
			set
			{
				if (_mac != value)
				{
					_mac = value;
					OnPropertyChanged();
				}
			}
		}

		public Device(IPEndPoint address, PhysicalAddress MAC)
		{
			Address = address;
			this.MAC = MAC;
		}

		public Device(IPAddress ip, int port, PhysicalAddress MAC)
		{
			Address = new IPEndPoint(ip, port);
			this.MAC = MAC;
		}
	}
}
