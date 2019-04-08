using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using WorkdayWSDL.Human_Resources;

namespace WorkdayWSDL
{
    class TestSample
    {
        private static void Main(string[] args)
        {
            var creator = new PortClientCreator("Human_Resources");
            var service = creator.CreateService<Human_ResourcesPortClient, Workday_Common_HeaderType>();
            var version = "v31.1";
            #region Put Location Sample
            Put_Location_RequestType put_Location_Request = new Put_Location_RequestType();
            put_Location_Request.version = version;
            put_Location_Request.Add_Only = false;
            put_Location_Request.Location_Data = new Location_iDataType();
            put_Location_Request.Location_Data.Location_ID = "ARG01";
            put_Location_Request.Location_Data.Location_Name = "Trend Argentina Office";
            put_Location_Request.Location_Data.Location_Usage_Reference = new Location_UsageObjectType[] {
                new Location_UsageObjectType(){ ID = new Location_UsageObjectIDType[]{
                    new Location_UsageObjectIDType(){ type = "Location_Usage_ID", Value = "BUSINESS SITE" }
                } }
            };
            put_Location_Request.Location_Data.Time_Profile_Reference = new Time_ProfileObjectType();
            put_Location_Request.Location_Data.Time_Profile_Reference.ID = new Time_ProfileObjectIDType[] {
                new Time_ProfileObjectIDType(){ type="Time_Profile_ID",  Value = "STANDARD_HOURS_40" }
            };
            put_Location_Request.Location_Data.Contact_Data = new Contact_Information_DataType();
            put_Location_Request.Location_Data.Contact_Data.Address_Data = new Address_Information_DataType[] {
                new Address_Information_DataType(){
                    Formatted_Address = "0",
                    Address_Format_Type = "Basic",
                    Defaulted_Business_Site_Address = false,
                    Country_Reference = new CountryObjectType(){ ID =new CountryObjectIDType[]{
                        new CountryObjectIDType(){ type = "ISO_3166-1_Alpha-3_Code", Value = "ARG" }
                    } },
                    Address_Line_Data = new Address_Line_Information_DataType[]{
                        new Address_Line_Information_DataType(){
                            Type = "ADDRESS_LINE_1",
                            Value = "Viamonte 1646 Piso 8 Oficina 62    ",
                            Descriptor = "Street Name"
                        }
                    },
                    Municipality = "Buenos Aires",
                    Postal_Code = "C1055ABF",
                    Usage_Data = new Communication_Method_Usage_Information_DataType[]{
                        new Communication_Method_Usage_Information_DataType()
                        {
                            Public = true,
                            Type_Data = new Communication_Usage_Type_DataType[]{
                                new Communication_Usage_Type_DataType(){
                                    Primary = true,
                                    Type_Reference = new Communication_Usage_TypeObjectType(){
                                        ID = new Communication_Usage_TypeObjectIDType[]{
                                            new Communication_Usage_TypeObjectIDType(){
                                                type = "Communication_Usage_Type_ID",
                                                Value = "BUSINESS"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            var Put_Location = service.client.Put_Location(service.header, put_Location_Request);
            #endregion
            #region Put Sample1
            Put_Job_Family_Group_RequestType put_Job_Family_Group_Request = new Put_Job_Family_Group_RequestType();
            put_Job_Family_Group_Request.version = version;
            put_Job_Family_Group_Request.Add_Only = false;
            put_Job_Family_Group_Request.Job_Family_Group_Reference = new Job_FamilyObjectType();
            put_Job_Family_Group_Request.Job_Family_Group_Reference.Descriptor = "?";
            put_Job_Family_Group_Request.Job_Family_Group_Reference.ID = new Job_FamilyObjectIDType[] {
                new Job_FamilyObjectIDType() { type = "WID", Value = "test$#@!#!" },
                new Job_FamilyObjectIDType() { type = "WID", Value = "13123" }
            };
            put_Job_Family_Group_Request.Job_Family_Group_Data = new Job_Family_Group_DataType();
            put_Job_Family_Group_Request.Job_Family_Group_Data.ID = "TM_IS";
            put_Job_Family_Group_Request.Job_Family_Group_Data.Effective_Date = DateTime.Parse("2018-12-25T00:00:00.000-08:00");
            put_Job_Family_Group_Request.Job_Family_Group_Data.Name = "IS (TM WD Tester)";
            put_Job_Family_Group_Request.Job_Family_Group_Data.Summary = "For Testing -- IS Job Family Group";
            put_Job_Family_Group_Request.Job_Family_Group_Data.Inactive = false;
            var PutJob_Family_Groups = service.client.Put_Job_Family_Group(service.header, put_Job_Family_Group_Request);
            #endregion
            #region Put Sample2
            Put_Job_Family_RequestType put_Job_Family_Request = new Put_Job_Family_RequestType()
            {
                version = version,
                Add_Only = false,
                Job_Family_Data = new Job_Family_DataType()
                {
                    ID = "IS-BA_Engineer(Test)",
                    Effective_Date = DateTime.Parse("2018-12-25T00:00:00.000-08:00"),
                    Name = "IS-BA_Engineer(Test)",
                    Summary = "IS-BA_Engineer",
                    Inactive = true
                }
            };
            var PutJob_Family = service.client.Put_Job_Family(service.header, put_Job_Family_Request);
            //只驗證，不改資料的用法
            using (OperationContextScope scope = new OperationContextScope(service.client.InnerChannel))
            {
                var httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers["X-Validate-Only"] = "1";
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;

                var PutJob_Family2 = service.client.Put_Job_Family(service.header, put_Job_Family_Request);
            }
            #endregion
            #region Location Hierachy Put Sample
            var org = new Organization_Add_UpdateType();
            org.version = version;
            org.Organization_Data = new Organization_DataType();
            org.Organization_Data.Effective_Date = new DateTime(2000, 1, 1);
            org.Organization_Data.Organization_Reference_ID = "LOC_H_GLOBAL";
            org.Organization_Data.Organization_Name = "GLOBAL";
            org.Organization_Data.Organization_Code = "GLOBAL";
            org.Organization_Data.Availability_Date = new DateTime(1900, 1, 1);
            org.Organization_Data.Organization_Subtype_Reference = new Organization_Subtype_Reference_DataType();
            org.Organization_Data.Organization_Subtype_Reference.Organization_Subtype_Name = "Geographic Division";
            org.Organization_Data.Organization_Type_Reference = new Organization_Type_Reference_DataType();
            org.Organization_Data.Organization_Type_Reference.Organization_Type_Name = "Location Hierarchy";
            org.Organization_Data.Organization_Visibility_Reference = new Organization_Visibility_Reference_DataType();
            org.Organization_Data.Organization_Visibility_Reference.Organization_Visibility_Name = "Everyone";
            var orgResult = service.client.Add_Update_Organization(service.header, org);
            var temp = orgResult.GetType();
            #endregion
        }
    }
}
