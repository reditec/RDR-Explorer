using System.Collections.Generic;
using System;
namespace RPFLib.Common
{
    public class File : fileSystemObject
    {
        #region Delegates

        public delegate byte[] DataLoadDelegate(bool getCustom);
        public delegate void DataStoreDelegate(byte[] data);
        public delegate bool DataIsCustomDelegate();
        public delegate int getSize();
        public delegate void setIndex(int nIndex);
        public delegate void Delete();

        #endregion

        public DataLoadDelegate _dataLoad;
        public DataStoreDelegate _dataStore;
        public DataIsCustomDelegate _dataCustom;
        public getSize d1;
        public setIndex _Index;
        public Delete _delete;
        private string _Attributes = "";

        public File()
        {
        }

        public File(setIndex newIndex)
        {
            _Index = newIndex;
        }

        public override bool ContainsSubfolder
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string[] ReturnSubfolderNames
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool IsDirectory
        {
            get { return false; }
        }

        public override bool IsReturnDirectory
        {
            get { return false; }
        }

        public string DF
        {
            get { return "File"; }
        }

        public override uint nameHash { get; set; }
        public bool IsCompressed { get; set; }
        public int CompressedSize { get; set; }

        public override string Size { get { return d1().ToString(); } set {} }

        public override string SizeS
        {
            get
            {
                if (Convert.ToInt32(Size)  < 1024)
                {
                    return Size + " B";
                }
                else if (Convert.ToInt32(Size) < 1024 * 1024)
                {
                    return Convert.ToInt32(Size) / (1024) + " KB";
                }
                else
                {
                    return Convert.ToInt32(Size) / (1024 * 1024) + " MB";
                }
            }
            set
            {

            }
        }

        public bool IsResource { get; set; }
        private int _resourcetype;
        public int resourcetype
        {
            get
            {
                if (_resourcetype != null)
                    return _resourcetype;
                else
                    return 0;
            }
            set { _resourcetype = value; }
        }

        public override string Attributes
        {
            get { return _Attributes;} set { _Attributes = value; }
        }

        public bool IsCustomData
        {
            get { return _dataCustom(); }
        }

        public byte[] GetData(bool getcustom)
        {
            return _dataLoad(getcustom);
        }

        public void SetData(byte[] data)
        {
            _dataStore(data);
        }

        public void SetIndex(int newIndex)
        {
            _Index(newIndex);
        }

        public void DeleteEntry()
        {
            _delete();
        }
    }
}