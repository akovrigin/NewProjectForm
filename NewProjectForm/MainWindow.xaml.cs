using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
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
using NewProjectForm.Core;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace NewProjectForm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new
            {
                MainLevel = MainMenuDataFiller.Fill(),
            };

            listBox.SelectedValuePath = StringConst.FieldId;
            listBox.DisplayMemberPath = StringConst.FieldName;


            var f = ".NET Framework ";
            var pairs = new List<KeyValuePair<string, string>>();
            pairs.Add(new KeyValuePair<string, string>("2.0", f + "2.0"));
            pairs.Add(new KeyValuePair<string, string>("3.0", f + "3.0"));
            pairs.Add(new KeyValuePair<string, string>("4.0", f + "4.0"));
            pairs.Add(new KeyValuePair<string, string>("4.5", f + "4.5"));

            cbFramewokrVersion.DisplayMemberPath = "Value";
            cbFramewokrVersion.SelectedValuePath = "Key";
            cbFramewokrVersion.ItemsSource = pairs;
            cbFramewokrVersion.SelectedIndex = 0;

            cbFramewokrVersion.SelectionChanged += OnFrameworkComboBoxChanged;

            var sorts = new List<KeyValuePair<int, string>>();
            sorts.Add(new KeyValuePair<int, string>(0, "Default"));
            sorts.Add(new KeyValuePair<int, string>(1, "Name Ascending"));
            sorts.Add(new KeyValuePair<int, string>(2, "Name Descending"));
            cbSort.DisplayMemberPath = "Value";
            cbSort.SelectedValuePath = "Key";
            cbSort.ItemsSource = sorts;
            cbSort.SelectedIndex = 0;

            cbSort.SelectionChanged += OnSortComboBoxChanged;

            cbSearchHistory.SelectionChanged += OnSearchHistoryComboBoxChanged;
        }

        private void OnSearchHistoryComboBoxChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbSearchHistory.SelectedValue == null)
                return;

            txtSearch.Text = cbSearchHistory.SelectedValue.ToString();
            SearchInstalledItems();
        }

        private void OnFrameworkComboBoxChanged(object sender, SelectionChangedEventArgs e)
        {
            txtDescription.Text = "";

            if (txtSearch.Text == "")
                FilterListItems();
            else
                SearchInstalledItems();
        }

        private void OnSortComboBoxChanged(object sender, SelectionChangedEventArgs e)
        {
            SortListItems();
        }

        private void SortListItems()
        {
            DataBase.GetLeafs().DefaultView.Sort = "";

            if (!cbSort.SelectedValue.ToString().Equals("0"))
            {
                var sort = StringConst.FieldName + " " +
                           (cbSort.SelectedValue.ToString() == "1" ? "ASC" : "DESC");

                DataBase.GetLeafs().DefaultView.Sort = sort;
            }

            listBox.DataContext = DataBase.GetLeafs().DefaultView;
        }

        private void TreeViewItem_LeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var item = sender as TreeViewItem;

            if (item == null) return;

            txtSearch.Text = "";

            FilterListItems();

            item.Focus();
            e.Handled = true;
        }

        private void FilterListItems()
        {
            txtDescription.Text = "";

            if (treeView.SelectedItem == null)
            {
                (treeView.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem).IsSelected = true;
            }

            var context = (TreeMenuData) treeView.SelectedItem;

            var filter = StringConst.FieldParentId;

            if (context.MenuItems.Count == 0)
            {
                filter += $" = {context.Id}";
            }
            else if (context.MenuItems.Count > 0)
            {
                var csv = context.MenuItems.Aggregate(new StringBuilder(), (a, b) => a.Append(b.Id).Append(","));
                filter += $" in ({csv.Remove(csv.Length - 1, 1)})";
            }
            else
            {
                filter += $" = {0}";
            }

            filter += GerFrameworkVersionFilterString();

            DataBase.GetLeafs().DefaultView.RowFilter = filter;

            SortListItems();

            CheckListItemExists();
        }

        private void CheckListItemExists()
        {
            var enable = listBox.Items.Count != 0;
            cbSort.IsEnabled = enable;

            if (listBox.Items.Count > 0)
                listBox.SelectedIndex = 0;
        }

        private string GerFrameworkVersionFilterString()
        {
            return $" AND {StringConst.FieldVersion} Like '%{cbFramewokrVersion.SelectedValue}%'";
        }

        private void ButtonStatusCheck()
        {
            btnOk.IsEnabled = listBox.SelectedItems.Count != 0;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (listBox.SelectedItems.Count == 0)
                return;

            var context = (DataRowView) listBox.SelectedItems[0];
            var txt = context.Row[StringConst.FieldName].ToString();

            MessageBox.Show(
                this, 
                $"Type:\t{txt}\nLocation:\t{txtLocation.Text}\n\nAdded to Recent->All",
                "Selected project was created");

            DataBase.AddRecentLeaf(txt);
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ButtonStatusCheck();

            if (listBox.SelectedItems.Count == 0)
                return;

            var context = (DataRowView)listBox.SelectedItems[0];
            txtDescription.Text = context.Row[StringConst.FieldDescription].ToString();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchInstalledItems();
        }

        private void SearchInstalledItems()
        {
            if (txtSearch.Text == "")
                return;

            cbSearchHistory.Items.Remove(txtSearch.Text);

            cbSearchHistory.Items.Add(txtSearch.Text);

            var max = Convert.ToInt32(ConfigurationManager.AppSettings[StringConst.SettingsMaxSearchHistory]);
            if (cbSearchHistory.Items.Count > max)
                cbSearchHistory.Items.RemoveAt(0);

            var filter = $"{StringConst.FieldName} Like '%{txtSearch.Text}%'";

            DataBase.GetLeafs().DefaultView.RowFilter = filter + GerFrameworkVersionFilterString();

            SortListItems();

            CheckListItemExists();

            cbSearchHistory.IsEnabled = !cbSearchHistory.Items.IsEmpty;

            if (listBox.Items.IsEmpty)
            {
                MessageBox.Show(this,
                    "Try to select another .net Framework version or enter another keyword.\n" +
                    "For example: keyword 'Report' and '.net Framework 3.0'",
                    "Information: nothing found.");
            }
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.SelectedPath = txtLocation.Text;
            var result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                txtLocation.Text = dialog.SelectedPath;
            }
        }

        private void txtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchInstalledItems();
            }
        }

        private void txtName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back)
            {
                if (txtSolution.Text.StartsWith(txtName.Text))
                    txtSolution.Text = txtName.Text;
            }
            else
            {
                if (txtName.Text.StartsWith(txtSolution.Text))
                    txtSolution.Text = txtName.Text;
            }
        }
    }
}
