using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RDR_Explorer.Inc.Compression.LZX
{
    public enum CompressionType : short
    {
        Stored = 0,
        Shrunk,
        Reduce1,
        Reduce2,
        Reduce3,
        Reduce4,
        Implode,
        Token,
        Deflate,
        Deflate64,
        LZX = 21
    }

    public class LZX : IDisposable
    {
        private FileStream File;

        public ZipDirEntry this[string name]
        {
            get
            {
                foreach (ZipDirEntry entry in entries)
                {
                    if (entry.FileName.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return entry;
                    }
                }
                return null;
            }
        }

        List<ZipDirEntry> entries;
        public List<ZipDirEntry> Entries
        {
            get { return entries; }
            set { entries = value; }
        }

        EndianStream io;
        public EndianStream Io
        {
            get { return io; }
            set { io = value; }
        }

        public LZX(string file)
        {
            File = new FileStream(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            io = new EndianStream(File, false);

            // Read it to make sure its a PkZip file
            io.Seek((int)io.Length - 0x16, SeekOrigin.Begin);
            if (io.ReadInt32() != 0x06054B50)
                throw new Exception("This is not a PkZip File!");

            // Lets read the structure
            io.Seek((int)io.Length - 0x16, SeekOrigin.Begin);
            ZipEndLocator el = new ZipEndLocator(io);

            // Now that we have this info lets read our directory entries
            entries = new List<ZipDirEntry>();

            io.Seek(el.DirectoryOffset, SeekOrigin.Begin);
            for (int i = 0; i < el.EntriesInDirectory; i++)
            {
                entries.Add(new ZipDirEntry(io));
            }
        }

        ~LZX()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (File != null) File.Dispose();
        }
    }

    public class ZipFileRecord
    {
        public enum XMemCodecType
        {
            Default = 0,
            LZX = 1
        }

        public struct XMemCodecParametersLZX
        {
            public int Flags;
            public int WindowSize;
            public int CompressionPartitionSize;
        }

        [DllImport("xcompress.dll", EntryPoint = "XMemCreateDecompressionContext")]
        public static extern int XMemCreateDecompressionContext(
            XMemCodecType codecType,
            int pCodecParams,
            int flags, ref int pContext);

        [DllImport("xcompress.dll", EntryPoint = "XMemDestroyDecompressionContext")]
        public static extern void XMemDestroyDecompressionContext(int context);

        [DllImport("xcompress.dll", EntryPoint = "XMemResetDecompressionContext")]
        public static extern int XMemResetDecompressionContext(int context);

        [DllImport("xcompress.dll", EntryPoint = "XMemDecompressStream")]
        public static extern int XMemDecompressStream(int context,
            byte[] pDestination, ref int pDestSize,
            byte[] pSource, ref int pSrcSize);

        [DllImport("xcompress.dll", EntryPoint = "XMemCreateCompressionContext")]
        public static extern int XMemCreateCompressionContext(
            XMemCodecType codecType, int pCodecParams,
            int flags, ref int pContext);

        [DllImport("xcompress.dll", EntryPoint = "XMemDestroyCompressionContext")]
        public static extern void XMemDestroyCompressionContext(int context);

        [DllImport("xcompress.dll", EntryPoint = "XMemResetCompressionContext")]
        public static extern int XMemResetCompressionContext(int context);

        [DllImport("xcompress.dll", EntryPoint = "XMemCompressStream")]
        public static extern int XMemCompressStream(int context,
            byte[] pDestination, ref int pDestSize,
            byte[] pSource, ref int pSrcSize);



        #region Fields
        private EndianStream stream;

        internal int CompressedSize;
        CompressionType compression;
        internal uint Crc;
        string extraField;
        short extraFieldLength;
        short fileDate;
        string fileName;
        short fileNameLength;
        short fileTime;
        short flags;
        int signature;
        internal int UncompressedSize;
        short version;

        byte[] data;
        #endregion

        #region Constructor
        public ZipFileRecord(EndianStream er)
        {
            stream = er;

            signature = er.ReadInt32();
            version = er.ReadInt16();
            flags = er.ReadInt16();
            compression = (CompressionType)er.ReadInt16();
            fileTime = er.ReadInt16();
            fileDate = er.ReadInt16();
            Crc = er.ReadUInt32();
            CompressedSize = er.ReadInt32();
            UncompressedSize = er.ReadInt32();
            fileNameLength = er.ReadInt16();
            extraFieldLength = er.ReadInt16();
            fileName = er.ReadASCII(fileNameLength);
            extraField = er.ReadASCII(extraFieldLength);

            data = er.ReadBytes(CompressedSize);
        }

        #endregion

        #region Methods

        public byte[] DecompressData()
        {
            // Decompress our data
            byte[] buffer;
            switch (compression)
            {
                case CompressionType.Stored:
                    {
                        return data;
                    }
                case CompressionType.Deflate:
                    {
                        MemoryStream ms = new MemoryStream(data);
                        DeflateStream ds = new DeflateStream(ms, CompressionMode.Decompress);

                        buffer = new byte[UncompressedSize];
                        if (ds.Read(buffer, 0, UncompressedSize) != UncompressedSize)
                            throw new Exception("Decompresson Error: Bad Decompress Length");

                        break;
                    }
                case CompressionType.LZX:
                    {
                        //Create our decompression context
                        int decompressionContext = 0;
                        XMemCreateDecompressionContext(
                            XMemCodecType.LZX,
                            0, 0, ref decompressionContext);

                        //Reset our context first
                        XMemResetDecompressionContext(decompressionContext);

                        //Now lets read and decompress
                        buffer = new byte[UncompressedSize];
                        XMemDecompressStream(decompressionContext,
                            buffer, ref UncompressedSize,
                            data, ref CompressedSize);

                        //Go ahead and destory our context
                        XMemDestroyDecompressionContext(decompressionContext);

                        break;
                    }
                default:
                    {
                        throw new Exception("Compression type " +
                            compression + " not supported");
                    }
            }

            // Check our CRC32 now
            if (ComputeCRC32(buffer) != Crc)
                throw new Exception("Decompresson Error: Bad CRC");

            // Return our data since we are good
            return buffer;
        }
        public static uint ComputeCRC32(byte[] bytes)
        {
            uint[] table = new uint[256];
            for (uint i = 0; i < table.Length; ++i)
            {
                uint temp = i;
                for (int j = 8; j > 0; --j)
                {
                    if ((temp & 1) == 1)
                        temp = ((temp >> 1) ^ 0xedb88320);
                    else
                        temp >>= 1;
                }
                table[i] = temp;
            }

            uint crc = 0xffffffff;
            for (int i = 0; i < bytes.Length; ++i)
            {
                byte index = (byte)(((crc) & 0xff) ^ bytes[i]);
                crc = ((crc >> 8) ^ table[index]);
            }
            return ~crc;
        }
        #endregion
    }
    public class ZipDirEntry
    {
        #region Fields
        private EndianStream stream;


        int compressedSize;
        CompressionType compression;
        uint crc;
        short diskNumberStart;
        int externalAttributes;
        string extraField;
        short extraFieldLength;
        short fileCommentLength;
        short fileDate;
        string fileName;
        short fileNameLength;
        short fileTime;
        short flags;
        int headerOffset;
        short internalAttributes;
        int signature;
        int uncompressedSize;
        short versionMadeBy;
        short versionToExtract;

        int offset;
        #endregion

        #region Properties
        public string FileName
        {
            get { return fileName; }
            set
            {
                fileName = value;
                fileNameLength = (short)fileName.Length;
            }
        }

        public byte[] Data
        {
            get
            {
                // Move to the record
                stream.Seek(headerOffset, SeekOrigin.Begin);

                // Read the record
                ZipFileRecord record = new ZipFileRecord(stream);

                // Decompress our data
                return record.DecompressData();
            }
        }
        #endregion

        #region Constructor
        public ZipDirEntry(EndianStream es)
        {
            stream = es;

            offset = (int)es.Position;
            signature = es.ReadInt32();
            versionMadeBy = es.ReadInt16();
            versionToExtract = es.ReadInt16();
            flags = es.ReadInt16();
            compression = (CompressionType)es.ReadInt16();
            fileTime = es.ReadInt16();
            fileDate = es.ReadInt16();
            crc = es.ReadUInt32();
            compressedSize = es.ReadInt32();
            uncompressedSize = es.ReadInt32();
            fileNameLength = es.ReadInt16();
            extraFieldLength = es.ReadInt16();
            fileCommentLength = es.ReadInt16();
            diskNumberStart = es.ReadInt16();
            internalAttributes = es.ReadInt16();
            externalAttributes = es.ReadInt32();
            headerOffset = es.ReadInt32();
            fileName = es.ReadASCII(fileNameLength);
            extraField = es.ReadASCII(extraFieldLength);
        }

        #endregion

        #region Methods
        public void Extract(string newFileName)
        {
            // Move to the record
            stream.Seek(headerOffset, SeekOrigin.Begin);

            // Read the record
            ZipFileRecord record = new ZipFileRecord(stream);

            // Decompress our data
            byte[] buffer = record.DecompressData();

            // Create and write our file
            FileStream fs = new FileStream(newFileName, FileMode.Create,
                FileAccess.Write);
            fs.Write(buffer, 0, buffer.Length);
            fs.Close();
        }

        public override string ToString()
        {
            return fileName;
        }
        #endregion
    }

    public class ZipEndLocator
    {
        EndianStream stream;

        string comment;
        short commentLength;
        internal int DirectoryOffset;
        int directorySize;
        short diskNumber;
        internal short EntriesInDirectory;
        short entriesOnDisk;
        int signature;
        short startDiskNumber;

        public ZipEndLocator(EndianStream er)
        {
            stream = er;
            signature = er.ReadInt32();
            diskNumber = er.ReadInt16();
            startDiskNumber = er.ReadInt16();
            entriesOnDisk = er.ReadInt16();
            EntriesInDirectory = er.ReadInt16();
            directorySize = er.ReadInt32();
            DirectoryOffset = er.ReadInt32();
            commentLength = er.ReadInt16();
            comment = er.ReadASCII(commentLength);
        }

        public void Write(EndianStream ew)
        {
            ew.Write(signature);
            ew.Write(diskNumber);
            ew.Write(startDiskNumber);
            ew.Write(entriesOnDisk);
            ew.Write(EntriesInDirectory);
            ew.Write(directorySize);
            ew.Write(DirectoryOffset);
            ew.Write(commentLength);
            ew.Write(comment);
        }
    }
}
