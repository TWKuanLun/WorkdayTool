using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;
using WorkdayWSDL.Human_Resources;
using WorkdayWSDL.Recruiting;
using WorkdayWSDL.Staffing;
using WorkdayWSDL.Benefits_Administration;
using WorkdayWSDL.Compensation;
using WorkdayWSDL.Cash_Management;
using WorkdayWSDL.Financial_Management;

namespace WorkdayWSDL
{
    public class PortClientCreator
    {
        private string serviceStr { get; set; }
        private string username { get; set; }
        private string password { get; set; }
        private string tenantId { get; set; }
        private EndpointAddress oEndpointAddress { get; set; }
        public PortClientCreator(string serviceStr, string tenantId = "敏感資訊消除", string username = "敏感資訊消除", string password = "敏感資訊消除")
        {
            this.serviceStr = serviceStr;
            this.tenantId = tenantId; //part of url after login.
            this.username = username;//username@tenantID
            this.password = password;
            string entPointAddress = $"https://wd3-impl-services1.workday.com/ccx/service/{tenantId}/{serviceStr}";
            oEndpointAddress = new EndpointAddress(new Uri(entPointAddress));
        }
        public (object client, object header) CreateService()
        {
            var oCommonHeader = Activator.CreateInstance(Type.GetType($"WorkdayWSDL.{serviceStr}.Workday_Common_HeaderType, WorkdayWSDL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"));

            //var proxy = new RecruitingPortClient(GetCustomBinding(), oEndpointAddress);
            //var proxy = new Human_ResourcesPortClient(GetCustomBinding(), oEndpointAddress);
            var proxyArgs = new object[] { GetCustomBinding(), oEndpointAddress };
            var proxy = Activator.CreateInstance(Type.GetType($"WorkdayWSDL.{serviceStr}.{serviceStr}PortClient, WorkdayWSDL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"), proxyArgs);

            //proxy.ClientCredentials.UserName.UserName = username;
            //proxy.ClientCredentials.UserName.Password = password;
            var clientCredentials = proxy.GetType().GetProperty("ClientCredentials").GetValue(proxy);
            var clientCredentials_UserName = clientCredentials.GetType().GetProperty("UserName").GetValue(clientCredentials);
            clientCredentials_UserName.GetType().GetProperty("UserName").SetValue(clientCredentials_UserName, $"{username}@{tenantId}");
            clientCredentials_UserName.GetType().GetProperty("Password").SetValue(clientCredentials_UserName, password);
            return (proxy, oCommonHeader);
        }
        public (TPortClient client, THeader header) CreateService<TPortClient, THeader>()
        {
            var service = CreateService();
            return ((TPortClient)service.client, (THeader)service.header);
        }

        private Binding GetCustomBinding(double timeoutMins = 2)
        {
            SecurityBindingElement oSecBinding = SecurityBindingElement.CreateUserNameOverTransportBindingElement();
            oSecBinding.IncludeTimestamp = false;
            const int iIntLimit = Int32.MaxValue;

            var timeout = TimeSpan.FromMinutes(timeoutMins);

            var oCustBinding = new CustomBinding(
                oSecBinding,
                new TextMessageEncodingBindingElement(MessageVersion.Soap11, Encoding.UTF8)
                {
                    ReaderQuotas = new XmlDictionaryReaderQuotas
                    {
                        MaxDepth = iIntLimit,
                        MaxStringContentLength = iIntLimit,
                        MaxArrayLength = iIntLimit,
                        MaxBytesPerRead = iIntLimit,
                        MaxNameTableCharCount = iIntLimit
                    }
                },
                new HttpsTransportBindingElement
                {
                    MaxBufferPoolSize = iIntLimit,
                    MaxReceivedMessageSize = iIntLimit,
                    MaxBufferSize = iIntLimit,
                    Realm = string.Empty
                })
            {
                SendTimeout = timeout,
                ReceiveTimeout = timeout
            };
            return oCustBinding;
        }
    }
}
