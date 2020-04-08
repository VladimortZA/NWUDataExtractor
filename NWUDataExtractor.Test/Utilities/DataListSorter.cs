using NWUDataExtractor.Core.Model;
using System.Collections.Generic;

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
