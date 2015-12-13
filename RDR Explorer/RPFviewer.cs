using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RPFLib;
using RPFLib.Common;
using System.Diagnostics;

namespace RDR_Explorer
{
    public partial class RPFviewer : Form
    {
        int countOfFiles = 0;
        int currentCount = 0;
        string extrPath = null;
        string EntirePath = null;
        string fileName = null;
        private Archive archiveFile;
        RPFLib.Common.Directory currentDir = null;
        List<fileSystemObject> masterlist = new List<fileSystemObject>();
        Inc.RageLIB.RSCTypes rscTypes = new Inc.RageLIB.RSCTypes();
        public RPFviewer(String path, String RPFname)
        {
            InitializeComponent();
            EntirePath = path;
            fileName = RPFname;
            listView1.Items.Clear();
            backgroundWorker1.RunWorkerAsync();
        }

        private void buildlist(RPFLib.Common.Directory dir)
        {
            masterlist.Clear();
            currentDir = dir;

            //Setup return dir
            foreach (fileSystemObject item in dir)
            {
                if (item.IsDirectory)
                {
                    var subdir = item as RPFLib.Common.Directory;
                    masterlist.Add(item);
                }
                else
                {
                    var subFile = item as RPFLib.Common.File;
                    masterlist.Add(item);
                }
            }
            setViewObjects(masterlist);
            pathLabel.Text = currentDir.FullName;
        }
        delegate void setViewObjectsDelegate(List<fileSystemObject> setlist);
        private void setViewObjects(List<fileSystemObject> setlist)
        {
            //sorting bunch of code
            List<fileSystemObject> sortedList = setlist.OrderBy(x => x.Name).ToList();

            //get the folders
            foreach (fileSystemObject myObject in sortedList)
            {
                if (InvokeRequired)
                {
                    Invoke(new setViewObjectsDelegate(setViewObjects), setlist);
                    return;
                }
                else
                {
                    ListViewItem myObjectListViewItem = new ListViewItem();
                    myObjectListViewItem.Text = myObject.Name;
                    if (myObject.IsDirectory)
                    {
                        myObjectListViewItem.Tag = "dir";
                    }
                    else
                    {
                        myObjectListViewItem.Tag = "file";
                    }

                    if (myObject.IsDirectory || myObject.IsReturnDirectory)
                    {
                        myObjectListViewItem.SubItems.Add(myObject.Attributes);
                        myObjectListViewItem.SubItems.Add(myObject.Size);
                        listView1.Items.Add(myObjectListViewItem);
                    }
                }
            }

            //get the files (0x)
            foreach (fileSystemObject myObject in sortedList)
            {
                if (InvokeRequired)
                {
                    Invoke(new setViewObjectsDelegate(setViewObjects), setlist);
                    return;
                }
                else
                {
                    ListViewItem myObjectListViewItem = new ListViewItem();
                    myObjectListViewItem.Text = myObject.Name;
                    if (myObject.IsDirectory)
                    {
                        myObjectListViewItem.Tag = "dir";
                    }
                    else
                    {
                        myObjectListViewItem.Tag = "file";
                    }

                    //change type if found, get hex files(unkown)
                    if (!myObject.IsReturnDirectory && !myObject.IsDirectory && myObject.Name.StartsWith("0x"))
                    {
                        string rscType = rscTypes.getType(myObject.Attributes);

                        if (rscType != "")
                        {
                            myObjectListViewItem.SubItems.Add(rscType);
                        }
                        else
                        {
                            myObjectListViewItem.SubItems.Add(myObject.Attributes);
                        }

                        myObjectListViewItem.SubItems.Add(myObject.SizeS);
                        listView1.Items.Add(myObjectListViewItem);
                    }
                }
            }
            //get the files (regular,already sorted with linq)
            foreach (fileSystemObject myObject in sortedList)
            {
                if (InvokeRequired)
                {
                    Invoke(new setViewObjectsDelegate(setViewObjects), setlist);
                    return;
                }
                else
                {
                    ListViewItem myObjectListViewItem = new ListViewItem();
                    myObjectListViewItem.Text = myObject.Name;
                    if (myObject.IsDirectory)
                    {
                        myObjectListViewItem.Tag = "dir";
                    }
                    else
                    {
                        myObjectListViewItem.Tag = "file";
                    }

                    //change type if found, get hex files(unkown)
                    if (!myObject.IsReturnDirectory && !myObject.IsDirectory && !myObject.Name.StartsWith("0x"))
                    {
                        string rscType = rscTypes.getType(myObject.Attributes);

                        if (rscType != "")
                        {
                            myObjectListViewItem.SubItems.Add(rscType);
                        }
                        else
                        {
                            myObjectListViewItem.SubItems.Add(myObject.Attributes);
                        }

                        myObjectListViewItem.SubItems.Add(myObject.SizeS);
                        listView1.Items.Add(myObjectListViewItem);
                    }
                }
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            using (BinaryReader s = new BinaryReader(new FileStream(EntirePath, FileMode.Open, FileAccess.Read)))
            {
                try
                {
                    char[] Magic = new char[4];
                    s.Read(Magic, 0, 4);
                    string magicStr = new string(Magic);
                    s.Close();
                    s.Dispose();
                    switch (magicStr)
                    {
                        case "RPF6":
                            {
                                archiveFile = new Version6();
                                break;
                            }
                        default:
                            MessageBox.Show(RDR_Explorer.Properties.Resources.ResourceManager.GetString("errInvArch"), RDR_Explorer.Properties.Resources.ResourceManager.GetString("err"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                    }
                    archiveFile.Open(EntirePath);
                    buildlist(archiveFile.RootDirectory);
                    //startBreadCrumb(archiveFile.RootDirectory);
                }
                catch (Exception ex)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        listView1.Items.Clear();
                        MessageBox.Show(ex.Message, RDR_Explorer.Properties.Resources.ResourceManager.GetString("err"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Application.Exit();
                    });
                }
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                if (listView1.SelectedItems[0].Tag.ToString() == "dir")
                {
                    RPFLib.Common.Directory directory = new RPFLib.Common.Directory();
                    foreach (RPFLib.Common.fileSystemObject entry in masterlist)
                    {
                        if (entry.Name == listView1.SelectedItems[0].Text)
                        {
                            directory = entry as RPFLib.Common.Directory;
                        }
                    }
                    listView1.Items.Clear();
                    buildlist(directory);
                    //addBreadCrumb(directory);
                }
                else
                {
                    RPFLib.Common.File file = new RPFLib.Common.File();
                    foreach (RPFLib.Common.fileSystemObject entry in masterlist)
                    {
                        if (entry.Name == listView1.SelectedItems[0].Text)
                        {
                            file = entry as RPFLib.Common.File;
                        }
                    }
                }
            }
        }



        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            backgroundWorker1.Dispose();
        }

        private void RPFviewer_FormClosed(object sender, FormClosedEventArgs e)
        {
            archiveFile.Close();
        }

        private void backButton_Click(object sender, EventArgs e)
        {
            if (currentDir.ParentDirectory != null)
            {
                listView1.Items.Clear();
                buildlist(currentDir.ParentDirectory);
            }

        }

        public void BrowseHier(RPFLib.Common.Directory dir, RPFLib.Common.Directory rDir)
        {
            foreach (RPFLib.Common.fileSystemObject d in dir)
            {
                if (d.IsDirectory)
                {
                    BrowseHier(d as RPFLib.Common.Directory, rDir);
                    System.IO.Directory.CreateDirectory(extrPath + "\\" + rDir.Name + "\\" + d.FullName.Replace(rDir.FullName + "\\", ""));
                }
                else
                {
                    currentCount++;
                    RPFLib.Common.File file = d as RPFLib.Common.File;
                    byte[] data = file.GetData(false);
                    if (!(System.IO.Directory.Exists(extrPath + "\\" + rDir.Name + "\\" + file.FullName.Replace(rDir.FullName + "\\", "").Replace(file.Name, ""))))
                        System.IO.Directory.CreateDirectory(extrPath + "\\" + rDir.Name + "\\" + file.FullName.Replace(rDir.FullName + "\\", "").Replace(file.Name, ""));
                    System.IO.File.WriteAllBytes(extrPath + "\\" + rDir.Name + "\\" + file.FullName.Replace(rDir.FullName + "\\", ""), data);
                }
            }

        }

        private void extrButton_Click(object sender, EventArgs e)
        {
            try
            {
                backgroundWorker2.RunWorkerAsync();
            }
            catch (Exception ex)
            {

            }

        }


        private int FolderCount(RPFLib.Common.Directory directory)
        {
            int ctr = 0;
            foreach (fileSystemObject x in directory)
            {
                if (x.IsDirectory)
                {
                    ctr++;
                }
            }
            return ctr;
        }

        private void uncompressButton_Click(object sender, EventArgs e)
        {
            backgroundWorker4.RunWorkerAsync();

        }

        private void extrAllButton_Click(object sender, EventArgs e)
        {
            using (var sfrm = new FolderBrowserDialog())
            {
                backgroundWorker3.RunWorkerAsync();
            }
        }

        public int CountFilesInHier(RPFLib.Common.Directory dir)
        {
            foreach (RPFLib.Common.fileSystemObject d in dir)
            {
                if (d.IsDirectory)
                {
                    Task taskA = Task.Factory.StartNew(() => CountFilesInHier(d as RPFLib.Common.Directory));
                    taskA.Wait();
                }
                else
                {
                    countOfFiles++;
                }
            }
            return countOfFiles;
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (backButton.GetCurrentParent().InvokeRequired)
                    backButton.GetCurrentParent().Invoke((MethodInvoker)(() => backButton.Enabled = false));
                else
                    backButton.Enabled = false;
                if (extrButton.GetCurrentParent().InvokeRequired)
                    extrButton.GetCurrentParent().Invoke((MethodInvoker)(() => extrButton.Enabled = false));
                else
                    extrButton.Enabled = false;
                if (extrAllButton.GetCurrentParent().InvokeRequired)
                    extrAllButton.GetCurrentParent().Invoke((MethodInvoker)(() => extrAllButton.Enabled = false));
                else
                    extrAllButton.Enabled = false;
                if (uncompressButton.GetCurrentParent().InvokeRequired)
                    uncompressButton.GetCurrentParent().Invoke((MethodInvoker)(() => uncompressButton.Enabled = false));
                else
                    uncompressButton.Enabled = false;

                currentCount = 0;

                using (var sfrm = new FolderBrowserDialog())
                {
                    DialogResult testthing = DialogResult.None;
                    if (this.InvokeRequired)
                        this.Invoke((MethodInvoker)(() => testthing = sfrm.ShowDialog(this)));
                    else
                        testthing = sfrm.ShowDialog(this);
                    if (testthing == DialogResult.OK)
                    {
                        extrPath = sfrm.SelectedPath;
                        List<string> nameList = new List<string>();
                        if (this.InvokeRequired)
                        {
                            this.Invoke((MethodInvoker)(() =>
                            {
                                foreach (ListViewItem myItem in listView1.SelectedItems)
                                {
                                    nameList.Add(myItem.Text);
                                }
                            }));
                        }
                        else
                        {
                            foreach (ListViewItem myItem in listView1.SelectedItems)
                            {
                                nameList.Add(myItem.Text);
                            }
                        }

                        countOfFiles = 0;
                        Task taskA = Task.Factory.StartNew(() =>
                        {
                            foreach (string myItem in nameList)
                            {
                                foreach (RPFLib.Common.fileSystemObject entry in masterlist)
                                {
                                    if (entry.Name == myItem)
                                    {
                                        if (entry.IsDirectory)
                                        {
                                            RPFLib.Common.Directory dir = entry as RPFLib.Common.Directory;
                                            CountFilesInHier(dir);
                                        }
                                        else
                                        {
                                            countOfFiles++;
                                        }
                                    }
                                }
                            }

                        });
                        taskA.Wait();
                        if (toolStripProgressBar1.GetCurrentParent().InvokeRequired)
                        {
                            toolStripProgressBar1.GetCurrentParent().Invoke((MethodInvoker)(() =>
                            {
                                timer1.Enabled = true;
                                timer1.Start();
                            }));
                        }
                        else
                        {
                            timer1.Enabled = true;
                            timer1.Start();
                        }
                        Task taskB = Task.Factory.StartNew(() =>
                        {
                            foreach (string myItem in nameList)
                            {
                                foreach (RPFLib.Common.fileSystemObject entry in masterlist)
                                {
                                    if (entry.Name == myItem)
                                    {
                                        if (entry.IsDirectory)
                                        {
                                            RPFLib.Common.Directory dir = entry as RPFLib.Common.Directory;
                                            System.IO.Directory.CreateDirectory(extrPath + "\\" + dir.Name);
                                            BrowseHier(dir, dir);
                                        }
                                        else
                                        {
                                            RPFLib.Common.File file = entry as RPFLib.Common.File;
                                            byte[] data = file.GetData(false);
                                            System.IO.File.WriteAllBytes(sfrm.SelectedPath + "\\" + file.Name, data);
                                        }
                                    }
                                }
                            }
                        });
                        taskB.Wait();
                        if (toolStripProgressBar1.GetCurrentParent().InvokeRequired)
                        {
                            toolStripProgressBar1.GetCurrentParent().Invoke((MethodInvoker)(() =>
                            {
                                timer1.Stop();
                                timer1.Enabled = false;
                                toolStripProgressBar1.Value = 0;
                            }));
                        }
                        else
                        {
                            timer1.Stop();
                            timer1.Enabled = false;
                            toolStripProgressBar1.Value = 0;
                        }
                    }
                }
                if (backButton.GetCurrentParent().InvokeRequired)
                    backButton.GetCurrentParent().Invoke((MethodInvoker)(() => backButton.Enabled = true));
                else
                    backButton.Enabled = true;
                if (extrButton.GetCurrentParent().InvokeRequired)
                    extrButton.GetCurrentParent().Invoke((MethodInvoker)(() => extrButton.Enabled = true));
                else
                    extrButton.Enabled = true;
                if (extrAllButton.GetCurrentParent().InvokeRequired)
                    extrAllButton.GetCurrentParent().Invoke((MethodInvoker)(() => extrAllButton.Enabled = true));
                else
                    extrAllButton.Enabled = true;
                if (uncompressButton.GetCurrentParent().InvokeRequired)
                    uncompressButton.GetCurrentParent().Invoke((MethodInvoker)(() => uncompressButton.Enabled = true));
                else
                    uncompressButton.Enabled = true;
            }
            catch (Exception ex)
            {

            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            toolStripProgressBar1.Maximum = countOfFiles;
            toolStripProgressBar1.Value = currentCount;
        }

        private void toolStripProgressBar1_Click(object sender, EventArgs e)
        {

        }

        private void backgroundWorker3_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (backButton.GetCurrentParent().InvokeRequired)
                    backButton.GetCurrentParent().Invoke((MethodInvoker)(() => backButton.Enabled = false));
                else
                    backButton.Enabled = false;
                if (extrButton.GetCurrentParent().InvokeRequired)
                    extrButton.GetCurrentParent().Invoke((MethodInvoker)(() => extrButton.Enabled = false));
                else
                    extrButton.Enabled = false;
                if (extrAllButton.GetCurrentParent().InvokeRequired)
                    extrAllButton.GetCurrentParent().Invoke((MethodInvoker)(() => extrAllButton.Enabled = false));
                else
                    extrAllButton.Enabled = false;
                if (uncompressButton.GetCurrentParent().InvokeRequired)
                    uncompressButton.GetCurrentParent().Invoke((MethodInvoker)(() => uncompressButton.Enabled = false));
                else
                    uncompressButton.Enabled = false;
                using (FolderBrowserDialog sfrm = new FolderBrowserDialog())
                {
                    DialogResult testthing = DialogResult.None;
                    if (this.InvokeRequired)
                        this.Invoke((MethodInvoker)(() => testthing = sfrm.ShowDialog(this)));
                    else
                        testthing = sfrm.ShowDialog(this);
                    if (testthing == DialogResult.OK)
                    {
                        extrPath = sfrm.SelectedPath;
                        countOfFiles = 0;
                        Task taskA = Task.Factory.StartNew(() =>
                        {
                            foreach (RPFLib.Common.fileSystemObject entry in masterlist)
                            {
                                if (entry.IsDirectory)
                                {
                                    RPFLib.Common.Directory dir = entry as RPFLib.Common.Directory;
                                    CountFilesInHier(dir);
                                }
                                else
                                {
                                    countOfFiles++;
                                }

                            }
                        });
                        taskA.Wait();

                        if (toolStripProgressBar1.GetCurrentParent().InvokeRequired)
                        {
                            toolStripProgressBar1.GetCurrentParent().Invoke((MethodInvoker)(() =>
                            {
                                timer1.Enabled = true;
                                timer1.Start();
                            }));
                        }
                        else
                        {
                            timer1.Enabled = true;
                            timer1.Start();
                        }

                        Task taskB = Task.Factory.StartNew(() =>
                        {
                            foreach (RPFLib.Common.fileSystemObject entry in masterlist)
                            {
                                if (entry.IsDirectory)
                                {
                                    RPFLib.Common.Directory dir = entry as RPFLib.Common.Directory;
                                    System.IO.Directory.CreateDirectory(extrPath + "\\" + dir.Name);
                                    BrowseHier(dir, dir);
                                }
                                else
                                {
                                    RPFLib.Common.File file = entry as RPFLib.Common.File;
                                    byte[] data = file.GetData(false);
                                    System.IO.File.WriteAllBytes(sfrm.SelectedPath + "\\" + file.Name, data);
                                }
                            }
                        });
                        taskB.Wait();

                        if (toolStripProgressBar1.GetCurrentParent().InvokeRequired)
                        {
                            toolStripProgressBar1.GetCurrentParent().Invoke((MethodInvoker)(() =>
                            {
                                timer1.Stop();
                                timer1.Enabled = false;
                                toolStripProgressBar1.Value = 0;
                            }));
                        }
                        else
                        {
                            timer1.Stop();
                            timer1.Enabled = false;
                            toolStripProgressBar1.Value = 0;
                        }
                    }
                }
                if (backButton.GetCurrentParent().InvokeRequired)
                    backButton.GetCurrentParent().Invoke((MethodInvoker)(() => backButton.Enabled = true));
                else
                    backButton.Enabled = true;
                if (extrButton.GetCurrentParent().InvokeRequired)
                    extrButton.GetCurrentParent().Invoke((MethodInvoker)(() => extrButton.Enabled = true));
                else
                    extrButton.Enabled = true;
                if (extrAllButton.GetCurrentParent().InvokeRequired)
                    extrAllButton.GetCurrentParent().Invoke((MethodInvoker)(() => extrAllButton.Enabled = true));
                else
                    extrAllButton.Enabled = true;
                if (uncompressButton.GetCurrentParent().InvokeRequired)
                    uncompressButton.GetCurrentParent().Invoke((MethodInvoker)(() => uncompressButton.Enabled = true));
                else
                    uncompressButton.Enabled = true;
            }
            catch (Exception ex)
            {
                Application.Exit();
            }
        }

