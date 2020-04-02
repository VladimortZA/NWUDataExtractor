using CsvHelper;
using Microsoft.Win32;
using NWUDataExtractor.Core.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWUDataExtractor.Core
{
    public static class ModuleCSVWriter
    {
        public static void GenerateCSV(List<ModuleDataEntry> data)
        {
            string tempFileName = Environment.CurrentDirectory + @"\temp.csv";

            using (StreamWriter writer = new StreamWriter(tempFileName))
            using (CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(data);
            }

            SaveFileDialog sfd = new SaveFileDialog()
            {
                Filter = "CSV file (*.csv)|*.csv",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if(sfd.ShowDialog() == true)
            {
                if (File.Exists(sfd.FileName))
                {
                    File.Delete(sfd.FileName);
                    File.Move(tempFileName, sfd.FileName);
                }   
            }
        }
    }
}
