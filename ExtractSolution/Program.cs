namespace ExtractSolution
{
    class Program
    {
        static void Main(string[] args)
        {
            var tenantId = args[0];
            var userName = args[1];
            var password = args[2];
            var reportName = args[3];
            var connectionString = args[4];
            var reportArgsString = args[5];
            var extractReportToDB = new ExtractReportToDB(tenantId, userName, password, reportName, connectionString, reportArgsString);
            extractReportToDB.Run();
        }
    }
}
