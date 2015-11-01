using System;
using System.Collections.Generic;
using System.Text;

namespace RageLib.Common
{
    public class KeyUtilRDR : KeyUtil
    {
        public override string ExecutableName
        {
            get { return "default.xex"; }
        }

        protected override uint[] SearchOffsets
        {
            get
            {
                return new uint[]
                           {
                               //Offsets of the XEX files
                               0xFBA078 /* GOTY Edition - I don't know the version - My xextool says v0.0.0.12 (probably 1.02) */,
                               
                           };
            }
        }
    }
}
