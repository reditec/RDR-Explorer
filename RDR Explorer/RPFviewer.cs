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

namespace RDR_Explorer
{
    public partial class RPFviewer : Form
    {
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
                            MessageBox.Show("Invalid archive selected", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                //returnDir
                //if(listView1.SelectedItems.)
                //    var returndirectory = filelistview.SelectedObject as RPFLib.Common.ReturnDir;
                //    filelistview.ClearObjects();
                //    buildlist(returndirectory.Tag);
                //    removeBreadCrumb();
                //directory
                    //var directory = filelistview.SelectedObject as RPFLib.Common.Directory;
                    //filelistview.ClearObjects();
                    //buildlist(directory);
                    //addBreadCrumb(directory);
                //file
                    //if (currentGame == "rdrXbox")
                    //{
                    //    var fileEntry = filelistview.SelectedObject as RPFLib.Common.File;
                    //    switch (fileEntry.resourcetype)
                    //    {
                    //        /*   case 0:
                    //               {
                    //                   Viewers.TextView TextViewer = new Viewers.TextView(fileEntry.GetData(true), fileEntry);
                    //                   TextViewer.ShowDialog();
                    //                   filelistview.RefreshObjects(masterlist);
                    //               }
                    //               break;
                    //           case 1:
                    //               {
                    //                   Viewers.StringsView StringViewer = new Viewers.StringsView(fileEntry.GetData(true));
                    //                   StringViewer.ShowDialog();
                    //               }
                    //               break;
                    //           case 2:
                    //               {
                    //                   Viewers.xscView xscViewer = new Viewers.xscView(fileEntry.GetData(true));
                    //                   xscViewer.ShowDialog();
                    //               }
                    //               break;
                    //           default:
                    //               break; */
                    //    }
                    //}
                    //else if (currentGame == "gtaVXbox")
                    //{
                    //    /*         var fileEntry = filelistview.SelectedObject as RPFLib.Common.File;
                    //             switch (Path.GetExtension(fileEntry.Name))
                    //             {
                    //                 case "xsc":
                    //                     {
                    //                         Viewers.xscViewV7 xscViewer = new Viewers.xscViewV7(fileEntry.GetData(true));
                    //                         xscViewer.ShowDialog();
                    //                     }
                    //                     break;
                    //                 default:
                    //                     Viewers.TextView TextViewer = new Viewers.TextView(fileEntry.GetData(true), fileEntry);
                    //                     TextViewer.ShowDialog();
                    //                     filelistview.RefreshObjects(masterlist);
                    //                     break;
                    //             } */
                    //}
                //}
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

        private void extrButton_Click(object sender, EventArgs e)
        {
            List<fileSystemObject> objectList = new List<fileSystemObject>();
            if (listView1.SelectedItems.Count == 1)
            {


                using (var sfrm = new SaveFileDialog())
                {
                    RPFLib.Common.File file = new RPFLib.Common.File();
                    foreach (RPFLib.Common.fileSystemObject entry in masterlist)
                    {
                        if (entry.Name == listView1.SelectedItems[0].Text && !(entry.IsDirectory) && !(entry.IsReturnDirectory))
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
    }
