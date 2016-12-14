using Hardcodet.Wpf.TaskbarNotification;
using MultiShare.Model;
using MultiShare.Server;
using MultiShare.View;
using MultiShare.ViewModel;
using MultiShare.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MultiShare
{
	public partial class App : Application
	{
		private const string LOG_FILE_NAME = @"error.log";

		private TaskbarIcon _taskbarIcon;

		private MultiShareServer _server = new MultiShareServer();
		public MultiShareServer Server { get { return _server; } }

		#region Команды
		/// <summary>
		/// Команда выхода из приложения.
		/// </summary>
		public SimpleCommand QuitCommand { get; set; }
        /// <summary>
		/// Команда развертывания приложения из трея.
		/// </summary>
        public SimpleCommand UnTrayCommand { get; set; }
		#endregion

		public App()
		{
			ShutdownMode = ShutdownMode.OnExplicitShutdown;
			AppDomain.CurrentDomain.UnhandledException += ProcessUnhandledException;

			QuitCommand = new SimpleCommand(() => { Shutdown(); });
            UnTrayCommand = new SimpleCommand(()=> { MainWindow.Show(); MainWindow.WindowState = WindowState.Normal; });
		}

		protected override async void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			_taskbarIcon = new TaskbarIcon();
			_taskbarIcon.ToolTip = "MultiShare";
			_taskbarIcon.IconSource = new BitmapImage(new Uri(@"pack://application:,,,/MultiShare;component/Images/Icons/Tray.ico"));
			_taskbarIcon.ContextMenu = new ContextMenu();
			MenuItem quitMenuItem = new MenuItem()
			{
				Command = QuitCommand,
				Header = "Выйти"
			};
			_taskbarIcon.ContextMenu.Items.Add(quitMenuItem);
            _taskbarIcon.LeftClickCommand = UnTrayCommand;

			MainWindow mainWindow = new MainWindow();
			MainWindowViewModel vm = mainWindow.DataContext as MainWindowViewModel;
			Server.NewClient += Server_NewClient;
			vm.DeviceSelect += DeviceSelect;
			vm.MessageSend += MessageSend;
            vm.DevicesUnselected += DevisesUnselect;

            mainWindow.Closed += MainWindow_Closed;
			MainWindow = mainWindow;
			mainWindow.Show();

			//await Server.ConnectToClient(new Device(new IPEndPoint(new IPAddress(new byte[] { 192, 168, 0, 101 }), 49016), PhysicalAddress.None));

			await Server.StartAsync();
		}

		protected override void OnExit(ExitEventArgs e)
		{
			_taskbarIcon.Dispose();

			base.OnExit(e);
		}

		private async void DeviceSelect(object sender, DeviceEventArgs e)
		{
			if (Server.IsConnected)
			{
				Server.DisconnectFromClient();
			}

			await Server.ConnectToClient(e.Device);
		}

		private async void MessageSend(object sender, MessageEventArgs e)
		{
			await Server.SendMessage(e.Message);
		}

		private void Server_NewClient(object sender, NewClientEventArgs e)
		{
			MainWindowViewModel vm = null;
			Window window = null;
			Dispatcher.Invoke(() =>
			{
				vm = MainWindow.DataContext as MainWindowViewModel;
				window = MainWindow;
			});

			if (vm != null)
			{
				window.Dispatcher.Invoke(() =>
				{
					if (!vm.Devices.Contains(e.Device))
					{
						vm.Devices.Add(e.Device);
					}
				});
			}
		}

		private void MainWindow_Closed(object sender, EventArgs e)
		{
			Shutdown();
		}

        private void DevisesUnselect(object sender, EventArgs e)
        {
            if (Server.IsConnected)
            {
                Server.DisconnectFromClient();
            }
        }

		private void ProcessUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			StringBuilder localAppFolderPath = new StringBuilder(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create));
			localAppFolderPath.Append(Path.DirectorySeparatorChar);
			localAppFolderPath.Append(LOG_FILE_NAME);
			string logFilePath = localAppFolderPath.ToString();

			using (StreamWriter sw = File.CreateText(logFilePath))
			{
				sw.WriteLine(DateTime.Now.ToString() + " - Unhandled exception:");
				Exception ex = (Exception)e.ExceptionObject;
				StackTrace st = new StackTrace(ex, true);
				StackFrame sf = st.GetFrame(0);

				sw.WriteLine($"Exception was generated in line {sf.GetFileName()}:{sf.GetFileLineNumber()}:{sf.GetFileColumnNumber()}");
			}

			MessageBox.Show($"The application has crashed due to the error. View log file at \"{ logFilePath }\"", "Exception");

			Process.Start(logFilePath);
		}
	}
}
