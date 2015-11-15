/*
 
    RPF7Viewer - Viewer for RAGE Package File version 7
    Copyright (C) 2013  koolk <koolkdev at gmail.com>
   
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
  
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
   
    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RPFLib.Common
{
    public class BitsStream
    {
        private Stream Stream;

        public long Position = 0;
        public int BitPosition = 0;

        public long Length;

        private long RoundUp(long x, long m)
        {
            long result = x + (m - (x % m));
            if ((x % m) == 0)
            {
                result -= m;
            }
            return result;
        }

        public BitsStream(Stream inputStream)
        {
            this.Stream = inputStream;
            this.Length = this.Stream.Length;
        }

        public long ReadBits(int bits)
        {
            if (bits > 64) {
                throw new Exception("Error: Can't read big number");
            }

            long StartBit = Position * 8 + BitPosition;
            long EndBit = StartBit + bits;
            long StartByte = StartBit / 8;
            long EndByte = RoundUp(EndBit, 8) / 8;

            if (EndByte > Length)
            {
                throw new Exception("Error: Out of bounds");
            }

            Byte [] bytes = new Byte[EndByte - StartByte];
            this.Stream.Position = StartByte;
            this.Stream.Read(bytes, 0, (int)(EndByte - StartByte));

            long result = 0;
            int shift_by = 0;
            for (long i = EndByte - 1; i >= StartByte; --i)
            {
                if (i == EndByte - 1)
                {
                    if (i == StartByte)
                    {
                        result = ((long)bytes[i - StartByte] & ((1 << (8 - BitPosition)) - 1)) >> (int)(RoundUp(EndBit, 8) - EndBit);
                    }
                    else
                    {
                        result = (long)bytes[i - StartByte] >> (int)(RoundUp(EndBit, 8) - EndBit);
                        shift_by = 8 - (int)(RoundUp(EndBit, 8) - EndBit);
                    }
                }
                else if (i == StartByte)
                {
                    result += ((long)bytes[i - StartByte] & ((1 << (8 - BitPosition)) - 1)) << shift_by;
                }
                else
                {
                    result += (long)bytes[i - StartByte] << shift_by;
                    shift_by += 8;
                }
            }

            Position = EndBit / 8;
            BitPosition = (int)(EndBit % 8);
            return result;
        }

        public int ReadInt()
        {
            return (int)ReadBits(32);
        }

        public bool ReadBool()
        {
            return ReadBits(1) == 1;
        }

        public String ReadString(int length)
        {
            return Encoding.Default.GetString(this.ReadBytes(length));
        }


        public byte[] ReadBytes(int length)
        {
            if (BitPosition != 0)
            {
                throw new Exception("Error: Must be in start of a byte for reading bytee");
            }
            if (Position + length > Length)
            {
                throw new Exception("Error: Out of bounds");
            }
            Byte[] buffer = new Byte[length];
            this.Stream.Position = Position;
            this.Stream.Read(buffer, 0, length);
            Position += length;
            return buffer;
        }

        public void Seek(long offset)
        {
            if (offset > Length)
            {
                throw new Exception("Error: Out of bounds");
            }
            Position = offset;
        }

        public String ReadCString()
        {
            this.Stream.Position = Position;
            String result = "";
            int lastChar = this.Stream.ReadByte();
            while (lastChar != 0) {
                Position += 1;
                result += (char)lastChar;
                lastChar = this.Stream.ReadByte();
            }
            return result;
        }
    }
}
