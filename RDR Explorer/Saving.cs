using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace RDR_Explorer
{
    internal partial class Saving : Form
    {
        private FileStream newRPFStream;
        RPFLib.RPF6.File rpfFile;
        BackgroundWorker backgroundWorker1 = new BackgroundWorker();

        public Saving(FileStream newRPF, RPFLib.RPF6.File file)
        {
            InitializeComponent();
            newRPFStream = newRPF;
            rpfFile = file;
            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.DoWork += new DoWorkEventHandler(bgWorker_DoWork);
            backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(bgWorker_ProgressChanged);
            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWorker_RunWorkerCompleted);
        }

        private void Saving_Load(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync();
        }

        void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            rpfFile.save(newRPFStream, backgroundWorker1, e);
        }

        void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Cancelled == true))
            {
            }
            else if (e.Error != null)
            {
                MessageBox.Show(RDR_Explorer.Properties.Resources.ResourceManager.GetString("failSaveArch")  + Environment.NewLine + e.Error.ToString(), RDR_Explorer.Properties.Resources.ResourceManager.GetString("err"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                this.Hide();
                MessageBox.Show(RDR_Explorer.Properties.Resources.ResourceManager.GetString("succArchive"), RDR_Explorer.Properties.Resources.ResourceManager.GetString("succ"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            this.Invoke(new MethodInvoker(delegate { this.Close(); }));
        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
        }
    }
}
