using DocumentFormat.OpenXml.Wordprocessing;
using PSAPLCDashboard.Web.Dashboard.Controllers;
using PSAPLCDashboard.Web.Dashboard.Extras;
using PSAPLCDashboard.Web.Dashboard.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;

namespace Kubota.Web.Dashboard.Controllers
{
    public class GraphController : Controller
    {
        DataSyncServer _serve = new DataSyncServer();
        // GET: Graph

        public ActionResult TestGraph()
        {
            // Sample data
            var data = new List<ChartData>
        {
            new ChartData { Date = DateTime.Now.AddDays(-2), PowerFactor = 0.95, MaxDemand = 150, ActiveEnergy = 1200 },
            new ChartData { Date = DateTime.Now.AddDays(-1), PowerFactor = 0.96, MaxDemand = 160, ActiveEnergy = 1300 },
            new ChartData { Date = DateTime.Now, PowerFactor = 0.97, MaxDemand = 170, ActiveEnergy = 1400 }
        };

            return View(data);
        }
        public ActionResult GraphIndex(string strId, string strName)
        {
            ViewBag.Id = strId;
            ViewBag.Name = strName;
            return View();
        }


        [HttpPost]
        public ActionResult GetGraphData(string strId, string strFD, string strTD)
        {
            string BQ = string.Empty;
            TestModel overAll = new TestModel();
            try
            {

                if (strFD != "" && strTD != "")
                {
                    DateTime fDate = DateTime.Parse(strFD);
                    DateTime tDate = DateTime.Parse(strTD);
                    BQ = "  AND CONVERT(DATETIME,SYNCDATETIME,103) BETWEEN CONVERT(DATETIME,'" + fDate + "',103) AND CONVERT(DATETIME,'" + tDate + "',103) ";
                }
                else
                {
                    var datetime = DateTime.Parse(DateTime.Now.ToString());
                    BQ = "  AND CONVERT(DATE,SYNCDATETIME,103) = CONVERT(DATE,'" + datetime + "',103)  ";
                }

                string Qry = "SELECT CURRENTA ,CURRENTB,CURRENTC,VOLTAGEAB,VOLTAGEBC,VOLTAGECA,MAXDEMAND,POWERFACTOR,ACTIVEENERGYDELIVERED FROM TBL_ENERGYMETER WHERE " +
                    "METERID='" + strId + "'" + BQ + " ";
                DataTable dt = _serve.GetDataTable(Qry);
                if (dt.Rows.Count > 0)
                {
                    List<string> maxdemand = new List<string>();
                    List<string> powerfactor = new List<string>();
                    List<string> activeenergy = new List<string>();

                    int j = 0;

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        j = i + 1;
                        if (j < dt.Rows.Count)
                        {
                            decimal val1 = Convert.ToDecimal(dt.Rows[i]["MAXDEMAND"].ToString());
                            decimal val2 = Convert.ToDecimal(dt.Rows[i]["POWERFACTOR"].ToString().Contains('E') ? "0" : dt.Rows[i]["POWERFACTOR"].ToString());
                            decimal val3 = Convert.ToDecimal(dt.Rows[i]["ACTIVEENERGYDELIVERED"].ToString()) - Convert.ToDecimal(dt.Rows[j]["ACTIVEENERGYDELIVERED"].ToString());

                            maxdemand.Add(val1.ToString(".00").Replace('-', ' '));
                            powerfactor.Add(val2.ToString(".00").Replace('-', ' '));
                            activeenergy.Add(val3.ToString(".00").Replace('-', ' '));
                        }

                    }
                    overAll.MaxDemand = maxdemand.ToArray();
                    overAll.PowerFactor = powerfactor.ToArray();
                    overAll.ActiveEnergy = activeenergy.ToArray();
                }
                return Json(overAll);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public ActionResult LstGraphIndex(List<string> strId, string strName, string strFdate, string strTdate)
        {


            ViewBag.Id = strId;
            ViewBag.Name = strName;
            ViewBag.Fdate = strFdate;
            ViewBag.Tdate = strTdate;
            return View();
        }

        [HttpPost]
        public ActionResult GetGraphDataList(List<string> lstId, string strFD, string strTD)
        {
            string BQ = string.Empty;
            TestModel overAll = new TestModel();
            List<TestModel> listOverAll = new List<TestModel>();
            try
            {
                string[] ids = lstId[0].ToString().Split(',');

                if (strFD != "" && strTD != "")
                {
                    DateTime fDate = DateTime.Parse(strFD);
                    DateTime tDate = DateTime.Parse(strTD);
                    BQ = "  AND CONVERT(DATETIME,SYNCDATETIME,103) BETWEEN CONVERT(DATETIME,'" + fDate + "',103) AND CONVERT(DATETIME,'" + tDate + "',103) ";
                }
                else
                {
                    var datetime = DateTime.Parse(DateTime.Now.ToString());
                    BQ = "  AND CONVERT(DATE,SYNCDATETIME,103) = CONVERT(DATE,'" + datetime + "',103)  ";
                }


                foreach (var id in ids)
                {
                    string Qry = "SELECT CURRENTA ,CURRENTB,CURRENTC,VOLTAGEAB,VOLTAGEBC,VOLTAGECA,MAXDEMAND,POWERFACTOR,ACTIVEENERGYDELIVERED FROM TBL_ENERGYMETER WHERE " +
                    "METERID='" + id + "'" + BQ + " ";
                    DataTable dt = _serve.GetDataTable(Qry);
                    if (dt.Rows.Count > 0)
                    {
                        List<string> maxdemand = new List<string>();
                        List<string> powerfactor = new List<string>();
                        List<string> activeenergy = new List<string>();

                        int j = 0;

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            j = i + 1;
                            if (j < dt.Rows.Count)
                            {
                                decimal val1 = Convert.ToDecimal(dt.Rows[i]["MAXDEMAND"].ToString());
                                decimal val2 = Convert.ToDecimal(dt.Rows[i]["POWERFACTOR"].ToString().Contains('E') ? "0" : dt.Rows[i]["POWERFACTOR"].ToString());
                                decimal val3 = Convert.ToDecimal(dt.Rows[i]["ACTIVEENERGYDELIVERED"].ToString()) - Convert.ToDecimal(dt.Rows[j]["ACTIVEENERGYDELIVERED"].ToString());

                                maxdemand.Add(val1.ToString(".00").Replace('-', ' '));
                                powerfactor.Add(val2.ToString(".00").Replace('-', ' '));
                                activeenergy.Add(val3.ToString(".00").Replace('-', ' '));
                            }

                        }
                        overAll.MaxDemand = maxdemand.ToArray();
                        overAll.PowerFactor = powerfactor.ToArray();
                        overAll.ActiveEnergy = activeenergy.ToArray();
                    }
                    listOverAll.Add(overAll);
                }
                return Json(listOverAll);
            }
            catch (Exception ex)
            {
                return Json(ex);
            }
        }


