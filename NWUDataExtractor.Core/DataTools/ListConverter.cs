using NWUDataExtractor.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWUDataExtractor.Core.DataTools
{
    public static class ListConverter
    {
        public static List<Module> ConvertToList(string rawString)
        {
            List<Module> list = new List<Module>();
            string[] modules = Array.Empty<string>();

            if (rawString != null)
                modules = rawString.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach(string module in modules)
                list.Add(new Module(module));

            return list;
        }
    }
}
