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
                if(listView1.SelectedItems[0].Tag.ToString() == "dir")
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
            if(currentDir.ParentDirectory != null)
            {
                listView1.Items.Clear();
                buildlist(currentDir.ParentDirectory);
            }
            
        }

        public void BrowseHier(RPFLib.Common.Directory dir, RPFLib.Common.Directory rDir)
        {
            foreach (RPFLib.Common.fileSystemObject d in dir)
            {
                if(d.IsDirectory)
                {
                    BrowseHier(d as RPFLib.Common.Directory, rDir);
                    System.IO.Directory.CreateDirectory(extrPath + "\\" + rDir.Name + "\\" + d.FullName.Replace(rDir.FullName + "\\", ""));
                }
                else
                {
                    RPFLib.Common.File file = d as RPFLib.Common.File;
                    byte[] data = file.GetData(false);
                    if (!(System.IO.Directory.Exists(extrPath + "\\" + rDir.Name + "\\" + file.FullName.Replace(rDir.FullName + "\\", "").Replace(file.Name, ""))))
                        System.IO.Directory.CreateDirectory(extrPath + "\\" + rDir.Name + "\\" + file.FullName.Replace(rDir.FullName + "\\", "").Replace(file.Name, ""));
                    System.IO.File.WriteAllBytes(extrPath + "\\" + rDir.Name + "\\" + file.FullName.Replace(rDir.FullName + "\\", "") , data);
                }
            }
            
        }

        private void extrButton_Click(object sender, EventArgs e)
        {
            if(listView1.SelectedItems.Count > 1)
            {
                using (var sfrm = new FolderBrowserDialog())
                {
                    if (sfrm.ShowDialog(this) == DialogResult.OK)
                    {
                        extrPath = sfrm.SelectedPath;
                        foreach (ListViewItem myItem in listView1.SelectedItems)
                        {
                            foreach (RPFLib.Common.fileSystemObject entry in masterlist)
                            {
                                if (entry.Name == myItem.Text)
                                {
                                    if (entry.IsDirectory)
                                    {
                                        RPFLib.Common.Directory dir = entry as RPFLib.Common.Directory;
                                        System.IO.Directory.CreateDirectory(extrPath+ "\\" + dir.Name);
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
                    }                    
                }
            }
            else
            {
                using (var sfrm = new SaveFileDialog())
                {
                    RPFLib.Common.File file = new RPFLib.Common.File();
                    foreach (RPFLib.Common.fileSystemObject entry in masterlist)
                    {
                        if (entry.Name == listView1.SelectedItems[0].Text && !(entry.IsReturnDirectory))
                        {
                            if (entry.IsDirectory)
                            {
                                //RPFLib.Common.Directory directory = entry as RPFLib.Common.Directory;
                                //long longcount = 1;
                                //do
                                //{
                                //    longcount = FolderCount(directory);
                                //    if (longcount > 0)
                                //    {
                                //        foreach (fileSystemObject item in directory)
                                //        {
                                //            MessageBox.Show(item.Name);
                                //        }
                                //    }
                                //} while (longcount != 0);

                                

                            }
                            else
                            {
                                file = entry as RPFLib.Common.File;
                                sfrm.FileName = file.Name;
                                if (sfrm.ShowDialog(this) == DialogResult.OK)
                                {
                                    byte[] data = file.GetData(false);
                                    System.IO.File.WriteAllBytes(sfrm.FileName, data);
                                }
                            }
                        }
                    }
                }
            }
            
        }

        
        private int FolderCount(RPFLib.Common.Directory directory)
        {
            int ctr = 0;
            foreach(fileSystemObject x in directory)
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
            using (var sfrm = new FolderBrowserDialog())
            {
                if (sfrm.ShowDialog(this) == DialogResult.OK)
                {
                    List<fileSystemObject> objectList = new List<fileSystemObject>();
                    if (!(System.IO.Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Reditec\\RDR Explorer\\extracted")))
                    {
                        System.IO.Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Reditec\\RDR Explorer\\extracted");
                    }
                    if (listView1.SelectedItems.Count > 1)
                    {
                        foreach (ListViewItem myItem in listView1.SelectedItems)
                        {
                            RPFLib.Common.File file = new RPFLib.Common.File();
                            foreach (RPFLib.Common.fileSystemObject entry in masterlist)
                            {
                                if (entry.Name == myItem.Text && !(entry.IsDirectory) && !(entry.IsReturnDirectory))
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
                            }
                        }



                    }
                    else
                    {
                        RPFLib.Common.File file = new RPFLib.Common.File();
                        foreach (RPFLib.Common.fileSystemObject entry in masterlist)
                        {
                            if (entry.Name == listView1.SelectedItems[0].Text && !(entry.IsDirectory) && !(entry.IsReturnDirectory))
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
                        }
                    }
                }
            }
        }

        private void extrAllButton_Click(object sender, EventArgs e)
        {
            using (var sfrm = new FolderBrowserDialog())
            {
                if (sfrm.ShowDialog(this) == DialogResult.OK)
                {
                    extrPath = sfrm.SelectedPath;
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
                }
            }
        }
    }
 }
