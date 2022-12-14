using System;
using System.Collections.Generic;
using DevExpress.XtraReports.UI;

namespace ConnectionProviderSample.Reports {
    public class ReportsFactory {
        public Dictionary<string, Func<XtraReport>> NorthwindBasedReports {
            get {
                return new Dictionary<string, Func<XtraReport>>() {
                    ["NorthwindBasedReport"] = () => new NWindReport()
                };
            }
        }
    }
}
