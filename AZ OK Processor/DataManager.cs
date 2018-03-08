using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.IO;

namespace AZ_OK_Processor
{
    class DataManager
    {
        private static string ConnectionString = ConfigurationManager.AppSettings["ConnectionString"];
        private static string logFilePath = ConfigurationManager.AppSettings["LogLocation"];
        private static string refTableName = ConfigurationManager.AppSettings["RefTableName"];
        private static string OKFileDirectory = ConfigurationManager.AppSettings["OKFileDirectory"] + "\\TestOKFile.OK";

        private static string sourceServer = ConfigurationManager.AppSettings["SourceServer"];
        private static string sourceDatabase = ConfigurationManager.AppSettings["SourceDatabase"];
        private static string targetServer = ConfigurationManager.AppSettings["TargetServer"];
        private static string targetDatabase = ConfigurationManager.AppSettings["TargetDatabase"];


        private static DataSet refData;// = DataManager.GetFileFormat();__
        private static string errorMsgDBConnection = "";
        private static string errorMsgTableExist = "";

        public static string ErrorMsgDBConnection { get { return DataManager.errorMsgDBConnection; } }
        public static string ErrorMsgTableExist { get { return DataManager.errorMsgTableExist; } }
        public static DataSet FileFormats { get { return DataManager.refData; } }
        public static string[] DBTableNames
        {
            get
            {
                string[] temp = new string[refData.Tables[0].Rows.Count];
                for (int i = 0; i < refData.Tables[0].Rows.Count; i++)
                {
                    temp[i] = refData.Tables[0].Rows[i]["TableName"].ToString();
                }
                return temp;
            }
        }

