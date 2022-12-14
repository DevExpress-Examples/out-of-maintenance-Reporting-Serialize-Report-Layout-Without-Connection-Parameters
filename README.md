<!-- default badges list -->
![](https://img.shields.io/endpoint?url=https://codecentral.devexpress.com/api/v1/VersionRange/297325960/2022.2)
[![](https://img.shields.io/badge/Open_in_DevExpress_Support_Center-FF7200?style=flat-square&logo=DevExpress&logoColor=white)](https://supportcenter.devexpress.com/ticket/details/T1000107)
[![](https://img.shields.io/badge/📖_How_to_use_DevExpress_Examples-e9f6fc?style=flat-square)](https://docs.devexpress.com/GeneralInformation/403183)
<!-- default badges end -->
# How to Serialize a Report Layout Without Connection Parameters
Your organization's workflow may require a single report to use different database connections. The connection depends on the current status of a user, the time of the day, or other factors that can be evaluated only at runtime.

To meet this requirement, you should use a connection name that is serialized, stored in an XtraReports Report Definition (REPX) data file, and restored at runtime. 

This example demonstrates a technique that allows a user to obtain individual connection parameters for a connection name. The connection name is serialized in the REPX file, while report connection parameters are discarded when a user saves the report.

## Implementation Details
### Connection Strings
Connection strings are stored in the *WebReportingConnectionStrings* section of the appsettings.json file. The "IsPrivate" configuration setting indicates whether access to this connection string is restricted.

### Connection Provider Service
The CustomConnectionProvider class implements the [IConnectionProviderService](https://docs.devexpress.com/CoreLibraries/DevExpress.DataAccess.Wizard.Services.IConnectionProviderService) and [IDataSourceWizardConnectionStringsProvider](https://docs.devexpress.com/CoreLibraries/DevExpress.DataAccess.Web.IDataSourceWizardConnectionStringsProvider) interfaces. The CustomConnectionProvider service finds a connection string by its connection name.

The service determines access rights for an authenticated user and excludes connection strings that the user is not allowed to access.

To make sure that the newly created reports do not serialize connection parameters in the report definition, the [GetDataConnectionParameters](https://docs.devexpress.com/CoreLibraries/DevExpress.DataAccess.Web.IDataSourceWizardConnectionStringsProvider.GetDataConnectionParameters(System.String)) method returns null.

### Report Storage
The EFCoreReportStorageWebExtension service is a custom report storage that stores report definitions in the application.db SQLite database.

## How the Application Works
When you run the application, select a user name from a list of users. The password is empty.

The first user in the list has administrative rights and can access the Northwind data connection to create and view reports. The second user cannot access Northwind, and the Document Viewer throws an exception when the Northwind report is loaded.
