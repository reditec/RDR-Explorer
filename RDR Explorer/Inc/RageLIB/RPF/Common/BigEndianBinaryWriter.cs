#region Usings

using System;
using System.IO;
using System.Text;

#endregion

namespace RPFLib.Common
{
    /// <summary>
    /// Writes primitive types to a stream, in Big Endian binary representation. Supports writing strings in a specific encoding (default encoding is UTF-8).
    /// </summary>
    public class BigEndianBinaryWriter : BinaryWriter
    {
        #region Constructors
        public BigEndianBinaryWriter(Stream output)
            : base(output)
        {
        }

        public BigEndianBinaryWriter(Stream output, Encoding encoding)
            : base(output, encoding)
        {
        }
        #endregion

        #region Implementation of abstract methods

        public override void Write(byte data)
        {
            byte[] buffer = new[] { data };
            Write(buffer);
        }

        public override void Write(short data)
        {
            byte[] bytes = BitConverter.GetBytes(data);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            Write(bytes);
        }

        public override void Write(int data)
        {
            byte[] bytes = BitConverter.GetBytes(data);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            Write(bytes);
        }

        public void writebigint(int data)
        {
            byte[] bytes = BitConverter.GetBytes(data);
            Array.Reverse(bytes);
            Write(bytes);
        }

        public override void Write(long data)
        {
            byte[] bytes = BitConverter.GetBytes(data);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            Write(bytes);
        }

        public override void Write(float data)
        {
            byte[] bytes = BitConverter.GetBytes(data);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            Write(bytes);
        }

        public override void Write(double data)
        {
            byte[] bytes = BitConverter.GetBytes(data);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            Write(bytes);
        }

        #endregion

    }
}