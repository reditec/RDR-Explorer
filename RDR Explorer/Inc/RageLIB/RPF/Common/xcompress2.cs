using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RPFLib.Common
{
    class xcompress2
    {
        private enum XMEMCODEC_TYPE
        {
            XMEMCODEC_DEFAULT = 0,
            XMEMCODEC_LZX = 1
        };

        [StructLayout(LayoutKind.Explicit)]
        public struct XMEMCODEC_PARAMETERS_LZX
        {
            [FieldOffset(0)]
            public int Flags;
            [FieldOffset(4)]
            public int WindowSize;
            [FieldOffset(8)]
            public int CompressionPartitionSize;
        }

        [DllImport(@"xcompress32.dll")]
        private static extern int XMemCreateDecompressionContext(XMEMCODEC_TYPE CodecType, ref XMEMCODEC_PARAMETERS_LZX pCodecParams, int Flags, ref IntPtr pContext);

        [DllImport(@"xcompress32.dll")]
        private static extern int XMemDecompress(IntPtr Context, byte[] pDestination, ref int pDestSize, byte[] pSource, int SrcSize);

        [DllImport(@"xcompress32.dll")]
        private static extern int XMemDestroyDecompressionContext(IntPtr pContext);

        public static byte[] Decompress(byte[] data, int uncompressedSize)
        {
            byte[] outputData = new byte[uncompressedSize];
            int outputDataLength = uncompressedSize;
            IntPtr ctx = IntPtr.Zero;

            XMEMCODEC_PARAMETERS_LZX codecParams;
            codecParams.Flags = 0;
            codecParams.WindowSize = 64 * 1024;
            codecParams.CompressionPartitionSize = 256 * 1024;

            if (XMemCreateDecompressionContext(XMEMCODEC_TYPE.XMEMCODEC_LZX, ref codecParams, 1, ref ctx) != 0)
            {
                throw new Exception("XMemCreateDecompressionContext failed");
            }

            if (XMemDecompress(ctx, outputData, ref outputDataLength, data, data.Length) != 0)
            {
                XMemDestroyDecompressionContext(ctx);
                throw new Exception("XMemDecompress failed");
            }
            XMemDestroyDecompressionContext(ctx);

            if (outputDataLength != uncompressedSize)
            {
                throw new Exception("Decompression Failed");
            }

            return outputData;
        }
    }
}
