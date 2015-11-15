using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using RPFLib.Common;

namespace RPFLib.RPF6
{
    internal class TOC : IEnumerable<TOCEntry>
    {
        private List<TOCEntry> _entries = new List<TOCEntry>();
        private string _nameStringTable;
        public int count { get { return _entries.Count; } }
        public long fileStart;

        public TOC(File file)
        {
            File = file;
        }


        public File File { get; private set; }

        public TOCEntry this[int index]
        {
            get
            {
                try
                {
                    return _entries[index];
                }
                catch (Exception)
                {
                    return _entries[0];
                }
            }
        }

        public bool Add(TOCEntry entry)
        {
            try
            {
                _entries.Add(entry);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Delete(TOCEntry entry)
        {
            try
            {
                int Index = _entries.IndexOf(entry);
                if (Index == -1)
                    return false;
                //_entries.Remove(entry);
                _entries.RemoveAt(Index);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void ReorderEntryIndex()
        {
            try
            {
                _entries = _entries.OrderBy(o => o.newEntryIndex).ToList();
            }
            catch (System.Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public void ReorderOffset()
        {
            try
            {
                _entries = _entries.OrderBy(o => o._Offset).ToList();
            }
            catch (System.Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public void ReorderCount()
        {
            try
            {
                _entries = _entries.OrderBy(o => o.count).ToList();
            }
            catch (System.Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public string GetName(int offset)
        {
            if (offset > _nameStringTable.Length)
            {
                throw new Exception("Invalid offset for name");
            }

            int endOffset = offset;
            while (_nameStringTable[endOffset] != 0)
            {
                endOffset++;
            }
            return _nameStringTable.Substring(offset, endOffset - offset);
        }

        #region IFileAccess Members

        public static void AppendAllBytes(string path, byte[] bytes)
        {
            //argument-checking here.

            using (var stream = new FileStream(path, FileMode.Append))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        public void Read(BigEndianBinaryReader br)
        {
            fileStart = 0x7FFFFFFF;
            if (File.Header.Encrypted)
            {
                int tocSize = DataUtil.RoundUp(File.Header.TOCSize, 16);
                byte[] tocData = br.ReadBytes(tocSize);

                tocData = DataUtil.Decrypt(tocData);

                // Create a memory stream and override our active binary reader
                var ms = new MemoryStream(tocData);
                br = new BigEndianBinaryReader(ms);
           //     System.IO.File.WriteAllBytes(@"C:\Users\Dageron\Desktop\toc_test.bin", tocData);
            }

            int entryCount = File.Header.EntryCount;

            for (int i = 0; i < entryCount; i++)
            {
                TOCEntry entry;
                if (TOCEntry.ReadAsDirectory(br))
                {
                    entry = new DirectoryEntry(this);
                    entry.Read(br);
                }
                else
                {
                    entry = new FileEntry(this);
                    entry.Read(br);
                    fileStart = fileStart > entry._Offset ? entry._Offset : fileStart;
                }
                entry.count = i;
                _entries.Add(entry);
            }

            int stringDataSize = File.Header.TOCSize - File.Header.EntryCount * 20;
            byte[] stringData = br.ReadBytes(stringDataSize);
            _nameStringTable = Encoding.ASCII.GetString(stringData);
        }

        public void Write(BigEndianBinaryWriter bw)
        {
            MemoryStream ms = new MemoryStream();
            BigEndianBinaryWriter tempbw = new BigEndianBinaryWriter(ms);
            foreach (var entry in _entries)
            {
                entry.Write(tempbw);
            }

            int padding = DataUtil.RoundUp((int)tempbw.BaseStream.Length, 16) - (int)tempbw.BaseStream.Length;
            if (padding > 0)
                tempbw.Write(new byte[padding]);

            BigEndianBinaryReader tempbr = new BigEndianBinaryReader(ms);
            ms.Position = 0;
            bw.Write(DataUtil.Encrypt(tempbr.ReadBytes((int)tempbr.BaseStream.Length)));
        }

        #endregion

        #region Implementation of IEnumerable

        public IEnumerator<TOCEntry> GetEnumerator()
        {
            return _entries.GetEnumerator();
        }

        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}