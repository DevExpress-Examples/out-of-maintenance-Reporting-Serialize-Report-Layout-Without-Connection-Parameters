using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConnectionProviderSample.Data;
using ConnectionProviderSample.Models;
using ConnectionProviderSample.Services;
using DevExpress.DataAccess.Wizard.Services;
using DevExpress.XtraReports.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConnectionProviderSample.Controllers {
    [Authorize]
    public class HomeController : Controller {

        public async Task<IActionResult> Index([FromServices]IUserService userService, [FromServices]ApplicationDbContext dBContext) {
            var reportEntity = dBContext.Reports
                .Where(a => a.User.ID == userService.GetCurrentUserId())
                .Select(a => new ReportModel { 
                    Id = a.ID.ToString(), 
                    Title = string.IsNullOrEmpty(a.DisplayName) ? "Noname Report" : a.DisplayName 
                });
            return View(await reportEntity.ToListAsync());
        }

        public IActionResult DesignReport(ReportModel model) {
            return View(model);
        }

        public IActionResult DisplayReport(ReportModel reportModel) {
            return View(reportModel);
        }

        public IActionResult ExportToPdf([FromServices] IReportProvider reportProvider, [FromServices] IConnectionProviderService connectionProvider, ReportModel reportModel) {
            var report = reportProvider.GetReport(reportModel.Id, null);
            if(report == null) {
                throw new ArgumentException();
            }
            var serviceContainer = report as IServiceContainer;
            serviceContainer.RemoveService(typeof(IConnectionProviderService));
            serviceContainer.AddService(typeof(IConnectionProviderService), connectionProvider);
            using(var ms = new MemoryStream()) {
                report.ExportToPdf(ms);
                return File(ms.ToArray(), System.Net.Mime.MediaTypeNames.Application.Pdf, report.DisplayName + ".pdf");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveReport([FromServices]IUserService userService, [FromServices]ApplicationDbContext dBContext, int reportId) {
            var userId = userService.GetCurrentUserId();
            var reportEntity = await dBContext.Reports.Where(a => a.ID == reportId && a.User.ID == userId).FirstOrDefaultAsync();
            if(reportEntity != null) {
                dBContext.Reports.Remove(reportEntity);
                await dBContext.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
