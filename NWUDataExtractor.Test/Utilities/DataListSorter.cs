using NWUDataExtractor.Core.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWUDataExtractor.Test.Utilities
{
    internal static class DataListSorter
    {
        public static List<string> StringifyList(IEnumerable<ModuleDataEntry> inputList)
        {
            List<string> list = new List<string>();
            
            foreach (var item in inputList)
            {
                list.Add(item.ToString());
            }

            return list;
        }
    }
}
