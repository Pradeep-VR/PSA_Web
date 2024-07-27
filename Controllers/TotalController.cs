using DocumentFormat.OpenXml.Spreadsheet;
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