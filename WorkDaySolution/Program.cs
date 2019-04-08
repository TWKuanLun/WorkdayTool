using log4net;
using log4net.Config;
using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using WorkdayWSDL;
using MigrationSolution;
using System.Configuration;
using System.Reflection;
using log4net.Core;
using System.Linq;
using System.Data;

namespace WorkDaySolution
{
    public class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));
        public static int Main(string[] args)
        {
            XmlConfigurator.Configure(new System.IO.FileInfo("log4net.config"));

            log.Info($"Args: {string.Join(",", args)}");

            var errorCount = 0;
            Action errorFunc = () => errorCount++;

            try
            {
                var tenantId = args.Length > 0 ? args[0] : ConfigurationManager.AppSettings["TenantID"].ToString();
                var username = args.Length > 1 ? args[1] : ConfigurationManager.AppSettings["UserName"].ToString();
                var password = args.Length > 2 ? args[2] : ConfigurationManager.AppSettings["Password"].ToString();
                var runtimeLogRootLevel = args.Length > 3 ? args[3] : null;
                var connectionString = args.Length > 5 ? args[5] : ConfigurationManager.AppSettings["DBConnectionString"].ToString();
                var serviceString = args.Length > 6 ? args[6] : ConfigurationManager.AppSettings["Service"].ToString();
                var specifyOperation = args.Length > 7 ? (args[7] == "All" ? "" : args[7]) : ConfigurationManager.AppSettings["SpecifyOperation"].ToString();
                var validateOnly = args.Length > 8 ? args[8] : ConfigurationManager.AppSettings["ValidateOnly"].ToString();
                var specifyCondition = args.Length > 9 ? args[9] : ConfigurationManager.AppSettings["SpecifyCondition"].ToString();

                if (runtimeLogRootLevel != null)
                {
                    var rootLevel = typeof(Level).GetFields(BindingFlags.Public | BindingFlags.Static).First(x => x.Name == runtimeLogRootLevel).GetValue(null);
                    ((log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository()).Root.Level = (Level)rootLevel;
                    ((log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository()).RaiseConfigurationChanged(EventArgs.Empty);
                }

                var service = new PortClientCreator(serviceString, tenantId, username, password).CreateService();

                using (OperationContextScope scope = new OperationContextScope((IContextChannel)service.client.GetType().GetProperty("InnerChannel").GetValue(service.client)))
                {
                    var httpRequestProperty = new HttpRequestMessageProperty();
                    httpRequestProperty.Headers["X-Validate-Only"] = validateOnly;
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;

                    IAction action = null;
                    if (args.Length > 4)
                    {
                        switch (args[4])
                        {
                            case "SendRequest":
                                action = new ReflectionRequest(service.client, service.header,
                                    connectionString, serviceString, specifyOperation, specifyCondition, errorFunc);
                                break;
                            case "GenerateTable":
                                action = new GenerateOperationTable(service.client, connectionString, serviceString, 
                                    specifyOperation, errorFunc, OperationType.Request);
                                break;
                            case "GenerateResponseTable":
                                action = new GenerateOperationTable(service.client, connectionString, serviceString, 
                                    specifyOperation, errorFunc, OperationType.Response);
                                break;
                        }
                    }
                    if (action != null)
                    {
                        log.Warn($"====={action.GetType().ToString()} Start=====");
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        action.Run();
                        sw.Stop();
                        log.Warn($"====={action.GetType().ToString()} End=====");
                        log.Warn($"It took {sw.ElapsedMilliseconds} ms");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex);
                errorFunc();
            }
            if(args.Length == 0)
            {
                //debug mode
                Console.WriteLine("Press any key to exit.");
                Console.ReadLine();
            }
            if (errorCount > 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}
