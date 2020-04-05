using KalosfideAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.CLF
{
    public class CLFCompare: IComparer<DocCLF>
    {
        public int Compare(DocCLF doc1, DocCLF doc2)
        {
            int compare = DateTime.Compare(doc1.Date.Value, doc2.Date.Value);
            if (compare != 0)
            {
                return compare;
            }
            string[] types = new string[]
            {
                "C",
                "L",
                "F"
            };
            int i1 = Array.IndexOf(types, doc1.Type);
            int i2 = Array.IndexOf(types, doc2.Type);
            if (i1 < i2)
            {
                return -1;
            }
            if (i1 > i2)
            {
                return 1;
            }
            return 0;
        }

    }
}
