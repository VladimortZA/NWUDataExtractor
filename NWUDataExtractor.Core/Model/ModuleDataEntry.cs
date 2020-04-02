using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NWUDataExtractor.Core.Model
{
    public class ModuleDataEntry
    {
        private string programName;
        private string faculty;

        public string ModuleCode { get; set; }
        public int? Year { get; set; }
        public int? Semester { get; set; }
        public string ModuleName { get; set; }
        public string Faculty
        {
            get
            {
                return faculty;
            }
            set
            {
                faculty = value.Replace("\n", " ");
            }
        }
        public string ProgrammeCode { get; set; }
        public string CurriculumCode { get; set; }
        public string OldCode { get; set; }
        public string ProgramName
        {
            get
            {
                return programName;
            }
            set
            {
                programName = Regex.Replace(value, @"-\s", "").Replace("\n", " ");

            }
        }

        public override string ToString()
        {
            return $"{ModuleCode},{Year},{Semester},{Faculty},{ProgrammeCode},{CurriculumCode},{OldCode},{ProgramName}";

        }
    }
}
