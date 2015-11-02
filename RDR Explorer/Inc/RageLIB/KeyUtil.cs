/**********************************************************************\

 RageLib
 Copyright (C) 2008  Arushan/Aru <oneforaru at gmail.com>

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

\**********************************************************************/

using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Win32;
using System.Diagnostics;

namespace RageLib.Common
{
    
    public abstract class KeyUtil
    {
        public static string MYgameExe;
        public abstract string ExecutableName { get; }
        protected abstract uint[] SearchOffsets { get; }
        public byte[] FindKey( string gamePath, string gameName )
        {
            
            byte[] key = null;
            ProcessStartInfo startInfo = new ProcessStartInfo("xextool.exe");
            startInfo.Arguments = "-b base.bin " + "\"" + MYgameExe + "\"";
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            Process BINextractor = Process.Start(startInfo);
            BINextractor.WaitForExit();
            if(File.Exists("base.bin"))
            {
                var fs = new FileStream("base.bin", FileMode.Open, FileAccess.Read);
                foreach (var u in SearchOffsets)
                {
                    if (u <= fs.Length - 32)
                    {
                        var tempKey = new byte[32];
                        fs.Seek(u, SeekOrigin.Begin);
                        fs.Read(tempKey, 0, 32);
                        var hash = BitConverter.ToString(SHA1.Create().ComputeHash(tempKey)).Replace("-", "");
                        if (hash == "87862497EE46855372B51C7A324A2BB5CD66F4AF")
                        {
                            key = tempKey;
                            break;
                        }
                    }
                }
                fs.Close();
            }
            
            return key;
        }
    }
}