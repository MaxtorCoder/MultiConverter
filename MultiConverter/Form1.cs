using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

using MultiConverterLib;
using Microsoft.Win32;
using System.Threading;
using ModelConverter_Old;

namespace WowConverter
{
    public partial class converter_form : Form
    {
        public static int PROGRESS = 5;
        private int threadRemaining = 0;
        private int converted = 0;
        private int toConverted = 0;

        public converter_form()
        {
            new Thread(() =>
            {
                if (!Listfile.IsInitialized)
                    Listfile.Initialize();
            }).Start();

            InitializeComponent();
            lb.HorizontalScrollbar = true;

            var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\MultiConverter", true);
            if (key == null)
                key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\MultiConverter");

            if (!key.GetValueNames().Contains("firstTime"))
                key.SetValue("firstTime", false);

            if (!bool.Parse(key.GetValue("firstTime").ToString()))
            {
                MessageBox.Show("First time converting might take a while since it is downloading + loading the listfile..");
                key.SetValue("firstTime", true);
            }
        }

        private void Clear()
        {
            lb.Items.Clear();
        }

        private void fix_btn_Click(object sender, EventArgs e)
        {
            if (lb.Items.Count == 0)
            {
                MessageBox.Show("No files");
                return;
            }

            if (lb.Items.Count > 0)
            {
                progress.Value = 0;
                progress.Maximum = lb.Items.Count;
                progress.Step = PROGRESS;
                Fix();
            }
        }

        struct ConvertionErrorInfo
        {
            public Exception exception;
            public string filename;

            public ConvertionErrorInfo(Exception e, string file)
            {
                exception = e;
                filename = file;
            }
        }

        private void Fix()
        {
            Enabled = false;
            // multi threading only if there's enough models to fix
            int pc = Environment.ProcessorCount * 2 - 1;
            pc = lb.Items.Count < pc ? 1 : pc;
            int count = lb.Items.Count, div = count / pc, r = count % pc;
            threadRemaining = pc;

            if (File.Exists("error.log"))
                File.WriteAllText("error.log", string.Empty);

            var items = new List<object>();
            var lbitems = new object[lb.Items.Count];
            lb.Items.CopyTo(lbitems, 0);
            items.AddRange(lbitems);

            converted = 0;
            toConverted = items.Count;

            var files = new List<string>();
            foreach (object o in items)
                files.Add((string)o);

            for (int i = 0; i < pc; i++)
            {
                var list = new List<string>();
                var n = div + ((r-- > 0) ? 1 : 0);

                foreach (var o in items.Take(n))
                    list.Add(o.ToString());

                items.RemoveRange(0, ((n > items.Count) ? items.Count : n));
                FixList(list);
            }
        }

        private void FixList(List<string> list)
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;

            if (cb_wod.Checked)
                list.Add("wod");

            bw.DoWork += new DoWorkEventHandler((sender, e) =>
            {
                List<string> items = e.Argument as List<string>;
                List<ConvertionErrorInfo> errors = new List<ConvertionErrorInfo>();
                int progress = 0;

                bool wod = (items[items.Count - 1] == "wod");
                if (wod)
                    items.RemoveAt(items.Count - 1);

                BackgroundWorker worker = sender as BackgroundWorker;

                foreach (string s in items)
                {
                    IConverter converter = null;

                    if (s.EndsWith("m2"))
                        converter = new M2Converter(s, helm_fix_cb.Checked);
                    else if (s.EndsWith("adt"))
                        converter = new AdtConverter(s, adt_water.Checked, adt_models.Checked);
                    else if (s.EndsWith("wdt"))
                        converter = new WDTConverter(s);
                    else if (Regex.IsMatch(s, @".*_[0-9]{3}(_(lod[0-9]))?\.(wmo)"))
                        converter = new WMOGroupConverter(s, wod);
                    else if (s.EndsWith("wmo") && !wod)
                        converter = new WMORootConverter(s);
                    else if (s.EndsWith("anim"))
                        converter = new AnimConverter(s);

                    // ? -> in case a file with a wrong extension/pattern was in the list
                    try
                    {
                        if (converter?.Fix() ?? false)
                            converter.Save();
                    }
                    catch (Exception exception)
                    {
                        errors.Add(new ConvertionErrorInfo(exception, s));
                    }

                    if (++progress == PROGRESS)
                    {
                        worker.ReportProgress(1);
                        progress = 0;
                    }

                }

                e.Result = errors;
            });

            bw.ProgressChanged += new ProgressChangedEventHandler((sender, e) =>
            {
                progress.PerformStep();
                converted++;
            });

            List<ConvertionErrorInfo> error_to_log = new List<ConvertionErrorInfo>();

            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler((sender, e) =>
            {
                if (e.Error != null)
                    MessageBox.Show(e.Error.ToString());

                List<ConvertionErrorInfo> errors = e.Result as List<ConvertionErrorInfo>;
                error_to_log.AddRange(errors);

                if ((--threadRemaining) == 0)
                {
                    progress.Value = progress.Maximum;
                    Enabled = true;
                    Clear();

                    if (error_to_log.Count > 0)
                    {
                        MessageBox.Show(error_to_log.Count + " error(s) while converting files, please send the error.log file to the developper so this issue can be fixed");

                        using (StreamWriter sw = new StreamWriter("error.log", true))
                        {
                            foreach (ConvertionErrorInfo error in error_to_log)
                            {
                                sw.WriteLine("Error fixing: " + error.filename);
                                sw.WriteLine(error.exception.Message);
                                sw.WriteLine(error.exception.StackTrace);
                            }
                        }
                    }
                }
            });

            bw.RunWorkerAsync(list);
        }

        private void filepath_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void filepath_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                progress.Value = 0;
                string[] list = (string[])e.Data.GetData(DataFormats.FileDrop, false);
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
                string[] files = e.Argument as string[];
                HashSet<string> lf = new HashSet<string>();

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
                HashSet<string> files = (HashSet<string>)e.Result;
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
    }
}
