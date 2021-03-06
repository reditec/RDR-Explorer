﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using RDR_Explorer.Inc;
using RageLib.Common;
using System.Threading;
using System.Diagnostics;
using System.Net;

namespace RDR_Explorer
{
    public partial class Main : Form
    {
        public Main()
        {
            if(Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Reditec\\RDR Explorer"))
            {
                File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Reditec\\RDR Explorer\\base.bin");
                
            }
            else
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Reditec\\RDR Explorer");
            }            
            //Disable some UI things
            InitializeComponent();
            openToolStripMenuItem1.Enabled = false;
            showInWindowsExplorerToolStripMenuItem2.Enabled = false;
            copyPathToolStripMenuItem2.Enabled = false;
            propertiesToolStripMenuItem1.Enabled = false;

            if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Reditec\\RDR Explorer"))
            {
                File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Reditec\\RDR Explorer\\base.bin");
            }
        }

        public string gameEXE = "";
        
        IniFile settingsIni = new IniFile(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Reditec\\RDR Explorer\\Settings.ini");
        byte[] key = new Byte[0];
        string gameDir = "";
        string currHier = "";
        bool isWorking = false; //Is the Background Worker currently working? No, it isn't! Aaah whatever...
        string filesystempath = "";
        string currentPath = "";
        FileInfo[] fileEntries = null; //array for displaying files
        DirectoryInfo[] folderEntries = null; ////array for displaying fodlers

        private void CheckExeFile()
        {
            
            do
            {


                openFolder.Description = RDR_Explorer.Properties.Resources.ResourceManager.GetString("selDir") + " " + "default.xex.";

                DialogResult dr = openFolder.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    if(Directory.Exists(openFolder.SelectedPath.ToString()))
                    {
                        if (Directory.GetFiles(openFolder.SelectedPath.ToString(), "default.xex", SearchOption.TopDirectoryOnly).Length > 0)
                        {
                            gameEXE = openFolder.SelectedPath + "\\default.xex";
                            
                            RageLib.Common.KeyStore.gameEXE = gameEXE;
                            RageLib.Common.KeyUtil.MYgameExe = gameEXE;
                            KeyUtil keyUtil = new KeyUtilRDR();
                            if(File.Exists("xextool.exe"))
                            {
                                byte[] key = keyUtil.FindKey(gameEXE, "RDR");
                                RPFLib.Common.DataUtil.setKey(key);
                                if (!(key == null))
                                {
                                    settingsIni.Write("GamePath", openFolder.SelectedPath);
                                }
                                else
                                {
                                    MessageBox.Show(RDR_Explorer.Properties.Resources.ResourceManager.GetString("UnsuppXEX")  + Environment.NewLine + RDR_Explorer.Properties.Resources.ResourceManager.GetString("adminFIX"), Environment.NewLine + RDR_Explorer.Properties.Resources.ResourceManager.GetString("UnsuppEX"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                            else
                            {
                                MessageBox.Show(RDR_Explorer.Properties.Resources.ResourceManager.GetString("xextDEL"), RDR_Explorer.Properties.Resources.ResourceManager.GetString("xextNF"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                                Application.Exit();
                            }
                            
                        }
                        else
                        {
                            MessageBox.Show(RDR_Explorer.Properties.Resources.ResourceManager.GetString("xexNF") + Environment.NewLine + RDR_Explorer.Properties.Resources.ResourceManager.GetString("xexSPEC"), RDR_Explorer.Properties.Resources.ResourceManager.GetString("xexNFtitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show(RDR_Explorer.Properties.Resources.ResourceManager.GetString("sfNF") + Environment.NewLine + RDR_Explorer.Properties.Resources.ResourceManager.GetString("xexSPEC"), RDR_Explorer.Properties.Resources.ResourceManager.GetString("sfNFtitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    
                }
                else
                {
                    openFolder.SelectedPath = "";
                    break;
                }
            } while (!(settingsIni.KeyExists("GamePath")));

            if (openFolder.SelectedPath == "")
            {
                Application.Exit();
                return;
            }
            
        }
        private void Main_Shown(object sender, EventArgs e)
        {
            if (!(File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Reditec\\RDR Explorer\\KnownFilenames.txt")))
            {
                MessageBox.Show(RDR_Explorer.Properties.Resources.ResourceManager.GetString("appdTXT"), RDR_Explorer.Properties.Resources.ResourceManager.GetString("nfTXT"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            statusProgress.ProgressBar.Value = 0;
            if (!(settingsIni.KeyExists("FirstLaunch")))
            {
                MessageBox.Show(RDR_Explorer.Properties.Resources.ResourceManager.GetString("thks") + Environment.NewLine + RDR_Explorer.Properties.Resources.ResourceManager.GetString("wip") + Environment.NewLine + RDR_Explorer.Properties.Resources.ResourceManager.GetString("gtaf") + Environment.NewLine + RDR_Explorer.Properties.Resources.ResourceManager.GetString("rptbg"), RDR_Explorer.Properties.Resources.ResourceManager.GetString("welc"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                settingsIni.Write("FirstLaunch", "true");
            }
            if (!(settingsIni.KeyExists("GamePath")))
            {

                CheckExeFile();
                //settingsIni.Write("FirstLaunch", "true");
            }
            else
            {
                if (Directory.Exists(settingsIni.Read("GamePath")))
                {
                    if (Directory.GetFiles(settingsIni.Read("GamePath"), "default.xex", SearchOption.TopDirectoryOnly).Length > 0)
                    {
                        gameEXE = settingsIni.Read("GamePath") + "\\default.xex";

                        RageLib.Common.KeyStore.gameEXE = gameEXE;
                        RageLib.Common.KeyUtil.MYgameExe = gameEXE;
                        KeyUtil keyUtil = new KeyUtilRDR();
                        if (File.Exists("xextool.exe"))
                        {
                            key = keyUtil.FindKey(gameEXE, "RDR");
                            RPFLib.Common.DataUtil.setKey(key);
                            if (!(key == null))
                            {
                                settingsIni.Write("GamePath", settingsIni.Read("GamePath"));
                            }
                            else
                            {
                                MessageBox.Show(RDR_Explorer.Properties.Resources.ResourceManager.GetString("UnsuppXEX") + Environment.NewLine + RDR_Explorer.Properties.Resources.ResourceManager.GetString("adminFIX"), RDR_Explorer.Properties.Resources.ResourceManager.GetString("UnsuppEX"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                                CheckExeFile();
                            }
                        }
                        else
                        {
                            MessageBox.Show(RDR_Explorer.Properties.Resources.ResourceManager.GetString("xextDEL"), RDR_Explorer.Properties.Resources.ResourceManager.GetString("xextNF"));
                            Application.Exit();
                        }
                    }
                    else
                    {
                        MessageBox.Show(RDR_Explorer.Properties.Resources.ResourceManager.GetString("sfNF") + Environment.NewLine + RDR_Explorer.Properties.Resources.ResourceManager.GetString("xexSPEC"), RDR_Explorer.Properties.Resources.ResourceManager.GetString("sfNFtitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                 }
                 else
                 {
                        MessageBox.Show(RDR_Explorer.Properties.Resources.ResourceManager.GetString("xexNF") + Environment.NewLine + RDR_Explorer.Properties.Resources.ResourceManager.GetString("xexSPEC"), RDR_Explorer.Properties.Resources.ResourceManager.GetString("xexNFtitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        CheckExeFile();
                 }
             }

            if (settingsIni.Read("GamePath") != "")
            {
                
                
                    
                    statusProgress.ProgressBar.Value = 100;
                    statusLabel.Text = RDR_Explorer.Properties.Resources.ResourceManager.GetString("keyF");
                    gameDir = settingsIni.Read("GamePath");

                    DirectoryInfo root = new DirectoryInfo(gameDir);
                    PrepareList();

                    if (Directory.Exists(gameDir))
                    {
                        try
                        {
                            DirectoryInfo[] directories = root.GetDirectories();
                            if (directories.Length > 0)
                            {
                                foreach (DirectoryInfo directory in directories)
                                {
                                    treeView1.Nodes[0].Nodes.Add(directory.Name, directory.Name, 0, 0);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, RDR_Explorer.Properties.Resources.ResourceManager.GetString("err"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        treeView1.Nodes[0].Expand();
                    }
                }
                else
                {
                    MessageBox.Show(RDR_Explorer.Properties.Resources.ResourceManager.GetString("adminREQ"), RDR_Explorer.Properties.Resources.ResourceManager.GetString("accDEN"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }              
            
            
        }

        private void PrepareList()
        {

            currHier = "";

            listView1.Clear();

            if (Directory.Exists(gameDir))
            {

                if (Directory.EnumerateFileSystemEntries(gameDir, "*", SearchOption.AllDirectories).Count() > 0)
                {

                    ContentDirectory();

                    DirectoryInfo root = new DirectoryInfo(gameDir);
                    fileEntries = root.GetFiles();
                    folderEntries = root.GetDirectories();
                    backgroundWorker1.RunWorkerAsync();
                }
                else
                {
                    EmptyDirectory();
                }
            }

        }

        private void EmptyDirectory()
        {
            EmptyFolderLabel.Visible = true;
            listView1.Clear();
        }

        //for displaying directories with content
        private void ContentDirectory()
        {
            EmptyFolderLabel.Visible = false;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(50);
            isWorking = true;
            Invoke((MethodInvoker)(() => backToolStripButton.Enabled = false));


            Invoke((MethodInvoker)(() => listView1.Clear()));


            foreach (DirectoryInfo fentry in folderEntries)
            {

                // Set a default icon for the file.
                Icon iconForFile = SystemIcons.WinLogo;

                ListViewItem item = new ListViewItem(fentry.Name, 1);


                if (fentry.EnumerateFileSystemInfos().Count() > 0)
                {
                    iconForFile = RDR_Explorer.Properties.Resources.full;

                    if (!imageList1.Images.ContainsKey("???"))
                    {

                        addImages(iconForFile, "???");


                    }
                    item.ImageKey = "???";
                }
                else
                {
                    iconForFile = RDR_Explorer.Properties.Resources.empty;

                    if (!imageList1.Images.ContainsKey("????"))
                    {

                        addImages(iconForFile, "????");


                    }
                    item.ImageKey = "????";
                }

                try
                {
                    Invoke((MethodInvoker)(() => listView1.Items.Add(item)));
                }
                catch (Exception ex)
                {
                    //
                }



            }


            foreach (FileInfo fentry in fileEntries)
            {

                // Set a default icon for the file.
                Icon iconForFile = SystemIcons.WinLogo;

                ListViewItem item = new ListViewItem(fentry.Name, 1);
                if(fentry.Extension == ".xex")
                {
                    iconForFile = RDR_Explorer.Properties.Resources.xex;
                }
                else if (fentry.Extension == ".rpf")
                {
                    iconForFile = RDR_Explorer.Properties.Resources.rpf;
                }
                else if(fentry.Extension == "")
                {
                    iconForFile = RDR_Explorer.Properties.Resources.noExt;
                }
                else
                {
                    iconForFile = Icon.ExtractAssociatedIcon(fentry.FullName);
                }
                


                // Check to see if the image collection contains an image 
                // for this extension, using the extension as a key. 
                if (!imageList1.Images.ContainsKey(fentry.Extension))
                {
                    if(fentry.Extension == "")
                    {
                        addImages(iconForFile, ".");
                    }
                    else
                    {
                        addImages(iconForFile, fentry.Extension);
                    }
                    


                }
                if (fentry.Extension == "")
                {
                    item.ImageKey = ".";
                }
                else
                { 
                    item.ImageKey = fentry.Extension; //wait a moment
                }
                try
                {
                    Invoke((MethodInvoker)(() => listView1.Items.Add(item)));
                }
                catch (Exception ex)
                {
                    //Application exits well while loading
                }
            }

            Invoke((MethodInvoker)(() => backToolStripButton.Enabled = true));
        }

        private void addImages(Icon iconForFile, String extension)
        {
            Invoke((MethodInvoker)(() => imageList1.Images.Add(extension, iconForFile)));
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (listView1.FocusedItem.Bounds.Contains(e.Location) == true)
                {
                    if (!(listView1.SelectedItems.Count > 1))
                    {
                        showInWindowsExplorerToolStripMenuItem1.Enabled = false;
                        if (listView1.SelectedItems[0].ImageKey == "????" || listView1.SelectedItems[0].ImageKey == "???")
                        {
                            showInWindowsExplorerToolStripMenuItem1.Enabled = true;
                        }
                        rightStrip.Show(Cursor.Position);
                    }
                }
            }
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                openFolderVoid();
            }
        }

        private void openFolderVoid()
        {
            if (listView1.SelectedItems[0].ImageKey == "???" || listView1.SelectedItems[0].ImageKey == "????")
            {
                currHier = currHier + listView1.SelectedItems[0].Text + @"\";

                filesystempath = gameDir + @"\" + currHier;
                if (Directory.Exists(filesystempath))
                {


                    if (Directory.EnumerateFileSystemEntries(filesystempath, "*", SearchOption.AllDirectories).Count() > 0)
                    {

                        ContentDirectory();

                        DirectoryInfo root = new DirectoryInfo(filesystempath);
                        fileEntries = root.GetFiles();
                        folderEntries = root.GetDirectories();
                        backgroundWorker1.RunWorkerAsync();
                    }
                    else
                    {
                        EmptyDirectory();
                    }
                    //Disable some UI things
                    openToolStripMenuItem1.Enabled = false;
                    showInWindowsExplorerToolStripMenuItem2.Enabled = false;
                    copyPathToolStripMenuItem2.Enabled = false;
                    propertiesToolStripMenuItem1.Enabled = false;
                    isWorking = false;
                }

            }
            else if (listView1.SelectedItems[0].ImageKey == ".rpf")
            {
                RPFviewer newForm = new RPFviewer(gameDir + @"\" + currHier + listView1.SelectedItems[0].Text, listView1.SelectedItems[0].Text);
                newForm.Show();
            }
            else
            {
                Process.Start(gameDir + @"\" + currHier + @"\" + listView1.SelectedItems[0].Text.ToString());
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0 && listView1.FocusedItem != null)
            {
                openToolStripMenuItem1.Enabled = true;
                if (listView1.FocusedItem.ImageKey == "????" || listView1.FocusedItem.ImageKey == "???")
                {
                    showInWindowsExplorerToolStripMenuItem2.Enabled = true;
                }
                copyPathToolStripMenuItem2.Enabled = true;
                propertiesToolStripMenuItem1.Enabled = true;
            }
            else
            {
                openToolStripMenuItem1.Enabled = false;
                showInWindowsExplorerToolStripMenuItem2.Enabled = false;
                copyPathToolStripMenuItem2.Enabled = false;
                propertiesToolStripMenuItem1.Enabled = false;
            }
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                treeView1.SelectedNode = e.Node;
                if (treeView1.SelectedNode.Bounds.Contains(e.Location) == true)
                {                    
                    leftStrip.Show(Cursor.Position);
                }
                
            }
            else
            {
                if (backgroundWorker1.IsBusy)
                {
                    try
                    {
                        backgroundWorker1.CancelAsync();
                    }
                    catch (System.InvalidOperationException ex)
                    {
                        //Do nothing
                    }
                }
                else
                {
                    currHier = "";
                    if (e.Node.Name != "Root")
                    {
                        currentPath = e.Node.Name;
                        currHier = e.Node.Name + @"\";
                    }


                    listView1.Clear();


                    string fullpath = e.Node.FullPath + "\\";
                    string rootname = treeView1.Nodes[0].Text;
                    string filepath = fullpath.Replace(rootname + "\\", "");
                    string filesystempath = gameDir + "\\" + filepath;
                    if (Directory.Exists(filesystempath))
                    {


                        if (Directory.EnumerateFileSystemEntries(filesystempath, "*", SearchOption.AllDirectories).Count() > 0)
                        {

                            ContentDirectory();

                            DirectoryInfo root = new DirectoryInfo(filesystempath);
                            fileEntries = root.GetFiles();
                            folderEntries = root.GetDirectories();
                            backgroundWorker1.RunWorkerAsync();
                        }
                        else
                        {
                            EmptyDirectory();
                        }
                    }
                }
            }
        }

        private void backToolStripButton_Click(object sender, EventArgs e)
        {
            if (currHier.Length > 0)
            {
                string path = currHier.Remove(currHier.Length - 1);
                path = path.Substring(0, path.LastIndexOf(@"\") + 1);


                currHier = path;

                filesystempath = gameDir + @"\" + currHier;
                if (Directory.Exists(filesystempath))
                {


                    if (Directory.EnumerateFileSystemEntries(filesystempath, "*", SearchOption.AllDirectories).Count() > 0)
                    {

                        ContentDirectory();

                        DirectoryInfo root = new DirectoryInfo(filesystempath);
                        fileEntries = root.GetFiles();
                        folderEntries = root.GetDirectories();
                        backgroundWorker1.RunWorkerAsync();
                    }
                    else
                    {
                        EmptyDirectory();
                    }
                    //Disable some UI things
                    openToolStripMenuItem1.Enabled = false;
                    showInWindowsExplorerToolStripMenuItem2.Enabled = false;
                    copyPathToolStripMenuItem2.Enabled = false;
                    propertiesToolStripMenuItem1.Enabled = false;
                    isWorking = false;
                }

            }
        }

        private void showInWindowsExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Name != "Root")
            {
                Process.Start(gameDir + @"\" + treeView1.SelectedNode.Name.ToString());
            }
            else
            {
                Process.Start(gameDir);
            }
        }

        private void copyPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Name != "Root")
            {
                Clipboard.SetText(gameDir + @"\" + treeView1.SelectedNode.Name.ToString());
            }
            else
            {
                Clipboard.SetText(gameDir);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFolderVoid();
        }

        private void showInWindowsExplorerToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listView1.FocusedItem.ImageKey == "????" || listView1.FocusedItem.ImageKey == "???")
            {
                Process.Start(gameDir + @"\" + currHier + listView1.SelectedItems[0].Text);
            }
        }

        private void copyPathToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (isWorking != true)
            {
                Clipboard.SetText(gameDir + @"\" + currHier + listView1.SelectedItems[0].Text);
            }
        }

        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PropertiesLoader.ShowFileProperties(gameDir + @"\" + currHier + listView1.SelectedItems[0].Text);
        }

        private void openToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            openFolderVoid();
        }

        private void showInWindowsExplorerToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (listView1.FocusedItem.ImageKey == "????" || listView1.FocusedItem.ImageKey == "???")
            {
                Process.Start(gameDir + @"\" + currHier + listView1.SelectedItems[0].Text);
            }
        }

        private void copyPathToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(gameDir + @"\" + currHier + listView1.SelectedItems[0].Text);
        }

        private void propertiesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            PropertiesLoader.ShowFileProperties(gameDir + @"\" + currHier + listView1.SelectedItems[0].Text);
        }

        private void beendenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void inhaltToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/reditec/RDR-Explorer");
        }

        private void reportABugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://gtaforums.com/topic/828211-wip-open-source-red-dead-redemption-explorer/");
        }

        private void infoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About myAboutForm = new About();
            myAboutForm.ShowDialog();
        }

        private void suchenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/reditec/RDR-Explorer/releases");
        }

    }
}
