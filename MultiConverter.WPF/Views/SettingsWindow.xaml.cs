using MultiConverter.Lib;
using MultiConverter.WPF.Configuration;
using MultiConverter.WPF.Constants;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace MultiConverter.WPF.Views
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private Dictionary<string, string> onlineBranches;

        public bool IsSettingInitialized = false;

        public SettingsWindow()
        {
            InitializeComponent();

            cancelBtn.Click += (o, e) => Close();

            onlineBranches = new Dictionary<string, string>
            {
                { "wow_beta",   "Beta" },
                { "wow",        "Retail" },
                { "wowt",       "PTR" },
            };

            foreach (var branch in onlineBranches)
                onlineBranch.Items.Add($"{branch.Key} ({branch.Value})");
        }

        private void StorageType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var loadType = (CascLoadType)storageType?.SelectedIndex;

            switch (loadType)
            {
                case CascLoadType.Local:
                    onlineBranch.IsEnabled = false;
                    storagePath.IsEnabled = true;
                    openStorage.IsEnabled = true;
                    localBranch.IsEnabled = true;
                    break;
                case CascLoadType.Online:
                    onlineBranch.IsEnabled = true;
                    storagePath.IsEnabled = false;
                    openStorage.IsEnabled = false;
                    localBranch.IsEnabled = false;
                    break;
                default:
                    onlineBranch.IsEnabled = false;
                    storagePath.IsEnabled = false;
                    openStorage.IsEnabled = false;
                    localBranch.IsEnabled = false;
                    break;
            }
        }

        private void OpenStorage_Click(object sender, RoutedEventArgs e)
        {
            using (var browserDialog = new FolderBrowserDialog())
            {
                var result = browserDialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    var directories = Directory.GetDirectories(browserDialog.SelectedPath).ToList();
                    if (!directories.Contains($"{browserDialog.SelectedPath}\\Data"))
                    {
                        System.Windows.MessageBox.Show("Invalid folder!", "Error", MessageBoxButton.OK);
                        result = browserDialog.ShowDialog();
                    }

                    storagePath.Text = browserDialog.SelectedPath;

                    var branches = Utils.GetLocalBranch(browserDialog.SelectedPath + "/.build.info");
                    localBranch.ItemsSource = branches;
                }
            }
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            var onlineStorage = onlineBranches.Keys.ToList()[onlineBranch.SelectedIndex];

            var config = new ConverterConfig
            {
                LoadType        = (CascLoadType)storageType?.SelectedIndex,
                LocalStorage    = storagePath.Text,
                LocalBranch     = (string)localBranch.SelectedItem,
                OnlineBranch    = onlineStorage
            };

            File.WriteAllText("settings.json", JsonConvert.SerializeObject(config, Formatting.Indented));

            // Close the dialog
            Close();
        }
    }
}
