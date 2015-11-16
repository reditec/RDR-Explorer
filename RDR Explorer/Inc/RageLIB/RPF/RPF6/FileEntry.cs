using System;
using System.Diagnostics;
using System.Windows.Forms;
//using RageLib.Common.Resources;
using RPFLib.Common;

namespace RPFLib.RPF6
{
    internal class FileEntry : TOCEntry
    {
        public FileEntry(TOC toc)
        {
            TOC = toc;
        }

        public override int newEntryIndex { get; set; }
        public int DecompressedSize;
        public Int64 Offset;
        public override Int64 _Offset { get { return Offset; } }
        public int SizeInArchive;
        public int Flags1;
        public int Flags2;
        public bool IsCompressed;
        public bool IsResourceFile;
        public byte ResourceType;
        public int customsize;

        //RSC flags2
        public int dwExtVSize; //: 14;
        public int dwExtPSize; //: 14;
        public int _f14_30;    //: 3; 
        public bool bUseExtSize; //: 1;

        public byte[] CustomData { get; private set; }

        public int getSize()
        {
            return (int)DecompressedSize;
        }

        public void setIndex(int index)
        {
            newEntryIndex = index; ;
        }

        public void setContentIndex(int newcontentindex)
        {
        }

        public TOCEntry getEntry()
        {
            return this as TOCEntry;
        }

        public void SetCustomData(byte[] data)
        {
            try
            {
                if (data == null)
                {
                    CustomData = null;
                }
                else
                {
                    customsize = data.Length;
                    if (IsCompressed && !IsResourceFile)
                    {
                        data = DataUtil.Compress(data, ICSharpCode.SharpZipLib.Zip.Compression.Deflater.BEST_COMPRESSION);
                    }
             
                    CustomData = data;
                   // System.IO.File.WriteAllBytes("C:\\Users\\Dageron\\Desktop\\1.raw", CustomData);
                    SizeInArchive = CustomData.Length;
                    DecompressedSize = customsize;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public override bool IsDirectory
        {
            get { return false; }
        }

        public long swap32(int val)
        {
            return ((val << 24) & 0xFF000000) + ((val << 8) & 0x00FF0000) +
((val >> 8) & 0x0000FF00) + ((val >> 24) & 0x000000FF);
        }

        public override void Read(BigEndianBinaryReader br, int extra = 0)
        {
            try
            {
                NameOffset = br.ReadInt32();
                SizeInArchive = br.ReadInt32();
                Offset = br.ReadInt32();           
                Flags1 = br.ReadInt32();
                Flags2 = br.ReadInt32();
                //int test = (int)(Flags1 & 0x80000000);
                //string bits1 = GetIntBinaryString(Flags1);
                IsResourceFile = (Flags1 & 0x80000000) == 0x80000000;
                IsCompressed = (Flags1 & 0x40000000) == 0x40000000;
                DecompressedSize = (int)(Flags1 & 0xbfffffff);

                if (IsResourceFile)
                {
                    //Flags2 = (int)swap32(Flags2);
                    ResourceType = (byte)(Offset & 0xFF);
                    Offset = (Offset & 0x7fffff00) * 8;
                    dwExtVSize = (int)(Flags2 & 0x7FFF);
                    dwExtPSize = (int)((Flags2 & 0xFFF7000) >> 14);
                    _f14_30 = (int)(Flags2 & 0x70000000); 
                    bUseExtSize = (Flags2 & 0x80000000) == 0x80000000 ? true : false;
                    DecompressedSize = (int)(getSizeV() + getSizeP()); 
                    if (Offset % 2048 != 0)
                    {
                        Debug.Print("Invalid Resource Offset");
                    }
                }
                else
                    Offset = Offset * 8;

                /////////////////////////
                // Couple of debug checks
                /////////////////////////
                /*
                #if DEBUG
                {             
                    if (IsResourceFile)
                    {
                        if (Offset % 2048 != 0)
                        {
                            Debug.Print("Invalid Resource Offset");
                        }
                        string bits = GetIntBinaryString(Flags2);
                        int SizeV = getSizeV();
                        int SizeP = getSizeP();
                        int objectStart = getObjectStart();
                    }
                    else
                    {
                        if (Offset % 8 != 0)
                        {
                            Debug.Print("Invalid File Offset");
                        }
                    }
                }
                #endif
                 */

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static string GetIntBinaryString(int n)
        {
            char[] b = new char[32];
            int pos = 31;
            int i = 0;

            while (i < 32)
            {
                if ((n & (1 << i)) != 0)
                {
                    b[pos] = '1';
                }
                else
                {
                    b[pos] = '0';
                }
                pos--;
                i++;
            }
            return new string(b);
        }

        private int getSizeV () 
	    {
		    //if (!IsResourceFile) return DecompressedSize;		 
            return bUseExtSize ? (dwExtVSize << 12) : ((int)(DecompressedSize & 0x7FF) << ((int)((DecompressedSize >> 11) & 15) + 8));
	    }

	    private int getSizeP () 
	    {
		    //if (!IsResourceFile) return 0;				 
            return bUseExtSize ? (dwExtPSize << 12) : ((int)((DecompressedSize >> 15) & 0x7FF) << ((int)((DecompressedSize >> 26) & 15) + 8));
	    }

	    private int getObjectStart () 
	    { 
		    // get offset of the main object in the resource
		    return (bUseExtSize && _f14_30 < 4) ? ((dwExtVSize >> (_f14_30+1)) << (_f14_30+13)) : 0;
	    }

        public override void Write(BigEndianBinaryWriter bw)
        {
            bw.Write(NameOffset);
            bw.Write(SizeInArchive);
            if (IsResourceFile)
                bw.Write((int)((Offset / 8) | (byte)ResourceType));
            else
                bw.Write((int)(Offset / 8));
            var temp = DecompressedSize;
            if (IsCompressed)
            {
                temp |= 0x40000000;
            }
            if (IsResourceFile)
            {
                temp = (int)(temp | 0x80000000);
                temp = Flags1;
            }
            bw.Write(temp);
            bw.Write(Flags2);
        }
    }
}