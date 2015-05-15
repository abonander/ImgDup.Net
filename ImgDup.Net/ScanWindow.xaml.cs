using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImgDup.Net
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BrowseForSearchFolder(object sender, RoutedEventArgs e)
        {
            var folderDialog = new FolderBrowserDialog();
            folderDialog.Description = "Browse for a folder for ImgDup.Net to search.";
            folderDialog.ShowDialog();

            this.SearchFolder.Text = folderDialog.SelectedPath;
        }

        private void BeginSearch(object sender, RoutedEventArgs e)
        {
            BeginSearch();
        }

        private void BeginSearch()
        {
            if (this.SearchFolder.Text.Length == 0)
            {
                System.Windows.MessageBox.Show("Search folder cannot be blank!");
                return;
            }

            if (this.SearchExtensions.Text.Length == 0)
            {
                System.Windows.MessageBox.Show("Search extensions cannot be blank!");
                return;
            }

            var extensions = this.SearchExtensions.Text.Split(',').Select(s => s.Trim()).ToArray();

            if (extensions.Length == 0)
            {
                System.Windows.MessageBox.Show("Search extensions must be a comma-separated list of file extensions!");
                return;
            }           

            var files = this.GetImageFiles(extensions).ToArray();

            System.Windows.MessageBox.Show(string.Format("{0} images found.", files.Length));
        }

        private IEnumerable<string> GetImageFiles(string[] extensions)
        {
            var searchOption = this.SearchRecurse.IsChecked.GetValueOrDefault(false) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            return Directory.EnumerateFileSystemEntries(this.SearchFolder.Text, "*.*", searchOption)
                .Where(file => extensions.Any(ext => file.EndsWith(ext)));
        }
    }
}
