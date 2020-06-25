using MultiConverter.Lib;
using MultiConverter.Lib.Updater;
using MultiConverter.Lib.Converters;
using MultiConverter.Lib.Converters.ADT;
using MultiConverter.Lib.Converters.Base;
using MultiConverter.Lib.Converters.WMO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;

namespace MultiConverter.GUI
{
    public partial class ConverterForm : Form
    {
        public static int PROGRESS = 5;

        public ConverterForm()
        {
            new Thread(() =>
            {
                if (!Listfile.IsInitialized)
                    Listfile.Initialize();
            }).Start();

            InitializeComponent();
            lb.HorizontalScrollbar = true;

            new Thread(() =>
            {
                Thread.Sleep(3000);

                var hasUpdate = UpdateManager.HasUpdates(AssemblyVersion);
                if (hasUpdate.Item1)
                {
                    var messageBox = MessageBox.Show($"An update is available! From: {AssemblyVersion} To: {hasUpdate.Item2}.\nPress OK to update.", "Update!", MessageBoxButtons.OK);
                    if (messageBox == DialogResult.OK)
                    {
                        // Start the updater..
                        UpdateManager.StartUpdater();

                        // Close the current window.
                        Invoke((MethodInvoker)delegate 
                        { 
                            Close(); 
                        });
                    }
                }
            }).Start();
        }

        private void Clear() => lb.Items.Clear();

        private void fix_btn_Click(object sender, EventArgs e)
        {
            if (lb.Items.Count == 0)
                return;

            if (lb.Items.Count > 0)
            {
                progress.Value = 0;
                progress.Maximum = lb.Items.Count;
                progress.Step = PROGRESS;
                Fix();
            }
        }

        private void Fix()
        {
            Enabled = false;

            var filenameList = new List<string>();
            foreach (var item in lb.Items)
                filenameList.Add(item.ToString());

            FixList(filenameList);
        }

        private void FixList(List<string> list)
        {
            var bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;

            bw.DoWork += new DoWorkEventHandler((sender, e) =>
            {
                var filenames = e.Argument as List<string>;
                var progress = 0;

                var worker = sender as BackgroundWorker;

                foreach (string filename in filenames)
                {
                    IConverter converter = null;

                    if (filename.EndsWith("m2"))
                    {
                        var m2converter = new M2Converter(filename, helmFix.Checked);
                        if (m2converter.Fix())
                            m2converter.Save();

                        continue;
                    }
                    else if (filename.EndsWith("adt"))
                        converter = new ADTFile(filename.Replace(".adt", "_obj0.adt"), filename.Replace(".adt", "_tex0.adt"));
                    // else if (filename.EndsWith("wdt"))
                    //     converter = new WDTConverter(filename);
                    else if (Regex.IsMatch(filename, @".*_[0-9]{3}(_(lod[0-9]))?\.(wmo)"))
                    {
                        var wmoconverter = new WMOGroupConverter(filename, false);
                        if (wmoconverter.Fix())
                            wmoconverter.Save();

                        continue;
                    }
                    else if (filename.EndsWith(".skin"))
                        continue;
                    if (filename.EndsWith("wmo"))
                        converter = new WMOFile();
                    // else if (filename.EndsWith("anim"))
                    //     converter = new AnimConverter(filename);

                    converter.Read(File.ReadAllBytes(filename));
                    converter.Write(filename);

                    if (++progress == PROGRESS)
                    {
                        worker.ReportProgress(1);
                        progress = 0;
                    }
                }
            });

            bw.ProgressChanged += new ProgressChangedEventHandler((sender, e) =>
            {
                progress.PerformStep();
            });

            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler((sender, e) =>
            {
                if (e.Error != null)
                    MessageBox.Show(e.Error.ToString());

                progress.Value = progress.Maximum;
                Enabled = true;
                Clear();
            });

            bw.RunWorkerAsync(list);
        }

        private void filepath_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void filepath_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                progress.Value = 0;
                var list = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                LoadFiles(list);
            }
        }

        private void LoadFiles(string[] list)
        {
            this.Enabled = false;

            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = false;

            bw.DoWork += new DoWorkEventHandler((sender, e) =>
            {
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
                var files = e.Argument as string[];
                var lf = new HashSet<string>();

                foreach (var s in files)
                {
                    if (Directory.Exists(s))
                    {
                        foreach (string file in Directory.EnumerateFiles(s, "*.*", SearchOption.AllDirectories))
                            if (Utils.IsCorrectFile(file) && !lf.Contains(file))
                                lf.Add(file.ToLower());
                    }
                    else if (Utils.IsCorrectFile(s) && !lf.Contains(s))
                        lf.Add(s.ToLower());
                }

                e.Result = lf;
            });

            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler((sender, e) =>
            {
                var files = (HashSet<string>)e.Result;
                foreach (string file in files)
                    lb.Items.Add(file);
                Enabled = true;
            });

            bw.RunWorkerAsync(list);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            progress.Value = 0;
            Clear();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var aboutBox = new About();
            aboutBox.Show();
        }

        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }
    }
}
