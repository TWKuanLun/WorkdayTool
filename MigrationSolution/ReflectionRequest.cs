using log4net;
using MigrationSolution.CustomException;
using MigrationSolution.Report;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;

namespace MigrationSolution
{
    public class ReflectionRequest : IAction
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ReflectionRequest));
        private readonly QueryManager db;
        private readonly object proxy;
        private readonly object header;
        private Dictionary<string, string> addrMapping;
        private readonly string apiPrefixStr;
        private readonly string refPrefixStr;
        private readonly string mappingPrefixStr;
        private readonly string connectionString;
        private readonly string service;
        private readonly string specifyOperation;
        private readonly string specifyCondition;
        private readonly Action errorFunc;
        private readonly CustomFunction customFunc;
        public ReflectionRequest(object proxy, object header, string connectionString, string service, string specifyOperation, string specifyCondition, Action errorFunc)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new Exception("Empty connection string");
            if (string.IsNullOrEmpty(service))
                throw new Exception("Empty service");

            db = new QueryManager(connectionString, service);
            this.header = header;
            this.proxy = proxy;
            apiPrefixStr = ConfigurationManager.AppSettings["APITableNamePrefix"].ToString();
            refPrefixStr = ConfigurationManager.AppSettings["ReferenceTableNamePrefix"].ToString();
            mappingPrefixStr = ConfigurationManager.AppSettings["MappingTablePrefix"].ToString();
            this.connectionString = connectionString;
            this.service = service;
            this.specifyOperation = specifyOperation;
            this.specifyCondition = specifyCondition;
            this.errorFunc = errorFunc;
            customFunc = new CustomFunction(proxy, header);
        }
        public void Run()
        {
            var allTables = GetAllTable();

            foreach (DataRow tableRow in allTables.Rows)
            {
                var tableName = tableRow["name"].ToString();

                var apiName = tableNameToApiName(tableName, apiPrefixStr);

                SetMappingList(apiName);

                var apiMethodInfo = proxy.GetType().GetMethod(apiName, BindingFlags.Public | BindingFlags.Instance);
                var requestType = apiMethodInfo.GetParameters()[1].ParameterType;

                var columnsNameTable = db.GetColumnsName(tableName);

                #region Just Write Report and log
                var reportModel = new ReportModel();
                reportModel.Title = $"{tableName} ({specifyCondition})";
                reportModel.Columns = columnsNameTable.AsEnumerable().Select(
                        x => (name: x["COLUMN_NAME"].ToString(), type: x["DATA_TYPE"].ToString())
                    )
                    .Where(x => (x.type != "bit" && x.type != "int") &&
                        (x.name.Contains("Name") || x.name.Contains("ID") || x.name.Contains("Code"))
                    ).Take(5).Select(x => x.name).ToList();
                reportModel.Columns.Add("ErrorMessage");
                log.Warn($"{tableName} Start");
                #endregion

                var dataTable = db.GetData(tableName, specifyCondition);
                var indexSet = new HashSet<int>(Enumerable.Range(0, dataTable.Rows.Count));
                var retryCount = 0;
                var countMaxLimit = 10;
                var retryFlag = true;
                while (retryFlag && retryCount < countMaxLimit)
                {
                    var retrySet = new HashSet<int>();
                    retryFlag = false;

                    foreach(var i in indexSet)
                    {
                        var requestInstance = Activator.CreateInstance(requestType);
                        try
                        {
                            foreach (DataRow columnRow in columnsNameTable.Rows)
                            {
                                var columnNmae = columnRow["COLUMN_NAME"].ToString();
                                var columntype = columnRow["DATA_TYPE"].ToString();

                                var preProcess = PreProcessSetValue(columnNmae, dataTable.Rows[i][columnNmae]);

                                log.Info($"Set {preProcess.FullColumnName} => {dataTable.Rows[i][columnNmae].ToString()} Start");
                                SetColumnValue(preProcess.FullColumnName, preProcess.FullColumnName, preProcess.Value, requestType, requestInstance, apiName);
                                log.Info($"Set {preProcess.FullColumnName} => {dataTable.Rows[i][columnNmae].ToString()} End");

                                #region Just Write Report
                                if (reportModel.Columns.Contains(columnNmae))
                                {
                                    reportModel.PutValue(i, columnNmae, dataTable.Rows[i][columnNmae].ToString());
                                }
                                #endregion
                            }
                        }
                        catch (CustomValueNullException e)
                        {
                            log.Warn(e.Message);
                            if (retryCount == countMaxLimit - 1)
                            {
                                log.Warn($"Fail Retry more than {countMaxLimit} index: {i}");
                                errorFunc();
                                reportModel.PutValue(i, "ErrorMessage", $"Fail Retry more than {countMaxLimit}");
                            }
                            else
                            {
                                retryFlag = true;
                                retrySet.Add(i);
                            }
                            continue;
                        }

                        try
                        {
                            object[] parametersArray = new object[] { header, requestInstance };

                            log.Warn($"{apiName} Send Request index: {i}");
                            log.Debug($"{ToJson(parametersArray)}");
                            var apiResult = apiMethodInfo.Invoke(proxy, parametersArray);
                            log.Warn($"{apiName} Send Request Finish");
                        }
                        catch (Exception e)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.ResetColor();
                            errorFunc();
                            log.Warn(e.ToString());
                            reportModel.PutValue(i, "ErrorMessage", e.InnerException == null ? e.Message : e.InnerException.Message);
                        }
                    }
                    indexSet = retrySet;
                    retryCount++;
                }
                log.Info($"{tableName} End");
                reportModel.ExportCSV();
                reportModel.ExportHTML();
            }
        }

        private void SetColumnValue(string columnName, string originalColumnName, object value, Type type, object instance, string apiName)
        {
            if (value == DBNull.Value)
            {
                log.Info($"Skip Set {columnName}, Null Value");
                return;
            }
            try
            {
                log.Debug($"SetColumnValue(columnName: {columnName}, value: {value.ToString()}, type: {type.ToString()},\r\n instance: {ToJson(instance)}");

                var splitStr = columnName.Split('.');
                var property = type.GetProperty(splitStr[0]);

                if (splitStr.Length > 1)
                {
                    var nextColumnIndex = columnName.IndexOf('.') + 1;
                    var nextColumnName = columnName.Substring(nextColumnIndex);
                    if (property.GetValue(instance) == null)
                    {
                        property.SetValue(instance, Activator.CreateInstance(property.PropertyType));
                    }
                    SetColumnValue(nextColumnName, originalColumnName, value, property.PropertyType, property.GetValue(instance), apiName);
                }
                else
                {
                    if (property.PropertyType.IsArray)
                    {
                        var array = GetArrayData(originalColumnName, value.ToString(), property.PropertyType, apiName);
                        var _value = Convert.ChangeType(array, property.PropertyType);
                        log.Info($"{property.Name}.SetValue({ToJson(_value)}\r\n)");
                        log.Debug($"Before: {ToJson(instance)}");
                        property.SetValue(instance, _value);
                        log.Debug($"After: {ToJson(instance)}");
                    }
                    else
                    {
                        var _value = value;
                        if (property.PropertyType != typeof(object))
                        {
                            _value = Convert.ChangeType(value, property.PropertyType);
                        }
                        log.Info($"{property.Name}.SetValue({_value.ToString()})");
                        log.Debug($"Before: {ToJson(instance)}");
                        property.SetValue(instance, _value);
                        log.Debug($"After: {ToJson(instance)}");
                    }
                }
            }
            catch (Exception e)
            {
                if (e is CustomValueNullException)
                {
                    throw e;
                }
                Console.ForegroundColor = ConsoleColor.Red;
                log.Error(e.ToString());
                Console.ResetColor();
                errorFunc();
            }
        }
        private Array GetArrayData(string originalColumnName, string key, Type arrayType, string apiName)
        {
            try
            {
                var viewName = $"{refPrefixStr}{apiName} {originalColumnName.Replace(".", " ")}";

                if (addrMapping.ContainsValue(viewName))
                    viewName = addrMapping.Single(x => x.Value == viewName).Key;

                var columnsNameTable = db.GetColumnsName(viewName);
                var dataTable = db.GetData(viewName, $"WHERE MappingKey='{key}'");
                var elementType = arrayType.GetElementType();
                var array = Array.CreateInstance(elementType, dataTable.Rows.Count);
                var i = 0;
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    object element = Activator.CreateInstance(elementType);
                    foreach (DataRow columnNameRow in columnsNameTable.Rows)
                    {
                        var columnNmae = columnNameRow["COLUMN_NAME"].ToString();
                        if (columnNmae == "MappingKey")
                            continue;

                        var preProcess = PreProcessSetValue(columnNmae, dataRow[columnNmae]);

                        SetColumnValue(preProcess.FullColumnName, originalColumnName + " " + preProcess.FullColumnName, preProcess.Value, elementType, element, apiName);
                    }
                    log.Info($"Array insert index {i}");
                    log.Debug($"Array insert {ToJson(element)}");
                    array.SetValue(element, i);
                    i++;
                }
                return array;
            }
            catch (Exception e)
            {
                if(e is CustomValueNullException)
                {
                    throw e;
                }
                Console.ForegroundColor = ConsoleColor.Red;
                log.Error(e.ToString());
                Console.ResetColor();
                errorFunc();
                return null;
            }
        }
        private string tableNameToApiName(string viewName, string startWith)
        {
            if (viewName.StartsWith(startWith, StringComparison.Ordinal))
            {
                return viewName.Substring(viewName.IndexOf(startWith) + startWith.Length);
            }
            return null;
        }
        private string ToJson(object obj)
        {
            return "\r\n" + JsonConvert.SerializeObject(obj, Formatting.Indented);
        }
        private void SetMappingList(string apiName)
        {
            var mappingTableName = $"{mappingPrefixStr}{apiName}";
            var haveMapping = db.GetTables($"WHERE [service] = '{service}' and name = '{mappingTableName}'").Rows.Count > 0;
            //reset
            addrMapping = new Dictionary<string, string>();
            if (haveMapping)
            {
                addrMapping = db.GetData(mappingTableName, null).AsEnumerable().ToDictionary(
                //0是abbr, 1是fullname
                row => row[0].ToString(), row => row[1].ToString());
            }
        }
        private DataTable GetAllTable()
        {
            //如果沒指定Operation，則載入全部
            var condition = $"WHERE [service] = '{service}' ";
            if (string.IsNullOrEmpty(specifyOperation))
                condition += "and name like 'T[_]%'";
            else
                condition += $"and name = 'T_{specifyOperation}'";
            return db.GetTables(condition);
        }
        private (string FullColumnName, object Value) PreProcessSetValue(string columnName, object value)
        {
            #region Get Mapping
            if (columnName.StartsWith("@", StringComparison.Ordinal))
            {
                columnName = addrMapping[columnName];
            }
            #endregion
            #region Get CustomFunc Value
            var newValue = value;
            if (newValue.ToString().StartsWith("[CUSTOM_FUN]", StringComparison.Ordinal))
            {
                var customSplit = newValue.ToString().Substring(12).Split('(');
                var parameter = customSplit[1].Substring(0, customSplit[1].Length - 1).Split(',');
                var customMethodInfo = customFunc.GetType().GetMethod(customSplit[0]);

                newValue = customMethodInfo.Invoke(customFunc, parameter);
                if (newValue == null)
                {
                    throw new CustomValueNullException($"Set {value.ToString()} fail");
                }
            }
            #endregion
            return (columnName, newValue);
        }
    }
}
