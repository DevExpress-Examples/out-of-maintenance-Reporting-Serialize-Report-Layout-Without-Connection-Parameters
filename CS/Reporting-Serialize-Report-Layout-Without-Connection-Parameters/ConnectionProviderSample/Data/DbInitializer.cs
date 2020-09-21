using System.IO;
using System.Linq;
using DevExpress.XtraReports.UI;
using ConnectionProviderSample.Reports;

namespace ConnectionProviderSample.Data {
    public static class DbInitializer {
        public static void Initialize(ApplicationDbContext context, ReportsFactory factory) {
            //context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Look for any users.
            if(context.Users.Any()) {
                return;   // DB has been seeded
            }

            var users = new ApplicationUser[] {
                new ApplicationUser { FirstMidName = "Carson", LastName = "Alexander", IsAdministrator = true },
                new ApplicationUser { FirstMidName = "Meredith", LastName = "Alonso", IsAdministrator = false }
            };
            foreach(var user in users) {
                context.Users.Add(user);
                foreach(var report in factory.NorthwindBasedReports.Select(r => new { r.Key, Value = r.Value() })) {
                    var reportDescription = new Report {
                        DisplayName = string.IsNullOrEmpty(report.Value.DisplayName) ? report.Key : report.Value.DisplayName,
                        ReportLayout = ReportToByteArray(report.Value),
                        User = user
                    };
                    if (!user.IsAdministrator) {
                        reportDescription.DisplayName += " (fails in Print Preview)";
                    }
                    context.Reports.Add(reportDescription);
                }
            }
            context.SaveChanges();
        }

        static byte[] ReportToByteArray(XtraReport report) {
            using(var memoryStream = new MemoryStream()) {
                report.SaveLayoutToXml(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
