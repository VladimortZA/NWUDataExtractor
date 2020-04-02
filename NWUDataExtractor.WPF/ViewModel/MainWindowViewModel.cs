using NWUDataExtractor.Core;
using NWUDataExtractor.Core.DataTools;
using NWUDataExtractor.Core.Model;
using NWUDataExtractor.WPF.Extensions;
using NWUDataExtractor.WPF.Services;
using NWUDataExtractor.WPF.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NWUDataExtractor.WPF.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ModuleDataEntry> dataEntries;
        private List<ModuleDataEntry> dataEntriesList;
        private string moduleString;
        private List<Module> modules;
        private readonly IModuleDataService moduleDataService;
        private string pdfLocation = null;
        private int progressValue;
        private bool operationStarted = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel(IModuleDataService moduleDataService)
        {
            this.moduleDataService = moduleDataService;
            LocatePDFCommand = new RelayCommand(LocatePDF, CanLocatePDF);
            ExtractDataCommand = new RelayCommand(ExtractData, CanExtractData);
            GenerateCSVCommand = new RelayCommand(GenerateCSV, CanGenerateCSV);
        }

        public ICommand LocatePDFCommand { get; set; }
        public ICommand ExtractDataCommand { get; set; }
        public ICommand GenerateCSVCommand { get; set; }

        public ObservableCollection<ModuleDataEntry> DataEntries
        {
            get { return dataEntries; }
            set
            {
                if (value != dataEntries)
                {
                    dataEntries = value;
                    RaisePropertyChanged("DataEntries");
                }
            }
        }

        public int ProgressValue
        {
            get { return progressValue; }
            set
            {
                if (value != progressValue)
                {
                    progressValue = value;
                    RaisePropertyChanged("ProgressValue");
                }
            }
        }


        public string ModuleString
        {
            get { return moduleString; }
            set
            {
                if (moduleString != value)
                {
                    moduleString = value;
                    modules = ListConverter.ConvertToList(moduleString);
                    RaisePropertyChanged("ModuleString");
                }
            }
        }

        private bool CanLocatePDF(object obj)
        {
            return true;
        }

        private void LocatePDF(object obj)
        {
            pdfLocation = moduleDataService.GetPDFLocation();
        }

        private bool CanExtractData(object obj)
        {
            if (pdfLocation != null && ModuleString != string.Empty && modules != null)
                return true;
            return false;
        }

        private async void ExtractData(object obj)
        {
            Progress<double> progress = new Progress<double>();
            progress.ProgressChanged += ReportProgress;

            try
            {
                if (operationStarted)
                    moduleDataService.Cancel();
                else
                {
                    operationStarted = true;
                    dataEntriesList = await moduleDataService.GetModuleDataAsync(modules, pdfLocation, progress);
                    DataEntries = new ObservableCollection<ModuleDataEntry>(dataEntriesList);
                }
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Cancelled", "The task was sucessfully aborted.\nClick \"OK\" to continue.");
            }
            finally
            {
                operationStarted = false;
            }
        }

        private void ReportProgress(object sender, double e)
        {
            ProgressValue = (int)e;
        }

        private bool CanGenerateCSV(object obj)
        {
            if (dataEntriesList != null)
                return true;
            return false;
        }

        private void GenerateCSV(object obj)
        {
            ModuleCSVWriter.GenerateCSV(dataEntriesList);
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
