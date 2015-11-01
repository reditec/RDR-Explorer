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

namespace RDR_Explorer
{
    public partial class Main : Form
    {
        public Main()
        {

            InitializeComponent();
            
        }

        public string gameEXE = "";

        private void Main_Shown(object sender, EventArgs e)
        {
            var settingsIni = new IniFile("Settings.ini");
            if (!(settingsIni.KeyExists("FirstLaunch")))
            {
                MessageBox.Show("Thank you for installing RDR Explorer." + Environment.NewLine + "RDR Explorer is still a WIP (work in progress) tool." + Environment.NewLine + "Please report any bugs to the official GTAForums thread" + Environment.NewLine + "(Help -> Report a bug).", "Welcome", MessageBoxButtons.OK, MessageBoxIcon.Information);
                settingsIni.Write("FirstLaunch", "true");
            }
            if (!(settingsIni.KeyExists("GamePath")))
            {
                do
                {


                    openFolder.Description = "Please select the directory containing default.xex.";
                   
                    DialogResult dr = openFolder.ShowDialog();
                    if(dr == DialogResult.OK)
                    {
                        if (Directory.GetFiles(openFolder.SelectedPath.ToString(), "default.xex", SearchOption.TopDirectoryOnly).Length > 0)
                        {
                            gameEXE = openFolder.SelectedPath + "\\default.xex";

                            RageLib.Common.KeyStore.gameEXE = gameEXE;
                            settingsIni.Write("GamePath", openFolder.SelectedPath);
                        }
                        else
                        {
                            MessageBox.Show("The file default.xex was not found!" + Environment.NewLine + "Please specify a new directory containing default.xex");
                        }
                    } 
                    else
                    {
                        openFolder.SelectedPath = "";
                        break;
                    }
                } while (!(settingsIni.KeyExists("GamePath")));

                if(openFolder.SelectedPath == "")
                {
                    Application.Exit();
                }
               
                
                

                //settingsIni.Write("FirstLaunch", "true");
            }
            RageLib.Common.KeyUtil.MYgameExe = gameEXE;
            LoadGameDirectory(new KeyUtilRDR());
        }

        private void LoadGameDirectory(KeyUtil keyUtil)
        {
            byte[] key = keyUtil.FindKey(gameEXE, "RDR"); //Key is not public yet and must be verified --> I will do this tomorrow.
        }

        //private void bgwListBuilder_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    try
        //    {
        //        using (Cursors.WaitCursor)
        //        {
        //            using (BinaryReader s = new BinaryReader(new FileStream(currentFileName, FileMode.Open, FileAccess.Read)))
        //            {
        //                char[] Magic = new char[4];
        //                s.Read(Magic, 0, 4);
        //                string magicStr = new string(Magic);
        //                switch (magicStr)
        //                {
        //                    case "RPF6":
        //                        {
        //                            archiveFile = new Version6();
        //                            break;
        //                        }
        //                    case "RPF3":
        //                        {
        //                            archiveFile = new Version3();
        //                            break;
        //                        }
        //                    case "RPF4":
        //                        {
        //                            archiveFile = new Version4();
        //                            break;
        //                        }
        //                    case "RPF7":
        //                        {
        //                            archiveFile = new Version7();
        //                            break;
        //                        }
        //                    default:
        //                        MessageBox.Show("Invalid archive selected", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //                        return;
        //                }
        //            }
        //            archiveFile.Open(currentFileName);
        //            buildlist(archiveFile.RootDirectory);
        //            startBreadCrumb(archiveFile.RootDirectory);
        //        }
        //        this.Invoke((MethodInvoker)delegate
        //        {
        //            Text = Application.ProductName + " - " + new FileInfo(currentFileName).Name;
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        this.Invoke((MethodInvoker)delegate
        //        {
        //            filelistview.ClearObjects();
        //            mainStatusbar.ItemLinks.Clear();
        //            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        });
        //    }
        //}
    }
}
