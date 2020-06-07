using CASCLib;
using MultiConverter.Lib;
using MultiConverter.Lib.Readers;
using MultiConverter.Lib.Readers.Base;
using MultiConverter.Lib.Readers.WMO;
using MultiConverter.WPF.Configuration;
using MultiConverter.WPF.OpenGL;
using MultiConverter.WPF.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

            Directory.CreateDirectory("Output");

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

        private void enqueueModel_Click(object sender, RoutedEventArgs e)
        {
            if (modelListView.SelectedItem != null)
            {
                var selectedItem = modelListView.SelectedItem as TreeViewItem;
                if (selectedItem != null && selectedItem.Tag is CASCFile file)
                {
                    if (Listfile.TryGetFileDataId(file.FullName, out var fileDataId))
                    {
                        if (ModelPreview.FilesXChildren.ContainsKey(fileDataId))
                            enqueuedModels.Items.Add(file.FullName);
                    }
                }
            }
        }

        private void convertModel_Click(object sender, RoutedEventArgs e)
        {
            if (enqueuedModels.Items.Count == 0)
                return;

            foreach (var model in enqueuedModels.Items)
            {
                var enqueuedModel = (string)model;
                if (!Listfile.TryGetFileDataId(enqueuedModel, out var fileDataId))
                    return;

                Parallel.ForEach(ModelPreview.FilesXChildren[fileDataId], children =>
                {
                    var filename = Listfile.LookupFilename(children, ".wmo");

                    IConverter converter = null;
                    if (filename.EndsWith("m2"))
                    {
                        var m2converter = new M2Converter(filename, false);
                        if (m2converter.Fix())
                            m2converter.Save();

                        return;
                    }
                    // else if (filename.EndsWith("adt"))
                    //     converter = new AdtConverter(filename, adt_water.Checked, adt_models.Checked);
                    // else if (filename.EndsWith("wdt"))
                    //     converter = new WDTConverter(filename);
                    else if (Regex.IsMatch(filename, @".*_[0-9]{3}(_(lod[0-9]))?\.(wmo)"))
                    {
                        var wmoconverter = new WMOGroupConverter(filename, false);
                        if (wmoconverter.Fix())
                            wmoconverter.Save();

                        return;
                    }
                    else if (filename.EndsWith("wmo"))
                        converter = new WMOFile();
                    else if (filename.EndsWith("blp"))
                    {
                        using (var stream = CASC.CascHandler.OpenFile((int)children))
                        {
                            var buffer = new byte[stream.Length];
                            stream.Read(buffer, 0, buffer.Length);
                            File.WriteAllBytes($"{Environment.CurrentDirectory}/Output/{filename}", buffer);
                        }

                        return;
                    }
                    // else if (filename.EndsWith("anim"))
                    //     converter = new AnimConverter(filename);

                    using (var stream = CASC.CascHandler.OpenFile((int)children))
                    {
                        converter.Read(stream);
                        converter.Write($"{Environment.CurrentDirectory}/Output/{filename}");
                    }
                });
            }
        }
    }
}
