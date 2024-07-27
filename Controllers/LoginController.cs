using PSAPLCDashboard.Web.Dashboard.Extras;
using System.Data;
using System.Web.Mvc;

namespace PSAPLCDashboard.Web.Dashboard.Controllers
{
    public class LoginController : Controller
    {
        DataSyncServer SetDb = new DataSyncServer();
        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }


        [HttpPost]
        public JsonResult UserLogin(string userid, string password)
        {
            DataTable dt = new DataTable();

            string query = "SELECT * FROM USERMASTER WHERE User_ID = '" + userid + "' AND Password = '" + password + "' AND Status ='Y'";
            dt = SetDb.GetDataTable(query);
            if (dt.Rows.Count > 0)
            {
                // Successful login
                Session["userid"] = dt.Rows[0]["User_ID"].ToString();
                Session["username"] = dt.Rows[0]["User_Name"].ToString();
                return Json(new { success = true, message = "Login successful." });

            }
            else
            {
                // Login failed
                return Json(new { success = false, message = "Invalid credentials." });
            }

        }



        [HttpPost]
        public JsonResult UserSignin(string NewUserName, string Newuserid, string Newpassword)
        {
            DataTable dt = new DataTable();


            string query = "SELECT * FROM UserMaster WHERE User_ID = '" + Newuserid + "'";
            dt = SetDb.GetDataTable(query);
            if (dt.Rows.Count > 0)
            {
                // Successful login
                return Json(new { success = true, message = "This User Already Exists...!" });
            }
            else
            {
                ;
                // Login failed
                string query1 = " Insert  UserMaster (User_ID,User_Name,Password,Status) values('" + Newuserid + "','" + NewUserName + "','" + Newpassword + "','Y')";

                bool Rowsinserted = SetDb.ExecuteNonQuery(query1);
                if (Rowsinserted == true)
                {
                    return Json(new { Datainsertsuccess = true, message = "User created successfully." });

                }
                else
                {
                    return Json(new { DataInsertederror = true, message = "Failed to create user." });

                }
            }

        }




        [HttpPost]
        public JsonResult UserPasswordUpdate(string Userid, string Newpassword, string Confirmpassword)
        {
            DataTable dt = new DataTable();
            string ActPass = string.Empty;
            if (Confirmpassword.Trim() == Newpassword.Trim())
            {
                ActPass = Confirmpassword;
            }
            else
            {
                return Json(new { Datainsertsuccess = true, message = "Password & Confirm Password Not Matched." });
            }

            string query = "SELECT * FROM UserMaster WHERE User_ID = '" + Userid + "'";
            dt = SetDb.GetDataTable(query);
            if (dt.Rows.Count > 0)
            {
                // Login failed
                if (dt.Rows[0]["Status"].ToString() != "Y")
                {
                    return Json(new { Datainsertsuccess = true, message = "User State is InActive Please Contach Admin." });
                }

                // Successful login
                string query1 = "Update UserMaster SET  Password='" + ActPass + "' WHERE User_ID='" + Userid + "'";
                bool Rowsinserted = SetDb.ExecuteNonQuery(query1);
                if (Rowsinserted == true)
                {
                    return Json(new { Datainsertsuccess = true, message = "Password Updated successfully." });
                }
                else
                {
                    return Json(new { DataInsertederror = true, message = "Password Updation Failed." });
                }

            }
            else
            {
                return Json(new { success = true, message = "Please Enter Valid User...!" });
            }
        }


    }
}