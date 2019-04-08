using System;
using System.Collections.Generic;
using System.Linq;

namespace MigrationSolution.Report
{
    public class ReportModel
    {
        public ReportModel()
        {
            RowCountColumnPairs = new Dictionary<int, Dictionary<string, string>>();
        }
        public string Title { get; set; }
        public List<string> Columns { get; set; }
        /// <summary>
        /// RowCountColumnPairs[RowCount][Column] = Value
        /// </summary>
        public Dictionary<int, Dictionary<string, string>> RowCountColumnPairs { get; set; }
        public void PutValue(int RowCount, string Column, string Value)
        {
            if (!RowCountColumnPairs.ContainsKey(RowCount))
                RowCountColumnPairs.Add(RowCount, new Dictionary<string, string>());
            if (!RowCountColumnPairs[RowCount].ContainsKey(Column))
                RowCountColumnPairs[RowCount].Add(Column, null);
            RowCountColumnPairs[RowCount][Column] = Value;
        }
        public void ExportCSV()
        {
            var report = new CsvReport();
            report.Write(Title + Environment.NewLine);
            report.Write(string.Join(",", Columns.Select(x => x.Substring(x.LastIndexOf(".") + 1)).ToList()) + Environment.NewLine);
            foreach (var rowCountColumnPair in RowCountColumnPairs)
            {
                foreach (var columnPair in rowCountColumnPair.Value)
                {
                    report.Write(columnPair.Value + ",");
                }
                report.Write(Environment.NewLine);
            }
            report.Write(Environment.NewLine);
            report.Write(Environment.NewLine);
        }
        public void ExportHTML()
        {
            var htmlReport = new HTMLReport();
            htmlReport.AddTitle(Title);
            htmlReport.AddHead(Columns.Select(x => x.Substring(x.LastIndexOf(".") + 1)).ToArray());
            htmlReport.StartTBody();
            foreach (var rowCountColumnPair in RowCountColumnPairs)
            {
                htmlReport.StartTr();
                foreach(var columnPair in rowCountColumnPair.Value)
                {
                    htmlReport.AddTd(columnPair.Value);
                }
                htmlReport.EndTr();
            }
            htmlReport.EndTBody();
            htmlReport.Finish();
        }
    }
}