        public static bool IsServerConnected()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                try
                {
                    DataManager.TxtLog("Begin database connection test");
                    connection.Open();
                    connection.Close();
                    DataManager.TxtLog("Database successfully connected");
                    return true;
                }
                catch (SqlException)
                {
                    errorMsgDBConnection = "Unable to Open Database";
                    MessageBox.Show(errorMsgDBConnection);
                    DataManager.TxtLog(errorMsgDBConnection + ": " + ConnectionString);
                    return false;
                }
            }   //using
        }   //IsServerConnected()


        public static bool IsTableExist(string tableName)
        {   //Test if tabel Princess table exists (Table Name held in Config file)
            bool retVal = false;
            string sql = "select case when exists((select * from information_schema.tables where table_name = '" + tableName + "')) then 1 else 0 end";

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    if ((int)command.ExecuteScalar() == 1)
                    {   //Returns 1. Means table does exist
                        retVal = true;
                    }
                    else
                    {   //Returns 0.  Means table does not exist
                        retVal = false;
                    }
                    connection.Close();
                }   //using (SqlCommand command
            }   //try 
            catch
            {   //some error happened when trying to connect to the database.  Run Text File Log.  
                DataManager.TxtLog("A SQL error occured when trying to see if table exists: " + tableName);
                MessageBox.Show("A SQL error occured when trying to see if table exists: " + tableName);
                retVal = false;
            }

            return retVal;
        }   //public void LogSelect


        public static void GetRefData()
        {
            DataSet ds = new DataSet();
            string sqlString = "SELECT * FROM " + DataManager.refTableName;
            DataManager.TxtLog("Beginning attempt to retrieve the Ref data from the reference table");

            if (IsTableExist(DataManager.refTableName))
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                using (SqlDataAdapter Adapter = new SqlDataAdapter(sqlString, connection))
                {
                    try
                    {
                        connection.Open();
                        try
                        {
                            DataManager.TxtLog("Database connection complete");
                            DataManager.TxtLog("Attempting to pull data back from reference table");
                            Adapter.Fill(ds);
                            refData = ds;
                            DataManager.TxtLog("Successfully retrieved the Ref data from the reference table");
                        }
                        catch
                        {
                            MessageBox.Show("Unable to get Keys: Database Connection Error");
                        }

                    }   //try
                    catch (SqlException)
                    {
                        MessageBox.Show("Unable to open database");
                    }
                    //return ds;
                }   //using
            }
            else
            {
                MessageBox.Show("Could not locate Ref table in database");
            }
        }


        public static DataRow ExtractFileRefDataRow(string fileType)
        {
            DataRow retColumnNames = null;
            for (int i = 0; i < DataManager.refData.Tables[0].Rows.Count; i++)
            {
                if (fileType == DataManager.refData.Tables[0].Rows[i]["FileType"].ToString())
                {
                    retColumnNames = DataManager.refData.Tables[0].Rows[i];
                }
            }
            return retColumnNames;
        }


        public static bool DatabaseWrite(DataTable UIDTable, string tableName)
        {
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(ConnectionString))
            {
                bulkCopy.DestinationTableName = "dbo." + tableName;
                try
                {
                    bulkCopy.WriteToServer(UIDTable);
                    return true;
                }
                catch (SqlException sqlEx)
                {
                    if (sqlEx.Message.Contains("Violation of PRIMARY KEY constraint"))
                    {
                        MessageBox.Show("This file contains records that already exist in the database: " + tableName);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error has occured while inserting data into table: " + tableName + '\n' + ex.Message);
                    return false;
                }
            }
            return false;
        }

        
        public static int RunSproc_PrincessULC(DataRow sprocData)
        {
            string outValue = "";
            int retValue = 0;

            DataManager.TxtLog("Beginning attempt to call Stored Procedure for " + sprocData[0].ToString() + " - " + sprocData[4].ToString());
            if (sprocData[4].Equals(""))
            {   //no sproc defined for this file type
                DataManager.TxtLog(string.Format("No Stored Procedure indicated for file type: {0}", sprocData[0]));
                return 0;
            }

            using (var conn = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(sprocData[4].ToString(), conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                try
                {
                    conn.Open();
                    DataManager.TxtLog(string.Format("Successfully connected to Database - Stored Procedure: {0}", sprocData[4].ToString()));
                    try
                    {
                        command.Parameters.AddWithValue("@SourceServer", DataManager.sourceServer);
                        command.Parameters.AddWithValue("@SourceDatabase", DataManager.sourceDatabase);
                        command.Parameters.AddWithValue("@SourceTable", sprocData[2].ToString());
                        command.Parameters.AddWithValue("@TargetServer", DataManager.targetServer);
                        command.Parameters.AddWithValue("@TargetDatabase", DataManager.targetDatabase);
                        command.Parameters.Add("@OutValue", SqlDbType.VarChar, 8).Direction = ParameterDirection.Output;

                        var returnValue = command.Parameters.Add("@RETURN_VALUE", SqlDbType.Int);
                        returnValue.Direction = ParameterDirection.ReturnValue;

                        command.ExecuteNonQuery();
                        outValue = command.Parameters["@OutValue"].Value.ToString();
                        string result = returnValue.Value.ToString();
                        conn.Close();
                        DataManager.TxtLog("Stored Procedure has completed running: " + sprocData[4].ToString());
                        //int tempvar;
                        if (int.TryParse(result.ToString(), out retValue) == true)
                        {
                            if (retValue != 0)
                            {
                                //If result isn't 0 (zero), then one of the errors defined in the sproc has happened.
                                return retValue;
                            }
                        }
                        DataManager.TxtLog(string.Format("Successfully run Stored Procedure: {0}", sprocData[4].ToString()));
                    }
                    catch (SqlException Ex)
                    {
                        MessageBox.Show(string.Format("An error has occured while running the stored procedure - {0}.  {1}", sprocData[4].ToString(), Ex.Message));
                    }
                }
                catch (SqlException Ex)
                {
                    errorMsgDBConnection = "Unable to Open Database";
                    MessageBox.Show(errorMsgDBConnection);
                    DataManager.TxtLog(errorMsgDBConnection + ": " + ConnectionString);
                }
            }
            return retValue;
        }
            

        public static void TxtLog(string Msg)
        {   //Log insert to text file
            StreamWriter log;
            FileStream fileStream = null;
            FileInfo logFileInfo = new FileInfo(logFilePath);
            DirectoryInfo logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);

            if (!logDirInfo.Exists) logDirInfo.Create();
            if (!logFileInfo.Exists)
            { fileStream = logFileInfo.Create(); }
            else
            { fileStream = new FileStream(logFilePath, FileMode.Append); }
            log = new StreamWriter(fileStream);
            log.WriteLine(Msg);
            log.Close();
        }


        public static void CreateTestOKFile()
        {   //Log insert to text file
            StreamWriter testFile;
            FileStream fileStream = null;
            FileInfo logFileInfo = new FileInfo(OKFileDirectory);
            DirectoryInfo logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);
            char[] hexVals = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
            char[] writeVal = { '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0' };
            string writeStr = "";
            DataManager.GetRefData();

            FileToProcess TestOKFile = new FileToProcess(OKFileDirectory);
            TestOKFile.DeleteFile();

            if (!logDirInfo.Exists) logDirInfo.Create();
            if (!logFileInfo.Exists)
            { fileStream = logFileInfo.Create(); }
            else
            { fileStream = new FileStream(OKFileDirectory, FileMode.Append); }
            testFile = new StreamWriter(fileStream);

            for (int i = 0; i < hexVals.Length; i++)
            {
                writeVal[0] = hexVals[i];
                for (int j = 0; j < hexVals.Length; j++)
                {
                    writeVal[1] = hexVals[j];
                    for (int k = 0; k < hexVals.Length; k++)
                    {
                        writeVal[12] = hexVals[k];
                        for (int l = 0; l < hexVals.Length; l++)
                        {
                            writeVal[13] = hexVals[l];
                            writeStr = "";
                            for (int z = 0; z < writeVal.Length; z++)
                            {
                                writeStr += writeVal[z];
                            }
                            testFile.WriteLine(writeStr + " ;");
                        }

                    }
                }
            }
            testFile.Close();
        }
    }
}
