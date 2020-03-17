using Microsoft.Win32;
using NWUDataExtractor.Core;
using NWUDataExtractor.Core.Model;
using System;
using System.Collections.Generic;
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
        private int numberOfPages;

        public ModuleDataService(IDataExtractor dataExtractor)
        {
            this.dataExtractor = dataExtractor;
        }

        public int SourceNumberOfPages
        {
            get { return numberOfPages; }
            set
            {
                if(value != numberOfPages)
                    numberOfPages = value;
            }
        }

        public async Task<List<ModuleDataEntry>> GetModuleDataAsync(List<Module> moduleList, string inputPDF, IProgress<double> progress)
        {
            return await dataExtractor.GetModuleDataAsync(moduleList, inputPDF, progress);
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
    }
}
