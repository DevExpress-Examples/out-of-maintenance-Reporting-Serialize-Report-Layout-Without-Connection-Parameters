using System.Collections.Generic;
using System.IO;
using System.Linq;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.Web.Extensions;
using ConnectionProviderSample.Data;
using ConnectionProviderSample.Models;

namespace ConnectionProviderSample.Services {
    public class EFCoreReportStorageWebExtension : ReportStorageWebExtension {
        private readonly IUserService userService;
        private readonly ApplicationDbContext dBContext;

        public EFCoreReportStorageWebExtension(IUserService userService, ApplicationDbContext dBContext) {
            this.userService = userService;
            this.dBContext = dBContext;
        }


        public override bool CanSetData(string url) {
            return true;
        }

        public override bool IsValidUrl(string url) {
            return true;
        }

        public override byte[] GetData(string url) {
            var userId = userService.GetCurrentUserId();
            var reportEntity = dBContext.Reports.Where(a => a.ID == int.Parse(url) && a.User.ID == userId).FirstOrDefault();
            if(reportEntity != null) {
                return reportEntity.ReportLayout;
            } else {
                throw new DevExpress.XtraReports.Web.ClientControls.FaultException(string.Format("Could not find report '{0}'.", url));
            }
        }

        public override Dictionary<string, string> GetUrls() {
            var userId = userService.GetCurrentUserId();
            var reportEntity = dBContext.Reports.Where(a => a.User.ID == userId).Select(a => new ReportModel() { Id = a.ID.ToString(), Title = string.IsNullOrEmpty(a.DisplayName) ? "Noname Report" : a.DisplayName });
            var reports = reportEntity.ToList();
            return reports.ToDictionary(x => x.Id.ToString(), y => y.Title);
        }

        public override void SetData(XtraReport report, string url) {
            var userId = userService.GetCurrentUserId();
            var reportEntity = dBContext.Reports.Where(a => a.ID == int.Parse(url) && a.User.ID == userId).FirstOrDefault();
            reportEntity.ReportLayout = ReportToByteArray(report);
            reportEntity.DisplayName = report.DisplayName;
            dBContext.SaveChanges();
        }

        public override string SetNewData(XtraReport report, string defaultUrl) {
            var userId = userService.GetCurrentUserId();
            var user = dBContext.Users.Find(userId);
            var newReport = new Report { DisplayName = defaultUrl, ReportLayout = ReportToByteArray(report), User = user };
            dBContext.Reports.Add(newReport);
            dBContext.SaveChanges();
            return newReport.ID.ToString();
        }

        static byte[] ReportToByteArray(XtraReport report) {
            using(var memoryStream = new MemoryStream()) {
                report.SaveLayoutToXml(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
