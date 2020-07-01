using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using Oracle.DataAccess.Client;

//namespace MacDirect.Helpers{
    public class OracleRS
    {
        private OracleDataReader myReader;
        private OracleConnection oConn;


        public static System.Data.DataTable GetDataTable(string queryString, string connectionProfileName)
        {
            string connectionString = ConfigurationManager.AppSettings[connectionProfileName];
            DataTable myDataTable = new DataTable();
            try
            {
                var cmd = new OracleCommand(queryString, new OracleConnection(connectionString));
                var adapter = new OracleDataAdapter(cmd);
                adapter.Fill(myDataTable);
            }
            catch (Exception ex)
            {
                //App.ErrorLog("OracleRS.GetDataTable()", ex.Message + " - " + queryString, "", ex.StackTrace);
                throw ex;
            }
            return myDataTable;

        }

        public static DataTable GetDataTableFromProc(string procedureName, string connectionProfileName)
        {
            string connectionString = System.Configuration.ConfigurationManager.AppSettings[connectionProfileName];
            OracleConnection oConn = new OracleConnection(connectionString);
            OracleCommand cmd = new OracleCommand(procedureName, oConn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("RC", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            OracleDataAdapter ad = new OracleDataAdapter();
            ad.SelectCommand = cmd;
            DataTable dtAns = new DataTable();
            ad.Fill(dtAns);
            oConn.Close();
            return dtAns;
        }
        public static DataTable GetDataTableFromProc(string procedureName, string connectionProfileName, string[] variableName, string[] variableValue)
        {
            string connectionString = System.Configuration.ConfigurationManager.AppSettings[connectionProfileName];
            DataTable dtAns = new DataTable();
            OracleCommand cmd = new OracleCommand(procedureName, new OracleConnection(connectionString));
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("RC", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            int counter = -1;
            foreach (string var in variableValue)
            {
                counter++;
                cmd.Parameters.Add(variableName[counter], OracleDbType.Varchar2).Value = var;
            }
            OracleDataAdapter ad = new OracleDataAdapter();
            ad.SelectCommand = cmd;
            ad.Fill(dtAns);
            return dtAns;
        }

        /*    public static string GetValueFromFunction(string functionName, string connectionProfileName, string[] variableName, string[] variableValue) {
                string connectionString = System.Configuration.ConfigurationManager.AppSettings[connectionProfileName];
                OracleCommand cmd = new OracleCommand(functionName, new OracleConnection(connectionString));
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("sAns", OracleType.VarChar).Direction = ParameterDirection.ReturnValue;
                int counter = -1;
                foreach (string var in variableValue) {
                    counter++;
                    cmd.Parameters.Add(variableName[counter], OracleType.VarChar).Value = var;
                }
                cmd.ExecuteScalar();
                return cmd.Parameters["sAns"].Value.ToString();
            }
         * */

        public OracleRS(string strSQL, string strConnection)
        {
            try
            {
                string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[strConnection].ConnectionString;
                oConn = new OracleConnection(connectionString);
                OracleCommand oComm = oConn.CreateCommand();
                OracleCommand myCommand = new OracleCommand(strSQL, oConn);
                oConn.Open(); 

                myReader = myCommand.ExecuteReader();
                myReader.Read();
            }
            catch (Exception Ex)
            {
                //MacAdvWeb.App.ErrorLog("OracleRS", Ex.Message + " - " + strSQL,"",Ex.StackTrace);
                throw Ex;
            }
        }

        public OracleRS(string strSQL, string strConnection, bool Execute)
        {
            try
            {
                if (!Execute)
                {
                    string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[strConnection].ConnectionString;
                    oConn = new OracleConnection(connectionString);
                    OracleCommand oComm = oConn.CreateCommand();
                    OracleCommand myCommand = new OracleCommand(strSQL, oConn);
                    oConn.Open();

                    myReader = myCommand.ExecuteReader();
                    myReader.Read();
                }
                else
                {
                    string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[strConnection].ConnectionString;
                    oConn = new OracleConnection(connectionString);
                    OracleCommand oComm = oConn.CreateCommand();
                    OracleCommand myCommand = new OracleCommand(strSQL, oConn);
                    oConn.Open();
                    myCommand.ExecuteNonQuery();
                    oConn.Close();
                }
            }
            catch (Exception Ex)
            {
                //MacAdvWeb.App.ErrorLog("OracleRS", Ex.Message + " - " + strSQL,"",Ex.StackTrace);
                throw Ex;
            }
        }

        public OracleDataReader getRS()
        {
            return myReader;
        }

        public void CloseRS()
        {
            myReader.Close();
            oConn.Close();
            oConn.Dispose();
        }

        public void OracleRunOnce(string strSQL, string strConnection)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[strConnection].ConnectionString;
            oConn = new OracleConnection(connectionString);
            OracleCommand oComm = oConn.CreateCommand();
            OracleCommand myCommand = new OracleCommand(strSQL, oConn);
            oConn.Open();
            myCommand.ExecuteNonQuery();
            myCommand.Dispose();
            oConn.Close();
            oConn.Dispose();
        }

        public static void Execute(string strSQL, string strConnection)
        {
            OracleConnection oConn;
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[strConnection].ConnectionString;
            oConn = new OracleConnection(connectionString);
            oConn.Open();
            OracleTransaction transaction = oConn.BeginTransaction();
            OracleCommand oComm = oConn.CreateCommand();
            OracleCommand myCommand = new OracleCommand(strSQL, oConn); //, transaction);
            try
            {
                myCommand.ExecuteNonQuery();
                transaction.Commit();
                transaction.Dispose();
                myCommand.Dispose();
                oConn.Close();
                oConn.Dispose();
            }
            catch (Exception e)
            {
                oConn.Close();
                oConn.Dispose();
                //App.ErrorLog("OracleRS.Execute", e.Message + " - " + strSQL, " ", e.StackTrace);
                throw e;
            }
        }

        /// <summary>
        /// Oracle Insert Statement that will also return the new value of the Primary Key.  Intended to be a single row insert statement.  Unsure what would happen with a multirow insert.
        /// </summary>
        /// <param name="strInsertSql">Base SQL statemetn</param>
        /// <param name="strConnection">Connection Name (i.e. "event")</param>
        /// <param name="pkFieldName">Name of the field that holds the primary key.</param>
        /// <returns></returns>
        public static int Insert(string strInsertSql, string strConnection, string pkFieldName)
        {
            string strSQL = strInsertSql + " RETURNING " + pkFieldName + " INTO :out_id";

            OracleConnection oConn;
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[strConnection].ConnectionString;
            oConn = new OracleConnection(connectionString);
            oConn.Open();
            OracleTransaction transaction = oConn.BeginTransaction();
            //OracleCommand oComm = oConn.CreateCommand();

            OracleDataAdapter oda = new OracleDataAdapter();
            oda.InsertCommand = new OracleCommand(strSQL, oConn); //, transaction);
            oda.InsertCommand.Parameters.Add("out_id", OracleDbType.Int32);
            oda.InsertCommand.Parameters["out_id"].Direction = ParameterDirection.ReturnValue;

            int newId = -1;

            try
            {
                oda.InsertCommand.ExecuteNonQuery();
                transaction.Commit();
                int.TryParse(oda.InsertCommand.Parameters["out_id"].Value.ToString(), out newId);
                transaction.Dispose();
                oda.Dispose();
                oConn.Close();
                oConn.Dispose();
            }
            catch (Exception ex)
            {
                oConn.Close();
                oConn.Dispose();
                //App.ErrorLog("OracleRS.Execute", ex.Message + " - " + strSQL, " ", ex.StackTrace);
                throw ex;
            }
            return newId;
        }

        public static string ExecuteScalar(string strSQL, string strConnection)
        {
            string ans = "";
            OracleConnection oConn;
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[strConnection].ConnectionString;
            oConn = new OracleConnection(connectionString);
            oConn.Open();
            OracleTransaction transaction = oConn.BeginTransaction();
            OracleCommand oComm = oConn.CreateCommand();
            OracleCommand myCommand = new OracleCommand(strSQL, oConn); // , transaction);
            try
            {
                ans = myCommand.ExecuteScalar().ToString();
                //Now, overdoit with shutdown.
                transaction.Dispose();
                myCommand.Dispose();
                oConn.Close();
                oConn.Dispose();
            }
            catch (Exception e)
            {
                oConn.Close();
                oConn.Dispose();
                //App.ErrorLog("OracleRS.Execute", e.Message + " - " + strSQL, " ", e.StackTrace);
                throw e;
            }
            ans = ans.GetType().Equals(System.DBNull.Value) ? "" : ans;
            return ans;
        }

        /// <summary>
        /// Command Structure to execute an Oracle Proceedure
        /// </summary>
        public class Command
        {
            OracleCommand cmd;
            OracleConnection conn;

            public Command(string procedureName, string connectionProfileName)
            {
                string connectionString = System.Configuration.ConfigurationManager.AppSettings[connectionProfileName];
                conn = new OracleConnection(connectionString);
                cmd = new OracleCommand(procedureName, conn);
                cmd.CommandType = CommandType.StoredProcedure;
            }

            /// <summary>
            /// For Example:
            /// OracleRS.Command aCmd = new OracleRS.Command(sql, "event", System.Data.CommandType.Text);
            /// aCmd.AddParameterWithValue(":event_id", Request.QueryString["id"]);
            /// aCmd.AddParameterWithValue(":css", uxCSSTextBox.Text);
            /// aCmd.ExecuteNonQuery();
            /// </summary>
            /// <param name="procedureName"></param>
            /// <param name="connectionProfileName"></param>
            /// <param name="oraCommandType"></param>
            public Command(string procedureName, string connectionProfileName, CommandType oraCommandType)
            {
                string connectionString = System.Configuration.ConfigurationManager.AppSettings[connectionProfileName];
                conn = new OracleConnection(connectionString);
                cmd = new OracleCommand(procedureName, conn);
                cmd.CommandType = oraCommandType;
            }

            public void AddParameter(string parameterName, OracleDbType oType, object value)
            {
                cmd.Parameters.Add(parameterName, oType).Value = value;
            }

            public void AddParameterWithValue(string parameterName, object parameterValue)
            {
                cmd.Parameters.Add(parameterName, parameterValue);
            }

            public DataTable GetDataTable(bool AddCursorParameter)
            {
                try
                {
                    DataTable dtAns = new DataTable();
                    if (AddCursorParameter)
                    {
                        cmd.Parameters.Add("RC", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                        cmd.BindByName = true;
                    }
                    OracleDataAdapter ad = new OracleDataAdapter();
                    ad.SelectCommand = cmd;
                    ad.Fill(dtAns);
                    ad.Dispose();
                    cmd.Dispose();
                    conn.Close();
                    conn.Dispose();
                    return dtAns;
                }
                catch (Exception Ex)
                {
                    conn.Dispose();
                    //App.ErrorLog("OracleRS.Command.GetDataTable",Ex.Message + " <br/>" + cmd.CommandText," ", Ex.StackTrace);
                    throw Ex;
                }
            }

            public DataTable GetDataTable()
            {
                return GetDataTable(false);
            }

            /// <summary>
            /// Inserts a parameter OracleType.Cursor "RC" then wrapps the command with
            /// necessary OracleDatatable wrappers to fill the dataset
            /// </summary>
            /// <returns>Output of the "RC" parameter from the stored proceedure</returns>
            public DataSet GetDataSet( bool insertRC= false)
            {
                try
                {
                    DataSet dtAns = new DataSet();

                    if (insertRC)
                    {
                        cmd.Parameters.Add("RC", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    }
                    OracleDataAdapter ad = new OracleDataAdapter();
                    ad.SelectCommand = cmd;
                    ad.Fill(dtAns);
                    ad.Dispose();
                    cmd.Dispose();
                    conn.Close();
                    conn.Dispose();
                    return dtAns;
                }
                catch (Exception Ex)
                {
                    conn.Dispose();
                    //App.ErrorLog("OracleRS.Command.GetDataSet",Ex.Message + " <br/>" + cmd.CommandText," ", Ex.StackTrace);
                    throw Ex;
                }
            }
            /// <summary>
            /// Executes the command, after adding a parameter "ANS" for which it expects a return value for
            /// </summary>
            /// <returns>String ANS</returns>
            public string GetStringResponse()
            {
                cmd.Parameters.Add("ANS", OracleDbType.Varchar2, 254).Direction = ParameterDirection.Output;
                //cmd.Parameters["ANS"].Size = 255;
                conn.Open();
                cmd.ExecuteNonQuery();
                string ans = cmd.Parameters["ANS"].Value.ToString();
                this.Close();
                return ans;
            }

            /// <summary>
            /// Executes the command. 
            /// </summary>
            /// <returns>int Number of Rows affected</returns>
            public int ExecuteNonQuery()
            {
                conn.Open();
                string tmp = cmd.ToString();
                try
                {
                    int rowsAffected = cmd.ExecuteNonQuery();
                    this.Close();
                    return rowsAffected;
                }
                catch (Exception ex)
                {
                    string deets = "";
                    int i = 0;
                    foreach (var p in cmd.Parameters)
                    {
                        deets += i.ToString() + " - " + p.ToString() + "\n";
                        i++;
                    }
                    //MacAdvWeb.App.ErrorLog("ExecuteNonQuery()", ex.Message, " ", cmd.CommandText + "\n\n" + deets + "\n\n" + ex.StackTrace);
                    throw new Exception(deets, ex);
                    //return 0;
                }
        }

        void Close()
            {
                cmd.Connection.Close();
                cmd.Connection.Dispose();
                cmd.Dispose();
                conn.Close();
                conn.Dispose();
            }

            protected string GetCommandLogString(IDbCommand command)
            {
                string outputText;

                if (command.Parameters.Count == 0)
                {
                    outputText = command.CommandText;
                }
                else
                {
                    StringBuilder output = new StringBuilder();
                    output.Append(command.CommandText);
                    output.Append("; ");

                    IDataParameter p;
                    int count = command.Parameters.Count;
                    for (int i = 0; i < count; i++)
                    {
                        p = (IDataParameter)command.Parameters[i];
                        output.Append(string.Format("{0} = '{1}'", p.ParameterName, p.Value));

                        if (i + 1 < count)
                        {
                            output.Append(", ");
                        }
                    }
                    outputText = output.ToString();
                }
                return outputText;
            }


        }
    }
//}