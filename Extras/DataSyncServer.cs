using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Threading;
using System.Web.Mvc;

namespace PSAPLCDashboard.Web.Dashboard.Extras
{
    enum SPCommand
    {
        Update, Insert, SelectMany, SelectScalar
    }

    public class DataSyncServer
    {
        private bool disposed;
        private SqlConnection con = null;
        private SqlCommand comm = null;
        private SqlConnection sqlImportConnection = null;
        private SqlCommand sqlImportCom = null;
        private SqlDataAdapter adap = null;
        public string strConnection = ConfigurationManager.AppSettings["DbConnection"].ToString();
        //private string strImportCon = null;
        private static DataSyncServer instance = null;


        public static DataSyncServer Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DataSyncServer();
                }
                return instance;
            }
        }

        public DataSyncServer()
        {
            disposed = false;
            strConnection = ConfigurationManager.AppSettings["DbConnection"].ToString();
            con = new SqlConnection(strConnection);
            comm = new SqlCommand();

            comm.Connection = con;
            comm.Connection.Open();
            sqlImportCom = new SqlCommand();
            adap = new SqlDataAdapter();
            comm.Connection.Close();

        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }
            if (disposing)
            {
                if (comm != null && comm.Connection.State == ConnectionState.Open)
                {
                    comm.Connection.Close();
                }
                instance = null;
            }
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
        }

        public DataSet GetDataset(string query)
        {
            DataSet ds = new DataSet();
            try
            {
                using (con = new SqlConnection(strConnection))
                {
                    using (var da = new SqlDataAdapter(query, con))
                    {
                        da.Fill(ds);
                    }

                }
            }
            catch (Exception ex)
            {

            }
            return ds;
        }

        public DataTable GetDataTable(string query)
        {
            DataTable dt = new DataTable();
            try
            {
                using (con = new SqlConnection(strConnection))
                {
                    using (var da = new SqlDataAdapter(query, con))
                    {
                        da.Fill(dt);
                    }

                }

            }
            catch (Exception ex)
            {

            }

            return dt;
        }


        public bool ExecuteNonQuery(string query)
        {
            bool res = false;
            try
            {
                using (con = new SqlConnection(strConnection))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        int x = cmd.ExecuteNonQuery();
                        res = x == 1;
                    }

                }

            }
            catch (Exception ex)
            {

            }
            return res;
        }

        public object ExecuteScalar(string query)
        {
            object x = null;
            try
            {
                using (con = new SqlConnection(strConnection))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        x = cmd.ExecuteScalar();
                    }

                }

            }
            catch (Exception ex)
            {
            }
            return x;
        }

        public bool Transaction(List<string> qryCollection)
        {
            bool res = false;
            try
            {
                using (con = new SqlConnection(strConnection))
                {
                    con.Open();
                    foreach (string qry in qryCollection)
                    {
                        using (SqlCommand cmd = new SqlCommand(qry, con))
                        {
                            int x = cmd.ExecuteNonQuery();
                            res = x == 1;
                        }

                    }
                }
                res = true;
            }
            catch (Exception ex)
            {
            }
            return res;
        }

        /*public bool ExecuteSPInsert(List<SqlParameter> parameters, string spName, SPCommand spCommand)
        {
            //bool res = false;
            int x = 0;
            try
            {
                comm.CommandText = spName;
                comm.CommandType = CommandType.StoredProcedure;
                comm.Parameters.Clear();
                foreach (SqlParameter pm2 in parameters)
                {
                    comm.Parameters.Add(pm2);
                }
                switch (spCommand)
                {
                    case SPCommand.Insert:
                        x = comm.ExecuteNonQuery();
                        break;
                    case SPCommand.Update:
                        x = comm.ExecuteNonQuery();
                        break;
                }

                return x == 1;
            }
            catch (InvalidOperationException ex)
            {
                string s = "<<Message>> " + ex.Message;
                s = s + "<<SPNAME>> " + spName;
                foreach (SqlParameter pm in parameters)
                {
                    s = string.Concat(s, "[parameter] <NAME : ", pm.ParameterName, "> <VALUE : ", pm.Value, " >");
                }
                throw new Exception(s);
            }

        }*/

        public T ExecuteSP<T>(List<SqlParameter> parameters, string spName)
        {
            object obj = null;
            try
            {
                using (sqlImportCom = new SqlCommand())
                {
                    sqlImportCom.Connection = sqlImportConnection;
                    sqlImportCom.CommandText = spName;
                    sqlImportCom.CommandType = CommandType.StoredProcedure;
                    sqlImportCom.Parameters.Clear();
                    foreach (SqlParameter pm2 in parameters)
                    {
                        sqlImportCom.Parameters.Add(pm2);
                    }
                    IAsyncResult aRes = null;
                    aRes = sqlImportCom.BeginExecuteNonQuery();
                    WaitHandle wHandle = aRes.AsyncWaitHandle;
                    wHandle.WaitOne();
                    obj = sqlImportCom.EndExecuteNonQuery(aRes);
                    obj = true;
                }
            }
            catch (InvalidOperationException ex)
            {
                string s = "<<Message>> " + ex.Message;
                s = s + "<<SPNAME>> " + spName;
                foreach (SqlParameter pm in parameters)
                {
                    s = string.Concat(s, "[parameter] <NAME : ", pm.ParameterName, "> <VALUE : ", pm.Value, " >");
                }
                throw new Exception(s);
            }

            return (T)Convert.ChangeType(obj, typeof(T));
        }

        public bool ExecuteSP_new(List<SqlParameter> parameters, string spName)
        {
            bool obj = false;
            try
            {
                using (comm = new SqlCommand())
                {
                    comm.Connection = con;
                    comm.CommandText = spName;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Clear();
                    foreach (SqlParameter pm2 in parameters)
                    {
                        comm.Parameters.Add(pm2);
                    }
                    IAsyncResult aRes = null;
                    aRes = comm.BeginExecuteNonQuery();
                    WaitHandle wHandle = aRes.AsyncWaitHandle;
                    wHandle.WaitOne();
                    int a = comm.EndExecuteNonQuery(aRes);
                    obj = true;
                }
            }
            catch (InvalidOperationException ex)
            {
                string s = "<<Message>> " + ex.Message;
                s = s + "<<SPNAME>> " + spName;
                foreach (SqlParameter pm in parameters)
                {
                    s = string.Concat(s, "[parameter] <NAME : ", pm.ParameterName, "> <VALUE : ", pm.Value, " >");
                }
                return false;
            }

            return true;
        }

        public DataTable ExecuteSP(List<SqlParameter> parameters, string spName)
        {
            DataTable dt = new DataTable();
            try
            {
                comm.CommandText = spName;
                comm.CommandType = CommandType.StoredProcedure;
                comm.Parameters.Clear();
                foreach (SqlParameter pm in parameters)
                {
                    SqlDbType typ = SqlDbType.VarChar;
                    switch (pm.SqlDbType)
                    {
                        case SqlDbType.DateTime:
                            typ = SqlDbType.DateTime;
                            break;
                        case SqlDbType.Int:
                            typ = SqlDbType.Int;
                            break;
                        case SqlDbType.Bit:
                            typ = SqlDbType.Bit;
                            break;
                    }
                    string pName = "@" + pm.ParameterName;
                    comm.Parameters.Add(new SqlParameter(pName, typ));
                    comm.Parameters[pName].Value = pm.Value;
                }
                adap.SelectCommand = comm;
                adap.Fill(dt);
            }
            catch (InvalidOperationException ex)
            {
                throw new Exception(ex.Message + " SPNAME " + spName);
            }

            return dt;
        }

        public bool UpdateUsingExecuteNonQueryList(List<string> strQuery)
        {
            bool b = false;
            string qryfl = "";
            SqlTransaction objTrans;
            using (SqlConnection objConn = new SqlConnection(strConnection))
            {
                objConn.Open();
                objTrans = objConn.BeginTransaction();
                try
                {
                    foreach (string qry in strQuery)
                    {
                        qryfl = qry;
                        SqlCommand objCmd1 = new SqlCommand(qry, objConn, objTrans);
                        objCmd1.ExecuteNonQuery();

                    }
                    objTrans.Commit();
                    b = true;
                }
                catch (Exception ex)
                {
                    b = false;
                    objTrans.Rollback();

                }

            }
            return b;
        }


    }
}