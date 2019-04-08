using log4net;
using System;
using System.Data;
using System.Data.SqlClient;

namespace MigrationSolution
{
    public class QueryManager
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(QueryManager));
        private readonly string service;
        private readonly string connectionString;
        public QueryManager(string connectionString, string service)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new Exception("Empty connection string");
            if (string.IsNullOrEmpty(service))
                throw new Exception("Empty service");

            this.service = service;
            this.connectionString = connectionString;
        }
        public DataTable DBQuery(string query)
        {
            var result = new DataTable();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlDataAdapter data = new SqlDataAdapter(query, connection))
                {
                    data.Fill(result);
                }
            }
            return result;
        }
        public DataTable GetViews(string condition)
        {
            return DBQuery($"SELECT * FROM (SELECT sys.schemas.name as [service], sys.views.* FROM sys.views left join sys.schemas on sys.views.schema_id = sys.schemas.schema_id) v {condition}");
        }
        public DataTable GetTables(string condition)
        {
            return DBQuery($"SELECT * FROM (SELECT sys.schemas.name as [service], sys.tables.* FROM sys.tables left join sys.schemas on sys.tables.schema_id = sys.schemas.schema_id) t {condition}");
        }
        public DataTable GetColumnsName(string viewName)
        {
            return DBQuery($"SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='{service}' and TABLE_NAME = '{viewName}'");
        }
        public DataTable GetData(string viewName, string condition)
        {
            return DBQuery($"SELECT * FROM [{service}].[{viewName}] {condition}");
        }
        public int ExecuteNonQuery(string query)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    return command.ExecuteNonQuery();
                }
            }
        }
        public int ExecuteNonQuerySP(string query)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = query;
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = connection.ConnectionTimeout;
                    return command.ExecuteNonQuery();
                }
            }
        }
    }
}
