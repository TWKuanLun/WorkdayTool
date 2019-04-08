using System;
using System.IO;
using System.Threading.Tasks;

namespace XsdToDataSetTool
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var tenantId = args[0];
            var userName = args[1];
            var password = args[2];
            var reportName = args[3];
            var xsdExePath = args[4];
            var outputPath = "..\\..\\..\\ExtractSolution\\Model";
            var @namespace = "ExtractSolution.Model";
            Directory.CreateDirectory("..\\..\\..\\ExtractSolution\\Model");
            var tool = new XsdToDataSetClassTool(tenantId, userName, password, reportName, xsdExePath, outputPath, @namespace);
            await tool.Run();
            tool.ChangeCsproj();
        }
    }
}