        [HttpPost]
        public JsonResult GetMetersIds(string meterId)
        {
            try
            {
                var listD = new List<Tuple<string, string>>();
                string qry = "SELECT METERID,METERNAME,GROUPID FROM METERMASTER WHERE GROUPID = (select GROUPID from METERMASTER where METERID='" + meterId + "' And FLAG=1) AND FLAG=1";
                var data = _serve.GetDataTable(qry);
                if (data != null)
                {
                    for (int i = 0; i < data.Rows.Count; i++)
                    {
                        var id = data.Rows[i]["METERID"].ToString();
                        var name = data.Rows[i]["METERNAME"].ToString();
                        listD.Add(Tuple.Create(name.ToString(), id.ToString()));
                    }

                }
                return Json(listD);
            }
            catch (Exception ex)
            {
                return Json(ex);
            }
        }

        public ActionResult OverAllGraphIndex(string strFD, string strTD, string groupName)
        {
            // Split the groupName to extract the groupId
            string[] parts = groupName.Split('-');

            if (parts.Length == 2 && int.TryParse(parts[1], out int groupId))
            {
                ViewBag.Id = groupId;
                ViewBag.Name = parts[0]; // Extracted group name without the ID
                ViewBag.FD = strFD;
                ViewBag._TD = strTD;
            }
            else
            {
                // Handle the case where groupName is not in the expected format
                ViewBag.Id = 0;
                ViewBag.Name = groupName;
                ViewBag.FD = strFD;
                ViewBag._TD = strTD;
            }

            return View();
        }

        /*
        [HttpPost]
        public ActionResult GetOverAllGraphData(string groupId, string strFD, string strTD, string groupName)
        {
            string BQ = string.Empty;
            TestModel overAll = new TestModel();
            try
            {

                if (strFD != "" && strTD != "" && groupName != "" && groupId != "")
                {
                    DateTime fDate = DateTime.Parse(strFD);
                    DateTime tDate = DateTime.Parse(strTD);
                    BQ = "  AND CONVERT(DATETIME,SYNCDATETIME,103) BETWEEN CONVERT(DATETIME,'" + fDate + "',103) AND CONVERT(DATETIME,'" + tDate + "',103) ";
                }
                else
                {
                    var datetime = DateTime.Parse(DateTime.Now.ToString());
                    BQ = "  AND CONVERT(DATE,SYNCDATETIME,103) = CONVERT(DATE,'" + datetime + "',103)  ";
                }

                string Qry = "SELECT CURRENTA ,CURRENTB,CURRENTC,VOLTAGEAB,VOLTAGEBC,VOLTAGECA,MAXDEMAND,POWERFACTOR,ACTIVEENERGYDELIVERED FROM TBL_ENERGYMETER WHERE " +
                    "GROUPID='" + groupId + "'" + BQ + " ";
                DataTable dt = _serve.GetDataTable(Qry);
                if (dt.Rows.Count > 0)
                {
                    List<string> maxdemand = new List<string>();
                    List<string> powerfactor = new List<string>();
                    List<string> activeenergy = new List<string>();

                    int j = 0;

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        j = i + 1;
                        if (j < dt.Rows.Count)
                        {
                            decimal val1 = Convert.ToDecimal(dt.Rows[i]["MAXDEMAND"].ToString());
                            decimal val2 = Convert.ToDecimal(dt.Rows[i]["POWERFACTOR"].ToString().Contains('E') ? "0" : dt.Rows[i]["POWERFACTOR"].ToString());
                            decimal val3 = Convert.ToDecimal(dt.Rows[i]["ACTIVEENERGYDELIVERED"].ToString()) - Convert.ToDecimal(dt.Rows[j]["ACTIVEENERGYDELIVERED"].ToString());

                            maxdemand.Add(val1.ToString(".00").Replace('-', ' '));
                            powerfactor.Add(val2.ToString(".00").Replace('-', ' '));
                            activeenergy.Add(val3.ToString(".00").Replace('-', ' '));
                        }

                    }
                    overAll.MaxDemand = maxdemand.ToArray();
                    overAll.PowerFactor = powerfactor.ToArray();
                    overAll.ActiveEnergy = activeenergy.ToArray();
                }
                return Json(overAll);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        */

