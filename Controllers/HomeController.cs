using System;
using System.Data;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Configuration;
using PSAPLCDashboard.Web.Dashboard.Models;
using System.Collections.Generic;
using PSAPLCDashboard.Web.Dashboard.Extras;
using System.Globalization;
using ClosedXML.Excel;
using System.IO;
using Microsoft.Ajax.Utilities;
using System.Linq;

namespace PSAPLCDashboard.Web.Dashboard.Controllers
{
    public class HomeController : Controller
    {
        DataSyncServer _serv = new DataSyncServer();

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetGroupsWithMeters()
        {
            var groups = new List<GROUPS>();

            string query = "SELECT GROUPID,GROUPNAME FROM  GroupMaster WHERE  FLAG = '1' ORDER BY GROUPORDER ASC;";
            DataTable dt = _serv.GetDataTable(query);
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var groupId = dt.Rows[i]["GROUPID"].ToString();
                    var groupName = dt.Rows[i]["GROUPNAME"].ToString();

                    var group = new GROUPS
                    {
                        GroupId = groupId,
                        GroupName = groupName,
                        MMASTER = new List<METERMASTER>()
                    };
                    groups.Add(group);
                }
            }
            return Json(groups);
        }
        public ActionResult InduvidualGraph(string strgroupId, string strgroupName, string strfromDate, string strtoDate)
        {
            ViewBag.groupId = strgroupId;
            ViewBag.groupName = strgroupName;
            ViewBag.FromDate = strfromDate;
            ViewBag.ToDate = strtoDate;
            return View();
        }




        public ActionResult TableData(string strgroupId, string strgroupName, string strfromDate, string strtoDate)
        {
            ViewBag.groupId = strgroupId;
            ViewBag.groupName = strgroupName;
            ViewBag.FromDate = strfromDate;
            ViewBag.ToDate = strtoDate;
            return View();
        }

        [HttpPost]
        public JsonResult GetMetersName(string Groupid)
        {
            var group = new GROUPS();
            string mtrnameqry = "SELECT METERID,METERNAME FROM METERMASTER WHERE GROUPID='" + Groupid + "' AND FLAG=1";
            DataTable dt = _serv.GetDataTable(mtrnameqry);
            if (dt.Rows.Count > 0)
            {
                var meters = new List<METERMASTER>();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    meters.Add(new METERMASTER
                    {
                        METERID = dt.Rows[i]["METERID"].ToString(),
                        METERNAME = dt.Rows[i]["METERNAME"].ToString(),
                    });
                    group.MMASTER = meters;
                }
            }

            return Json(group);
        }


        [HttpPost]
        public JsonResult GetMetersDtls(string strGroupId, bool strCon)
        {
            var group = new GROUPS();
            string BqCon = string.Empty;
            if (strCon == true)
            {
                BqCon = "GROUPID='" + strGroupId + "' AND";
            }
            else
            {
                BqCon = "";
            }

            string mtrnameqry = "SELECT METERID,METERNAME FROM METERMASTER WHERE " + BqCon + " FLAG=1";
            DataTable dt = _serv.GetDataTable(mtrnameqry);
            if (dt.Rows.Count > 0)
            {
                var meters = new List<METERMASTER>();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    meters.Add(new METERMASTER
                    {
                        METERID = dt.Rows[i]["METERID"].ToString(),
                        METERNAME = dt.Rows[i]["METERNAME"].ToString(),
                    });
                    group.MMASTER = meters;
                }
            }

            return Json(group);
        }

        [HttpPost]
        public JsonResult GetTsWData(string strTransformer)
        {
            DateTime yesterday = DateTime.Today.AddDays(-1);
            string formYesDay = yesterday.ToString("dd/MM/yyyy 00:00:00", CultureInfo.InvariantCulture);
            string toYesDay = yesterday.ToString("dd/MM/yyyy 23:59:59", CultureInfo.InvariantCulture);

            DateTime today = DateTime.Today;
            string fromToday = today.ToString("dd/MM/yyyy 00:00:00", CultureInfo.InvariantCulture);
            string toToday = today.ToString("dd/MM/yyyy 23:59:59", CultureInfo.InvariantCulture);

            decimal strYesCon = 0;
            decimal strToCon = 0;

            var YesDayCon = GetTransformerConsumptions("GTW", formYesDay, toYesDay, "G", strTransformer);
            var ToDayCon = GetTransformerConsumptions("GTW", fromToday, toToday, "G", strTransformer);

            if (!YesDayCon.IsNullOrWhiteSpace())
            {
                strYesCon = Convert.ToDecimal(strYesCon) + Convert.ToDecimal(YesDayCon);
            }

            if (!ToDayCon.IsNullOrWhiteSpace())
            {
                strToCon = Convert.ToDecimal(strToCon) + Convert.ToDecimal(ToDayCon);
            }

            var data = new
            {
                YesdayCon = strYesCon.ToString(".00"),
                TodayCon = strToCon.ToString(".00")
            };

            return Json(data);
        }


        [HttpPost]
        public JsonResult GetEnergyData_ReWork(string groupId)
        {
            TBLENERGYMETER data = new TBLENERGYMETER();
            DateTime yesterday = DateTime.Today.AddDays(-1);
            string formYesDay = yesterday.ToString("dd/MM/yyyy 00:00:00", CultureInfo.InvariantCulture);
            string toYesDay = yesterday.ToString("dd/MM/yyyy 23:59:59", CultureInfo.InvariantCulture);

            DateTime today = DateTime.Today;
            string fromToday = today.ToString("dd/MM/yyyy 00:00:00", CultureInfo.InvariantCulture);
            string toToday = today.ToString("dd/MM/yyyy 23:59:59", CultureInfo.InvariantCulture);

            decimal YesDayCon = 0;
            decimal ToDayCon = 0;
            decimal Var_YesdayCon = 0; decimal Var_TodayCon = 0; decimal Fix_YesdayCon = 0; decimal Fix_TodayCon = 0;

            string Qry = "SELECT METERID FROM METERMASTER WHERE GROUPID='" + groupId + "' AND FLAG=1";
            var Meterdata = _serv.GetDataTable(Qry);
            if (Meterdata.Rows.Count > 0)
            {
                List<decimal> lst1 = new List<decimal>();
                List<decimal> lst2 = new List<decimal>();
                List<decimal> lstF1 = new List<decimal>();
                List<decimal> lstF2 = new List<decimal>();
                List<decimal> lstV1 = new List<decimal>();
                List<decimal> lstV2 = new List<decimal>();

                for (int i = 0; i < Meterdata.Rows.Count; i++)
                {
                    string meterId = Meterdata.Rows[i]["METERID"].ToString();

                    var Yescon = GetConsumptions(meterId, formYesDay, toYesDay, "M");
                    var Tocon = GetConsumptions(meterId, fromToday, toToday, "M");
                    if (!Yescon.IsNullOrWhiteSpace())
                    {
                        lst1.Add(Convert.ToDecimal(Yescon));
                    }

                    if (!Tocon.IsNullOrWhiteSpace())
                    {
                        lst2.Add(Convert.ToDecimal(Tocon));
                    }

                    var Fix_YesCon = GetVFConsumptions(meterId, formYesDay, toYesDay, "M", "F");
                    if (!Fix_YesCon.IsNullOrWhiteSpace())
                    {
                        lstF1.Add(Convert.ToDecimal(Fix_YesCon));
                        //Fix_YesdayCon = Convert.ToDecimal(Fix_YesCon);
                    }
                    var Fix_ToCon = GetVFConsumptions(meterId, fromToday, toToday, "M", "F");
                    if (!Fix_ToCon.IsNullOrWhiteSpace())
                    {
                        lstF2.Add(Convert.ToDecimal(Fix_ToCon));
                        //Fix_TodayCon = Convert.ToDecimal(Fix_ToCon);
                    }

                    var Var_YesCon = GetVFConsumptions(meterId, formYesDay, toYesDay, "M", "V");
                    if (!Var_YesCon.IsNullOrWhiteSpace())
                    {
                        lstV1.Add(Convert.ToDecimal(Var_YesCon));
                        //Var_YesdayCon = Convert.ToDecimal(Var_YesCon);
                    }
                    var Var_ToCon = GetVFConsumptions(meterId, fromToday, toToday, "M", "V");
                    if (!Var_ToCon.IsNullOrWhiteSpace())
                    {
                        lstV2.Add(Convert.ToDecimal(Var_ToCon));
                        //Var_TodayCon = Convert.ToDecimal(Var_ToCon);
                    }
                }

                foreach (var item in lst1)
                {
                    YesDayCon += item;
                }

                foreach (var item in lst2)
                {
                    ToDayCon += item;
                }

                foreach (var item in lstF1)
                {
                    Fix_YesdayCon += item;
                }

                foreach (var item in lstF2)
                {
                    Fix_TodayCon += item;
                }

                foreach (var item in lstV1)
                {
                    Var_YesdayCon += item;
                }

                foreach (var item in lstV2)
                {
                    Var_TodayCon += item;
                }
            }

            data.YeasterdayConsume = YesDayCon.ToString(".00");
            data.TodayConsume = ToDayCon.ToString(".00");

            data.variable_YesterdayCon = Var_YesdayCon.ToString(".00");
            data.variable_TodayCon = Var_TodayCon.ToString(".00");

            data.fixed_YesterdayCon = Fix_YesdayCon.ToString(".00");
            data.fixed_TodayCon = Fix_TodayCon.ToString(".00");

            string qry2 = "";
            var dt2 = _serv.GetDataTable(qry2);
            if (dt2.Rows.Count > 0)
            {

            }

            string qry6 = " SELECT  GROUPNAME,GROUPID FROM GROUPMASTER   WHERE FLAG='1'";
            DataTable dt6 = _serv.GetDataTable(qry6);
            data.GroupNames = new List<string>();
            foreach (DataRow row in dt6.Rows)
            {
                string formattedGroupName = $"{row["GROUPNAME"]} - {row["GROUPID"]}";
                data.GroupNames.Add(formattedGroupName);
            }

            return Json(data);
        }


        [HttpPost]
        public JsonResult GetEnergyData(string groupId)
        {
            TBLENERGYMETER data = new TBLENERGYMETER();
            DateTime yesterday = DateTime.Today.AddDays(-1);
            string formYesDay = yesterday.ToString("dd/MM/yyyy 00:00:00", CultureInfo.InvariantCulture);
            string toYesDay = yesterday.ToString("dd/MM/yyyy 23:59:59", CultureInfo.InvariantCulture);

            DateTime today = DateTime.Today;
            string fromToday = today.ToString("dd/MM/yyyy 00:00:00", CultureInfo.InvariantCulture);
            string toToday = today.ToString("dd/MM/yyyy 23:59:59", CultureInfo.InvariantCulture);

            decimal YesDayCon = 0;
            decimal ToDayCon = 0;
            decimal Var_YesdayCon = 0; decimal Var_TodayCon = 0; decimal Fix_YesdayCon = 0; decimal Fix_TodayCon = 0;

            var Yescon = GetConsumptions(groupId, formYesDay, toYesDay, "G");
            var Tocon = GetConsumptions(groupId, fromToday, toToday, "G");
            if (!Yescon.IsNullOrWhiteSpace())
            {
                YesDayCon = Convert.ToDecimal(YesDayCon) + Convert.ToDecimal(Yescon);
            }

            if (!Tocon.IsNullOrWhiteSpace())
            {
                ToDayCon = Convert.ToDecimal(ToDayCon) + Convert.ToDecimal(Tocon);
            }

            var Fix_YesCon = GetVFConsumptions(groupId, formYesDay, toYesDay, "G", "F");
            if (!Fix_YesCon.IsNullOrWhiteSpace())
            {
                Fix_YesdayCon = Convert.ToDecimal(Fix_YesCon);
            }
            var Fix_ToCon = GetVFConsumptions(groupId, fromToday, toToday, "G", "F");
            if (!Fix_ToCon.IsNullOrWhiteSpace())
            {
                Fix_TodayCon = Convert.ToDecimal(Fix_ToCon);
            }

            var Var_YesCon = GetVFConsumptions(groupId, formYesDay, toYesDay, "G", "V");
            if (!Var_YesCon.IsNullOrWhiteSpace())
            {
                Var_YesdayCon = Convert.ToDecimal(Var_YesCon);
            }
            var Var_ToCon = GetVFConsumptions(groupId, fromToday, toToday, "G", "V");
            if (!Var_ToCon.IsNullOrWhiteSpace())
            {
                Var_TodayCon = Convert.ToDecimal(Var_ToCon);
            }


            data.YeasterdayConsume = YesDayCon.ToString(".00");
            data.TodayConsume = ToDayCon.ToString(".00");

            data.variable_YesterdayCon = Var_YesdayCon.ToString(".00");
            data.variable_TodayCon = Var_TodayCon.ToString(".00");

            data.fixed_YesterdayCon = Fix_YesdayCon.ToString(".00");
            data.fixed_TodayCon = Fix_TodayCon.ToString(".00");

            string qry2 = "";
            var dt2 = _serv.GetDataTable(qry2);
            if (dt2.Rows.Count > 0)
            {

            }

            string qry6 = " SELECT  GROUPNAME,GROUPID FROM GROUPMASTER   WHERE FLAG='1'";
            DataTable dt6 = _serv.GetDataTable(qry6);
            data.GroupNames = new List<string>();
            foreach (DataRow row in dt6.Rows)
            {
                string formattedGroupName = $"{row["GROUPNAME"]} - {row["GROUPID"]}";
                data.GroupNames.Add(formattedGroupName);
            }

            return Json(data);
        }


        [HttpPost]
        public JsonResult StationsData(string groupId, string groupName)
        {
            List<object> dataList = new List<object>();
            DateTime today = DateTime.Today;
            string fromToday = today.ToString("dd/MM/yyyy 00:00:00", CultureInfo.InvariantCulture);
            string toToday = today.ToString("dd/MM/yyyy 23:59:59", CultureInfo.InvariantCulture);

            string qry = " WITH HourlyData AS (SELECT DATEPART(HOUR, SYNCDATETIME) AS Hour,SUM(CONVERT(float, MAXDEMAND)) AS MAXDEMAND,  " +
                         " SUM(CONVERT(float, POWERFACTOR)) AS POWERFACTOR,SUM(CONVERT(float, ACTIVEENERGYDELIVERED)) AS ACTIVEENERGYDELIVERED FROM TBL_ENERGYMETER   " +
                         " WHERE GROUPID = '" + groupId + "' AND CONVERT(datetime, SYNCDATETIME,103) >= CONVERT(datetime, '" + fromToday + "',103)   AND CONVERT(datetime, SYNCDATETIME,103)  " +
                         " <= CONVERT(datetime, '" + toToday + "',103)  " +
                         " GROUP BY  DATEPART(HOUR, SYNCDATETIME)),HourlyDifferences AS (SELECT h1.Hour,h1.MAXDEMAND,h1.POWERFACTOR, h1.ACTIVEENERGYDELIVERED, " +
                         " (h1.MAXDEMAND - ISNULL(h2.MAXDEMAND, 0)) AS MAXDEMAND_DIFF,(h1.POWERFACTOR - ISNULL(h2.POWERFACTOR, 0)) AS POWERFACTOR_DIFF, " +
                         " (h1.ACTIVEENERGYDELIVERED - ISNULL(h2.ACTIVEENERGYDELIVERED, 0)) AS ACTIVEENERGYDELIVERED_DIFF FROM HourlyData h1  LEFT JOIN  HourlyData h2 ON h1.Hour = h2.Hour + 1)  " +
                         " SELECT  Hour, MAXDEMAND, POWERFACTOR, ACTIVEENERGYDELIVERED, MAXDEMAND_DIFF,  POWERFACTOR_DIFF, ACTIVEENERGYDELIVERED_DIFF FROM  HourlyDifferences ORDER BY  Hour; ";

            DataTable dt = _serv.GetDataTable(qry);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    var maxDemandDiff = Convert.ToDouble(row["MAXDEMAND_DIFF"]);
                    var powerFactorDiff = Convert.ToDouble(row["POWERFACTOR_DIFF"]);
                    var activeEnergyDeliveredDiff = Convert.ToDouble(row["ACTIVEENERGYDELIVERED_DIFF"]);

                    // Ensure negative values are converted to positive
                    if (maxDemandDiff < 0)
                    {
                        maxDemandDiff *= -1;
                    }

                    if (powerFactorDiff < 0)
                    {
                        powerFactorDiff *= -1;
                    }

                    if (activeEnergyDeliveredDiff < 0)
                    {
                        activeEnergyDeliveredDiff *= -1;
                    }

                    var data = new
                    {
                        Hour = row["Hour"].ToString(),
                        MAXDEMAND = row["MAXDEMAND"].ToString(),
                        POWERFACTOR = row["POWERFACTOR"].ToString(),
                        ACTIVEENERGYDELIVERED = row["ACTIVEENERGYDELIVERED"].ToString(),
                        MAXDEMAND_DIFF = maxDemandDiff.ToString(), // Converted to positive
                        POWERFACTOR_DIFF = powerFactorDiff.ToString(), // Converted to positive
                        ACTIVEENERGYDELIVERED_DIFF = activeEnergyDeliveredDiff.ToString() // Converted to positive
                    };
                    dataList.Add(data);
                }
            }

            return Json(dataList);
        }

        [HttpPost]
        public JsonResult GetMeters(string groupId, string strMeterDiv)
        {
            var group = new GROUPS();

            //string query = "SELECT gm.GroupId,gm.GroupName,mm.MeterId,mm.MeterName FROM GroupMaster gm LEFT JOIN  MeterMaster mm ON gm.GroupId = mm.GroupId WHERE gm.FLAG = '1' AND mm.FLAG='1' AND gm.GroupId = '" + groupId + "';";
            string query = "SELECT GM.GROUPID,GM.GROUPNAME,MM.METERID,MM.METERNAME,MM.METERDIVISION FROM GROUPMASTER GM LEFT JOIN  METERMASTER MM ON GM.GROUPID = MM.GROUPID " +
                "WHERE GM.FLAG = '1' AND MM.FLAG='1' AND GM.GROUPID = '" + groupId + "' AND MM.METERDIVISION IN (" + strMeterDiv + ")";
            DataTable dt = _serv.GetDataTable(query);
            if (dt.Rows.Count > 0)
            {
                var meters = new List<METERMASTER>();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (group.GroupId == null || group.GroupId.Length == 0)
                    {
                        group.GroupId = dt.Rows[i]["GROUPID"].ToString();
                        group.GroupName = dt.Rows[i]["GROUPNAME"].ToString();
                    }


                    meters.Add(new METERMASTER
                    {
                        METERID = dt.Rows[i]["MeterId"].ToString(),
                        METERNAME = dt.Rows[i]["MeterName"].ToString(),
                    });

                }
                group.MMASTER = meters;
            }

            return Json(group);
        }


        private string GetTransformerConsumptions(string Id, string fd, string td, string lol, string strTrans)
        {
            string ret = string.Empty;
            List<string> consumptions = new List<string>();

            string qry2 = string.Empty;


            qry2 = "SELECT ACTIVEENERGYDELIVERED AS kwh FROM TBL_ENERGYMETER AS EM JOIN METERMASTER AS MM ON MM.METERID=EM.METERID WHERE  MM.TRANSFORMER='" + strTrans + "' AND CONVERT(DATETIME, SYNCDATETIME,103) " +
            "BETWEEN CONVERT(DATETIME, '" + fd + "',103) AND  CONVERT(DATETIME, '" + td + "',103)";


            var dt = _serv.GetDataTable(qry2);
            if (dt.Rows.Count > 0)
            {
                int j = 0;
                // Adding data
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    j = i + 1;
                    if (j < dt.Rows.Count)
                    {
                        decimal val3 = Convert.ToDecimal(dt.Rows[i]["kwh"].ToString()) - Convert.ToDecimal(dt.Rows[j]["kwh"].ToString());
                        consumptions.Add(val3.ToString(".00"));
                    }
                }
            }
            if (consumptions.Count > 0)
            {
                decimal count = 0;
                foreach (var item in consumptions)
                {
                    count = count + Convert.ToDecimal(item);
                }
                ret = count.ToString(".00").Replace('-', ' ').Trim();
            }
            return ret;
        }


        public string GetVFConsumptions(string Id, string fd, string td, string lol, string strMeterDiv)
        {
            string ret = string.Empty;
            List<string> consumptions = new List<string>();
            string lols = string.Empty;
            if (lol == "G")
            {
                lols = "GROUPID = '" + Id + "'";
            }
            else if (lol == "M")
            {
                lols = "METERID = '" + Id + "'";
            }
            string qry2 = string.Empty;

            if (strMeterDiv != "")
            {
                //qry2 = "SELECT ACTIVEENERGYDELIVERED AS kwh FROM TBL_ENERGYMETER AS EM LEFT JOIN METERMASTER AS MM ON MM.GROUPID=EM.GROUPID WHERE EM." + lols + " AND MM.METERDIVISION='" + strMeterDiv + "' AND CONVERT(datetime, SYNCDATETIME,103) " +
                //"BETWEEN CONVERT(datetime, '" + fd + "',103) AND  CONVERT(datetime, '" + td + "',103)";
                qry2 = "SELECT ACTIVEENERGYDELIVERED AS kwh FROM TBL_ENERGYMETER WHERE " + lols + " AND METERID IN (SELECT METERID FROM METERMASTER WHERE " + lols + " AND METERDIVISION='" + strMeterDiv + "' AND FLAG=1)" +
                    "  AND CONVERT(DATETIME, SYNCDATETIME,103) BETWEEN CONVERT(DATETIME, '" + fd + "',103) AND  CONVERT(DATETIME, '" + td + "',103)";
            }
            else
            {
                qry2 = "SELECT ACTIVEENERGYDELIVERED AS kwh FROM TBL_ENERGYMETER WHERE " + lols + " AND CONVERT(datetime, SYNCDATETIME,103) " +
                "BETWEEN CONVERT(datetime, '" + fd + "',103) AND  CONVERT(datetime, '" + td + "',103)";
            }

            var dt = _serv.GetDataTable(qry2);
            if (dt.Rows.Count > 0)
            {
                int j = 0;
                // Adding data
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    j = i + 1;
                    if (j < dt.Rows.Count)
                    {
                        decimal val3 = Convert.ToDecimal(dt.Rows[i]["kwh"].ToString()) - Convert.ToDecimal(dt.Rows[j]["kwh"].ToString());
                        consumptions.Add(val3.ToString(".00"));
                    }
                }
            }
            if (consumptions.Count > 0)
            {
                decimal count = 0;
                foreach (var item in consumptions)
                {
                    count = count + Convert.ToDecimal(item);
                }
                ret = count.ToString(".00").Replace('-', ' ').Trim();
            }
            return ret;
        }
        public string GetConsumptions(string Id, string fd, string td, string lol)
        {
            string ret = string.Empty;
            List<string> consumptions = new List<string>();
            string lols = string.Empty;
            if (lol == "G")
            {
                lols = "GROUPID = '" + Id + "'";
            }
            else if (lol == "M")
            {
                lols = "METERID = '" + Id + "'";
            }

            string qry2 = "SELECT ACTIVEENERGYDELIVERED AS kwh FROM TBL_ENERGYMETER WHERE " + lols + " AND CONVERT(DATETIME, SYNCDATETIME,103) " +
                "BETWEEN CONVERT(DATETIME, '" + fd + "',103) AND  CONVERT(DATETIME, '" + td + "',103)";
            var dt = _serv.GetDataTable(qry2);
            if (dt.Rows.Count > 0)
            {
                int j = 0;
                // Adding data
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    j = i + 1;
                    if (j < dt.Rows.Count)
                    {
                        decimal val3 = Convert.ToDecimal(dt.Rows[i]["kwh"].ToString()) - Convert.ToDecimal(dt.Rows[j]["kwh"].ToString());
                        consumptions.Add(val3.ToString(".00"));
                    }
                }
            }
            if (consumptions.Count > 0)
            {
                decimal count = 0;
                foreach (var item in consumptions)
                {
                    count = count + Convert.ToDecimal(item);
                }
                ret = count.ToString(".00").Replace('-', ' ').Trim();
            }
            return ret;
        }


        [HttpPost]
        public JsonResult GetEnergyDataUsingMeterid(string meterId)
        {
            TBLENERGYMETER data = null;
            DateTime yesterday = DateTime.Today.AddDays(-1);
            string fromYesday = yesterday.ToString("dd/MM/yyyy 00:00:00", CultureInfo.InvariantCulture);
            string toYesDay = yesterday.ToString("dd/MM/yyyy 23:59:59", CultureInfo.InvariantCulture);

            DateTime today = DateTime.Today;
            string fromToday = today.ToString("dd/MM/yyyy 00:00:00", CultureInfo.InvariantCulture);
            string toToday = today.ToString("dd/MM/yyyy 23:59:59", CultureInfo.InvariantCulture);

            string Qry = "SELECT * FROM (SELECT CURRENTA AS CRY,CURRENTB AS CYB,CURRENTC AS CBR,VoltageAB AS VRY,VoltageBC AS VYB,VoltageCA AS VBR, " +
                "ACTIVEENERGYDELIVERED AS ADE , CONVERT(datetime, SYNCDATETIME,103) as Datetimes FROM TBL_ENERGYMETER WHERE METERID = '" + meterId + "' ) AS C " +
                "WHERE CONVERT(date,C.Datetimes,103) = CONVERT(date, '" + fromToday + "',103) ORDER BY C.Datetimes DESC";
            var dt = _serv.GetDataTable(Qry);

            decimal CA, CB, CC, VA, VB, VC, AED, YesCon, ToCon = 0;

            string YesConsum = GetConsumptions(meterId, fromYesday, toYesDay, "M");
            string ToConsum = GetConsumptions(meterId, fromToday, toToday, "M");
            YesCon = Convert.ToDecimal(YesConsum == "" ? "0" : YesConsum);
            ToCon = Convert.ToDecimal(ToConsum == "" ? "0" : ToConsum);
            CA = dt.Rows.Count > 0 && dt.Rows[0]["CRY"] != DBNull.Value ? Convert.ToDecimal(dt.Rows[0]["CRY"]) : 0;
            CB = dt.Rows.Count > 0 && dt.Rows[0]["CYB"] != DBNull.Value ? Convert.ToDecimal(dt.Rows[0]["CYB"]) : 0;
            CC = dt.Rows.Count > 0 && dt.Rows[0]["CBR"] != DBNull.Value ? Convert.ToDecimal(dt.Rows[0]["CBR"]) : 0;
            VA = dt.Rows.Count > 0 && dt.Rows[0]["VRY"] != DBNull.Value ? Convert.ToDecimal(dt.Rows[0]["VRY"]) : 0;
            VB = dt.Rows.Count > 0 && dt.Rows[0]["VYB"] != DBNull.Value ? Convert.ToDecimal(dt.Rows[0]["VYB"]) : 0;
            VC = dt.Rows.Count > 0 && dt.Rows[0]["VBR"] != DBNull.Value ? Convert.ToDecimal(dt.Rows[0]["VBR"]) : 0;
            AED = dt.Rows.Count > 0 && dt.Rows[0]["ADE"] != DBNull.Value ? Convert.ToDecimal(dt.Rows[0]["ADE"]) : 0;

            data = new TBLENERGYMETER
            {
                YeasterdayConsume = YesCon.ToString("0.00"),
                TodayConsume = ToCon.ToString("0.00"),
                CURRENTA = CA.ToString("0.00"),
                CURRENTB = CB.ToString("0.00"),
                CURRENTC = CC.ToString("0.00"),
                VOLTAGEAB = VA.ToString("0.00"),
                VOLTAGEBC = VB.ToString("0.00"),
                VOLTAGECA = VC.ToString("0.00"),
                ACTIVEENERGYDELIVERED = AED.ToString("0.00"),
            };
            return Json(data);
        }


        public ActionResult InduvidualStationsData(string groupId, string groupName)
        {
            ViewBag.GroupId = groupId;
            ViewBag.GroupName = groupName;

            return View();
        }



        [HttpPost]
        public ActionResult GetXLData(string strGroupId, string strFromDate, string strToDate)
        {
            TestModel valuess = new TestModel();

            DateTime fDate = DateTime.Parse(strFromDate);
            DateTime tDate = DateTime.Parse(strToDate);
            string Query = "SELECT mm.MeterName, em.SYNCDATETIME, em.MAXDEMAND, em.POWERFACTOR, em.ACTIVEENERGYDELIVERED FROM TBL_ENERGYMETER em INNER JOIN MeterMaster mm ON em.MeterID = mm.MeterID WHERE em.GROUPID = '" + strGroupId + "' AND CONVERT(DATE, em.SYNCDATETIME, 103) BETWEEN CONVERT(DATE, '" + fDate + "', 103) AND CONVERT(DATE, '" + tDate + "', 103) ORDER BY em.SYNCDATETIME ASC;";

            DataTable dt = _serv.GetDataTable(Query);
            if (dt.Rows.Count > 0)
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Energy Data");

                    // Adding headers
                    worksheet.Cell(1, 1).Value = "Meter Name";
                    worksheet.Cell(1, 2).Value = "Sync DateTime";
                    worksheet.Cell(1, 3).Value = "Max Demand";
                    worksheet.Cell(1, 4).Value = "Power Factor";
                    worksheet.Cell(1, 5).Value = "Active Energy Delivered";
                    int j = 0;
                    // Adding data
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        j = i + 1;
                        if (j < dt.Rows.Count)
                        {
                            worksheet.Cell(i + 2, 1).Value = dt.Rows[i]["MeterName"].ToString();
                            worksheet.Cell(i + 2, 2).Value = Convert.ToDateTime(dt.Rows[i]["SYNCDATETIME"]).ToString("yyyy-MM-dd HH:mm:ss");

                            decimal val1 = Convert.ToDecimal(dt.Rows[i]["MAXDEMAND"].ToString()) - Convert.ToDecimal(dt.Rows[j]["MAXDEMAND"].ToString());
                            decimal val2 = Convert.ToDecimal(dt.Rows[i]["POWERFACTOR"].ToString().Contains('E') ? "0" : dt.Rows[i]["POWERFACTOR"].ToString()) - Convert.ToDecimal(dt.Rows[j]["POWERFACTOR"].ToString().Contains('E') ? "0" : dt.Rows[j]["POWERFACTOR"].ToString());
                            decimal val3 = Convert.ToDecimal(dt.Rows[i]["ACTIVEENERGYDELIVERED"].ToString()) - Convert.ToDecimal(dt.Rows[j]["ACTIVEENERGYDELIVERED"].ToString());

                            worksheet.Cell(i + 2, 3).Value = val1.ToString(".00").Replace('-', ' ');
                            worksheet.Cell(i + 2, 4).Value = val2.ToString(".00").Replace('-', ' ');
                            worksheet.Cell(i + 2, 5).Value = val3.ToString(".00").Replace('-', ' ');

                        }
                    }
                    // Saving the file to a memory stream
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Position = 0;
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "EnergyData.xlsx");
                    }
                }
            }
            else
            {
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Energy Data");
                worksheet.Cell(1, 1).Value = "Meter Name";
                worksheet.Cell(1, 2).Value = "Sync DateTime";
                worksheet.Cell(1, 3).Value = "Max Demand";
                worksheet.Cell(1, 4).Value = "Power Factor";
                worksheet.Cell(1, 5).Value = "Active Energy Delivered";
                var stream = new MemoryStream();
                workbook.SaveAs(stream);

                stream.Position = 0;
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "EnergyData.xlsx");
            }
        }


        /// Viewing Graph Methods Start


        [HttpPost]
        public ActionResult GetGraphData(string strGroupId, string strFromDate, string strToDate)
        {
            TestModel valuess = new TestModel();

            DateTime fDate = DateTime.Parse(strFromDate);
            DateTime tDate = DateTime.Parse(strToDate);

            string Query = "SELECT SYNCDATETIME, MAXDEMAND, POWERFACTOR, ACTIVEENERGYDELIVERED FROM TBL_ENERGYMETER WHERE GROUPID = '" + strGroupId + "' AND CONVERT(DATE, SYNCDATETIME,103)" +
                " BETWEEN CONVERT(DATE,'" + fDate + "',103) AND CONVERT(DATE,'" + tDate + "',103) ORDER BY SYNCDATETIME ASC;";
            DataTable dt = _serv.GetDataTable(Query);
            if (dt.Rows.Count > 0)
            {
                List<string> data = new List<string>();
                List<string> data1 = new List<string>();
                List<string> data2 = new List<string>();

                int j = 0;

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    j = i + 1;
                    if (j < dt.Rows.Count)
                    {
                        decimal val1 = Convert.ToDecimal(dt.Rows[i]["MAXDEMAND"].ToString());// - Convert.ToDecimal(dt.Rows[j]["MAXDEMAND"].ToString());
                        decimal val2 = Convert.ToDecimal(dt.Rows[i]["POWERFACTOR"].ToString().Contains('E') ? "0" : dt.Rows[i]["POWERFACTOR"].ToString());// - Convert.ToDecimal(dt.Rows[j]["POWERFACTOR"].ToString().Contains('E') ? "0" : dt.Rows[j]["POWERFACTOR"].ToString());
                        decimal val3 = Convert.ToDecimal(dt.Rows[i]["ACTIVEENERGYDELIVERED"].ToString()) - Convert.ToDecimal(dt.Rows[j]["ACTIVEENERGYDELIVERED"].ToString());

                        data.Add(val1.ToString(".00").Replace('-', ' '));
                        data1.Add(val2.ToString(".00").Replace('-', ' '));
                        data2.Add(val3.ToString(".00").Replace('-', ' '));
                    }

                }
                valuess.MaxDemand = data.ToArray();
                valuess.PowerFactor = data1.ToArray();
                valuess.ActiveEnergy = data2.ToArray();
            }
            return Json(valuess);
        }



        [HttpPost]
        public ActionResult GetTableData(string strGroupId, string strFromDate, string strToDate)
        {
            TestModel valuess = new TestModel();

            DateTime fDate = DateTime.Parse(strFromDate);
            DateTime tDate = DateTime.Parse(strToDate);
            string Query = "SELECT  mm.MeterName ,em.SYNCDATETIME, em.MAXDEMAND, em.POWERFACTOR, em.ACTIVEENERGYDELIVERED FROM TBL_ENERGYMETER em INNER JOIN MeterMaster mm ON em.MeterID = mm.MeterID WHERE em.GROUPID = '" + strGroupId + "'   AND CONVERT(DATE, em.SYNCDATETIME, 103) BETWEEN CONVERT(DATE, '" + fDate + "', 103) AND CONVERT(DATE, '" + tDate + "', 103) ORDER BY em.SYNCDATETIME ASC;";

            DataTable dt = _serv.GetDataTable(Query);
            if (dt.Rows.Count > 0)
            {
                List<string> MeterName = new List<string>();
                List<string> SYNCDATETIME = new List<string>();
                List<string> maxDemands = new List<string>();
                List<string> powerFactors = new List<string>();
                List<string> activeEnergies = new List<string>();
                int j = 0;

                for (int i = 0; i < dt.Rows.Count - 1; i++)
                {
                    j = i + 1;
                    if (j < dt.Rows.Count)
                    {
                        decimal val1 = Convert.ToDecimal(dt.Rows[i]["MAXDEMAND"].ToString()) - Convert.ToDecimal(dt.Rows[j]["MAXDEMAND"].ToString());
                        decimal val2 = Convert.ToDecimal(dt.Rows[i]["POWERFACTOR"].ToString().Contains('E') ? "0" : dt.Rows[i]["POWERFACTOR"].ToString()) - Convert.ToDecimal(dt.Rows[j]["POWERFACTOR"].ToString().Contains('E') ? "0" : dt.Rows[j]["POWERFACTOR"].ToString());
                        decimal val3 = Convert.ToDecimal(dt.Rows[i]["ACTIVEENERGYDELIVERED"].ToString()) - Convert.ToDecimal(dt.Rows[j]["ACTIVEENERGYDELIVERED"].ToString());


                        MeterName.Add(dt.Rows[i]["MeterName"].ToString());
                        SYNCDATETIME.Add(Convert.ToDateTime(dt.Rows[i]["SYNCDATETIME"]).ToString("yyyy-MM-dd HH:mm:ss"));
                        maxDemands.Add(val1.ToString(".00").Replace('-', ' '));
                        powerFactors.Add(val2.ToString(".00").Replace('-', ' '));
                        activeEnergies.Add(val3.ToString(".00").Replace('-', ' '));
                    }
                }

                valuess.MeterName = MeterName.ToArray();
                valuess.SYNCDATETIME = SYNCDATETIME.ToArray();
                valuess.MaxDemand = maxDemands.ToArray();
                valuess.PowerFactor = powerFactors.ToArray();
                valuess.ActiveEnergy = activeEnergies.ToArray();
            }
            return Json(valuess);
        }

        /// Viewing Graph Methods End

        /// Viewing Over All Methods Starts

        public ActionResult OverAllIndex()
        {
            return View();
        }

        /// Viewing Over All Methods End
        private DataTable ExportingData(string Qry, SqlParameter[] parameters)
        {
            try
            {
                DataTable dt = new DataTable();
                var connectionString = ConfigurationManager.AppSettings["DbConnection"].ToString();
                using (var _sqlCon = new SqlConnection(connectionString))
                {
                    _sqlCon.Open();
                    // Modify your SQL query to include parameters for fromDate and toDate
                    SqlCommand cmd = new SqlCommand(Qry, _sqlCon);
                    cmd.Parameters.AddRange(parameters);

                    SqlDataAdapter _da = new SqlDataAdapter(cmd);
                    _da.Fill(dt);
                }
                return dt;
            }
            catch (Exception)
            {
                return new DataTable();
            }
        }

        private DataTable GetDataUsingDate(string Qry, SqlParameter[] parameters)
        {
            try
            {
                DataTable dt = new DataTable();
                var connectionString = ConfigurationManager.AppSettings["DbConnection"].ToString();
                using (var _sqlCon = new SqlConnection(connectionString))
                {
                    _sqlCon.Open();
                    // Modify your SQL query to include parameters for fromDate and toDate
                    SqlCommand cmd = new SqlCommand(Qry, _sqlCon);
                    cmd.Parameters.AddRange(parameters);

                    SqlDataAdapter _da = new SqlDataAdapter(cmd);
                    _da.Fill(dt);
                }
                return dt;
            }
            catch (Exception)
            {
                return new DataTable();
            }
        }

        private DataTable GetData(string Qry)
        {
            try
            {
                DataTable dt = new DataTable();
                var connectionString = ConfigurationManager.AppSettings["DbConnection"].ToString();
                using (var _sqlCon = new SqlConnection(connectionString))
                {
                    _sqlCon.Open();
                    SqlDataAdapter _da = new SqlDataAdapter(Qry, _sqlCon);
                    _da.Fill(dt);
                }
                return dt;


            }
            catch (Exception)
            {

                return new DataTable();
            }
        }

    }
}