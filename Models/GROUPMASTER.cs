using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PSAPLCDashboard.Web.Dashboard.Models
{
    public class GROUPMASTER
    {
        public int GROUPID { get; set; }
        public string GROUPNAME { get; set; }
        public Nullable<bool> FLAG { get; set; }
        public string CREATEDBY { get; set; }
        public Nullable<System.DateTime> CREATEDDATE { get; set; }
        public string UPDATEDBY { get; set; }
        public Nullable<System.DateTime> UPDATEDDATE { get; set; }
    }
}