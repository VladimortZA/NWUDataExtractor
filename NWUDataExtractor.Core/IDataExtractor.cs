using NWUDataExtractor.Core.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NWUDataExtractor.Core
{
    public interface IDataExtractor
    {
        int TotalPageCount { get; }
        Task<List<ModuleDataEntry>> GetModuleDataAsync(List<Module> moduleList, string inputPDF, IProgress<double> progress = null);
    }
}