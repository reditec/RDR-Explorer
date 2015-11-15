using System;
using System.Collections.Generic;
using System.IO;
using RPFLib.RPF6;
using RPFLib.Common;
using System.Reflection;
using System.Linq;
using System.Windows.Forms;
using System.Text;

namespace RPFLib
{
    class Version6 : Archive
    {
        #region Vars
        public override RPFLib.Common.Directory RootDirectory { get; set; }
        public Dictionary<uint, string> _knownFilenames = new Dictionary<uint, string>();
        private RPFLib.RPF6.File _rpfFile;
        private int entryIndex = 0;
        string streamfilename;

        #endregion

        public Version6()
        {
            string s = "KnownFilenames.txt";

            if (s != null)
            {
                var sw = new StreamReader(s);
                string name;
                while ((name = sw.ReadLine()) != null)
                {
                    uint hash = Hasher.Hash(name);
                    if (!_knownFilenames.ContainsKey(hash))
                    {
                        _knownFilenames.Add(hash, name);
                    }
                }
            }
        }

        public override void Open(string filename)
        {
            streamfilename = filename;
            _rpfFile = new RPFLib.RPF6.File();
            int filecount = _rpfFile.Open(filename);
            if (filecount < 1)
            {
                throw new Exception("Could not open RPF file.");
            }
            BuildFS();
        }

        private string GetName(TOCEntry entry)
        {
            string name;
            if (_rpfFile.Header.Identifier < HeaderIDs.Version3 || _rpfFile.Header.Identifier == HeaderIDs.Version4)
            {
                name = _rpfFile.TOC.GetName(Convert.ToInt32(entry.NameOffset));
            }
            else
            {
                if (entry == _rpfFile.TOC[0])
                {
                    name = "Root";
                }
                else if (_knownFilenames.ContainsKey((uint)entry.NameOffset))
                {
                    name = _knownFilenames[(uint)entry.NameOffset];
                }
                else
                {
                    name = string.Format("0x{0:x}", entry.NameOffset);
                }
            }
            return name;
        }

        public override List<fileSystemObject> search(RPFLib.Common.Directory dir, string searchText)
        {
            List<fileSystemObject> searchList = new List<fileSystemObject>();
            subsearch(dir, searchList, searchText);
            return searchList;
        }

        public void subsearch(RPFLib.Common.Directory dir, List<fileSystemObject> searchList, string searchText)
        {
            foreach (fileSystemObject item in dir)
            {
                if (item.IsDirectory)
                {
                    var subdir = item as RPFLib.Common.Directory;
                    searchList.AddRange((from pv in subdir._fsObjectsByName
                                         where pv.Key.Contains(searchText)
                                         select pv.Value));
                    subsearch(subdir, searchList, searchText);
                }
            }
        }

        private byte[] LoadData(FileEntry entry, bool getCustom)
        {
            byte[] data;
            if (getCustom && entry.CustomData != null)
                data = entry.CustomData;
            else
                data = _rpfFile.ReadData(entry.Offset, entry.SizeInArchive);
            if (entry.IsCompressed && !entry.IsResourceFile)
            {
                data = DataUtil.DecompressDeflate(data, (int)entry.DecompressedSize);
            }
            return data;
        }

        private void StoreData(FileEntry entry, byte[] data)
        {
            entry.SetCustomData(data);
        }

        private void Add(RPFLib.Common.Directory fsDirectory, byte[] data)
        {
            FileEntry newFileEntry = new FileEntry(_rpfFile.TOC);
            _rpfFile.TOC.Add(newFileEntry as TOCEntry);

            var file = new Common.File();
            file._dataLoad = getCustom => LoadData(newFileEntry, getCustom);
            file._dataStore = setdata => StoreData(newFileEntry, setdata);
            file._dataCustom = () => newFileEntry.CustomData != null;
            file.d1 = () => newFileEntry.getSize();
            file._Index = nIndex => newFileEntry.setIndex(nIndex);
            file._delete = () => newFileEntry.Delete();

            file.CompressedSize = newFileEntry.SizeInArchive;
            file.IsCompressed = newFileEntry.IsCompressed;
            file.Name = GetName(newFileEntry);
            file.IsResource = newFileEntry.IsResourceFile;
            file.ParentDirectory = fsDirectory;
            fsDirectory.AddObject(file);

            newFileEntry.SetCustomData(data);
        }

        public override void Close()
        {
            _rpfFile.Close();
        }

