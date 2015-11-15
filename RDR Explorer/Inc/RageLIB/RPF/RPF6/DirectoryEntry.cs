using System.IO;
using RPFLib.Common;
using System;

namespace RPFLib.RPF6
{
    internal class DirectoryEntry : TOCEntry
    {
        public DirectoryEntry(TOC toc)
        {
            TOC = toc;
        }

        public int Flags { get; set; }
        public int UNKNOWN { get; set; }
        public int ContentEntryIndex { get; set; }
        public int ContentEntryCount { get; set; }
        public override int newEntryIndex { get; set; }
        public override Int64 _Offset { get { return 0; } }

        public override bool IsDirectory
        {
            get { return true; }
        }

        public void setContentcount(int ContentCount)
        {
            ContentEntryCount = ContentCount; ;
        }

        public void setContentIndex(int newcontentindex)
        {
            ContentEntryIndex = newcontentindex;
        }

        public void setNewContentIndex(int neEntrywcontentindex)
        {
            newEntryIndex = neEntrywcontentindex;
        }

        public override void Read(BigEndianBinaryReader br, int extra = 0)
        {
            NameOffset = br.ReadInt32();
            Flags = br.ReadInt32();
            ContentEntryIndex = (int)(br.ReadUInt32() & 0x7fffffff);
            ContentEntryCount = br.ReadInt32() & 0x0fffffff;
            UNKNOWN = br.ReadInt32();
        }

        public override void Write(BigEndianBinaryWriter bw)
        {
            bw.Write(NameOffset);
            bw.Write(Flags);
            bw.Write((int)((uint)ContentEntryIndex | 0x80000000));
            /*
            if (newEntryIndex > 0)
                bw.Write((int)((uint)newEntryIndex | 0x80000000));
            else
                bw.Write((int)((uint)ContentEntryIndex | 0x80000000));      
             */
            bw.Write(ContentEntryCount);
            bw.Write(UNKNOWN);
        }
    }
}