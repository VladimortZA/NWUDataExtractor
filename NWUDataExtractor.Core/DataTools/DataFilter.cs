using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWUDataExtractor.Core.DataTools
{
    internal static class DataFilter
    {
        public static string Faculty { get; } = 
            @"^chapter\s*\d.\s*(?<faculty>[\D\s]*)\s\d{1,3}";
        public static string ProjectCode { get; } = 
            @"\d.\d+\s(?<proCode>\w{6})-(?<curCode>\w{5}):\s*(?:\(old code:\s*(?<oldCode>\w{6}\s*[-:]\s*\w{4,5})\s*?\))?";
        public static string ProgramName { get; } = 
            @"\d.\d+\s\w{6}-\w{5}:\s?(?:\(old code\s*:\s*.*\w{6}[:-]\s*\w{4,5}\s*\))?(?<progName>[\s\S]*?)year";
        public static string ModulePattern { get; } = @".*year\s(?<year>\d).*?";

        public static string GetFullModulePattern(string module)
        {
            return ModulePattern + module + @"[^\w]";
        }
    }
}
