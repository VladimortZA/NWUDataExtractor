using Microsoft.Win32;
using NWUDataExtractor.Core;
using NWUDataExtractor.Core.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NWUDataExtractor.WPF.Services
{
    public class ModuleDataService : IModuleDataService
    {
        private readonly IDataExtractor dataExtractor;

        public ModuleDataService(IDataExtractor dataExtractor)
        {
            this.dataExtractor = dataExtractor;
        }

        public async Task<List<ModuleDataEntry>> GetModuleDataAsync(List<Module> moduleList, string inputPDF, IProgress<double> progress)
        {
            try
            {
                return await Task.Run(() => dataExtractor.GetModuleDataAsync(moduleList, inputPDF, progress));
            }
            catch(OperationCanceledException)
            {
                throw;
            }
        }

        public string GetPDFLocation()
        {
            string filePath = null;

            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                InitialDirectory = Directory.GetCurrentDirectory()
            };

            if (ofd.ShowDialog() == true)
            {
                filePath = ofd.FileName;
            }

            return filePath;
        }

        public int GetMaxReportValue()
        {
            return dataExtractor.TotalPageCount;
        }

        public void Cancel()
        {
            dataExtractor.Cancel();
        }
    }
}
