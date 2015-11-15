using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPFLib.Common;

namespace RPFLib
{
    internal abstract class Archive
    {
        public abstract void Close();
        public abstract void Save();
        public abstract RPFLib.Common.Directory RootDirectory { get; set; }
        public abstract List<fileSystemObject> search(RPFLib.Common.Directory dir, string searchText);
        public abstract void Open(string filename);
    }
}
