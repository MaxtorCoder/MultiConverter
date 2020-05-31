using CASCLib;
using MultiConverter.Lib;
using MultiConverter.WPF.Configuration;
using MultiConverter.WPF.OpenGL;
using MultiConverter.WPF.Util;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace MultiConverter.WPF.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string settingFile = "settings.json";

        private ConverterConfig config;
        private ModelPreview modelPreview;

        public MainWindow()
        {
            new Thread(Listfile.Initialize).Start();

            InitializeComponent();

            LoadSettings();

            statusText.Content = "Idle...";

            // Initialize events..
            openCascStorage.Click += (e, o) => LoadCasc();
            exitBtn.Click += (e, o) => Close();

            modelPreview = new ModelPreview(renderCanvas, blpCanvas);

            CompositionTarget.Rendering += modelPreview.CompositionTarget_Rendering;
            wfHost.Initialized += modelPreview.WFHost_Initialized;
        }

        /// <summary>
        /// Initialize the settings.
        /// </summary>
        private void LoadSettings()
        {
            statusText.Content = "Loading Settings...";

            if (!File.Exists(settingFile))
            {
                var settingsWindow = new SettingsWindow { Topmost = true };
                while ((bool)settingsWindow.ShowDialog()) { }
            }

            ConfigurationManager<ConverterConfig>.Initialize(settingFile);
            config = ConfigurationManager<ConverterConfig>.Config;
        }

        /// <summary>
        /// Initialize the CASC Storage.
        /// </summary>
        private void LoadCasc()
        {
            statusText.Content = "Loading CASC Storage...";
            if (CASC.IsInitialized)
                return;

            CASC.InitializeCasc(config, this);
        }

        private void modelListView_Expanded(object sender, RoutedEventArgs e)
        {
            CASC.CreateTreeNode(e.Source as TreeViewItem);
        }

        private void modelListView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Make sure the selected item is indeed a file.
            if (modelListView.SelectedItem != null)
            {
                var selectedItem = modelListView.SelectedItem as TreeViewItem;
                if (selectedItem != null && selectedItem.Tag is CASCFile file)
                {
                    modelPreview.LoadModel(file.FullName);

                    if (file.FullName.EndsWith(".blp"))
                    {
                        blpCanvas.Visibility = Visibility.Visible;
                        wfHost.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        blpCanvas.Visibility = Visibility.Hidden;
                        wfHost.Visibility = Visibility.Visible;
                    }
                }
            }
        }
    }
}
