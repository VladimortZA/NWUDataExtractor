using NUnit.Framework;
using NWUDataExtractor.Core;
using NWUDataExtractor.Core.Model;
using NWUDataExtractor.Test.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace NWUDataExtractor.Test
{
    public class DataExtractorTest
    {
        IDataExtractor dataExtractor;
        string pdfLocation = @"Resources\NWUSamplePDF.pdf";
        readonly List<Module> globalModules = new List<Module>() { new Module("STTN111u") };
        List<ModuleDataEntry> globalResult;

        public DataExtractorTest()
        {
            dataExtractor = new DataExtractor();
            globalResult = dataExtractor.GetModuleDataAsync(globalModules, pdfLocation).Result;
        }

        [Test]
        public async Task GetModuleDataAsync_ValidData()
        {
            List<Module> modules = new List<Module>() { new Module("STTN111u") };
            List<ModuleDataEntry> expected = new List<ModuleDataEntry>()
            {
                new ModuleDataEntry()
                {
                    ModuleCode = "STTN111U",
                    ProgrammeCode = "8DJH01",
                    CurriculumCode = "G301P",
                    Year = 2,
                    Semester = 1,
                    Faculty = "HEALTH SCIENCES",
                    ProgramName = @"Bachelor of Health Sciences with Physiology and Biochemistry(phasing in 2018) "
                },
                new ModuleDataEntry()
                {
                    ModuleCode = "STTN111U",
                    ProgrammeCode = "8DJH02",
                    CurriculumCode = "G301P",
                    Year = 2,
                    Semester = 1,
                    Faculty = "HEALTH SCIENCES",
                    ProgramName = @"Bachelor of Health Sciences with Physiology and Psychology (phasing in 2018) "
                }
            };

            var actual = await dataExtractor.GetModuleDataAsync(modules, pdfLocation);
            var sortedActual = DataListSorter.StringifyList(actual.OrderBy(o => o.ProgrammeCode));
            var sortedExptected = DataListSorter.StringifyList(expected.OrderBy(o => o.ProgrammeCode));

            CollectionAssert.AreEqual(sortedExptected, sortedActual);
        }

        [Test]
        public void GetModuleDataAsync_Count_List()
        {
            Assert.AreEqual(2, globalResult.Count);
        }

        [Test]
        public async Task GetModuleDataAsync_BadFilePath_Null()
        {
            var actual = await dataExtractor.GetModuleDataAsync(globalModules, "Bad path");

            Assert.AreEqual(null, actual);
        }

        [Test]
        public async Task GetModuleDataAsync_WrongFileType_Null()
        {
            var actual = await dataExtractor.GetModuleDataAsync(globalModules, @"Resources\WrongFile.txt");

            Assert.AreEqual(null, actual);
        }

        [Test]
        public void GetModuleDataAsync_InitSemester_1()
        {
            int actual = globalResult[0].Semester.Value;

            Assert.AreEqual(1, actual);
        }

        [Test]
        public async Task GetModuleDataAsync_NonPortioned_Empty()
        {
            var modules = new List<Module>() { new Module("STTN111") };
            var actual = await dataExtractor.GetModuleDataAsync(modules, pdfLocation);

            Assert.AreEqual(0, actual.Count);
        }

        [Test]
        public async Task GetModuleDataAsync_IncaseSensitive_True()
        {
            var modules = new List<Module>() { new Module("eCoN211B") };
            var actual = await dataExtractor.GetModuleDataAsync(modules, pdfLocation);

            Assert.AreEqual(1, actual.Count);
        }
    }
}