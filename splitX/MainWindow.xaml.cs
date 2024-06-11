using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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

namespace splitX
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Split(string sourcePath, string targetPath, int splitMB = 50) {
            Stopwatch st = new Stopwatch();
            st.Start();
            TxtFileSplitPart txtSplit = new TxtFileSplitPart();
            txtSplit.FileCut(sourcePath, targetPath, splitMB, worker);
            st.Stop();
            Console.WriteLine($"完成,耗时:{st.ElapsedMilliseconds / 1000}s");
            //statusTB.Text = $"完成,耗时:{st.ElapsedMilliseconds / 1000}s";
            ProcessState userState = new ProcessState();
            userState.itemStatus = $"完成,耗时:{st.ElapsedMilliseconds / 1000}s";
            worker.ReportProgress(100, userState);
        }

        void SplitByCount(string sourcePath, string targetpath) {
            //string sourcePath = txtBox1.Text.Trim();           //@"C:\temp\data.bin";
            //string targetPath = @"D:\store";
            FileStream fsr = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
            long FileLength = fsr.Length;
            byte[] btArr = new byte[FileLength];
            fsr.Read(btArr, 0, (int)FileLength);
            fsr.Close();
            int splitcount = 3;
            long PartLength = FileLength / splitcount + FileLength % splitcount;
            int nCount = (int)Math.Ceiling((double)FileLength / PartLength);
            string strFileName = System.IO.Path.GetFileName(sourcePath);
            long byteCount = 0;
            for (int i = 1; i <= nCount; i++, byteCount = (i < nCount ? byteCount + PartLength : FileLength - PartLength))
            {
                FileStream fsw = new FileStream(targetpath + System.IO.Path.DirectorySeparatorChar + strFileName + i, FileMode.Create, FileAccess.Write);
                long bc = byteCount;
                long PartCount = i < nCount ? PartLength : FileLength - bc;
                int PartBufferCount = (int)(PartCount < int.MaxValue / 32 ? PartCount : int.MaxValue / 32);
                int nc = (int)Math.Ceiling((double)PartCount / PartBufferCount);
                for (int j = 1; j <= nc; j++, bc = (j < nCount ? bc + PartBufferCount : PartCount - PartBufferCount))
                    fsw.Write(btArr, (int)bc, (int)(j < nc ? PartBufferCount : PartCount - bc));
                fsw.Flush();
                fsw.Close();
            }
            fsr.Close();
        }

        class SplitParam
        {
            public string sourcePath;
            public string targetFolder;
            public int singleFileSize;
            public SplitParam(string source, string target, int size) {
                sourcePath = source;
                targetFolder = target;
                singleFileSize = size;
            }
        }
        private BackgroundWorker worker = null;

        public void initTask(string sourcePath, string targetPath, int splitMB = 50) {
            worker = new BackgroundWorker();
            worker.DoWork += splitHandler;
            worker.RunWorkerCompleted += splitCompleteHandler;
            worker.ProgressChanged += splitProgressHandler;
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.RunWorkerAsync(new SplitParam(sourcePath, targetPath, splitMB));
        }

        private void splitProgressHandler(object sender, ProgressChangedEventArgs e)
        {
            //throw new NotImplementedException();
            ProcessState  userState = (ProcessState) e.UserState;
            statusTB.Text = userState.itemStatus;

        }

        private void splitCompleteHandler(object sender, RunWorkerCompletedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void splitHandler(object sender, DoWorkEventArgs e)
        {
            //throw new NotImplementedException();
            if (worker.CancellationPending) {
                e.Cancel = true;
                return;
            }
            SplitParam s = (SplitParam)e.Argument;
            Split(s.sourcePath, s.targetFolder, s.singleFileSize);

        }

        private void splitBtn_Click(object sender, RoutedEventArgs e)
        {
            string sourcePath = txtBox1.Text.Trim();
            if (sourcePath == "" || sourcePath == String.Empty) {
                MessageBox.Show("Please input file Path", "Error");
                return;
            }
            int singleFileSize = 0;
            string strSingleSize = sptSizeTB.Text.Trim();
            if (strSingleSize == "" || strSingleSize == String.Empty)
            {
                //MessageBox.Show("Please input split size", "Error");
                //return;
                singleFileSize = 50;
            }
            else
            {
                 singleFileSize =  Convert.ToInt32(strSingleSize);
            }

            FileInfo fileInfo = new FileInfo(sourcePath);
            string targetFolder = fileInfo.DirectoryName;
            if (saveTxtBox.Text != "")
                targetFolder = saveTxtBox.Text;
            //Split(sourcePath, targetFolder, singleFileSize);
            initTask(sourcePath, targetFolder, singleFileSize);


        }

        private void RecvFileDrag_Drop(object sender, DragEventArgs e)
        {
            //TextBox txtbox = sender as TextBox;
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                var data = e.Data.GetData(DataFormats.Text);
                //txtbox.Text = data.ToString();
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string path = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
                //label.Text = File.ReadAllText(path);
                txtBox1.Text = path;
            }
        }

        private void RecvFileDrag_DragEnter(object sender, DragEventArgs e)
        {
            //if (e.Data.GetDataPresent(typeof(Button)))
            //{
            //    e.Effects = DragDropEffects.Copy;
            //}
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog of = new Microsoft.Win32.OpenFileDialog();
            if (of.ShowDialog() == true)
            {
               txtBox1.Text = of.FileName;
            }

        }

        private void saveBrowse_Click(object sender, RoutedEventArgs e)
        {
            //Microsoft.Win32.OpenFileDialog of = new Microsoft.Win32.OpenFileDialog();
            Microsoft.Win32.SaveFileDialog of = new Microsoft.Win32.SaveFileDialog();
            if (of.ShowDialog() == true)
            {
                FileInfo fileInfo = new FileInfo(of.FileName);
                string targetFolder = fileInfo.DirectoryName;
                saveTxtBox.Text = targetFolder;
            }
        }
    }
}
