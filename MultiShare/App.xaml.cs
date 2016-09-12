using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
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

        public App()
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            DispatcherUnhandledException += ProcessUnhandledException;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow mainWindow = new MainWindow();
            MainWindow = mainWindow;
            mainWindow.Show();
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
