using MultiShare.Model;
using MultiShare.Server;
using MultiShare.View;
using MultiShare.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MultiShare
{
    public partial class App : Application
    {
        private const string LOG_FILE_NAME = @"error.log";

		private readonly MultiShareServer _server = new MultiShareServer();
		public MultiShareServer Server { get { return _server; } }

        public App()
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            DispatcherUnhandledException += ProcessUnhandledException;
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow mainWindow = new MainWindow();
			MainWindowViewModel vm = mainWindow.DataContext as MainWindowViewModel;
			Server.NewClient += Server_NewClient;
			vm.DeviceSelect += DeviceSelect;
            vm.MessageSend += MessageSend;

			mainWindow.Closed += MainWindow_Closed;
            MainWindow = mainWindow;
            mainWindow.Show();

			//await Server.ConnectToClient(new Device(new IPEndPoint(new IPAddress(new byte[] { 192, 168, 0, 101 }), 49016), PhysicalAddress.None));

			await Server.StartAsync();
		}

		private async void DeviceSelect(object sender, DeviceEventArgs e)
		{
			await Server.ConnectToClient(e.Device);
		}

        private async void MessageSend(object sender, MessageEventArgs e)
        {
            Encoding asciEnc = Encoding.ASCII;
            /*MainWindowViewModel vm = null;
            vm = MainWindow.DataContext as MainWindowViewModel;*/            
            await Server.SendMessage(asciEnc.GetBytes(e.Message));
            
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
					vm.Devices.Add(e.Device);
				});
			}
		}

		private void MainWindow_Closed(object sender, EventArgs e)
		{
			Shutdown();
		}

		private void ProcessUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create);

            StringBuilder sb = new StringBuilder(localAppDataPath);
            sb.Append(Path.DirectorySeparatorChar);
            Assembly assembly = Assembly.GetExecutingAssembly();
            sb.Append(assembly.GetCustomAttribute<AssemblyCompanyAttribute>().Company);
            sb.Append(Path.DirectorySeparatorChar);
            sb.Append(assembly.GetCustomAttribute<AssemblyProductAttribute>().Product);
            Directory.CreateDirectory(sb.ToString());
            sb.Append(Path.DirectorySeparatorChar);
            sb.Append(LOG_FILE_NAME);
            string logFilePath = sb.ToString();

            using (StreamWriter sw = File.CreateText(logFilePath))
            {
                sw.WriteLine(DateTime.Now.ToString() + " - Unhandled exception:");
                sw.WriteLine(e.Exception);
            }

            MessageBox.Show($"The application has crashed due to the error. View log file at \"{ logFilePath }\"", "Exception");
        }
    }
}
