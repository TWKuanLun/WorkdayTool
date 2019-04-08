using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using ExtractSolution.Model;

namespace ExtractSolution
{
    class ExtractReportToDB
    {
        private readonly string TenantID;
        private readonly string UserName;
        private readonly string Password;
        private readonly string ReportName;
        private readonly string ConnectionString;
        private readonly string ReportArgsString;
        public ExtractReportToDB(string tenantID, string username, string password, string reportName, string connectionString, string reportArgsString)
        {
            TenantID = tenantID;
            UserName = username;
            Password = password;
            ReportName = reportName;
            ConnectionString = connectionString;
            ReportArgsString = reportArgsString;
        }
        public void Run()
        {
            try
            {
                //Download XML Report
                using (WebClient client = new WebClient())
                {
                    client.Credentials = new NetworkCredential(UserName, Password);
                    client.DownloadFile($"https://wd3-impl-services1.workday.com/ccx/service/customreport2/{TenantID}/wd-support/{ReportName}?{ReportArgsString}format=simplexml", $"{ReportName}.xml");
                }

                //var dataSet = new Extract_Supervisory_OrganizationsDataSet();
                var dataSetType = Type.GetType($"ExtractSolution.Model.{ReportName}DataSet");
                var dataSet = Activator.CreateInstance(dataSetType);
                //dataSet.ReadXml($"{reportName}.xml", XmlReadMode.ReadSchema);
                var readXmlMethods = dataSetType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                var readXmlMethod = readXmlMethods.Where(x => x.Name == "ReadXml").Single(x => {
                    var parameters = x.GetParameters();
                    return parameters.Length == 2 && parameters.Any(y => y.Name == "fileName") && parameters.Any(y => y.Name == "mode");
                });

                object[] parametersArray = new object[] { $"{ReportName}.xml", XmlReadMode.ReadSchema };
                readXmlMethod.Invoke(dataSet, parametersArray);

                //var columns = dataSet.Report_Entry.Columns;
                var reportEntryProperty = dataSetType.GetProperty("Report_Entry");
                var reportEntry = reportEntryProperty.GetValue(dataSet);
                var columnsProperty = reportEntryProperty.PropertyType.GetProperty("Columns");
                var columns = columnsProperty.GetValue(reportEntry) as DataColumnCollection;
                #region Check Table Exist 
                var tables = new DataTable();
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (SqlDataAdapter data = new SqlDataAdapter($"SELECT name FROM sys.tables WHERE name = '{ReportName}'", connection))
                    {
                        data.Fill(tables);
                    }
                }
                if (tables.Rows.Count > 0)
                {
                    using (var connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand($"DROP TABLE [{ReportName}]", connection))
                        {
                            var result = command.ExecuteNonQuery();
                        }
                    }
                }
                #endregion

                #region Create Table
                StringBuilder createTableSQL = new StringBuilder($"CREATE TABLE [{ReportName}] (");
                foreach (DataColumn column in columns)
                {
                    //var name = dataSet.Report_Entry.Availability_DateColumn.ColumnName;
                    //var type = dataSet.Report_Entry.Availability_DateColumn.DataType;
                    createTableSQL.Append($"[{column.ColumnName}] {GetSQLType(column.DataType)}, ");
                }
                createTableSQL.Remove(createTableSQL.Length - 2, 2);
                createTableSQL.Append(");");
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(createTableSQL.ToString(), connection))
                    {
                        var result = command.ExecuteNonQuery();
                    }
                }
                #endregion

                #region Insert Data

                var rowsProperty = reportEntryProperty.PropertyType.GetProperty("Rows");
                var rows = rowsProperty.GetValue(reportEntry) as DataRowCollection;
                //SQL "INSERT INTO" once insert limit 1000 rows
                var _1000Count = Math.Ceiling((double)rows.Count / 1000);

                for (int i = 0; i < _1000Count; i++)
                {
                    StringBuilder insertDataSQL = new StringBuilder($"INSERT INTO [{ReportName}] (");
                    foreach (DataColumn column in columns)
                    {
                        insertDataSQL.Append($"[{column.ColumnName}], ");
                    }
                    insertDataSQL.Remove(insertDataSQL.Length - 2, 2);
                    insertDataSQL.Append(") VALUES ");

                    for (int j = 0; j < 1000; j++)
                    {
                        var index = i * 1000 + j;
                        if (index >= rows.Count)
                            break;
                        insertDataSQL.Append($"(");
                        foreach (DataColumn column in columns)
                        {
                            insertDataSQL.Append($"{GetValueByType(rows[j][column], column.DataType)}, ");
                        }
                        insertDataSQL.Remove(insertDataSQL.Length - 2, 2);
                        insertDataSQL.Append($"), ");
                    }
                    insertDataSQL.Remove(insertDataSQL.Length - 2, 2);

                    using (var connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand(insertDataSQL.ToString(), connection))
                        {
                            var result = command.ExecuteNonQuery();
                        }
                    }
                }
                #endregion
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private string GetSQLType(Type type)
        {
            if (type == typeof(int) || type == typeof(decimal))
            {
                return "int";
            }
            else if (type == typeof(DateTime))
            {
                return "datetime";
            }
            else if (type == typeof(bool))
            {
                return "bit";
            }
            else
            {
                return "nvarchar(max)";
            }
        }
        private string GetValueByType(object value, Type type)
        {
            if (type == typeof(int) || type == typeof(decimal))
            {
                return value.ToString();
            }
            else if (type == typeof(DateTime))
            {
                return $"'{Convert.ToDateTime(value).ToString("yyyy-MM-dd HH:mm:ss.fff")}'";
            }
            else if (type == typeof(bool))
            {
                return $"'{value}'";
            }
            else
            {
                return $"'{value.ToString().Replace("'", "''")}'";
            }
        }
    }
}
