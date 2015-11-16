using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;

namespace RDR_Explorer.Inc.RageLIB
{
    public class RSCTypes
    {
        protected Dictionary<int, string> dicType = new Dictionary<int, string>();

        public RSCTypes()
        {
            //init the different types , only known ones for now
            dicType.Add(133, "85"); //models
            dicType.Add(31, "1F");
            dicType.Add(26, "05");
            dicType.Add(36, "05");
            dicType.Add(116, "05");
            dicType.Add(33, "05");
            dicType.Add(8, "05");
            dicType.Add(1, "01");
            dicType.Add(138, "8A"); //fragments
            dicType.Add(30, "05");
            dicType.Add(134, "86 (xsi)");
            dicType.Add(18, "12");
            dicType.Add(10, "0A");
            dicType.Add(39, "27");
        }

        public string getType(string attributeString)
        {
            string rscType = "";
            string finalString = "";

            if (attributeString != "Compressed" && attributeString != "None")
            {
                string[] attributeSplit = attributeString.Split(' ');
                //index = 2

                finalString = attributeSplit[2].Remove(attributeSplit[2].Length - 1);

                int version = Convert.ToInt32(finalString);

                string type;

                dicType.TryGetValue(version, out type);
                rscType = "RSC" + type + " (Version: " + finalString + ")";
            }
            else
            {
                rscType = "Resource, " + attributeString;
            }

            return rscType;
        }
    }
}