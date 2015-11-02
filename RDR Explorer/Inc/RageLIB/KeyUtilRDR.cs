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
                               0xF87278, //Red Dead Redemption 1-Disk: Title Update #9
                               0xFBA018, //Red Dead Redemption 1-Disk: Title Update #8
                               0xFBA078, // GOTY Edition - Retail Version & Red Dead Redemption 1-Disk: Title Update #7
                               0xFB89F8, //Red Dead Redemption 1-Disk: Title Update #6
                               0xF98088, //Red Dead Redemption 1-Disk: Title Update #5
                               0xF846D8, //Red Dead Redemption 1-Disk: Title Update #2 & #4
                               0xF846E8, //Red Dead Redemption 1-Disk: Title Update #3
                               0xF872A8, // GOTY Edition - Title Update #1
                               0xF87268, //Zombie DLC: Title Update #7
                               0xFBA038, //Zombie DLC: Title Update #6
                               0xFBA098, //Zombie DLC: Title Update #5
                           };
            }
        }
    }
}
