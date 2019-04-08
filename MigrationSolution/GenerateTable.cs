using log4net;
using MigrationSolution.CustomException;
using StringExtensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace MigrationSolution
{
    public abstract class GenerateTable
    {
        protected static readonly ILog log = LogManager.GetLogger(typeof(GenerateTable));
        protected readonly QueryManager db;
        protected readonly string rootPrefix;
        protected readonly string refPrefix;
        protected readonly string mappingPrefixStr;
        protected readonly string connectionString;
        protected readonly string service;
        protected readonly string mainTableName;
        protected readonly Action errorFunc;
        protected GenerateTable(string connectionString, string service, string mainTableName, string rootPrefix, string refPrefix, Action errorFunc)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new Exception("Empty connection string");
            if (string.IsNullOrEmpty(service))
                throw new Exception("Empty service");
            if (string.IsNullOrEmpty(mainTableName))
                throw new Exception("Empty specify operation");

            db = new QueryManager(connectionString, service);
            this.rootPrefix = rootPrefix;
            this.refPrefix = refPrefix;
            mappingPrefixStr = ConfigurationManager.AppSettings["MappingTablePrefix"].ToString();
            this.service = service;
            this.connectionString = connectionString;
            this.mainTableName = mainTableName;
            this.errorFunc = errorFunc;
        }
        protected void Run(Type type)
        {
            //Add schema
            db.ExecuteNonQuery($"IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = '{service}') BEGIN EXEC('CREATE SCHEMA {service}') END");

            var columnNameLists = new Dictionary<string, List<(string columnNmae, Type columnType)>>();
            process(type, "", $"{rootPrefix}{mainTableName}", columnNameLists);

            ExecuteSQL(columnNameLists);
        }
        private void process(Type type, string nodeString, string tableName, Dictionary<string, List<(string, Type)>> columnNameLists)
        {
            if (!columnNameLists.ContainsKey(tableName))
                columnNameLists.Add(tableName, new List<(string, Type)>());

            var properties = type.GetProperties();

            foreach (var property in properties)
            {
                var newNodeString = nodeString + property.Name;
                if (property.PropertyType.IsValueType || property.PropertyType == typeof(string) || property.PropertyType == typeof(object))
                {
                    columnNameLists[tableName].Add((newNodeString, property.PropertyType));
                }
                else if (property.PropertyType.IsArray)
                {
                    columnNameLists[tableName].Add((newNodeString, property.PropertyType));
                    process(property.PropertyType.GetElementType(), "", $"{tableName.Replace(rootPrefix, refPrefix)} {newNodeString.Replace(".", " ")}", columnNameLists);
                }
                else
                {
                    process(property.PropertyType, $"{newNodeString}.", tableName, columnNameLists);
                }
            }
        }
        
        private string GetSQLType(Type type)
        {
            if (type.IsArray)
            {
                return "nvarchar(max)";
            }
            else if (type == typeof(int))
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
        private string AbbreviationStrategy(string original, char splitChar , Dictionary<string, string> abbrMapping)
        {
            var clone = original;
            var stringlimit = 4;
            while (clone.Length > 127 && stringlimit > 0)
            {
                clone = original.ToAbbr(splitChar, stringlimit);
                stringlimit--;
            }
            try
            {
                if (clone.Length > 127)
                    throw new LengthMoreThan127Exception();
                if (stringlimit < 4)
                    abbrMapping.Add(clone, original);
            }
            catch (Exception e)
            {
                if(e is ArgumentException || e is LengthMoreThan127Exception)
                {
                    stringlimit = 4;
                    clone = original;
                    while (clone.Length > 127 && stringlimit > 0)
                    {
                        clone = original.ToAbbrFromEnd(splitChar, stringlimit);
                        stringlimit--;
                    }
                    try
                    {
                        if (clone.Length > 127)
                            throw new LengthMoreThan127Exception();
                        if (stringlimit < 4)
                        {

                            abbrMapping.Add(clone, original);

                        }
                    }
                    catch (Exception e2)
                    {
                        log.Error(e2.ToString());
                        throw new Exception("從前面縮從後面縮都重複");
                    }
                }
                else
                {
                    log.Error(e.ToString());
                }
            }
            return clone;
        }
        private void ExecuteSQL(Dictionary<string, List<(string columnNmae, Type columnType)>> columnNameLists)
        {
            var abbrMapping = new Dictionary<string, string>();
            try
            {
                foreach (var columnNameList in columnNameLists)
                {

                    var tableName = AbbreviationStrategy(columnNameList.Key, ' ', abbrMapping);

                    StringBuilder createTableSQL = new StringBuilder($"CREATE TABLE [{service}].[{tableName}] (");
                    if (columnNameList.Key.StartsWith(refPrefix) || columnNameList.Key.StartsWith($"@{refPrefix}"))
                    {
                        createTableSQL.Append($"[MappingKey] nvarchar(max) NOT NULL, ");
                    }
                    foreach (var tuple in columnNameList.Value)
                    {
                        var cloneColumnNmae = AbbreviationStrategy(tuple.columnNmae, '.', abbrMapping);

                        createTableSQL.Append($"[{cloneColumnNmae}] {GetSQLType(tuple.columnType)}, ");
                    }
                    createTableSQL.Remove(createTableSQL.Length - 2, 2);
                    createTableSQL.Append(");");
                    db.ExecuteNonQuery(createTableSQL.ToString());
                }

                //產生mapping表
                if (abbrMapping.Count > 0)
                {
                    var createMappingTableSQL = $"CREATE TABLE [{service}].[{mappingPrefixStr}{mainTableName}] ([Abbreviation] nvarchar(128) COLLATE Chinese_Taiwan_Stroke_CS_AI NOT NULL PRIMARY KEY, [FullString] nvarchar(max) NOT NULL);";
                    db.ExecuteNonQuery(createMappingTableSQL.ToString());
                    var insertMappingTableSQL = new StringBuilder($"INSERT INTO [{service}].[{mappingPrefixStr}{mainTableName}] ([Abbreviation], [FullString]) VALUES ");
                    foreach (var mappingPair in abbrMapping)
                    {
                        insertMappingTableSQL.Append($"('{mappingPair.Key}', '{mappingPair.Value}'), ");
                    }
                    insertMappingTableSQL = insertMappingTableSQL.Remove(insertMappingTableSQL.Length - 2, 2);
                    insertMappingTableSQL.Append(";");
                    db.ExecuteNonQuery(insertMappingTableSQL.ToString());
                }
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                errorFunc();
            }
        }
    }
}
