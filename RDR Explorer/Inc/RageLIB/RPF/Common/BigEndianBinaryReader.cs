using System;
using System.IO;

namespace RPFLib.Common
{
    public class BigEndianBinaryReader : BinaryReader
    {
        public BigEndianBinaryReader(Stream input) : base(input) { }

        public override short ReadInt16()
        {
            byte[] byteBuffer = base.ReadBytes(2);
            return (short)((byteBuffer[0] << 8) | byteBuffer[1]);
        }

        public override int ReadInt32()
        {
            byte[] byteBuffer = base.ReadBytes(4);
            return (int)((byteBuffer[0] << 24) | (byteBuffer[1] << 16) | (byteBuffer[2] << 8) | byteBuffer[3]);
        }

        public override ushort ReadUInt16()
        {
            byte[] byteBuffer = base.ReadBytes(2);
            return (ushort)((byteBuffer[0] << 8) | byteBuffer[1]);
        }

        public override uint ReadUInt32()
        {
            byte[] byteBuffer = base.ReadBytes(4);
            return (uint)((byteBuffer[0] << 24) | (byteBuffer[1] << 16) | (byteBuffer[2] << 8) | byteBuffer[3]);
        }

        public override float ReadSingle()
        {
            byte[] byteBuffer = BitConverter.GetBytes(ReadUInt32());
            return BitConverter.ToSingle(byteBuffer, 0);
        }

        public string ReadNullTerminatedString()
        {
            string newString = "";
            char temp;
            while ((temp = ReadChar()) != '\0')
            {
                if (temp != '\0') newString += temp;
                else break;
            }
            return newString;
        }
    }
}