        private void BuildFSDirectory(DirectoryEntry dirEntry, RPFLib.Common.Directory fsDirectory)
        {
            try
            {
                fsDirectory.Name = GetName(dirEntry);
                for (int i = 0; i < dirEntry.ContentEntryCount; i++)
                {
                    TOCEntry entry = _rpfFile.TOC[dirEntry.ContentEntryIndex + i];
                    if (entry.IsDirectory)
                    {
                        var subdirEntry = entry as DirectoryEntry;
                        var dir = new RPFLib.Common.Directory();
                        dir._Contentcount = nCount => subdirEntry.setContentcount(nCount);
                        dir._ContentIndex = ContentIndex => subdirEntry.setContentIndex(ContentIndex);
                        dir._Index = NewContentIndex => subdirEntry.setNewContentIndex(NewContentIndex);
                        dir.nameHash = (uint)subdirEntry.NameOffset;
                        dir.ParentDirectory = fsDirectory;
                        dir.Attributes = "Folder";
                        BuildFSDirectory(entry as DirectoryEntry, dir);
                        fsDirectory.AddObject(dir);
                    }
                    else
                    {
                        var fileEntry = entry as FileEntry;
                        var file = new Common.File();
                        file._dataLoad = getCustom => LoadData(fileEntry, getCustom);
                        file._dataStore = data => StoreData(fileEntry, data);
                        file._dataCustom = () => fileEntry.CustomData != null;
                        file.d1 = () => fileEntry.getSize();
                        file._Index = nIndex => fileEntry.setIndex(nIndex);
                        file._delete = () => fileEntry.Delete();

                        file.CompressedSize = fileEntry.SizeInArchive;
                        file.IsCompressed = fileEntry.IsCompressed;
                        file.nameHash = (uint)fileEntry.NameOffset;
                        file.Name = GetName(fileEntry);
                        file.IsResource = fileEntry.IsResourceFile;
                        file.resourcetype = (int)fileEntry.ResourceType;//Convert.ToString((ResourceType)fileEntry.ResourceType);
                        file.ParentDirectory = fsDirectory;

                        StringBuilder attributes = new StringBuilder();
                        if (file.IsResource)
                        {
                            attributes.Append(string.Format("Resource [Version {0}", fileEntry.ResourceType));
                            if (file.IsCompressed)
                            {
                                attributes.Append(", Compressed");
                            }
                            attributes.Append("]");
                        }
                        else if (file.IsCompressed)
                        {
                            attributes.Append("Compressed");
                        }
                        else
                            attributes.Append("None");
                        file.Attributes = attributes.ToString();
                        fsDirectory.AddObject(file);
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        private bool ReIndex(RPFLib.Common.Directory dir)
        {
            try
            {
                dir.Reorder();
                foreach (var fsobject in dir)
                {
                    entryIndex++;
                    if (fsobject is RPFLib.Common.File)
                    {
                        var file = fsobject as RPFLib.Common.File;
                        file.SetIndex(entryIndex);
                    }
                    else if (fsobject is RPFLib.Common.Directory)
                    {
                        var directory = fsobject as RPFLib.Common.Directory;
                        directory.SetNewContentIndex(entryIndex);
                    }
                }
                if (dir._Contentcount != null)
                    dir.SetContentCount();
                foreach (var fsobject in dir)
                {
                    if (fsobject is RPFLib.Common.Directory)
                    {
                        RPFLib.Common.Directory dirEntry = fsobject as RPFLib.Common.Directory;
                        dirEntry.SetContentIndex(entryIndex + 1);
                        if (!ReIndex(dirEntry))
                            return false;                    
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                //throw new Exception(ex.Message, ex);
                return false;
            }
        }

        public override void Save()
        {
            using (var sfrm = new SaveFileDialog())
            {
                sfrm.FileName = Path.GetFileName(streamfilename);
                if (sfrm.ShowDialog() == DialogResult.OK)
                {
                    if (sfrm.FileName == streamfilename)
                    {
                        MessageBox.Show("Cannot overwrite the open archive, please choose a different filename.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Save();
                        return;
                    }
                    try
                    {
                        if (System.IO.File.Exists(sfrm.FileName))
                        {
                            System.IO.File.Delete(sfrm.FileName);
                        }
                        using (System.IO.File.Create(sfrm.FileName));
                    }
                    catch
                    {
                        MessageBox.Show("Could not create a new archive, make sure you have permissions to write to the directory.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    /*
                    entryIndex = 0;
                    if (!ReIndex(RootDirectory))
                    {
                        MessageBox.Show("Failed to re-index the archive.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    
                    if (entryIndex + 1 != (_rpfFile.TOC.count))
                    {
                        if (MessageBox.Show("The number of entries is equal to the number of TOC entries, this may cause archive corruption.", "Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == DialogResult.Cancel)
                            return;                      
                    }
                    */
                    FileStream newRPFStream = new FileStream(sfrm.FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                    RDR_Explorer.Saving saveDialog = new RDR_Explorer.Saving(newRPFStream, _rpfFile);
                    saveDialog.ShowDialog();
                    newRPFStream.Close();
                }
            }
        }

        private void BuildFS()
        {
            RootDirectory = new RPFLib.Common.Directory();
            TOCEntry entry = _rpfFile.TOC[0];
            BuildFSDirectory(entry as DirectoryEntry, RootDirectory);
        }

        private enum ResourceType
        {
            XCS_XFD_XST = 0x01,
            XTL = 0x08,
            Texture = 0x0A,
            Texture_ = 0x24,
            XEDT = 0x0B,
            XSG_XGD = 0x12,
            UNKNOWN = 0x1A,
            XAT = 0x1E,
            BOUNDS = 0x1F,
            XNM_XSF = 0x21,
            XVD = 0x85,
            XFT = 0x8A,
        }
    }
}