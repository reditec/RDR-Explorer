using System;
using System.Security.Cryptography;
using ICSharpCode.SharpZipLib.Zip.Compression;
using System.IO;
using ComponentAce.Compression.Libs.zlib;

namespace RPFLib.Common
{
    public static class DataUtil
    {
        private static byte[] key;

        public static void setKey(byte[] k)
        {
            key = k;
        }

        public static byte[] Decrypt(byte[] dataIn)
        {
            byte[] data = new byte[dataIn.Length];
            dataIn.CopyTo(data, 0);

            if (data == null)
            {
                return null;
            }
            int inputCount = data.Length & -16;
            if (inputCount > 0)
            {
                // Create our Rijndael class
                Rijndael rj = Rijndael.Create();
                rj.BlockSize = 128;
                rj.KeySize = 256;
                rj.Mode = CipherMode.ECB;
                rj.Key = key;
                rj.IV = new byte[16];
                rj.Padding = PaddingMode.None;

                ICryptoTransform transform = rj.CreateDecryptor();

                for (int i = 0; i < 0x10; i++)
                {
                    transform.TransformBlock(data, 0, inputCount, data, 0);
                }
            }
            return data;
        }

        public static byte[] DecryptNew(byte[] dataIn)
        {
            byte[] data = new byte[dataIn.Length];
            dataIn.CopyTo(data, 0);

            if (data == null)
            {
                return null;
            }
            int inputCount = data.Length & -16;
            if (inputCount > 0)
            {
                // Create our Rijndael class
                Rijndael rj = Rijndael.Create();
                rj.BlockSize = 128;
                rj.KeySize = 256;
                rj.Mode = CipherMode.ECB;
                rj.Key = key;
                rj.IV = new byte[16];
                rj.Padding = PaddingMode.None;

                ICryptoTransform transform = rj.CreateDecryptor();

                transform.TransformBlock(data, 0, inputCount, data, 0);
            }
            return data;
        }

        public static byte[] Encrypt(byte[] dataIn)
        {
            byte[] data = new byte[dataIn.Length];
            dataIn.CopyTo(data, 0);

            if (data == null)
            {
                return null;
            }
            int inputCount = data.Length & -16;
            if (inputCount > 0)
            {
                // Create our Rijndael class
                Rijndael rj = Rijndael.Create();
                rj.BlockSize = 128;
                rj.KeySize = 256;
                rj.Mode = CipherMode.ECB;
                rj.Key = key;
                rj.IV = new byte[16];
                rj.Padding = PaddingMode.None;

                ICryptoTransform transform = rj.CreateEncryptor();

                for (int i = 0; i < 0x10; i++)
                {
                    transform.TransformBlock(data, 0, inputCount, data, 0);
                }
            }
            return data;
        }

        public static byte[] EncryptNew(byte[] dataIn)
        {
            byte[] data = new byte[dataIn.Length];
            dataIn.CopyTo(data, 0);

            if (data == null)
            {
                return null;
            }
            int inputCount = data.Length & -16;
            if (inputCount > 0)
            {
                // Create our Rijndael class
                Rijndael rj = Rijndael.Create();
                rj.BlockSize = 128;
                rj.KeySize = 256;
                rj.Mode = CipherMode.ECB;
                rj.Key = key;
                rj.IV = new byte[16];
                rj.Padding = PaddingMode.None;

                ICryptoTransform transform = rj.CreateEncryptor();

                transform.TransformBlock(data, 0, inputCount, data, 0);
            }
            return data;
        }

        public static byte[] DecompressDeflate(byte[] data, int decompSize)
        {
            var decompData = new byte[decompSize];

            var inflater = new Inflater(true);
            inflater.SetInput(data);
            inflater.Inflate(decompData);

            return decompData;
        }

        public static byte[] Compress(byte[] input, int level)
        {

            byte[] bytesOut;
            byte[] temp = new byte[input.Length];
            Deflater deflater = new Deflater(level, true);
            try
            {
                deflater.SetInput(input, 0, input.Length);
                deflater.Finish();
                bytesOut = new byte[deflater.Deflate(temp)];
            }
            catch (Exception e)
            {
                throw e;
            }


            Array.Copy(temp, 0, bytesOut, 0, bytesOut.Length);

            return bytesOut;
        }

        public static void CompressData(byte[] inData, out byte[] outData)
        {
            using (MemoryStream outMemoryStream = new MemoryStream())
            using (ZOutputStream outZStream = new ZOutputStream(outMemoryStream, zlibConst.Z_DEFAULT_COMPRESSION))
            using (Stream inMemoryStream = new MemoryStream(inData))
            {
                CopyStream(inMemoryStream, outZStream);
                outZStream.finish();
                outData = outMemoryStream.ToArray();
            }
        }

        public static void DecompressData(byte[] inData, out byte[] outData)
        {
            using (MemoryStream outMemoryStream = new MemoryStream())
            using (ZOutputStream outZStream = new ZOutputStream(outMemoryStream))
            using (Stream inMemoryStream = new MemoryStream(inData))
            {
                CopyStream(inMemoryStream, outZStream);
                outZStream.finish();
                outData = outMemoryStream.ToArray();
            }
        }

        public static void CopyStream(System.IO.Stream input, System.IO.Stream output)
        {
            byte[] buffer = new byte[2000];
            int len;
            while ((len = input.Read(buffer, 0, 2000)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            output.Flush();
        }

        public static uint SwapEndian(uint v)
        {
            return ((v >> 24) & 0xFF) |
                   ((v >> 8) & 0xFF00) |
                   ((v & 0xFF00) << 8) |
                   ((v & 0xFF) << 24);
        }

        public static int RoundUp(int num, int multiple)
        {
            if (multiple == 0)
                return 0;
            int add = multiple / Math.Abs(multiple);
            return ((num + multiple - add) / multiple) * multiple;
        }
    }
}
