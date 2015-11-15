using System;
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
            File.Delete("base.bin");
            //Disable some UI things
            InitializeComponent();
            openToolStripMenuItem1.Enabled = false;
            showInWindowsExplorerToolStripMenuItem2.Enabled = false;
            copyPathToolStripMenuItem2.Enabled = false;
            propertiesToolStripMenuItem1.Enabled = false;

        }

        public string gameEXE = "";
        IniFile settingsIni = new IniFile("Settings.ini");
        byte[] key = new Byte[0];
        string gameDir = "";
        string currHier = "";
        bool isWorking = false; //Is the Background Worker currently working? No, it isn't! Aaah whatever...
        string filesystempath = "";
        string currentPath = "";
        FileInfo[] fileEntries = null; //array for displaying files
        DirectoryInfo[] folderEntries = null; ////array for displaying fodlers
        int version = 1000001; //version = 1.00 build = 0001

        private void CheckExeFile()
        {
            
            do
            {


                openFolder.Description = "Please select the directory containing default.xex.";

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
                            byte[] key = keyUtil.FindKey(gameEXE, "RDR");
                            RPFLib.Common.DataUtil.setKey(key);
                            if (!(key == null))
                            {
                                settingsIni.Write("GamePath", openFolder.SelectedPath);
                            }
                            else
                            {
                                MessageBox.Show("The file default.xex was found, but it seems that this version of the file is not supported or your file is broken. Please try another one.", "Executable file not supported", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show("The file default.xex was not found!" + Environment.NewLine + "Please specify a new directory containing default.xex", "Executable file not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("The specified folder was not found!" + Environment.NewLine + "Please specify a new directory containing default.xex", "Folder not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            statusProgress.ProgressBar.Value = 0;
            if (!(settingsIni.KeyExists("FirstLaunch")))
            {
                MessageBox.Show("Thank you for installing RDR Explorer." + Environment.NewLine + "RDR Explorer is still a WIP (work in progress) tool." + Environment.NewLine + "Please report any bugs to the official GTAForums thread" + Environment.NewLine + "(Help -> Report a bug).", "Welcome", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                        key = keyUtil.FindKey(gameEXE, "RDR");
                        RPFLib.Common.DataUtil.setKey(key);
                        if (!(key == null))
                        {
                            settingsIni.Write("GamePath", settingsIni.Read("GamePath"));
                        }
                        else
                        {
                            MessageBox.Show("The file default.xex was found, but it seems that this version of the file is not supported or your file is broken. Please try another one.", "Executable file not supported", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            CheckExeFile();
                        }
                    }
                    else
                    {
                        MessageBox.Show("The specified folder was not found!" + Environment.NewLine + "Please specify a new directory containing default.xex", "Folder not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                 }
                 else
                 {
                        MessageBox.Show("The file default.xex was not found!" + Environment.NewLine + "Please specify a new directory containing default.xex", "Executable file not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        CheckExeFile();
                 }
             }

            if (settingsIni.Read("GamePath") != "")
            {
                statusProgress.ProgressBar.Value = 100;
                statusLabel.Text = "Key found.";
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
                        MessageBox.Show(ex.Message);
                    }
                    treeView1.Nodes[0].Expand();
                }
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
            if (CheckForInternetConnection())
            {
                string url = @"https://raw.githubusercontent.com/reditec/RDR-Explorer/master/Update.ini";
                // Create an instance of WebClient
                WebClient client = new WebClient();
                // Hookup DownloadFileCompleted Event
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);

                // Start the download
                client.DownloadFileAsync(new Uri(url), "Update.ini");
            }
            else
            {

                MessageBox.Show("We couldn't check for updates. Please check your internet connection.");
            }
        }

        //Read result and ask for update
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            IniFile MyIni = new IniFile("Update.ini");
            if (MyIni.KeyExists("Version") && MyIni.KeyExists("File"))
            {
                string ver = MyIni.Read("Version");
                int xver = Int32.Parse(ver.Replace(".", string.Empty));
                if (xver > version)
                {
                    DialogResult dialogResult = MessageBox.Show("An update is avilable. Perform?","Update found", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        string url = MyIni.Read("File");

                        WebClient client = new WebClient();

                        client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFile2Completed);

                        client.DownloadFileAsync(new Uri(url), "Update.exe");
                    }

                }
                else
                {
                    MessageBox.Show("You already run the most current version of RDR Explorer.", "No update found");
                }

            }
            else
            {
                MessageBox.Show("Something went wrong.", "Error");
            }
        }
        //Perform update
        void client_DownloadFile2Completed(object sender, AsyncCompletedEventArgs e)
        {
            Process.Start("Update.exe");
            Application.Exit();
        }

        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (var stream = new System.Net.Sockets.TcpClient("www.google.com", 80))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

    }
}
