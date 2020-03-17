using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWUDataExtractor.Core.Model
{
    public class Module
    {
        public Module(string code)
        {
            Code = code;
        }
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
