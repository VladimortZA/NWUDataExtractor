using NWUDataExtractor.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NWUDataExtractor.Core.DataTools
{
    internal enum MatchType
    {
        Faculty,
        Module,
        NextPageProj,
        ProjectCode,
        ProgramName,
        NextPageFaculty
    }

    internal class DataMatcher
    {
        public DataMatcher()
        {

        }

        public Match FacultyMatch { get; private set; }
        public Match ModuleMatch { get; private set; }
        public Match NextPageProjMatch { get; private set; }
        public Match ProjCodeMatch { get; private set; }
        public Match ProgramNameMatch { get; private set; }

        public Match GetMatch(string data, MatchType type, string module = "")
        {
            Match match = null;

            switch (type)
            {
                case MatchType.Faculty:
                    FacultyMatch = Regex.Match(data, DataFilter.Faculty, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    match = FacultyMatch;
                    break;
                case MatchType.ProjectCode:
                    ProjCodeMatch = Regex.Match(data, DataFilter.ProjectCode, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    match = ProjCodeMatch;
                    break;
                case MatchType.NextPageProj:
                    NextPageProjMatch = Regex.Match(data, DataFilter.ProjectCode, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    match = NextPageProjMatch;
                    break;
                case MatchType.ProgramName:
                    ProgramNameMatch = Regex.Match(data, DataFilter.ProgramName, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    match = ProgramNameMatch;
                    break;
                case MatchType.Module:
                    ModuleMatch = Regex.Match(data, DataFilter.GetFullModulePattern(module), RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    match = ModuleMatch;
                    break;
                default:
                    break;
            }

            return match;
        }
    }
}
