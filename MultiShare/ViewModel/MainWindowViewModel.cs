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
using System.Windows.Input;

namespace MultiShare.ViewModel
{
    public class DeviceEventArgs : EventArgs
    {
        private Device _device;
        public Device Device
        {
            get { return _device; }
        }

        public DeviceEventArgs(Device device)
        {
            _device = device;
        }
    }
    public class MessageEventArgs : EventArgs
    {
        private string _message;
        public string Message
        {
            get { return _message; }
        }

        public MessageEventArgs(string message)
        {
            _message = message;
        }
    }

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _messageText = "";

        private int _selectedDeviceIndex = -1;
        public SimpleCommand MessageSendCommand { get; private set; }
        public SimpleCommand EscCommand { get; private set; }
        /// <summary>
        /// Номер выбранного устройства (номер первого = 0).
        /// </summary>
        public int SelectedDeviceIndex
        {
            get { return _selectedDeviceIndex; }
            set
            {
                if (value < -1)
                {
                    throw new ArgumentOutOfRangeException(nameof(SelectedDeviceIndex), value, "Device index cannot be less than 0");
                }

                if (_selectedDeviceIndex != value)
                {
                    _selectedDeviceIndex = value;
                    if (value >= 0)
                    {
                        OnDeviceSelected(_selectedDeviceIndex);
                    }
                    OnPropertyChanged();
                    MessageSendCommand.RaiseCanExecuteChanged();
                }

            }
        }

        protected virtual void OnDeviceSelected(int deviceIndex)
        {
            Device device = Devices[deviceIndex];

            DeviceSelect?.Invoke(this, new DeviceEventArgs(device));
        }
        protected virtual void OnDevicesUnselected()
        {
            DevicesUnselected?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<DeviceEventArgs> DeviceSelect;
        public event EventHandler DevicesUnselected;
        public event EventHandler<MessageEventArgs> MessageSend;

        private ObservableCollection<Device> _devices = new ObservableCollection<Device>();
        /// <summary>
        /// Список подключённых устройств-клиентов.
        /// </summary>
        public ObservableCollection<Device> Devices
        {
            get { return _devices; }
        }

        public string MessageText
        {
            get { return _messageText; }
            set
            {
                if (value != null && !value.Equals(_messageText))
                {
                    _messageText = value;
                    OnPropertyChanged();
                    MessageSendCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public MainWindowViewModel()
        {
            MessageSendCommand = new SimpleCommand(SendMessage, CanSendMessage);
            EscCommand = new SimpleCommand(CanselDeviceSelection, CanCanselDeviceSelection);

        }

        private void SendMessage()
        {
            MessageSend?.Invoke(this, new MessageEventArgs(_messageText));
        }

        private bool CanSendMessage()
        {
            return !string.IsNullOrWhiteSpace(_messageText) && (SelectedDeviceIndex != -1);
        }

        private void CanselDeviceSelection()
        {
            OnDevicesUnselected();
        }
        private bool CanCanselDeviceSelection()
        {
            return SelectedDeviceIndex != -1;
        }
    }
}
