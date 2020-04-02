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
        CancellationTokenSource cts;
        int semester = 0;

        public async Task<List<ModuleDataEntry>> GetModuleDataAsync(List<Module> moduleList, string pdfPath, IProgress<double> progress = null)
        {
            cts = new CancellationTokenSource();
            dataEntries = new Stack<ModuleDataEntry>();
            int tempCount = 0;
            ParallelOptions options = new ParallelOptions()
            {
                CancellationToken = cts.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            if (!File.Exists(pdfPath) || System.IO.Path.GetExtension(pdfPath) != ".pdf")
                return null;

            using (PdfReader reader = new PdfReader(pdfPath))
            {
                TotalPageCount = reader.NumberOfPages;

                await Task.Run(() => Parallel.For(1, TotalPageCount, options, (i) =>
                {
                    string pageData, pageNext, sectionData = string.Empty;
                    DataMatcher matcher = new DataMatcher();
                    lock (locker)
                        pageData = PdfTextExtractor.GetTextFromPage(reader, i);

                    progress?.Report(Math.Round((double)(Interlocked.Increment(ref tempCount) * 100) / TotalPageCount));

                    if (semester == 0)
                    {
                        lock (locker)
                        {
                            semester = int.Parse(Regex.Match(pageData, @"semester\s*(\d)", RegexOptions.IgnoreCase).Groups[1].Value);
                        }
                    }

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
                        progress?.Report(Math.Round((double)(Interlocked.Increment(ref tempCount) * 100) / TotalPageCount));
                        if (sectionData == string.Empty)
                            return;

                        sectionData += $"\n{pageNext}";
                    }
                    matcher.GetMatch(sectionData, MatchType.ProgramName);
                    AddMatchedModule(moduleList, sectionData, matcher);
                    options.CancellationToken.ThrowIfCancellationRequested();
                }));
            }
            return new List<ModuleDataEntry>(dataEntries);
        }

        public void Cancel()
        {
            if (cts != null)
                cts.Cancel();
        }

        private void AddMatchedModule(List<Module> moduleList, string sectionData, DataMatcher matcher)
        {
            ModuleDataEntry dataEntry;
            foreach (var module in moduleList)
            {             
                if (sectionData.IndexOf(module.Code, StringComparison.OrdinalIgnoreCase) >= 0 &&
                    matcher.GetMatch(sectionData, MatchType.Module, module.Code).Success)
                {
                    dataEntry = new ModuleDataEntry
                    {
                        Faculty = matcher.FacultyMatch.Groups["faculty"].Value,
                        ModuleCode = module.Code.ToUpper(),
                        Year = int.TryParse(matcher.ModuleMatch.Groups["year"].Value, out int year) ? year : (int?)null,
                        Semester = semester,
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
