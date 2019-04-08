using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MigrationSolution
{
    class ReflectionInsertData
    {
        private static ArrayList GetAllData(object proxy, object header, string apiName)
        {
            decimal? totalPages = null;
            decimal currentPage = 1;
            var result = new ArrayList();
            while (totalPages == null || totalPages > currentPage++)
            {
                var apiMethodInfo = proxy.GetType().GetMethod(apiName, BindingFlags.Public | BindingFlags.Instance);
                var requestType = apiMethodInfo.GetParameters()[1].ParameterType;
                var requestInstance = Activator.CreateInstance(requestType);

                requestType.GetProperty("version").SetValue(requestInstance, "v31.1");

                var filterProperty = requestType.GetProperty("Response_Filter");
                var filterType = filterProperty.PropertyType;
                var filterInstance = Activator.CreateInstance(filterType);
                filterType.GetProperty("Count").SetValue(filterInstance, (decimal)999);
                filterType.GetProperty("CountSpecified").SetValue(filterInstance, true);
                filterType.GetProperty("Page").SetValue(filterInstance, currentPage);
                filterType.GetProperty("PageSpecified").SetValue(filterInstance, true);
                filterProperty.SetValue(requestInstance, filterInstance);

                object[] parametersArray = new object[] { header, requestInstance };
                var apiResult = apiMethodInfo.Invoke(proxy, parametersArray);
                var responseResultsProperty = apiResult.GetType().GetProperty("Response_Results");
                var responseResults = responseResultsProperty.GetValue(apiResult);
                if (responseResultsProperty.PropertyType.IsArray)
                {
                    var totalPagesProperty = responseResultsProperty.PropertyType.GetElementType().GetProperty("Total_Pages");
                    totalPages = (decimal)totalPagesProperty.GetValue((responseResults as Array).GetValue(0));
                }
                else
                {
                    var totalPagesProperty = responseResultsProperty.PropertyType.GetProperty("Total_Pages");
                    totalPages = (decimal)totalPagesProperty.GetValue(responseResults);
                }
                var responseDataProperty = apiResult.GetType().GetProperty("Response_Data");
                result.AddRange(responseDataProperty.GetValue(apiResult) as ICollection);
            }
            return result;
        }
    }
}
