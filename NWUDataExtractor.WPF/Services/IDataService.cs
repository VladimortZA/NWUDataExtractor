using NWUDataExtractor.Core.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NWUDataExtractor.WPF.Services
{
    public interface IModuleDataService
    {
        int GetMaxReportValue();
        string GetPDFLocation();
        Task<List<ModuleDataEntry>> GetModuleDataAsync(List<Module> moduleList, string inputPDF, IProgress<double> progress = null);
        void Cancel();
    }
}
