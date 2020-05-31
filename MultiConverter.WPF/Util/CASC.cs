using CASCLib;
using MultiConverter.WPF.Configuration;
using MultiConverter.WPF.Constants;
using MultiConverter.WPF.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MultiConverter.WPF.Util
{
    public static class CASC
    {
        public static CASCHandler CascHandler = null;
        public static bool IsInitialized = false;

        private static BackgroundWorker cascWorker;
        private static ProgressBar progress;
        private static Label progressLabel;
        private static TreeView treeView;

        public static void InitializeCasc(ConverterConfig config, MainWindow window)
        {
            progress = window.progressBar;
            progressLabel = window.statusText;
            treeView = window.modelListView;

            cascWorker = new BackgroundWorker();
            cascWorker.WorkerReportsProgress = true;
            cascWorker.DoWork += CascWorker_DoWork;
            cascWorker.ProgressChanged += CascWorker_ProgressChanged;
            cascWorker.RunWorkerCompleted += (o, e) => IsInitialized = true;
            cascWorker.RunWorkerAsync(config);
        }

        private static void CascWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progress.Value = e.ProgressPercentage;
            progressLabel.Content = e.UserState.ToString();
        }

        private static void CascWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var config = (ConverterConfig)e.Argument;

            CascHandler = config.LoadType == CascLoadType.Online ?
                CASCHandler.OpenOnlineStorage(cascWorker, config.OnlineBranch) :
                CASCHandler.OpenLocalStorage(cascWorker, config.LocalStorage, config.LocalBranch);

            if (!File.Exists("listfile.csv"))
            {
                var webClient = new WebClient();
                webClient.DownloadFile("https://wow.tools/casc/listfile/download/csv/unverified", "listfile.csv");
            }

            CascHandler.Root.SetFlags(LocaleFlags.enUS, false, false);
            CascHandler.Root.LoadListFile("listfile.csv", cascWorker);
            cascWorker.ReportProgress(0, "Done!");

            OnStorageChanged();
        }

        /// <summary>
        /// Create a tree node based on <see cref="TreeViewItem"/>
        /// </summary>
        public static void CreateTreeNode(TreeViewItem item)
        {
            if (item.Tag is CASCFolder baseEntry)
            {
                // Remove dummy
                item.Items.Clear();

                var orderedEntries = baseEntry.Entries.OrderBy(x => x.Value.Name);
                foreach (var entry in orderedEntries)
                {
                    var cascEntry = entry.Value;

                    // Fuckoff with these files will ya.
                    if (cascEntry.Name.Contains("html") || cascEntry.Name.Contains("dll") ||
                        cascEntry.Name.Contains("txt"))
                        continue;

                    if (!item.Items.Contains(cascEntry.Name))
                    {
                        var newNode = new TreeViewItem
                        {
                            Header = cascEntry.Name,
                            Tag = cascEntry
                        };

                        if (cascEntry is CASCFolder folder && folder.Entries.Count > 0)
                            newNode.Items.Add(new TreeViewItem { Name = "tempNode", Tag = folder });

                        item.Items.Add(newNode);
                    }
                }
            }
        }

        private static void OnStorageChanged()
        {
            treeView.Dispatcher.Invoke(() =>
            {
                var cascFolder = CascHandler.Root.SetFlags(LocaleFlags.enUS);

                var tree = new TreeViewItem { Name = cascFolder.Name, Tag = cascFolder, Header = "Root [Read Only]" };
                tree.Items.Add(new TreeViewItem { Header = "tempNode" });
                treeView.Items.Add(tree);
            });
        }
    }
}
