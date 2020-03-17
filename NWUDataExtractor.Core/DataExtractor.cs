using CsvHelper;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using NWUDataExtractor.Core.DataTools;
using NWUDataExtractor.Core.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NWUDataExtractor.Core
{
    public class DataExtractor : IDataExtractor
    {
        Stack<ModuleDataEntry> dataEntries;
        readonly object locker = new object();
        public int TotalPageCount { get; private set; } = 0;

        public async Task<List<ModuleDataEntry>> GetModuleDataAsync(List<Module> moduleList, string inputPDF, IProgress<double> progress = null)
        {
            dataEntries = new Stack<ModuleDataEntry>();
            int tempCount = 0;

            using (PdfReader reader = new PdfReader(inputPDF))
            {
                TotalPageCount = reader.NumberOfPages;

                await Task.Run(() => Parallel.For(1, TotalPageCount, (i) =>
                {
                    string pageData, pageNext, sectionData = string.Empty;
                    DataMatcher matcher = new DataMatcher();
                    lock (locker)
                        pageData = PdfTextExtractor.GetTextFromPage(reader, i);
                    Interlocked.Increment(ref tempCount);
                    progress.Report(Math.Round((double)(tempCount * 100) / TotalPageCount));

                    if (i < TotalPageCount)
                    {
                        lock (locker)
                            pageNext = PdfTextExtractor.GetTextFromPage(reader, i + 1);
                    }
                    else
                        pageNext = string.Empty;

                    InitialiseMatchers(pageData, pageNext, matcher);

                    if (!matcher.FacultyMatch.Success || !matcher.ProjCodeMatch.Success)
                    {
                        sectionData = string.Empty;
                        return;
                    }
                    if (matcher.FacultyMatch.Success && matcher.ProjCodeMatch.Success)
                        sectionData = string.Empty + pageData;
                    if (!matcher.NextPageProjMatch.Success && i < TotalPageCount)
                    {
                        i++;
                        if (sectionData == string.Empty)
                            return;

                        sectionData += $"\n{pageNext}";
                    }
                    matcher.GetMatch(sectionData, MatchType.ProgramName);
                    NewMethod(moduleList, sectionData, matcher);
                }));
            }
            return new List<ModuleDataEntry>(dataEntries);
        }

        private void NewMethod(List<Module> moduleList, string sectionData, DataMatcher matcher)
        {
            ModuleDataEntry dataEntry;
            foreach (var module in moduleList)
            {
                if (matcher.GetMatch(sectionData, MatchType.Module, module.Code).Success)
                {
                    dataEntry = new ModuleDataEntry
                    {
                        Faculty = matcher.FacultyMatch.Groups["faculty"].Value,
                        ModuleCode = module.Code,
                        Year = int.TryParse(matcher.ModuleMatch.Groups["year"].Value, out int year) ? year : (int?)null,
                        Semester = int.TryParse(matcher.ModuleMatch.Groups["semester"].Value, out int semester) ? semester : (int?)null,
                        ProgrammeCode = matcher.ProjCodeMatch.Groups["proCode"].Value,
                        CurriculumCode = matcher.ProjCodeMatch.Groups["curCode"].Value,
                        OldCode = matcher.ProjCodeMatch.Groups["oldCode"].Value,
                        ProgramName = matcher.ProgramNameMatch.Groups["progName"].Value
                    };
                    lock (locker)
                        dataEntries.Push(dataEntry);
                }
            }
        }

        private static void InitialiseMatchers(string pageData, string pageNext, DataMatcher matcher)
        {
            matcher.GetMatch(pageData, MatchType.Faculty);
            matcher.GetMatch(pageData, MatchType.ProjectCode);
            matcher.GetMatch(pageNext, MatchType.NextPageProj);
        }
    }
}
