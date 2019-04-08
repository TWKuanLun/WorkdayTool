using System.Configuration;
using System.IO;
using System.Linq;

namespace MigrationSolution.Report
{
    internal class HTMLReport
    {
        public string fileFullPath { get; set; }
        private string html { get; set; }
        public HTMLReport()
        {
            fileFullPath = ConfigurationManager.AppSettings["CsvReportName"] + ".html";
            CreateFile();
            Write(@"<!DOCTYPE html><html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml""><head><meta charset=""utf-8"" /><title>Report</title><link rel=""stylesheet"" href=""https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css"" integrity=""sha384-ggOyR0iXCbMQv3Xipma34MD+dH/1fQ784/j6cY/iJTQUOhcWr7x9JvoRxT2MZw1T"" crossorigin=""anonymous""><script src=""https://code.jquery.com/jquery-3.3.1.slim.min.js"" integrity=""sha384-q8i/X+965DzO0rT7abK41JStQIAqVgRVzpbzo5smXKp4YfRvH+8abtTE1Pi6jizo"" crossorigin=""anonymous""></script><script src=""https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.7/umd/popper.min.js"" integrity=""sha384-UO2eT0CpHqdSJQ6hJty5KVphtPhzWj9WO1clHTMGa3JDZwrnQq4sF86dIHNDz0W1"" crossorigin=""anonymous""></script><script src=""https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.min.js"" integrity=""sha384-JjSmVgyd0p3pXB1rRibZUAYoIIy6OrQ6VrjIEaFf/nJGzIxFDsf4x0xIM+B07jRM"" crossorigin=""anonymous""></script></head><body><div class=""container"">");
        }
        public void Finish()
        {
            Write("</div></body></html>");
        }
        public void AddTitle(string title)
        {
            Write($"<h2>{title}</h2>");
        }
        public void AddHead(params string[] head)
        {
            Write($@"<table class=""table table-striped table-dark""><thead><tr>{string.Concat(head.Select(x => $@"<th scope=""col"">{x}</th>"))}</tr></thead>");
        }
        public void StartTBody()
        {
            Write("<tbody>");
        }
        public void StartTr()
        {
            Write("<tr>");
        }
        public void AddTd(string str)
        {
            Write($"<td>{str}</td>");
        }
        public void EndTr()
        {
            Write("</tr>");
        }
        public void EndTBody()
        {
            Write("</tbody>");
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
