using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationSolution
{
    public class GenerateCustomTable : GenerateTable, IAction
    {
        private readonly Type customType;
        public GenerateCustomTable(Type type, string connectionString, string service, string mainTableName, Action errorFunc) :
            base(connectionString, service, mainTableName, ConfigurationManager.AppSettings["CustomTableNamePrefix"].ToString(), ConfigurationManager.AppSettings["CustomReferenceTableNamePrefix"].ToString(), errorFunc)
        {
            customType = type;
        }
        public void Run()
        {
            Run(customType);
        }
    }
}
