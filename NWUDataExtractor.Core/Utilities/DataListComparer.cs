using NWUDataExtractor.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWUDataExtractor.Core.Utilities
{
    public class DataListComparer : IComparer<List<ModuleDataEntry>>
    {
        public int Compare(List<ModuleDataEntry> x, List<ModuleDataEntry> y)
        {
            foreach (var item in x)
            {
                //item.ModuleName
            }

            return 0;
        }
    }
}
