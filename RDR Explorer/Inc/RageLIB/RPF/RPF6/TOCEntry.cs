using System;
using System.IO;
using RPFLib.Common;

namespace RPFLib.RPF6
{
    internal abstract class TOCEntry : Entry
    {
        public int NameOffset { get; set; }
        public TOC TOC { get; set; }

        public abstract bool IsDirectory { get; }

        public abstract void Read(BigEndianBinaryReader br, int extra = 0);
        public abstract void Write(BigEndianBinaryWriter bw);
        public abstract int newEntryIndex { get; set; }
        public abstract Int64 _Offset { get; }

        public int count;
        
        public override void Delete()
        {
            TOC.Delete(this);
        }

        internal static bool ReadAsDirectory(BigEndianBinaryReader br)
        {
            bool dir;

            br.BaseStream.Seek(8, SeekOrigin.Current);
            byte tocData = br.ReadByte();
            dir = tocData == 128;

            br.BaseStream.Seek(-9, SeekOrigin.Current);

            return dir;
        }
    }
}