        private void backgroundWorker4_DoWork(object sender, DoWorkEventArgs e)
        {
            if (backButton.GetCurrentParent().InvokeRequired)
                backButton.GetCurrentParent().Invoke((MethodInvoker)(() => backButton.Enabled = false));
            else
                backButton.Enabled = false;
            if (extrButton.GetCurrentParent().InvokeRequired)
                extrButton.GetCurrentParent().Invoke((MethodInvoker)(() => extrButton.Enabled = false));
            else
                extrButton.Enabled = false;
            if (extrAllButton.GetCurrentParent().InvokeRequired)
                extrAllButton.GetCurrentParent().Invoke((MethodInvoker)(() => extrAllButton.Enabled = false));
            else
                extrAllButton.Enabled = false;
            if (uncompressButton.GetCurrentParent().InvokeRequired)
                uncompressButton.GetCurrentParent().Invoke((MethodInvoker)(() => uncompressButton.Enabled = false));
            else
                uncompressButton.Enabled = false;

            using (var sfrm = new FolderBrowserDialog())
            {
                int countOfItems = 0;
                if (listView1.InvokeRequired)
                    listView1.Invoke((MethodInvoker)(() => countOfItems = listView1.SelectedItems.Count));
                else
                    countOfItems = listView1.SelectedItems.Count;
                if(countOfItems == 1)
                {
                    bool isDir = false;
                    foreach (RPFLib.Common.fileSystemObject fEntry in masterlist)
                    {
                        string fSelectedString = "";
                        if (this.InvokeRequired)
                        {
                            this.Invoke((MethodInvoker)(() =>
                            {
                                fSelectedString = listView1.SelectedItems[0].Text;
                            }));
                        }
                        else
                        {
                            fSelectedString = listView1.SelectedItems[0].Text;
                        }
                        if (fEntry.Name == fSelectedString)
                        {
                            isDir = fEntry.IsDirectory;
                        }
                    }
                    if (!(isDir))
                    {
                        DialogResult testthing = DialogResult.None;
                        if (this.InvokeRequired)
                            this.Invoke((MethodInvoker)(() => testthing = sfrm.ShowDialog(this)));
                        else
                            testthing = sfrm.ShowDialog(this);
                        if (testthing == DialogResult.OK)
                        {
                            List<fileSystemObject> objectList = new List<fileSystemObject>();
                            if (!(System.IO.Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Reditec\\RDR Explorer\\extracted")))
                            {
                                System.IO.Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Reditec\\RDR Explorer\\extracted");
                            }

                            RPFLib.Common.File file = new RPFLib.Common.File();
                            foreach (RPFLib.Common.fileSystemObject entry in masterlist)
                            {
                                string selectedString = "";
                                if (this.InvokeRequired)
                                {
                                    this.Invoke((MethodInvoker)(() =>
                                    {
                                        selectedString = listView1.SelectedItems[0].Text;
                                    }));
                                }
                                else
                                {
                                    selectedString = listView1.SelectedItems[0].Text;
                                }
                                if (entry.Name == selectedString && !(entry.IsDirectory))
                                {
                                    file = entry as RPFLib.Common.File;
                                    byte[] data = file.GetData(false);
                                    System.IO.File.WriteAllBytes(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Reditec\\RDR Explorer\\extracted\\" + file.Name, data);
                                    ProcessStartInfo startInfo = new ProcessStartInfo("RSCUnpacker.exe");
                                    startInfo.Arguments = "\"" + Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Reditec\\RDR Explorer\\extracted\\" + file.Name + "\" -Xbox360";
                                    startInfo.UseShellExecute = false;
                                    startInfo.CreateNoWindow = true;
                                    Process Extractor = Process.Start(startInfo);
                                    Extractor.WaitForExit();
                                    System.IO.File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Reditec\\RDR Explorer\\extracted\\" + file.Name);
                                    System.IO.DirectoryInfo myDir = new System.IO.DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Reditec\\RDR Explorer\\extracted\\");
                                    foreach (System.IO.FileInfo myFile in myDir.GetFiles())
                                    {
                                        if (System.IO.File.Exists(sfrm.SelectedPath + "\\" + myFile.Name))
                                        {
                                            System.IO.File.Delete(sfrm.SelectedPath + "\\" + myFile.Name);
                                        }
                                        myFile.MoveTo(sfrm.SelectedPath + "\\" + myFile.Name);
                                    }
                                }
                                else
                                {

                                }
                            }
                        }
                    }
                
                    }
                }
            if (backButton.GetCurrentParent().InvokeRequired)
                backButton.GetCurrentParent().Invoke((MethodInvoker)(() => backButton.Enabled = true));
            else
                backButton.Enabled = true;
            if (extrButton.GetCurrentParent().InvokeRequired)
                extrButton.GetCurrentParent().Invoke((MethodInvoker)(() => extrButton.Enabled = true));
            else
                extrButton.Enabled = true;
            if (extrAllButton.GetCurrentParent().InvokeRequired)
                extrAllButton.GetCurrentParent().Invoke((MethodInvoker)(() => extrAllButton.Enabled = true));
            else
                extrAllButton.Enabled = true;
            if (uncompressButton.GetCurrentParent().InvokeRequired)
                uncompressButton.GetCurrentParent().Invoke((MethodInvoker)(() => uncompressButton.Enabled = true));
            else
                uncompressButton.Enabled = true;
        }
        }
    }
