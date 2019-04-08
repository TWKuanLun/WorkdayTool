using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MigrationSolution;

namespace TaskSequence
{
    class Program
    {
        static void Main(string[] args)
        {
            #region Organizations
            #region Company Hierarchies
            Run("Human_Resources", 
                "Add_Update_Organization", 
                "Where [Organization_Data.Organization_Type_Reference.Organization_Type_Name] = 'Company Hierarchy'", 
                "[Human_Resources].[Add_Update_Organization.CompanyHierarchy]");
            #endregion

            #region Companies
            Run("Financial_Management",
                "Put_Company_Organization",
                "",
                "[Financial_Management].[Put_Company_Organization]");
            #endregion

            #region Cost Center Hierarchies
            Run("Human_Resources",
                "Add_Update_Organization",
                "Where [Organization_Data.Organization_Type_Reference.Organization_Type_Name] = 'Cost Center Hierarchy'",
                "[Human_Resources].[Add_Update_Organization.CostCenterHierarchy]");
            #endregion

            #region Cost Centers
            Run("Human_Resources",
                "Add_Update_Organization",
                "Where [Organization_Data.Organization_Type_Reference.Organization_Type_Name] = 'Cost Center'",
                "[Human_Resources].[Add_Update_Organization.CostCenter]");
            #endregion

            #region Location Hierarchies
            Run("Human_Resources",
                "Add_Update_Organization",
                "Where [Organization_Data.Organization_Type_Reference.Organization_Type_Name] = 'Location Hierarchy'",
                "[Human_Resources].[Add_Update_Organization.LocationHierarchy]");
            #endregion

            #region Locations
            Run("Human_Resources",
                "Put_Location",
                "",
                "[Human_Resources].[Put_Location]");
            #endregion

            #region Supervisory Organizations
            Run("Human_Resources",
                "Add_Update_Organization",
                "Where [Organization_Data.Organization_Type_Reference.Organization_Type_Name] = 'Supervisory'",
                "[Human_Resources].[Add_Update_Organization.Supervisory]");
            #endregion

            #region Supervisory Organization Assignment Restrictions
            Run("Human_Resources",
                "Put_Supervisory_Organization_Assignment_Restrictions",
                "",
                "[Human_Resources].[Put_Supervisory_Organization_Assignment_Restrictions]");
            #endregion
            #endregion

            #region Job and Position
            #region Job Families
            Run("Staffing",
                "Put_Job_Family",
                "",
                "[Staffing].[Put_Job_Family]");
            #endregion

            #region Job Family Groups
            Run("Staffing",
                "Put_Job_Family_Group",
                "",
                "[Staffing].[Put_Job_Family_Group]");
            #endregion

            #region Job Profiles
            Run("Human_Resources",
                "Put_Job_Profile",
                "",
                "[Human_Resources].[Put_Job_Profile]");
            #endregion

            #region Create Positions
            Run("Staffing",
                "Create_Position",
                "",
                "[Staffing].[Create_Position]");
            #endregion

            #region Set hiring restriction
            Run("Staffing",
                "Set_Hiring_Restrictions",
                "",
                "[Staffing].[Set_Hiring_Restrictions]");
            #endregion
            #endregion

            #region Employee Data
            #region Create Pre-Hires
            Run("Staffing",
                "Put_Applicant",
                "",
                "[Staffing].[Put_Applicant]");
            #endregion

            #region Hire Employees
            Run("Staffing",
                "Hire_Employee",
                "",
                "[Staffing].[Hire_Employee]");
            #endregion

            #region Edit Service Dates
            Run("Staffing",
                "Edit_Service_Dates",
                "",
                "[Staffing].[Edit_Service_Dates]");
            #endregion

            #region Personal Data
            Run("Human Resources",
                "Change_Personal_Information",
                "",
                "[Human Resources].[Change_Personal_Information]");
            #endregion

            #region Contact Information
            Run("Human_Resources",
                "Maintain_Contact_Information",
                "",
                "[Human_Resources].[Maintain_Contact_Information]");
            #endregion

            #region Emergency Contact
            Run("Human_Resources",
                "Change_Emergency_Contacts",
                "",
                "[Human_Resources].[Change_Emergency_Contacts]");
            #endregion

            #region Add Dependent
            Run("Benefits_Administration",
                "Add_Dependent",
                "",
                "[Benefits_Administration].[Add_Dependent]");
            #endregion

            #region Maintain Contract
            Run("Staffing",
                "Maintain_Employee_Contracts",
                "",
                "[Staffing].[Maintain_Employee_Contracts]");
            #endregion

            #region Edit IDs
            Run("Human_Resources",
                "Change_Government_IDs",
                "",
                "[Human_Resources].[Change_Government_IDs]");
            Run("Human_Resources",
                "Change_Other_IDs",
                "",
                "[Human_Resources].[Change_Other_IDs]");
            #endregion

            #region Payment Election
            Run("Cash_Management",
                "Submit_Payment_Election_Enrollment",
                "",
                "[Cash_Management].[Submit_Payment_Election_Enrollment]");
            #endregion

            #region Additional Job
            Run("Staffing",
                "Add_Additional_Job",
                "",
                "[Staffing].[Add_Additional_Job]");
            #endregion

            #region International Assignment
            Run("Staffing",
                "Start_International_Assignment",
                "",
                "[Staffing].[Start_International_Assignment]");
            #endregion

            #region Terminate
            Run("Staffing",
                "Terminate_Employee",
                "",
                "[Staffing].[Terminate_Employee]");
            #endregion
            #endregion

            #region Contingent Worker Data
            #region Contract Contingent Worker
            Run("Staffing",
                "Contract_Contingent_Worker",
                "",
                "[Staffing].[Contract_Contingent_Worker]");
            #endregion
            #endregion

            #region Compensation
            #region Request_Compensation_Change
            Run("Compensation",
                "Request_Compensation_Change",
                "",
                "[Compensation].[Request_Compensation_Change]");
            #endregion
            #endregion

            #region History Data
            #region Former Worker
            Run("Human_Resources",
                "Put_Former_Worker",
                "",
                "[Human_Resources].[Put_Former_Worker]");
            #endregion

            #region Previous System Job History
            Run("Human_Resources",
                "Put_Previous_System_Job_History",
                "",
                "[Human_Resources].[Put_Previous_System_Job_History]");
            #endregion

            #region Previous System Compensation History
            Run("Compensation",
                "Put_Previous_System_Compensation_History",
                "",
                "[Compensation].[Put_Previous_System_Compensation_History]");
            #endregion
            #endregion

            #region Security
            #region Create Workday Account
            Run("Human_Resources",
                "Add_Workday_Account",
                "",
                "[Human_Resources].[Add_Workday_Account]");
            #endregion

            #region Assign Roles
            Run("Staffing",
                "Assign_Roles",
                "",
                "[Staffing].[Assign_Roles]");
            #endregion

            #region User Based Security
            Run("Human_Resources",
                "Put_Assign_UserBased_Security_Group",
                "",
                "[Human_Resources].[Put_Assign_UserBased_Security_Group]");
            #endregion
            #endregion

            #region Recruiting
            #region Create Job Requisition
            Run("Recruiting",
                "Create_Job_Requisition",
                "",
                "[Recruiting].[Create_Job_Requisition]");
            #endregion

            #region Candidate
            Run("Recruiting",
                "Put_Candidate",
                "",
                "[Recruiting].[Put_Candidate]");
            #endregion
            #endregion
        }
        static void Run(string service, string operation, string condition, string storeProcedure)
        {
            var connectionString = ConfigurationManager.AppSettings["DBConnectionString"].ToString();

            //GenerateTable
            WorkDaySolution.Program.Main(new string[]
                {
                    "Warn",
                    "GenerateTable",
                    connectionString,
                    service,
                    operation
                });

            //call SP
            var queryManager = new QueryManager(connectionString, service);
            queryManager.ExecuteNonQuerySP(storeProcedure);

            //send Request
            WorkDaySolution.Program.Main(new string[]
                {
                    "Warn",
                    "SendRequest",
                    connectionString,
                    service,
                    operation,
                    "0",
                    condition
                });

            //Move Report and log
            Directory.CreateDirectory("Output");
            File.Move("report.csv", $"Output\\{storeProcedure.Replace('.', '-')}-report.csv");
            File.Move("report.html", $"Output\\{storeProcedure.Replace('.', '-')}-report.html");
            File.Move("sample.log", $"Output\\{storeProcedure.Replace('.', '-')}-sample.log");
        }
    }
}
