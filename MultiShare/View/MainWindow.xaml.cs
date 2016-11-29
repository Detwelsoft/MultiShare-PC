using MultiShare.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MultiShare.View
{
    public partial class MainWindow : Window
    {
        private void DevicesUnselected (object sender, EventArgs e)
        {
            SendComponentsContainer.Visibility = Visibility.Hidden;
            devicesListBox.UnselectAll();
        }
        private void DevicesSelected(object sender, EventArgs e)
        {
            SendComponentsContainer.Visibility = Visibility.Visible;
        }
        public MainWindow()
        {
            InitializeComponent();
            MainWindowViewModel vm = DataContext as MainWindowViewModel;
            vm.DevicesUnselected += DevicesUnselected;
            vm.DeviceSelect += DevicesSelected;
        }       

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