        [HttpPost]
        public ActionResult GetOverAllIndex_Graph_All(string groupId, string strMeterId, string strFD, string strTD, int Interval)
        {
            string MQ = string.Empty;
            string BQ = string.Empty;
            string Qry = string.Empty;
            string query = string.Empty;

            var startDate = DateTime.Now;
            var endDate = DateTime.Now;
            TestModel overAll = new TestModel();
            try
            {
                if (groupId != "")
                {

                    query = "SELECT GROUPID,GROUPNAME FROM GROUPMASTER WHERE FLAG='1'";
                    DataTable dt1 = _serve.GetDataTable(query);
                    if (dt1.Rows.Count > 0)
                    {
                        for (int k = 0; k < dt1.Rows.Count; k++)
                        {
                            string Groupid = dt1.Rows[k][0].ToString();


                            if (strMeterId == "ALLMETERS" && groupId == "ALLGROUPS")
                            {
                                MQ = "E.GROUPID = '" + Groupid + "'";
                            }
                            else if (strMeterId != "ALLMETERS" && groupId == "ALLGROUPS")
                            {
                                MQ = "E.GROUPID = '" + groupId + "' AND E.METERID = '" + strMeterId + "' ";

                            }


                            if (strFD != "" && strTD != "")
                            {
                                startDate = DateTime.Parse(strFD);
                                endDate = DateTime.Parse(strTD);

                            }
                            else
                            {
                                DateTime startOfToday = DateTime.Today;
                                DateTime endOfToday = DateTime.Today.AddDays(1).AddTicks(-1);
                                startDate = startOfToday;
                                endDate = endOfToday;
                            }

                            Qry = " WITH TIMEINTERVALS AS (SELECT CONVERT(DATETIME, '" + startDate + "', 103) AS INTERVALTIME  UNION ALL  SELECT DATEADD(MINUTE, " + Interval + ", INTERVALTIME)  " +
                                  " FROM TIMEINTERVALS  WHERE DATEADD(MINUTE, " + Interval + ", INTERVALTIME) <= CONVERT(DATETIME, '" + endDate + "', 103)) " +
                                  " SELECT T.INTERVALTIME AS DATETIMES,E.CURRENTA,E.CURRENTB,E.CURRENTC,E.VOLTAGEAB,E.VOLTAGEBC,E.VOLTAGECA,E.MAXDEMAND,E.POWERFACTOR,E.ACTIVEENERGYDELIVERED AS KWH " +
                                  " FROM TIMEINTERVALS T LEFT JOIN TBL_ENERGYMETER E ON CAST(CONVERT(DATETIME, E.SYNCDATETIME, 103) AS SMALLDATETIME) = CAST(T.INTERVALTIME AS SMALLDATETIME) " +
                                  " WHERE  " + MQ + "  ORDER BY T.INTERVALTIME ASC  OPTION (MAXRECURSION 32767); ";

                            DataTable dt = _serve.GetDataTable(Qry);
                            if (dt.Rows.Count > 0)
                            {
                                List<string> syncdatetime = new List<string>();
                                List<string> activeenergy = new List<string>();

                                int j = 0;

                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    j = i + 1;
                                    if (j < dt.Rows.Count)
                                    {
                                        decimal val3 = Convert.ToDecimal(dt.Rows[j]["KWH"].ToString()) - Convert.ToDecimal(dt.Rows[i]["KWH"].ToString());
                                        syncdatetime.Add(dt.Rows[i]["DATETIMES"].ToString());
                                        activeenergy.Add(val3.ToString(".00").Replace('-', ' '));
                                    }

                                }
                                overAll.SYNCDATETIME = syncdatetime.ToArray();

                                if (Groupid == "GBS")
                                {
                                    overAll.BODY_SHOP_ActiveEnergy = activeenergy.ToArray();
                                }
                                else if (Groupid == "GGA")
                                {
                                    overAll.GENERAL_ASSEMBLY_ActiveEnergy = activeenergy.ToArray();
                                }
                                else if (Groupid == "GHS")
                                {
                                    overAll.HOSTED_SERVICES_ActiveEnergy = activeenergy.ToArray();
                                }
                                else if (Groupid == "GPS")
                                {
                                    overAll.PAINT_SHOP_ActiveEnergy = activeenergy.ToArray();
                                }
                                else if (Groupid == "GTW")
                                {
                                    overAll.TRANSFORMER_WISE_ActiveEnergy = activeenergy.ToArray();
                                }
                                else if (Groupid == "GU")
                                {
                                    overAll.UTILITIES_ActiveEnergy = activeenergy.ToArray();
                                }

                            }

                            //k = k + 1;
                        }
                    }
                    //MQ = "GROUPID = '" + groupId + "' AND METERID = '" + strMeterId + "'";
                    //MQ = "GROUPID = '" + groupId + "'";
                }
                //else
                //{
                //    MQ = "METERID = '" + strMeterId + "'";
                //}

                return Json(overAll);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, "Internal server error: " + ex.Message); ;
            }
        }


        [HttpPost]
        public ActionResult GetOverAllIndex_Graph(string groupId, string strMeterId, string strFD, string strTD, int Interval)
        {
            string MQ = string.Empty;
            string BQ = string.Empty;
            string Qry = string.Empty;
            TestModel overAll = new TestModel();
            var startDate = DateTime.Now;
            var endDate = DateTime.Now;
            try
            {
                if (groupId != "" && strMeterId != "")
                {
                    MQ = "E.GROUPID = '" + groupId + "' AND E.METERID = '" + strMeterId + "'";
                }
                else
                {
                    MQ = "E.METERID = '" + strMeterId + "'";
                }


                if (strFD != "" && strTD != "")
                {
                    startDate = DateTime.Parse(strFD);
                    endDate = DateTime.Parse(strTD);

                }
                else
                {
                    DateTime startOfToday = DateTime.Today;
                    DateTime endOfToday = DateTime.Today.AddDays(1).AddTicks(-1);
                    startDate = startOfToday;
                    endDate = endOfToday;
                }

                Qry = " WITH TIMEINTERVALS AS (SELECT CONVERT(DATETIME, '" + startDate + "', 103) AS INTERVALTIME  UNION ALL  SELECT DATEADD(MINUTE, " + Interval + ", INTERVALTIME)   " +
                      " FROM TIMEINTERVALS  WHERE DATEADD(MINUTE, " + Interval + ", INTERVALTIME) <= CONVERT(DATETIME, '" + endDate + "', 103)) " +
                      " SELECT T.INTERVALTIME AS DATETIMES,E.CURRENTA,E.CURRENTB,E.CURRENTC,E.VOLTAGEAB,E.VOLTAGEBC,E.VOLTAGECA,E.MAXDEMAND,E.POWERFACTOR,E.ACTIVEENERGYDELIVERED AS KWH " +
                      " FROM TIMEINTERVALS T LEFT JOIN TBL_ENERGYMETER E ON CAST(CONVERT(DATETIME, E.SYNCDATETIME, 103) AS SMALLDATETIME) = CAST(T.INTERVALTIME AS SMALLDATETIME) " +
                      " WHERE  " + MQ + "  ORDER BY T.INTERVALTIME ASC  OPTION (MAXRECURSION 32767); ";

                DataTable dt = _serve.GetDataTable(Qry);
                if (dt.Rows.Count > 0)
                {

                    List<string> syncdatetime = new List<string>();
                    List<string> activeenergy = new List<string>();

                    int j = 0;

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        j = i + 1;
                        if (j < dt.Rows.Count)
                        {
                            decimal val3 = Convert.ToDecimal(dt.Rows[j]["KWH"].ToString()) - Convert.ToDecimal(dt.Rows[i]["KWH"].ToString());
                            syncdatetime.Add(dt.Rows[i]["DATETIMES"].ToString());
                            activeenergy.Add(val3.ToString(".00"));
                        }

                    }
                    overAll.SYNCDATETIME = syncdatetime.ToArray();
                    overAll.ActiveEnergy = activeenergy.ToArray();
                }
                return Json(overAll);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, "Internal server error: " + ex.Message); ;
            }
        }



        [HttpPost]
        public ActionResult GetOverAllGraphData(string groupId, string strFD, string strTD, string groupName, int Interval)
        {
            string BQ = string.Empty;
            string Qry = string.Empty;
            TestModel overAll = new TestModel();
            try
            {
                //var startDate = "";
                //var endDate = "";
                var startDate = DateTime.Now;
                var endDate = DateTime.Now;
                if (strFD != "" && strTD != "" && groupId != "")
                {
                    startDate = DateTime.Parse(strFD);//.ToString("yyyy-MM-dd hh:mm:ss");
                    endDate = DateTime.Parse(strTD);//.ToString("yyyy-MM-dd hh:mm:ss");                    
                }
                else
                {
                    DateTime startOfToday = DateTime.Today;
                    DateTime endOfToday = DateTime.Today.AddDays(1).AddTicks(-1);
                    startDate = startOfToday;//.ToString("yyyy-MM-dd hh:mm:ss");
                    endDate = endOfToday;//.ToString("yyyy-MM-dd hh:mm:ss");
                }

                //Qry = "WITH TIMEINTERVALS AS (SELECT CONVERT(DATETIME, '" + startDate + "' , 120) AS INTERVALTIME UNION ALL SELECT DATEADD(MINUTE, " + Interval + ", INTERVALTIME) " +
                //    "FROM TIMEINTERVALS WHERE DATEADD(MINUTE, " + Interval + ", INTERVALTIME) <= CONVERT(DATETIME, '" + endDate + "', 120))  " +
                //    "SELECT T.INTERVALTIME AS DATETIMES,E.CURRENTA,E.CURRENTB,E.CURRENTC,E.VOLTAGEAB,E.VOLTAGEBC,E.VOLTAGECA,E.MAXDEMAND,E.POWERFACTOR,E.ACTIVEENERGYDELIVERED AS KWH FROM " +
                //    "TIMEINTERVALS T LEFT JOIN (SELECT CONVERT(DATETIME, SYNCDATETIME, 103) AS SYNCDATETIME,CURRENTA,CURRENTB,CURRENTC,VOLTAGEAB,VOLTAGEBC,VOLTAGECA,MAXDEMAND,POWERFACTOR,ACTIVEENERGYDELIVERED,METERID " +
                //    "FROM TBL_ENERGYMETER WHERE METERID = '" + groupId + "' ) E ON CAST(CONVERT(DATETIME, E.SYNCDATETIME, 103) AS SMALLDATETIME) = CAST(T.INTERVALTIME AS SMALLDATETIME) " +
                //    "ORDER BY T.INTERVALTIME ASC OPTION (MAXRECURSION 32767);";

                Qry = "WITH TIMEINTERVALS AS (SELECT CONVERT(DATETIME, '" + startDate + "', 103) AS INTERVALTIME  UNION ALL  SELECT DATEADD(MINUTE, " + Interval + ", INTERVALTIME)   FROM TIMEINTERVALS  WHERE DATEADD(MINUTE, " + Interval + ", INTERVALTIME) <= CONVERT(DATETIME, '" + endDate + "', 103)) " +
                    "SELECT T.INTERVALTIME AS DATETIMES,E.CURRENTA,E.CURRENTB,E.CURRENTC,E.VOLTAGEAB,E.VOLTAGEBC,E.VOLTAGECA,E.MAXDEMAND,E.POWERFACTOR,E.ACTIVEENERGYDELIVERED AS KWH " +
                    "FROM TIMEINTERVALS T LEFT JOIN TBL_ENERGYMETER E ON CAST(CONVERT(DATETIME, E.SYNCDATETIME, 103) AS SMALLDATETIME) = CAST(T.INTERVALTIME AS SMALLDATETIME) " +
                    "WHERE  METERID = '" + groupId + "'  ORDER BY T.INTERVALTIME ASC  OPTION (MAXRECURSION 32767); ";

                DataTable dt = _serve.GetDataTable(Qry);
                if (dt.Rows.Count > 0)
                {

                    List<string> syncdatetime = new List<string>();
                    List<string> maxdemand = new List<string>();
                    List<string> powerfactor = new List<string>();
                    List<string> activeenergy = new List<string>();

                    int j = 0;

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        j = i + 1;
                        if (j < dt.Rows.Count)
                        {
                            decimal val1 = Convert.ToDecimal(dt.Rows[i]["MAXDEMAND"].ToString());
                            decimal val2 = Convert.ToDecimal(dt.Rows[i]["POWERFACTOR"].ToString().Contains('E') ? "0" : dt.Rows[i]["POWERFACTOR"].ToString());
                            decimal val3 = Convert.ToDecimal(dt.Rows[j]["KWH"].ToString()) - Convert.ToDecimal(dt.Rows[i]["KWH"].ToString());
                            syncdatetime.Add(dt.Rows[i]["DATETIMES"].ToString());
                            maxdemand.Add(val1.ToString(".00"));
                            powerfactor.Add(val2.ToString(".00"));
                            activeenergy.Add(val3.ToString(".00"));
                        }

                    }
                    overAll.SYNCDATETIME = syncdatetime.ToArray();
                    overAll.MaxDemand = maxdemand.ToArray();
                    overAll.PowerFactor = powerfactor.ToArray();
                    overAll.ActiveEnergy = activeenergy.ToArray();
                }
                return Json(overAll);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, "Internal server error: " + ex.Message); ;
            }
        }

        HomeController _HomeController = new HomeController();

        [HttpPost]
        public ActionResult GetGroupGraph(string groupId, int Interval)
        {
            string Qry = string.Empty;
            List<string> Cons = new List<string>();
            List<string> Days = new List<string>();
            try
            {
                var startDate = DateTime.Now;
                var endDate = DateTime.Now;

                if (groupId != "" && Interval != 0)
                {
                    for (int k = Interval; k > 0; k--)
                    {
                        startDate = DateTime.Today.AddDays(-k);
                        endDate = DateTime.Today.AddDays(1).AddTicks(-(k - 1));
                        var Consumption = "";
                        Qry = "SELECT METERID FROM METERMASTER WHERE GROUPID='" + groupId + "' AND FLAG = 1;";
                        var meters = _serve.GetDataTable(Qry);
                        if (meters.Rows.Count > 0)
                        {
                            for (int i = 0; i < meters.Rows.Count; i++)
                            {
                                string meter = meters.Rows[i].ToString();
                                Consumption = _HomeController.GetConsumptions(meter, startDate.ToString(), endDate.ToString(), "M");
                            }
                            Cons.Add(Consumption);
                            Days.Add(startDate.ToString("dd-MM-yyyy"));
                        }

                    }
                }
                var res = new { days = Days, consumption = Cons };
                return Json(res);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, "Internal server error: " + ex.Message); ;
            }
        }


        /*----------------------------------TABLE-------------------------------------------*/

        public ActionResult TableSwap(string id, string fd, string td)
        {
            ViewBag.id = id;
            ViewBag.fd = fd;
            ViewBag.td = td;
            string qry = "  SELECT METERNAME FROM METERMASTER WHERE METERID ='" + id + "'";
            DataTable dt = _serve.GetDataTable(qry);
            if (dt.Rows.Count > 0)
            {
                ViewBag.MeterName = dt.Rows[0]["METERNAME"].ToString();
            }
            else
            {
                ViewBag.MeterName = "Meter";
            }
            return View();
        }

        [HttpPost]
        public JsonResult GetTableData_Overall(string strGroup, string strFD, string strTD)
        {
            string BQ = string.Empty;
            decimal Ov_Cons = 0;
            try
            {
                if (strFD != "" && strTD != "")
                {
                    DateTime fDate = DateTime.Parse(strFD);
                    DateTime tDate = DateTime.Parse(strTD);
                    BQ = "  AND CONVERT(DATETIME,SYNCDATETIME,103) BETWEEN CONVERT(DATETIME,'" + fDate + "',103) AND CONVERT(DATETIME,'" + tDate + "',103) ";
                }
                else
                {
                    var datetime = DateTime.Parse(DateTime.Now.ToString());
                    //BQ = "  AND CONVERT(DATE,SYNCDATETIME) = CONVERT(DATE,'08-07-2024')  ";
                    BQ = "  AND CONVERT(DATE,SYNCDATETIME,103) = CONVERT(DATE,'" + datetime + "',103)  ";
                }

                string Qry = "SELECT MM.METERNAME,SYNCDATETIME, CURRENTA ,CURRENTB,CURRENTC,VOLTAGEAB,VOLTAGEBC,VOLTAGECA,MAXDEMAND,POWERFACTOR,ACTIVEENERGYDELIVERED ,SYNCDATETIME" +
                    " FROM TBL_ENERGYMETER EM LEFT JOIN METERMASTER MM ON EM.METERID = MM.METERID WHERE EM.GROUPID='" + strGroup + "' " + BQ + "";
                DataTable dt = _serve.GetDataTable(Qry);
                if (dt.Rows.Count > 0)
                {
                    List<string> consumption = new List<string>();

                    int j = 0;

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        j = i + 1;
                        if (j < dt.Rows.Count)
                        {
                            decimal Con = Convert.ToDecimal(dt.Rows[i]["ACTIVEENERGYDELIVERED"].ToString()) - Convert.ToDecimal(dt.Rows[j]["ACTIVEENERGYDELIVERED"].ToString());
                            consumption.Add(Con.ToString(".00"));
                        }

                    }


                    if (consumption.Count > 0)
                    {
                        foreach (var item in consumption)
                        {
                            Ov_Cons = Ov_Cons + Convert.ToDecimal(item);
                        }
                    }



                }

                var retRes = new
                {
                    group = strGroup,
                    datetime = strFD + " - " + strTD,
                    consumption = Ov_Cons
                };

                return Json(retRes);
            }
            catch (Exception ex)
            {
                return Json(ex);
            }
        }


        [HttpPost]
        public JsonResult GetTableData(string strId, string strFD, string strTD)
        {
            string BQ = string.Empty;
            EnergyMeterModel overAll = new EnergyMeterModel();
            try
            {
                if (strFD != "" && strTD != "")
                {
                    DateTime fDate = DateTime.Parse(strFD);
                    DateTime tDate = DateTime.Parse(strTD);
                    BQ = "  AND CONVERT(DATETIME,SYNCDATETIME,103) BETWEEN CONVERT(DATETIME,'" + fDate + "',103) AND CONVERT(DATETIME,'" + tDate + "',103) ";
                }
                else
                {
                    var datetime = DateTime.Parse(DateTime.Now.ToString());
                    //BQ = "  AND CONVERT(DATE,SYNCDATETIME) = CONVERT(DATE,'08-07-2024')  ";
                    BQ = "  AND CONVERT(DATE,SYNCDATETIME,103) = CONVERT(DATE,'" + datetime + "',103)  ";
                }

                string Qry = "SELECT MM.METERNAME,SYNCDATETIME, CURRENTA ,CURRENTB,CURRENTC,VOLTAGEAB,VOLTAGEBC,VOLTAGECA,MAXDEMAND,POWERFACTOR,ACTIVEENERGYDELIVERED ,SYNCDATETIME" +
                    " FROM TBL_ENERGYMETER EM LEFT JOIN METERMASTER MM ON EM.METERID = MM.METERID WHERE EM.METERID='" + strId + "' " + BQ + "";
                DataTable dt = _serve.GetDataTable(Qry);
                if (dt.Rows.Count > 0)
                {
                    List<string> metername = new List<string>();
                    List<string> dateAndTime = new List<string>();
                    List<string> currentA = new List<string>();
                    List<string> currentB = new List<string>();
                    List<string> currentC = new List<string>();
                    List<string> voltageA = new List<string>();
                    List<string> voltageB = new List<string>();
                    List<string> voltageC = new List<string>();
                    List<string> maxdemand = new List<string>();
                    List<string> powerfactor = new List<string>();
                    List<string> activeenergy = new List<string>();
                    List<string> consumption = new List<string>();
                    List<string> syncdatetime = new List<string>();

                    int j = 0;

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        j = i + 1;
                        if (j < dt.Rows.Count)
                        {
                            decimal cA = Convert.ToDecimal(dt.Rows[i]["CURRENTA"].ToString());
                            decimal cB = Convert.ToDecimal(dt.Rows[i]["CURRENTB"].ToString());
                            decimal cC = Convert.ToDecimal(dt.Rows[i]["CURRENTC"].ToString());
                            decimal vA = Convert.ToDecimal(dt.Rows[i]["VOLTAGEAB"].ToString());
                            decimal vB = Convert.ToDecimal(dt.Rows[i]["VOLTAGEBC"].ToString());
                            decimal vC = Convert.ToDecimal(dt.Rows[i]["VOLTAGECA"].ToString());
                            decimal mD = Convert.ToDecimal(dt.Rows[i]["MAXDEMAND"].ToString());
                            decimal pF = Convert.ToDecimal(dt.Rows[i]["POWERFACTOR"].ToString().Contains('E') ? "0" : dt.Rows[i]["POWERFACTOR"].ToString());
                            decimal aED = Convert.ToDecimal(dt.Rows[i]["ACTIVEENERGYDELIVERED"].ToString());
                            decimal Con = Convert.ToDecimal(dt.Rows[i]["ACTIVEENERGYDELIVERED"].ToString()) - Convert.ToDecimal(dt.Rows[j]["ACTIVEENERGYDELIVERED"].ToString());


                            metername.Add(dt.Rows[i]["METERNAME"].ToString());
                            dateAndTime.Add(dt.Rows[i]["SYNCDATETIME"].ToString());
                            currentA.Add(cA.ToString(".00"));
                            currentB.Add(cB.ToString(".00"));
                            currentC.Add(cC.ToString(".00"));
                            voltageA.Add(vA.ToString(".00"));
                            voltageB.Add(vB.ToString(".00"));
                            voltageC.Add(vC.ToString(".00"));
                            maxdemand.Add(mD.ToString(".00").Replace('-', ' '));
                            powerfactor.Add(pF.ToString(".00").Replace('-', ' '));
                            activeenergy.Add(aED.ToString(".00").Replace('-', ' '));
                            consumption.Add(Con.ToString(".00").Replace('-', ' '));
                            syncdatetime.Add(dt.Rows[i]["SYNCDATETIME"].ToString());
                        }

                    }
                    overAll.MeterName = metername.ToArray();
                    overAll.DateAndTime = dateAndTime.ToArray();
                    overAll.Current_A = currentA.ToArray();
                    overAll.Current_B = currentB.ToArray();
                    overAll.Current_C = currentC.ToArray();
                    overAll.Voltage_A = voltageA.ToArray();
                    overAll.Voltage_B = voltageB.ToArray();
                    overAll.Voltage_C = voltageC.ToArray();
                    overAll.MaxDemand = maxdemand.ToArray();
                    overAll.PowerFactor = powerfactor.ToArray();
                    overAll.ActiveEnergy = activeenergy.ToArray();
                    overAll.Consumption = consumption.ToArray();
                    overAll.syncdatetime = syncdatetime.ToArray();
                }
                return Json(overAll);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public ActionResult GroupTableSwap(string id, string fd, string td)
        {
            ViewBag.id = id;
            ViewBag.fd = fd;
            ViewBag.td = td;
            string qry = " SELECT GROUPNAME FROM GROUPMASTER WHERE GROUPID='" + id + "' AND FLAG=1";
            DataTable dt = _serve.GetDataTable(qry);
            if (dt.Rows.Count > 0)
            {
                ViewBag.GroupName = dt.Rows[0]["GROUPNAME"].ToString();
            }
            else
            {
                ViewBag.GroupName = "Group Data";
            }
            return View();
        }

        [HttpPost]
        public JsonResult GetGroupTableData(string strId, string strFD, string strTD)
        {
            string BQ = string.Empty;
            EnergyMeterModel overAll = new EnergyMeterModel();
            try
            {
                if (strFD != "" && strTD != "")
                {
                    DateTime fDate = DateTime.Parse(strFD);
                    DateTime tDate = DateTime.Parse(strTD);
                    BQ = "  AND CONVERT(DATETIME,SYNCDATETIME,103) BETWEEN CONVERT(DATETIME,'" + fDate + "',103) AND CONVERT(DATETIME,'" + tDate + "',103) ";
                }
                else
                {
                    var datetime = DateTime.Parse(DateTime.Now.ToString());
                    BQ = "  AND CONVERT(DATE,SYNCDATETIME,103) = CONVERT(DATE,'" + datetime + "',103)  ";
                }

                string Qry = "SELECT MM.METERNAME,SYNCDATETIME, CURRENTA ,CURRENTB,CURRENTC,VOLTAGEAB,VOLTAGEBC,VOLTAGECA,MAXDEMAND,POWERFACTOR,ACTIVEENERGYDELIVERED " +
                    " FROM TBL_ENERGYMETER EM LEFT JOIN METERMASTER MM ON EM.METERID = MM.METERID WHERE EM.GroupId='" + strId + "'" + BQ + " ";
                DataTable dt = _serve.GetDataTable(Qry);
                if (dt.Rows.Count > 0)
                {
                    List<string> metername = new List<string>();
                    List<string> dateAndTime = new List<string>();
                    List<string> currentA = new List<string>();
                    List<string> currentB = new List<string>();
                    List<string> currentC = new List<string>();
                    List<string> voltageA = new List<string>();
                    List<string> voltageB = new List<string>();
                    List<string> voltageC = new List<string>();
                    List<string> maxdemand = new List<string>();
                    List<string> powerfactor = new List<string>();
                    List<string> activeenergy = new List<string>();
                    List<string> consumption = new List<string>();

                    int j = 0;

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        j = i + 1;
                        if (j < dt.Rows.Count)
                        {
                            decimal cA = Convert.ToDecimal(dt.Rows[i]["CURRENTA"].ToString());
                            decimal cB = Convert.ToDecimal(dt.Rows[i]["CURRENTB"].ToString());
                            decimal cC = Convert.ToDecimal(dt.Rows[i]["CURRENTC"].ToString());
                            decimal vA = Convert.ToDecimal(dt.Rows[i]["VOLTAGEAB"].ToString());
                            decimal vB = Convert.ToDecimal(dt.Rows[i]["VOLTAGEBC"].ToString());
                            decimal vC = Convert.ToDecimal(dt.Rows[i]["VOLTAGECA"].ToString());
                            decimal mD = Convert.ToDecimal(dt.Rows[i]["MAXDEMAND"].ToString());
                            decimal pF = Convert.ToDecimal(dt.Rows[i]["POWERFACTOR"].ToString().Contains('E') ? "0" : dt.Rows[i]["POWERFACTOR"].ToString());
                            decimal aED = Convert.ToDecimal(dt.Rows[i]["ACTIVEENERGYDELIVERED"].ToString());
                            decimal Con = Convert.ToDecimal(dt.Rows[i]["ACTIVEENERGYDELIVERED"].ToString()) - Convert.ToDecimal(dt.Rows[j]["ACTIVEENERGYDELIVERED"].ToString());

                            metername.Add(dt.Rows[i]["METERNAME"].ToString());
                            dateAndTime.Add(dt.Rows[i]["SYNCDATETIME"].ToString());
                            currentA.Add(cA.ToString(".00"));
                            currentB.Add(cB.ToString(".00"));
                            currentC.Add(cC.ToString(".00"));
                            voltageA.Add(vA.ToString(".00"));
                            voltageB.Add(vB.ToString(".00"));
                            voltageC.Add(vC.ToString(".00"));
                            maxdemand.Add(mD.ToString(".00").Replace('-', ' '));
                            powerfactor.Add(pF.ToString(".00").Replace('-', ' '));
                            activeenergy.Add(aED.ToString(".00").Replace('-', ' '));
                            consumption.Add(Con.ToString(".00").Replace('-', ' '));
                        }

                    }
                    overAll.MeterName = metername.ToArray();
                    overAll.DateAndTime = dateAndTime.ToArray();
                    overAll.Current_A = currentA.ToArray();
                    overAll.Current_B = currentB.ToArray();
                    overAll.Current_C = currentC.ToArray();
                    overAll.Voltage_A = voltageA.ToArray();
                    overAll.Voltage_B = voltageB.ToArray();
                    overAll.Voltage_C = voltageC.ToArray();
                    overAll.MaxDemand = maxdemand.ToArray();
                    overAll.PowerFactor = powerfactor.ToArray();
                    overAll.ActiveEnergy = activeenergy.ToArray();
                    overAll.Consumption = consumption.ToArray();
                }
                return Json(overAll);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /*----------------------------------TABLE-------------------------------------------*/
    }
}