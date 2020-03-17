using CsvHelper;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
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
        Stack<ModuleDataEntry> dataEntries = new Stack<ModuleDataEntry>();
        Stack<ModuleDataEntry> remainderList = new Stack<ModuleDataEntry>();

        const string facultyFilter = @"^chapter\s*\d.\s*(?<faculty>[\D\s]*)\s\d{1,3}";
        const string projCodeFilter = @"\d.\d+\s(?<proCode>\w{6})-(?<curCode>\w{5}):\s*(?:\(old code:\s*(?<oldCode>\w{6}\s*[-:]\s*\w{4,5})\s*?\))?";
        const string programNameFilter = @"\d.\d+\s\w{6}-\w{5}:\s?(?:\(old code\s*:\s*.*\w{6}[:-]\s*\w{4,5}\s*\))?(?<progName>[\s\S]*?)year";
        const string modulePatternFilter = @"(?<=year\s\d\ssemester\s\d).*(?:year\s(?<year>\d)\ssemester\s(?<semester>\d)).*?";

        int coreCount = Environment.ProcessorCount;
        int tempCount = 0;
        object locker = new object();
        string sectionData = string.Empty;
        public int TotalPageCount { get; private set; } = 0;

        public async Task<List<ModuleDataEntry>> GetModuleDataAsync(List<Module> moduleList, string inputPDF, IProgress<double> progress = null)
        {
            using (PdfReader reader = new PdfReader(inputPDF))
            {
                TotalPageCount = reader.NumberOfPages;

                await Task.Run(() => Parallel.For(1, coreCount + 1, (n) =>
                {
                    int toPageCountPerCore, fromPageCountPerCore;
                    string pageData, modulePattern, pageNext, sectionData = string.Empty;
                    ModuleDataEntry dataEntry;

                    Match facultyMatch, moduleMatch, nextPageProjMatch, projCodeMatch, programNameMatch, nextPageFacultyMatch;

                    toPageCountPerCore = n * (TotalPageCount / coreCount);
                    fromPageCountPerCore = toPageCountPerCore - (TotalPageCount / coreCount);

                    for (int i = fromPageCountPerCore + 1; i <= toPageCountPerCore; i++)
                    {
                        lock (locker)
                            pageData = PdfTextExtractor.GetTextFromPage(reader, i);

                        tempCount = Interlocked.Increment(ref tempCount);

                        if (i < TotalPageCount)
                        {
                            lock (locker)
                                pageNext = PdfTextExtractor.GetTextFromPage(reader, i + 1);
                        }
                        else
                            pageNext = string.Empty;

                        facultyMatch = Regex.Match(pageData, facultyFilter, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        projCodeMatch = Regex.Match(pageData, projCodeFilter, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        nextPageProjMatch = Regex.Match(pageNext, projCodeFilter, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        nextPageFacultyMatch = Regex.Match(pageNext, facultyFilter, RegexOptions.IgnoreCase | RegexOptions.Singleline);

                        if (!facultyMatch.Success || !projCodeMatch.Success)
                        {
                            sectionData = string.Empty;
                            continue;
                        }
                        if (facultyMatch.Success && projCodeMatch.Success)
                        {
                            if (i == toPageCountPerCore && !nextPageProjMatch.Success)
                            {
                                sectionData = string.Empty + pageData;
                                sectionData += $"\n{pageNext}";
                                goto EndFound;
                            }
                            sectionData = string.Empty + pageData;
                        }

                        if (!nextPageProjMatch.Success && i < TotalPageCount)
                        {
                            //i++;
                            // If section data is empty, that means the section remainder data
                            // has already been handled in another thread.
                            if (sectionData == string.Empty)
                                continue;

                            sectionData += $"\n{pageNext}";
                        }

                    EndFound:
                        programNameMatch = Regex.Match(sectionData, programNameFilter, RegexOptions.IgnoreCase);

                        foreach (var module in moduleList)
                        {
                            modulePattern = modulePatternFilter + module.Code;
                            moduleMatch = Regex.Match(sectionData, modulePattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

                            if (moduleMatch.Success)
                            {
                                dataEntry = new ModuleDataEntry
                                {
                                    Faculty = facultyMatch.Groups["faculty"].Value,
                                    ModuleCode = module.Code,
                                    Year = int.TryParse(moduleMatch.Groups["year"].Value, out int year) ? year : (int?)null,
                                    Semester = int.TryParse(moduleMatch.Groups["semester"].Value, out int semester) ? semester : (int?)null,
                                    ProgrammeCode = projCodeMatch.Groups["proCode"].Value,
                                    CurriculumCode = projCodeMatch.Groups["curCode"].Value,
                                    OldCode = projCodeMatch.Groups["oldCode"].Value,
                                    ProgramName = programNameMatch.Groups["progName"].Value
                                };

                                lock (locker)
                                    dataEntries.Push(dataEntry);
                            }
                        }
                        progress.Report(Math.Round((double)(tempCount * 100) / TotalPageCount));
                    }
                }));
            }
            return new List<ModuleDataEntry>(dataEntries);
        }

        public List<ModuleDataEntry> GetModuleData(List<Module> moduleList, string inputPDF)
        {
            string pageData, pageNext, modulePattern;
            string sectionData = string.Empty;

            Match initialMatch;
            Match projCodeMatch;
            Match nextPageMatch;
            Match moduleMatch;
            Match programNameMatch;

            ModuleDataEntry dataEntry;

            using (PdfReader reader = new PdfReader(inputPDF))
            {
                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    pageData = PdfTextExtractor.GetTextFromPage(reader, i);

                    if (i < reader.NumberOfPages)
                    {
                        pageNext = PdfTextExtractor.GetTextFromPage(reader, i + 1);
                    }
                    else
                    {
                        pageNext = string.Empty;
                    }

                    initialMatch = Regex.Match(pageData, facultyFilter, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    projCodeMatch = Regex.Match(pageData, projCodeFilter, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    nextPageMatch = Regex.Match(pageNext, projCodeFilter, RegexOptions.IgnoreCase | RegexOptions.Singleline);

                    if (!initialMatch.Success || !projCodeMatch.Success)
                    {
                        sectionData = string.Empty;
                        continue;
                    }
                    if (initialMatch.Success && projCodeMatch.Success)
                        sectionData = string.Empty + pageData;
                    if (nextPageMatch.Success && !projCodeMatch.Success)
                    {
                        if (i < reader.NumberOfPages) i++;
                        sectionData += $"\n{pageNext}";
                    }

                    programNameMatch = Regex.Match(sectionData, programNameFilter, RegexOptions.IgnoreCase);

                    foreach (var module in moduleList)
                    {
                        modulePattern = modulePatternFilter + module.Code;
                        moduleMatch = Regex.Match(sectionData, modulePattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

                        if (moduleMatch.Success)
                        {
                            dataEntry = new ModuleDataEntry
                            {
                                Faculty = initialMatch.Groups["faculty"].Value,
                                ModuleCode = module.Code,
                                Year = int.TryParse(moduleMatch.Groups["year"].Value, out int year) ? year : (int?)null,
                                Semester = int.TryParse(moduleMatch.Groups["semester"].Value, out int semester) ? semester : (int?)null,
                                ProgrammeCode = projCodeMatch.Groups["proCode"].Value,
                                CurriculumCode = projCodeMatch.Groups["curCode"].Value,
                                OldCode = projCodeMatch.Groups["oldCode"].Value,
                                ProgramName = programNameMatch.Groups["progName"].Value
                            };

                            //Console.WriteLine(dataEntry.ToString());
                            dataEntries.Push(dataEntry);
                        }
                    }
                }
            }
            return new List<ModuleDataEntry>(dataEntries);
        }
    }
}
