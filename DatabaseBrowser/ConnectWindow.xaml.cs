using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
using System.Windows.Shapes;

namespace DatabaseBrowser
{
    /// <summary>
    /// Interaction logic for ConnectWindow.xaml
    /// </summary>
    public partial class ConnectWindow : Window
    {
        public ConnectWindow()
        {
            InitializeComponent();

            chkLocalAccount.Click += chkLocalAccountChecked;
            btnConnect.Click += btnConnectClick;
        }

        private void btnConnectClick(object sender, RoutedEventArgs e)
        {
            MainGrid.IsEnabled = false;
            ProgressBar.IsIndeterminate = true;

            if (chkLocalAccount.IsChecked ?? false)
                Settings.ConnectionString = 
                    String.Format("Server={0};Integrated Security=true;", txtServer.Text);
            else
                Settings.ConnectionString = 
                    String.Format("Server={0};User Id={1};Password={2};", txtServer.Text, txtUsername.Text, txtPassword.Password);

            Task.Run(() =>
            {
                bool isConnected = true;
                try
                {
                    using (SqlConnection connection = new SqlConnection(Settings.ConnectionString))
                        connection.Open();
                }
                catch
                {
                    isConnected = false;
                }
                Settings.Connected = isConnected;
                Dispatcher.Invoke(() =>
                {
                    MainGrid.IsEnabled = true;
                    ProgressBar.IsIndeterminate = false;
                    if (!isConnected)
                    {
                        MessageBox.Show("Unable to connect", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }
                    this.Close();
                });
            });
        }

        private void chkLocalAccountChecked(object sender, RoutedEventArgs e)
        {
            lblUsername.IsEnabled = !chkLocalAccount.IsChecked ?? false;
            txtUsername.IsEnabled = !chkLocalAccount.IsChecked ?? false;

            lblPassword.IsEnabled = !chkLocalAccount.IsChecked ?? false;
            txtPassword.IsEnabled = !chkLocalAccount.IsChecked ?? false;
        }
    }
}
