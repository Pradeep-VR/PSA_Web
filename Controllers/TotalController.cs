﻿using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using PSAPLCDashboard.Web.Dashboard.Controllers;
using PSAPLCDashboard.Web.Dashboard.Extras;
using PSAPLCDashboard.Web.Dashboard.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Kubota.Web.Dashboard.Controllers
{
    public class TotalController : Controller
    {
        DataSyncServer _serve = new DataSyncServer();

        // GET: Total
        public ActionResult TotalIndex(string id)
        {
            ViewBag.Id = id;
            return View();
        }

        HomeController _Home = new HomeController();
        
        [HttpPost]
        public JsonResult GetGroupsData(string strGroup, string strStartD, string strEndD)
        {
            var stratDate = DateTime.Now;
            var endDate = DateTime.Now;
            var Consumption = "";

            List<string> GroupName = new List<string>();
            List<string> GroupCons = new List<string>();

            if (strStartD != "" && strEndD != "")
            {
                stratDate = DateTime.Parse(strStartD);
                endDate = DateTime.Parse(strEndD);
            }
            else
            {
                stratDate = DateTime.Today;
                endDate = DateTime.Today.AddDays(1).AddTicks(-1);
            }

            if (strGroup != "All")
            {
                decimal SpeCons = 0;
                string Qry1 = "SELECT METERID, METERNAME FROM METERMASTER WHERE GROUPID='" + strGroup + "' AND FLAG = 1;";
                var meters = _serve.GetDataTable(Qry1);

                if (meters.Rows.Count > 0)
                {
                    for (int j = 0; j < meters.Rows.Count; j++)
                    {
                        string meter = meters.Rows[j]["METERID"].ToString();
                        Consumption = _Home.GetConsumptions(meter, stratDate.ToString(), endDate.ToString(), "M");
                        var cons = Consumption == "" ? "0" : Consumption;
                        SpeCons += Convert.ToDecimal(cons);

                        GroupName.Add(meters.Rows[j]["METERNAME"].ToString());
                        GroupCons.Add(SpeCons.ToString("0.00"));
                    }
                }
            }
            else
            {
                string Qry = "SELECT GROUPID, GROUPNAME FROM GROUPMASTER WHERE FLAG=1;";
                var dt = _serve.GetDataTable(Qry);
                if (dt.Rows.Count > 0)
                {
                    decimal SpeCons = 0;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        string group = dt.Rows[i]["GROUPID"].ToString();
                        string Qry1 = "SELECT METERID, METERNAME FROM METERMASTER WHERE GROUPID='" + group + "' AND FLAG = 1;";
                        var meters = _serve.GetDataTable(Qry1);

                        if (meters.Rows.Count > 0)
                        {
                            for (int j = 0; j < meters.Rows.Count; j++)
                            {
                                string meter = meters.Rows[j]["METERID"].ToString();
                                Consumption = _Home.GetConsumptions(meter, stratDate.ToString(), endDate.ToString(), "M");
                                var cons = Consumption == "" ? "0" : Consumption;
                                SpeCons += Convert.ToDecimal(cons);
                            }
                        }

                        GroupName.Add(dt.Rows[i]["GROUPNAME"].ToString());
                        GroupCons.Add(SpeCons.ToString("0.00"));
                    }
                }
            }

            // Combine GroupName and GroupCons into tuples and sort
            var combinedList = GroupName.Zip(GroupCons, (name, cons) => new { Name = name, Cons = cons })
                                        .OrderByDescending(x => x.Cons)                                        
                                        .ToList();

            // Separate the sorted data back into GroupName and GroupCons
            GroupName = combinedList.Select(x => x.Name).ToList();
            GroupCons = combinedList.Select(x => x.Cons).ToList();

            var res = new
            {
                groupname = GroupName.ToArray(),
                groupcons = GroupCons.ToArray(),
            };

            return Json(res);
        }


        [HttpPost]
        public JsonResult GetMetersReadings(string strId, string strFDate, string strTDate)
        {

            try
            {
                DateTime fDate = DateTime.Parse(strFDate);
                DateTime tDate = DateTime.Parse(strTDate);

                List<string> kwh = new List<string>();
                string meterName = string.Empty;

                //string[] meterIds = strId.Split(',');
                //foreach(string meterId in meterIds)
                //{
                string Qry = "SELECT ACTIVEENERGYDELIVERED AS KWH , EM.METERID , MM.METERNAME FROM TBL_ENERGYMETER EM LEFT JOIN METERMASTER MM ON MM.METERID = EM.METERID " +
                    " WHERE EM.METERID = '" + strId + "' AND CONVERT(DATETIME, SYNCDATETIME,103) BETWEEN CONVERT(DATETIME,'" + fDate + "',103) " +
                    "AND CONVERT(DATETIME,'" + tDate + "',103) ORDER BY METERID";
                var dt = _serve.GetDataTable(Qry);
                if (dt.Rows.Count > 0)
                {
                    meterName = dt.Rows[0]["METERNAME"].ToString();
                    int j = 0;
                    // Adding data
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        j = i + 1;
                        if (j < dt.Rows.Count)
                        {
                            decimal val3 = Convert.ToDecimal(dt.Rows[i]["KWH"].ToString()) - Convert.ToDecimal(dt.Rows[j]["KWH"].ToString());
                            kwh.Add(val3.ToString(".00").Replace('-', ' '));
                        }
                    }
                }
                //}
                decimal meterCon = 0;
                if (kwh.Count > 0)
                {
                    foreach (var item in kwh)
                    {
                        meterCon = meterCon + Convert.ToDecimal(item);
                    }
                }

                return Json(new
                {
                    name = meterName,
                    data = meterCon,
                });
            }
            catch (Exception ex)
            {
                return Json(ex);
            }
        }
    }
}