using System.Collections.Generic;
using System.Linq;
using ConnectionProviderSample.Models;
using DevExpress.Data.Entity;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Native;
using DevExpress.DataAccess.Sql;
using DevExpress.DataAccess.Web;
using DevExpress.DataAccess.Wizard.Services;
using Microsoft.Extensions.Configuration;

namespace ConnectionProviderSample.Services {
    public class CustomConnectionProvider : IConnectionProviderService, DevExpress.DataAccess.Web.IDataSourceWizardConnectionStringsProvider {
        readonly IEnumerable<ConnectionStringModel> connectionStrings;

        public CustomConnectionProvider(IUserService userService, IConfiguration Configuration) {
            var configConnections = new List<ConnectionStringModel>();
            Configuration.GetSection("WebReportingConnectionStrings").Bind(configConnections);
            var isCurrentUserAdministrator = userService.IsCurrentUserAdministrator();
            connectionStrings = configConnections.Where(x => !x.IsPrivate || isCurrentUserAdministrator);
        }

        SqlDataConnection IConnectionProviderService.LoadConnection(string connectionName) {
            var connectionStringModel = connectionStrings.Where(x => x.Name == connectionName).FirstOrDefault();
            ConnectionStringInfo connectionStringInfo = connectionStringModel == null || string.IsNullOrEmpty(connectionStringModel.ConnectionString)
                ? null
                : new ConnectionStringInfo {
                    RunTimeConnectionString = connectionStringModel.ConnectionString,
                    Name = connectionName,
                    ProviderName = "SQLite"
                };
            DataConnectionParametersBase connectionParameters;
            if(connectionStringInfo == null
                || !AppConfigHelper.TryCreateSqlConnectionParameters(connectionStringInfo, out connectionParameters)
                || connectionParameters == null) {
                throw new KeyNotFoundException($"Connection string '{connectionName}' not found.");
            }
            return new SqlDataConnection(connectionName, connectionParameters);
        }

        Dictionary<string, string> IDataSourceWizardConnectionStringsProvider.GetConnectionDescriptions() {
            return connectionStrings.ToDictionary(x => x.Name, x => x.DisplayName);
        }

        DataConnectionParametersBase IDataSourceWizardConnectionStringsProvider.GetDataConnectionParameters(string name) {
            return null;//to prevent serialization of connection parameters along with a report layout.
        }
    }

    public class CustomConnectionProviderFactory : IConnectionProviderFactory {
        readonly IConnectionProviderService connectionProviderService;
        public CustomConnectionProviderFactory(IConnectionProviderService connectionProviderService) {
            this.connectionProviderService = connectionProviderService;
        }
        public IConnectionProviderService Create() {
            return connectionProviderService;
        }
    }
}
