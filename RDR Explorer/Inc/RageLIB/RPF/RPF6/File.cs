using System;
using System.IO;
using RPFLib.Common;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

namespace RPFLib.RPF6
{
    internal class File
    {
        private Stream _stream;

        public File()
        {
            Header = new Header(this);
            TOC = new TOC(this);
        }

        public Header Header { get; private set; }
        public TOC TOC { get; private set; }

        public int Open(string filename)
        {

            _stream = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

            var br = new BigEndianBinaryReader(_stream);
            Header.Read(br);


       /*     _stream.Position = 204410912;
            var br1 = new BinaryReader(_stream);
            int tocSize = 12320;

            byte[] tocData1 = br.ReadBytes(tocSize);
            System.IO.File.WriteAllBytes(@"C:\Users\Dageron\Desktop\test0.bin", tocData1);

            _stream.Position = 204410912;
            byte[] tocData2 = br.ReadBytes(tocSize);

            tocData2 = DataUtil.Decrypt(tocData2);
            System.IO.File.WriteAllBytes(@"C:\Users\Dageron\Desktop\test1.bin", tocData2); */
            
            if (!Enum.IsDefined(typeof(HeaderIDs), (int)Header.Identifier))
            {
                _stream.Close();
                return 0;
            }

            _stream.Seek(0x10, SeekOrigin.Begin);
            TOC.Read(br);
            return Header.EntryCount;
        }

        public void Close()
        {
            if (_stream != null)
            {
                _stream.Close();
            }
        }

        public void save(FileStream newRPFstream, BackgroundWorker thread, DoWorkEventArgs e)
        {
            int fileprogress = 0;
            try
            {
                if (_stream != null)
                {
                    _stream.Position = 0;
                    var bw = new BigEndianBinaryWriter(newRPFstream);
                    var br = new BinaryReader(_stream);


                    Header.EntryCount = TOC.count;
                    Header.Write(bw);
                    bw.Write(new byte[Header.EntryCount * 20]);
                    var tocOffset = 0x10;
                    TOC.ReorderOffset();
                    bw.Write(new byte[(TOC.fileStart-1) - bw.BaseStream.Position]); // 614400
                    //if (System.IO.File.Exists(@"D:\xbox stuff\xboxtemp\RDR undead\offsets.txt"))
                    //    System.IO.File.Delete(@"D:\xbox stuff\xboxtemp\RDR undead\offsets.txt");
                    //if (System.IO.File.Exists(@"D:\xbox stuff\xboxtemp\RDR undead\offsets_mod.txt"))
                    //    System.IO.File.Delete(@"D:\xbox stuff\xboxtemp\RDR undead\offsets_mod.txt");
                    foreach (var entry in TOC)
                    {
                        //    System.IO.File.AppendAllText(@"D:\xbox stuff\xboxtemp\RDR undead\offsets.txt", entry._Offset.ToString() + " - " + entry.count + Environment.NewLine);
                        if ((thread.CancellationPending == true))
                        {
                            e.Cancel = true;
                            return;
                        }

                        thread.ReportProgress(Convert.ToInt32(((double)fileprogress / TOC.count) * 100.0));
                        var fileEntry = entry as FileEntry;
                        if (fileEntry != null)
                        {
                            if (!fileEntry.IsResourceFile && !fileEntry.IsCompressed && fileEntry.SizeInArchive > 100000)
                            {
                                long paddingsize = (RoundUp(bw.BaseStream.Position, 2048)) - bw.BaseStream.Position;
                                bw.Write(new byte[(int)paddingsize]);
                            }
                            else if (fileEntry.IsResourceFile)
                            {
                                long paddingsize = (RoundUp(bw.BaseStream.Position, 2048)) - bw.BaseStream.Position;
                                if (paddingsize == 0)
                                    paddingsize = 2048;
                                bw.Write(new byte[(int)paddingsize]);
                            }
                            else
                            {
                                long paddingsize;
                                if (fileEntry.SizeInArchive > 110000)
                                    paddingsize = (RoundUp(bw.BaseStream.Position, 2048)) - bw.BaseStream.Position;
                                else
                                {
                                    paddingsize = (RoundUp(bw.BaseStream.Position, 8)) - bw.BaseStream.Position;
                                    if (paddingsize == 0)
                                        paddingsize = 8;
                                }
                                bw.Write(new byte[(int)paddingsize]);
                            }
                            if (fileEntry.CustomData != null)
                            {
                                fileEntry.Offset = bw.BaseStream.Position;
                                bw.Write(fileEntry.CustomData);
                                //fileEntry.SetCustomData(null);
                            }
                            else
                            {
                                br.BaseStream.Position = fileEntry.Offset;
                                byte[] buffer = br.ReadBytes(fileEntry.SizeInArchive);
                                fileEntry.Offset = bw.BaseStream.Position;
                                bw.Write(buffer);
                            }
                        }
                        fileprogress++;
                        //   System.IO.File.AppendAllText(@"D:\xbox stuff\xboxtemp\RDR undead\offsets_mod.txt", entry._Offset.ToString() + " - " + entry.count + Environment.NewLine);
                    }
                    if (br.BaseStream.Length > bw.BaseStream.Length)
                    {
                        bw.Write(new byte[br.BaseStream.Length - bw.BaseStream.Length]);
                    }
                    bw.Seek(tocOffset, SeekOrigin.Begin);
                    TOC.ReorderCount();
                    TOC.Write(bw);
                    newRPFstream.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(fileprogress.ToString() + ex.Message + Environment.NewLine + ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        public static uint round(long origNum, long padding)
        {
            long num = origNum / padding;
            if ((num * padding) == origNum)
            {
                return 0;
            }
            long num3 = (num + 1L) * padding;
            long num4 = num3 - origNum;
            return (uint)num4;
        }

        public static long RoundUp(long num, long multiple)
        {
            if (multiple == 0)
                return 0;
            long add = multiple / Math.Abs(multiple);
            long test = ((num + multiple - add) / multiple) * multiple;
            return ((num + multiple - add) / multiple) * multiple;
        }

        public byte[] ReadData(long offset, int length)
        {
                var buffer = new byte[length];
                _stream.Seek(offset, SeekOrigin.Begin);
                _stream.Read(buffer, 0, length);
                return buffer;
        }

    }
}