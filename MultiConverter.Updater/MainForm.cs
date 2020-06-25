using MultiConverter.Updater.Updater;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace MultiConverter.Updater
{
    public partial class MainForm : Form
    {
        private bool hasBeta = false;
        private BackgroundWorker updateWorker;

        public MainForm(string[] args)
        {
            InitializeComponent();

            // for (var i = 0; i < args.Length; ++i)
            // {
            //     if (args[i] == "-b")
            //         hasBeta = bool.Parse(args[++i]);
            // }

            boxChangelog.Text = UpdateManager.GetChangelog(hasBeta);
            DownloadUpdate();
        }

        private void DownloadUpdate()
        {
            updateWorker = new BackgroundWorker();
            updateWorker.WorkerReportsProgress = true;
            updateWorker.DoWork += UpdateWorker_DoWork;
            updateWorker.ProgressChanged += UpdateWorker_ProgressChanged;
            updateWorker.RunWorkerCompleted += UpdateWorker_RunWorkerCompleted;

            using (var webClient = new WebClient())
            {
                var reponseString = webClient.DownloadString(hasBeta ? UpdateManager.BetaUpdateUrl : UpdateManager.UpdateUrl);
                var update = JsonConvert.DeserializeObject<Update>(reponseString);

                update.WebClient = webClient;

                updateWorker.RunWorkerAsync(update);
            }
        }

        private void UpdateWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Thread.Sleep(5000);
            Process.Start("MultiConverter.GUI.exe");
            Close();
        }

        private void UpdateWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
            boxStatus.AppendText($"[{DateTime.Now}] {e.UserState}\n");
        }

        private void UpdateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Let it load for a minute..
            Thread.Sleep(2000);

            var update = e.Argument as Update;

            updateWorker.ReportProgress(0, "Downloading update...");

            // https://github.com/MaxtorCoder/MultiConverter/releases/download/3.6.1/MultiConverter3.6.1.rar
            update.WebClient.DownloadFile($"{update.DownloadUrl}/{update.VersionString}/MultiConverter{update.VersionString}", "update.zip");

            updateWorker.ReportProgress(45, "Downloaded update.. Extracting update..");

            var zip = new ZipArchive(File.OpenRead("update.zip"));

            var currentIdx = 0;
            foreach (var file in zip.Entries)
            {
                updateWorker.ReportProgress(45 + (++currentIdx), $"Extracting file: {file.FullName}");

                File.Delete(file.FullName);
                using (var stream = file.Open())
                {
                    var data = new byte[file.Length];
                    stream.Read(data, 0, (int)file.Length);

                    File.WriteAllBytes(file.FullName, data);
                }
            }

            zip.Dispose();
            File.Delete("update.zip");

            updateWorker.ReportProgress(100, "Done extracting update.. Closing this in 5 seconds!");
        }
    }
}
