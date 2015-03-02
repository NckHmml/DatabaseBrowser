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

namespace DatabaseBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            btnConnect.Click += btnConnectClicked;
            btnExecute.Click += btnExecuteClick;
            txtQuery.TextChanged += txtQueryTextChanged;
        }

        private void btnConnectClicked(object sender, RoutedEventArgs e)
        {
            var dlgConnectWindow = new ConnectWindow();
            dlgConnectWindow.Closing += (s, cea) => dlgConnectWindowClosing();
            dlgConnectWindow.ShowDialog();
        }

        private async void btnExecuteClick(object sender, RoutedEventArgs e)
        {
            try
            {
                FillGrid(await SqlHelper.ExecuteQuery(txtQuery.Text));                
            }
            catch
            {
                MessageBox.Show("Something went wrong while executing the query", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void txtQueryTextChanged(object sender, TextChangedEventArgs e)
        {
            btnExecute.IsEnabled = !String.IsNullOrWhiteSpace(txtQuery.Text);
        }

        private void dlgConnectWindowClosing()
        {
            if (!Settings.Connected) return;
            trvBrowser.Items.Clear();

            foreach (string database in SqlHelper.GetDatabases())
            {
                TreeViewItem toAdd = new TreeViewItem();
                toAdd.Header = database;

                TreeViewItem toAddTable = new TreeViewItem();
                toAddTable.Header = "Tables";
                toAdd.Items.Add(toAddTable);

                foreach (string table in SqlHelper.GetTablesForDatabase(database))
                {
                    TreeViewItem toAddSub = new TreeViewItem();
                    toAddSub.Header = table;
                    toAddSub.MouseDoubleClick += (s, e) => TableSelected(toAddSub);
                    toAddSub.MouseRightButtonDown += (s, e) => ViewTableStruct(toAddSub);
                    toAddTable.Items.Add(toAddSub);
                }

                TreeViewItem toAddViews = new TreeViewItem();
                toAddViews.Header = "Views";
                toAdd.Items.Add(toAddViews);

                foreach (string view in SqlHelper.GetViewsForDatabase(database))
                {
                    TreeViewItem toAddSub = new TreeViewItem();
                    toAddSub.Header = view;
                    toAddSub.MouseDoubleClick += (s, e) => TableSelected(toAddSub);
                    toAddSub.MouseRightButtonDown += (s, e) => ViewTableStruct(toAddSub);
                    toAddViews.Items.Add(toAddSub);
                }

                trvBrowser.Items.Add(toAdd);
            }
        }

        private void TableSelected(TreeViewItem item)
        {
            var parentDatabase = trvBrowser.Items.OfType<TreeViewItem>()
                .FirstOrDefault(x => x.Items.OfType<TreeViewItem>().Any(z => z.Items.OfType<TreeViewItem>().Contains(item)));
            txtQuery.Text = String.Format("SELECT * FROM {0}.dbo.{1}", parentDatabase.Header, item.Header);
        }

        private async void ViewTableStruct(TreeViewItem item)
        {
            var parentDatabase = trvBrowser.Items.OfType<TreeViewItem>()
                .FirstOrDefault(x => x.Items.OfType<TreeViewItem>().Any(z => z.Items.OfType<TreeViewItem>().Contains(item)));
            FillGrid(await SqlHelper.GetTableStruct(parentDatabase.Header as string, item.Header as string));
        }

        private void FillGrid(QueryResult result)
        {
            grdResult.Columns.Clear();

            int i = 0;
            result.Columns.ForEach(column =>
                grdResult.Columns.Add(new DataGridTextColumn()
                {
                    Header = column,
                    Binding = new Binding(String.Format("[{0}]", i++))
                })
            );

            grdResult.ItemsSource = result.Rows;
        }
    }
}
