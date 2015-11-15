using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RPFLib.Common
{
    public class Directory : fileSystemObject, IEnumerable<fileSystemObject>
    {
        public delegate void setContentCount(int nContentcount);
        public setContentCount _Contentcount;
        public delegate void setContentIndex(int nIndex);
        public setContentIndex _ContentIndex;
        public delegate void setIndex(int nIndex);
        public setIndex _Index;

        private List<fileSystemObject> _fsObjects = new List<fileSystemObject>();
        public readonly Dictionary<string, fileSystemObject> _fsObjectsByName = new Dictionary<string, fileSystemObject>();
        public string _Attributes = "";

        public Directory()
        {
        }

        public override bool IsDirectory
        {
            get { return true; }
        }

        public override bool IsReturnDirectory
        {
            get { return false; }
        }

        public fileSystemObject this[int index]
        {
            get { return _fsObjects[index]; }
        }

        public fileSystemObject FindByName(string name)
        {
            fileSystemObject obj;
            _fsObjectsByName.TryGetValue(name.ToLower(), out obj);
            return obj;
        }

        #region IEnumerable<FSObject> Members

        public IEnumerator<fileSystemObject> GetEnumerator()
        {
            return _fsObjects.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _fsObjects.GetEnumerator();
        }

        #endregion

        private string empty;

        public override uint nameHash { get; set; }
        public override string Size { get { return ""; } set { empty = value; } }
        public override string SizeS { get { return ""; } set { empty = value; } }
        public string IsResource { get { return ""; } set { empty = value; } }
        public string resourcetype { get { return ""; } set { empty = value; } }
        public string IsCompressed { get { return ""; } set { empty = value; } }

        public string DF
        {
            get { return "Directory"; }
        }

        public override string Attributes
        {
            get { return _Attributes; }
            set { _Attributes = value; }
        }

        public void AddObject(fileSystemObject obj)
        {
            _fsObjects.Add(obj);
            _fsObjectsByName.Add(obj.Name.ToLower(), obj);
        }

        public void Reorder()
        {
            _fsObjects = _fsObjects.OrderBy(o => o.nameHash).ToList();
        }

        public void DeleteObject(fileSystemObject obj)
        {
            _fsObjectsByName.Remove(obj.Name.ToLower());
            _fsObjects.Remove(obj);
        }

        public void SetContentCount()
        {
            _Contentcount(_fsObjects.Count);
        }

        public void SetContentIndex(int newContentIndex)
        {
            _ContentIndex(newContentIndex);
        }

        public void SetNewContentIndex(int newEntryContentIndex)
        {
            _Index(newEntryContentIndex);
        }
    }
}