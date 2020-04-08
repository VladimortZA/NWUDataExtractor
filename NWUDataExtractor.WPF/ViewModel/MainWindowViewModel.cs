using NWUDataExtractor.Core;
using NWUDataExtractor.Core.DataTools;
using NWUDataExtractor.Core.Model;
using NWUDataExtractor.WPF.Services;
using NWUDataExtractor.WPF.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace NWUDataExtractor.WPF.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private List<ModuleDataEntry> dataEntriesList;
        private List<Module> modules;
        private List<Module> moduleErrors;
        private readonly IModuleDataService moduleDataService;
        private string pdfLocation = null;

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

        private ObservableCollection<ModuleDataEntry> dataEntries;
        public ObservableCollection<ModuleDataEntry> DataEntries
        {
            get => dataEntries;
            set
            {
                if (value != dataEntries)
                {
                    dataEntries = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int progressValue;
        public int ProgressValue
        {
            get => progressValue;
            set
            {
                if (value != progressValue)
                {
                    progressValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string moduleString;
        public string ModuleString
        {
            get => moduleString;
            set
            {
                if (moduleString != value)
                {
                    moduleString = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool operationStarted;
        public bool OperationStarted
        {
            get { return operationStarted; }
            set
            {
                if(value != operationStarted)
                {
                    operationStarted = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool hasErrors;
        public bool HasErrors
        {
            get { return hasErrors; }
            set
            {
                if (hasErrors != value)
                {
                    hasErrors = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string errorMessage;
        public string ErrorMessage
        {
            get { return errorMessage; }
            set
            {
                if(value != errorMessage)
                {
                    errorMessage = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool CanLocatePDF(object obj)
        {
            if (operationStarted)
                return false;
            return true;
        }

        private void LocatePDF(object obj)
        {
            pdfLocation = moduleDataService.GetPDFLocation();
        }

        private bool CanExtractData(object obj)
        {
            if (pdfLocation != null && ModuleString != string.Empty)
                return true;
            return false;
        }

        private async void ExtractData(object obj)
        {
            Progress<double> progress = new Progress<double>();
            modules = ListConverter.ConvertToList(moduleString);
            progress.ProgressChanged += ReportProgress;

            if (!ValidateModuleList())
            {
                try
                {
                    if (operationStarted)
                        moduleDataService.Cancel();
                    else
                    {
                        OperationStarted = true;
                        dataEntriesList = await moduleDataService.GetModuleDataAsync(modules, pdfLocation, progress);
                        DataEntries = new ObservableCollection<ModuleDataEntry>(dataEntriesList);
                    }
                }
                catch (OperationCanceledException)
                {
                    MessageBox.Show("The operation was sucessfully aborted.", "Cancelled", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    dataEntriesList = null;
                    ProgressValue = 0;
                }
                finally
                {
                    OperationStarted = false;
                }
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
            try
            {
                HasErrors = false;
                ModuleCSVWriter.GenerateCSV(dataEntriesList);
            }
            catch (IOException)
            {
                HasErrors = true;
                ErrorMessage = "Unable to overwrite the file.\n" +
                    "Please ensure that the file is not being used by another application.";
            }
        }

        private void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool ValidateModuleList()
        {
            StringBuilder sb = new StringBuilder();
            moduleErrors = new List<Module>();
            foreach (var module in modules)
            {
                if (!Regex.IsMatch(module.Code, @"[a-zA-Z]{4}\d{3}[a-zA-Z]?$", RegexOptions.IgnoreCase))
                {
                    moduleErrors.Add(module);
                    sb.AppendLine(module.Code);
                }
            }

            HasErrors = (moduleErrors.Count == 0) ? false : true;

            ErrorMessage = "Some values did not match the standard module format.\n" +
                    "The format (case-insensitive) starts with 4 characters" +
                    " followed by 3 digits and an optional character.\n" +
                    "Example: BMAN111 or STTN211u\n\n" +
                    "Please rectify the following items:\n" + sb.ToString();

            return HasErrors;
        }
    }
}
