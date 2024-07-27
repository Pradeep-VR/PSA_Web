using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PSAPLCDashboard.Web.Dashboard.Models
{
    public class TBLENERGYMETER
    {
        public int ID { get; set; }
        public string STATIONS { get; set; }
        public Nullable<int> METERID { get; set; }
        public Nullable<int> GROUPID { get; set; }
        public string CURRENTA { get; set; }
        public string CURRENTB { get; set; }
        public string CURRENTC { get; set; }
        public string VOLTAGEAB { get; set; }
        public string VOLTAGEBC { get; set; }
        public string VOLTAGECA { get; set; }
        public string VOLTAGELL { get; set; }
        public string VOLTAGEAN { get; set; }
        public string VOLTAGEBN { get; set; }
        public string VOLTAGECN { get; set; }
        public string VOLTAGELN { get; set; }
        public string MAXDEMAND { get; set; }
        public string POWERFACTOR { get; set; }
        public string ACTIVEENERGYDELIVERED { get; set; }
        public string SYNCDATETIME { get; set; }
        public string YeasterdayConsume { get; set; }
        public string TodayConsume { get; set; }        
        public string variable_TodayCon {  get; set; }
        public string variable_YesterdayCon {  get; set; }
        public string fixed_TodayCon {  get; set; }
        public string fixed_YesterdayCon {  get; set; }
        
        
        public List<string> MeterNames { get; set; }
        public List<string> GroupNames { get; set; }
    }
}