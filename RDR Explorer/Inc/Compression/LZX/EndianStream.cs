using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDR_Explorer.Inc.Compression.LZX
{
    public enum EndianType
    {
        BigEndian,
        LittleEndian
    }

    public class EndianStream : Stream
    {
        #region Fields
        private Stream @base;
        private BinaryReader reader;
        private BinaryWriter writer;
        private bool swapEndian;
        #endregion

        #region Properties
        public override long Position { get { return @base.Position; } set { @base.Position = value; } }
        public override bool CanRead { get { return @base.CanRead; } }
        public override bool CanSeek { get { return @base.CanSeek; } }
        public override bool CanWrite { get { return @base.CanWrite; } }
        public override bool CanTimeout { get { return @base.CanTimeout; } }
        public bool SwapEndian { get { return swapEndian; } set { swapEndian = value; } }
        #endregion

        #region Constructor / Destructor
        public EndianStream(Stream stream, bool swapEndian)
        {
            @base = stream;
            this.swapEndian = swapEndian;
            if (stream.CanRead) reader = new BinaryReader(stream);
            if (stream.CanWrite) writer = new BinaryWriter(stream);
        }

        public EndianStream(byte[] data, bool swapEndian)
        {
            @base = new MemoryStream(data);
            this.swapEndian = swapEndian;
            if (@base.CanRead) reader = new BinaryReader(@base);
            if (@base.CanWrite) writer = new BinaryWriter(@base);
        }

        ~EndianStream()
        {
            Dispose();
        }

        public new void Dispose()
        {
            if (@base != null) @base.Dispose();
            if (reader != null) reader.Close();
            if (writer != null) writer.Close();
        }
        #endregion

        #region Methods
        public override void Flush() { @base.Flush(); }
        public override long Seek(long offset, SeekOrigin origin) { return @base.Seek(offset, origin); }
        public override long Length { get { return @base.Length; } }
        public override void SetLength(long value) { @base.SetLength(value); }
        public override int Read(byte[] buffer, int offset, int count) { return @base.Read(buffer, offset, count); }
        public override void Write(byte[] buffer, int offset, int count) { @base.Write(buffer, offset, count); }

        public bool ReadBoolean()
        {
            return reader.ReadBoolean();
        }

        public new byte ReadByte()
        {
            return reader.ReadByte();
        }

        public char ReadChar()
        {
            return reader.ReadChar();
        }

        public short ReadInt16()
        {
            if (swapEndian)
            {
                byte[] data = reader.ReadBytes(2);
                Array.Reverse(data);
                return BitConverter.ToInt16(data, 0);
            }
            else return reader.ReadInt16();
        }

        public ushort ReadUInt16()
        {
            if (swapEndian)
            {
                byte[] data = reader.ReadBytes(2);
                Array.Reverse(data);
                return BitConverter.ToUInt16(data, 0);
            }
            else return reader.ReadUInt16();
        }

        public int ReadInt32()
        {
            if (swapEndian)
            {
                byte[] data = reader.ReadBytes(4);
                Array.Reverse(data);
                return BitConverter.ToInt32(data, 0);
            }
            else return reader.ReadInt32();
        }

        public uint ReadUInt32()
        {
            if (swapEndian)
            {
                byte[] data = reader.ReadBytes(4);
                Array.Reverse(data);
                return BitConverter.ToUInt32(data, 0);
            }
            else return reader.ReadUInt32();
        }

        public long ReadInt64()
        {
            if (swapEndian)
            {
                byte[] data = reader.ReadBytes(8);
                Array.Reverse(data);
                return BitConverter.ToInt64(data, 0);
            }
            else return reader.ReadInt64();
        }

        public ulong ReadUInt64()
        {
            if (swapEndian)
            {
                byte[] data = reader.ReadBytes(8);
                Array.Reverse(data);
                return BitConverter.ToUInt64(data, 0);
            }
            else return reader.ReadUInt64();
        }

        public float ReadSingle()
        {
            if (swapEndian)
            {
                byte[] data = reader.ReadBytes(4);
                Array.Reverse(data);
                return BitConverter.ToSingle(data, 0);
            }
            else return reader.ReadSingle();
        }

        public double ReadDouble()
        {
            if (swapEndian)
            {
                byte[] data = reader.ReadBytes(8);
                Array.Reverse(data);
                return BitConverter.ToDouble(data, 0);
            }
            else return reader.ReadDouble();
        }

        public string ReadASCII(int length)
        {
            return ASCIIEncoding.ASCII.GetString(reader.ReadBytes(length));
        }
        public string ReadString()
        {
            return reader.ReadString();
        }

        public byte[] ReadBytes(int count)
        {
            return reader.ReadBytes(count);
        }



        public void Write(byte[] data)
        {
            writer.Write(data);
        }

        public void Write(object obj)
        {
            switch (Convert.GetTypeCode(obj))
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.Char: writer.Write(Convert.ToByte(obj)); break;
                case TypeCode.Int16:
                case TypeCode.UInt16:
                    if (swapEndian)
                    {
                        byte[] data = BitConverter.GetBytes(Convert.ToUInt16(obj));
                        Array.Reverse(data);
                        writer.Write(data);
                    }
                    else writer.Write(Convert.ToUInt16(obj));
                    break;
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    if (swapEndian)
                    {
                        byte[] data = BitConverter.GetBytes(Convert.ToUInt32(obj));
                        Array.Reverse(data);
                        writer.Write(data);
                    }
                    else writer.Write(Convert.ToUInt32(obj));
                    break;
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    if (swapEndian)
                    {
                        byte[] data = BitConverter.GetBytes(Convert.ToUInt64(obj));
                        Array.Reverse(data);
                        writer.Write(data);
                    }
                    else writer.Write(Convert.ToUInt64(obj));
                    break;
                case TypeCode.Single:
                    if (swapEndian)
                    {
                        byte[] data = BitConverter.GetBytes(Convert.ToSingle(obj));
                        Array.Reverse(data);
                        writer.Write(data);
                    }
                    else writer.Write(Convert.ToSingle(obj));
                    break;
                case TypeCode.Double:
                    if (swapEndian)
                    {
                        byte[] data = BitConverter.GetBytes(Convert.ToDouble(obj));
                        Array.Reverse(data);
                        writer.Write(data);
                    }
                    else writer.Write(Convert.ToDouble(obj));
                    break;
                case TypeCode.String: writer.Write(ASCIIEncoding.ASCII.GetBytes((string)obj + "\0")); break;    // assumes youre writing an ascii string
                case TypeCode.Object:
                    byte[] bytes = obj as byte[]; // tries converting unknown object to byte array
                    if (bytes != null) writer.Write(bytes);
                    else throw new NotSupportedException("Invalid datatype.");
                    break;
                default: throw new NotSupportedException("Invalid datatype.");
            }
        }
        #endregion
    }
}
