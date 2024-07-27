using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PSAPLCDashboard.Web.Dashboard.Models
{
    public class GROUPS
    {
        public string GroupId { get; set; }
        public string GroupName { get; set; }
        public List<METERMASTER> MMASTER { get; set; }
        public List<GROUPMASTER> GMaster { get; set; }
        public List<TBLENERGYMETER> EMETER { get; set; }
    }

    public class ChartData
    {
        public DateTime Date { get; set; }
        public double PowerFactor { get; set; }
        public double MaxDemand { get; set; }
        public double ActiveEnergy { get; set; }
    }
}