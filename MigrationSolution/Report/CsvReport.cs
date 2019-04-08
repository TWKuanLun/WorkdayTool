using System.Configuration;
using System.IO;

namespace MigrationSolution.Report
{
    internal class CsvReport
    {
        public string fileFullPath { get; set; }
        public CsvReport()
        {
            fileFullPath = ConfigurationManager.AppSettings["CsvReportName"] + ".csv";
            CreateFile();
        }
        public void Write(string content)
        {
            if (File.Exists(fileFullPath))
            {
                File.AppendAllText(fileFullPath, content);
            }
            else
            {
                File.WriteAllText(fileFullPath, content);
            }
        }
        private void CreateFile()
        {
            FileInfo fh = new FileInfo(fileFullPath);
            StreamWriter sw = fh.CreateText();
            sw.WriteLine("");
            sw.Flush();
            sw.Close();
        }
    }
}
