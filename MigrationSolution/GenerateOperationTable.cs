using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MigrationSolution
{
    public class GenerateOperationTable : GenerateTable, IAction
    {
        private readonly object proxy;
        private readonly OperationType operationType;
        public GenerateOperationTable(object proxy, string connectionString, string service, string specifyOperation, Action errorFunc, OperationType operationType) :
            base(connectionString, service, specifyOperation, ConfigurationManager.AppSettings["APITableNamePrefix"].ToString(), ConfigurationManager.AppSettings["ReferenceTableNamePrefix"].ToString(), errorFunc)
        {
            this.proxy = proxy;
            this.operationType = operationType;
        }
        public void Run()
        {
            MethodInfo apiMethodInfo = null;
            Type requestType = null;
            switch (operationType)
            {
                case OperationType.Request:
                    apiMethodInfo = proxy.GetType().GetMethod(mainTableName, BindingFlags.Public | BindingFlags.Instance);
                    requestType = apiMethodInfo.GetParameters()[1].ParameterType;
                    break;
                case OperationType.Response:
                    apiMethodInfo = proxy.GetType().GetMethod(mainTableName, BindingFlags.Public | BindingFlags.Instance);
                    requestType = apiMethodInfo.ReturnType.GetProperty("Response_Data").PropertyType.GetElementType();
                    break;
            }
            Run(requestType);
        }
    }
    public enum OperationType
    {
        Request,
        Response
    }
}
