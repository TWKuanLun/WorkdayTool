using log4net;
using System;
using System.Linq;
using WorkdayWSDL.Human_Resources;
using WorkdayWSDL.Staffing;

namespace MigrationSolution
{
    class CustomFunction
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CustomFunction));
        private readonly object proxy;
        private readonly object header;
        public CustomFunction(object proxy, object header)
        {
            this.proxy = proxy;
            this.header = header;
        }
        //System ID only can use WD-WID or WD-I
        //see https://community.boomi.com/s/question/0D51W00006As0FV/map-superior-organizations-during-a-workday-add-update-org-operation
        public string GetWIDByOrganizationReferenceID(string Organization_Reference_ID)
        {
            string wid = null;
            try
            {
                var requestInstance = new Get_Organization_Reference_IDs_RequestType()
                {
                    Item = new Organization_Reference_ID_Request_ReferencesType()
                    {
                        Organization_Reference_ID_Reference = new WorkdayWSDL.Human_Resources.OrganizationObjectType[]
                        {
                            new WorkdayWSDL.Human_Resources.OrganizationObjectType()
                            {
                                ID = new WorkdayWSDL.Human_Resources.OrganizationObjectIDType[]
                                {
                                    new WorkdayWSDL.Human_Resources.OrganizationObjectIDType(){ type = "Organization_Reference_ID", Value = Organization_Reference_ID }
                                }
                            }
                        }
                    },
                    version = "v31.1"
                };
                var proxyStrongType = proxy as Human_ResourcesPortClient;
                var headerStrongType = header as WorkdayWSDL.Human_Resources.Workday_Common_HeaderType;
                var result = proxyStrongType.Get_Organization_Reference_IDs(headerStrongType, requestInstance);
                wid = result.Response_Data[0].Organization_Reference.ID.Where(x => x.type == "WID").SingleOrDefault().Value;
            }catch(Exception e)
            {
                log.Warn(e.InnerException == null? e.Message : e.InnerException.Message);
            }
            return wid;
        }
        public string GetWIDByLocationID(string Location_ID)
        {
            string wid = null;
            try
            {
                var requestInstance = new Get_Locations_RequestType()
                {
                    Request_References = new Location_Request_ReferencesType()
                    {
                        Location_Reference = new WorkdayWSDL.Human_Resources.LocationObjectType[] {
                            new WorkdayWSDL.Human_Resources.LocationObjectType(){
                                ID = new WorkdayWSDL.Human_Resources.LocationObjectIDType[]{
                                    new WorkdayWSDL.Human_Resources.LocationObjectIDType(){
                                        type = "Location_ID",
                                        Value = Location_ID
                                    }
                                }
                            }
                        }
                    },
                    version = "v31.1"
                };
                var proxyStrongType = proxy as Human_ResourcesPortClient;
                var headerStrongType = header as WorkdayWSDL.Human_Resources.Workday_Common_HeaderType;
                var result = proxyStrongType.Get_Locations(headerStrongType, requestInstance);
                wid = result.Response_Data[0].Location_Reference.ID.Where(x => x.type == "WID").SingleOrDefault().Value;
            }
            catch (Exception e)
            {
                log.Warn(e.InnerException == null ? e.Message : e.InnerException.Message);
            }
            return wid;
        }

        //以下全都是多個選一個
        public WorkdayWSDL.Staffing.ApplicantObjectType SetApplicantReferenceByApplicantID(string Applicant_ID)
        {
            return new WorkdayWSDL.Staffing.ApplicantObjectType()
            {
                ID = new WorkdayWSDL.Staffing.ApplicantObjectIDType[] 
                {
                    new WorkdayWSDL.Staffing.ApplicantObjectIDType()
                    {
                        type = "Applicant_ID",
                        Value = Applicant_ID
                    }
                }
            };
        }
        public WorkdayWSDL.Staffing.Position_RestrictionsObjectType SetPositionReferenceByPositionID(string Position_ID)
        {
            return new WorkdayWSDL.Staffing.Position_RestrictionsObjectType()
            {
                ID = new WorkdayWSDL.Staffing.Position_RestrictionsObjectIDType[]
                {
                    new WorkdayWSDL.Staffing.Position_RestrictionsObjectIDType()
                    {
                        type = "Position_ID",
                        Value = Position_ID
                    }
                }
            };
        }
        public WorkdayWSDL.Human_Resources.Employee_ReferenceType SetEmployeeReferenceByEmployeeID(string Employee_ID)
        {
            string wid = null;
            try
            {
                var requestInstance = new WorkdayWSDL.Human_Resources.Get_Workers_RequestType()
                {
                    Request_References = new WorkdayWSDL.Human_Resources.Worker_Request_ReferencesType()
                    {
                        Worker_Reference = new WorkdayWSDL.Human_Resources.WorkerObjectType[] {
                            new WorkdayWSDL.Human_Resources.WorkerObjectType()
                            {
                                ID = new WorkdayWSDL.Human_Resources.WorkerObjectIDType[]{
                                    new WorkdayWSDL.Human_Resources.WorkerObjectIDType()
                                    {
                                        type = "Employee_ID",
                                        Value = Employee_ID
                                    }
                                }
                            }
                        }
                    },
                    version = "v31.1"
                };
                var proxyStrongType = proxy as Human_ResourcesPortClient;
                var headerStrongType = header as WorkdayWSDL.Human_Resources.Workday_Common_HeaderType;
                var result = proxyStrongType.Get_Workers(headerStrongType, requestInstance);
                wid = result.Response_Data[0].Worker_Reference.ID.Where(x => x.type == "WID").SingleOrDefault().Value;
            }
            catch (Exception e)
            {
                log.Warn(e.InnerException == null ? e.Message : e.InnerException.Message);
            }
            return new WorkdayWSDL.Human_Resources.Employee_ReferenceType() {
                Integration_ID_Reference = new WorkdayWSDL.Human_Resources.External_Integration_ID_Reference_DataType()
                {
                    ID = new WorkdayWSDL.Human_Resources.IDType()
                    {
                        System_ID = "WD-WID",
                        Value = wid
                    }
                }
            };
        }
    }
}
