using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.IO;
using System.Data;
using System.Configuration;

namespace AZ_OK_Processor
{
    class FileToProcess
    {
        private int totalRecords = 0;
        private int totalGoodRecords = 0;
        private int totalBadRecords = 0;

        private string fileAddress = "";
        private string fileName = "";
        private string fileExt = "";
        private string fileAcceptableExt = "";
        private bool isExtOK = false;
        private string fileType = "";
        private string fileNameFormat = "";
        private string fileDataFormat = "";
        private bool isNameFormatOK = false;
        private bool isDataFormatOK = false;
        private string fileCopyAddress = "";
        private string fileCopyName = "";

        private string DBTableName = "";
        private DataRow refDataRow;
        private DataTable dataTable = new DataTable();
        private string[] dataColumns = new string[0];
        private string sproc = "";

        private string errorMsgInvalidExt = "";
        private string errorMsgFileFormat = "";
        private string errorMsgCopyFile = "";
        private string errorMsgWriteDB = "";
        private string errorMsgDeleteFile = "";
        private string errorMsgReadFile = "";

        public FileToProcess(string fileAddress)
        {
            this.fileAddress = fileAddress;
            this.fileName = Path.GetFileName(fileAddress);
            this.fileExt = Path.GetExtension(fileAddress).ToUpper();
            this.fileType = this.FileFormatCheck();
            if (this.fileType != "")
            {
                this.isDataFormatOK = true;
                this.fileCopyAddress = ConfigurationManager.AppSettings["FileCopyDirectory" + fileType];// + "\\" + fileName;
                this.fileCopyName = this.fileCopyAddress + "\\" + fileName;
                this.refDataRow = DataManager.ExtractFileRefDataRow(this.fileType);
                this.fileNameFormat = refDataRow["FileNameFormat"].ToString();
                this.DBTableName = refDataRow["TableName"].ToString();
                this.dataColumns = refDataRow["ColumnNames"].ToString().Split(',');
                this.sproc = refDataRow["StoredProcedure"].ToString();
                this.fileAcceptableExt = refDataRow["FileExtension"].ToString();
                this.dataTable = new DataTable(DBTableName);
                this.isExtOK = (this.fileExt.Equals(this.fileAcceptableExt)) ? true : false;
                this.errorMsgInvalidExt = this.isExtOK ? "" : "Invalid Extension: " + fileExt + ". Epxected Extension: " + fileAcceptableExt;

                this.AddDataTableColumns(dataTable, dataColumns, false, false, "");
            }
            else
            {
                this.isDataFormatOK = false;
            }
        }

        public double PercentGoodRecords
        {
            get
            {
                if (totalRecords > 0)
                {
                    return Math.Round(totalGoodRecords * 100.0 / totalRecords, 4);
                }
                else
                {
                    return 0;
                }
            }
        }

        public double PercentBadRecords
        {
            get
            {
                if (totalRecords > 0)
                {
                    return Math.Round(totalBadRecords * 100.0 / totalRecords, 4);                }
                else
                {
                    return 0;
                }
            }
        }

        public int TotalRecords { get { return totalRecords; } }
        public int TotalGoodRecords { get { return totalGoodRecords; } }
        public int TotalBadRecords { get { return totalBadRecords; } }
        public string FileAddress { get { return fileAddress; } }
        public string FileName { get { return fileName; } }
        public string FileCopyAddress { get { return fileCopyAddress; } }
        public string FileCopyName { get { return fileCopyName; } set { fileCopyName = value; } }
        public DataRow RefDataRow { get { return refDataRow; } }
        public bool IsExtOK { get { return isExtOK; } }
        public bool IsNameFormatOK { get { return isNameFormatOK; } }
        public bool IsDataFormatOK { get { return isDataFormatOK; } }
        public string Sproc { get { return sproc; } }
        

        public string ErrorMsgInvalidExt { get { return errorMsgInvalidExt; } }
        public string ErrorMsgeFileFormat { get { return errorMsgFileFormat; } }
        public string ErrorMsgCopyFile { get { return errorMsgCopyFile; } }
        public string ErrorMsgWriteDB { get { return errorMsgWriteDB; } }
        public string ErrorMsgDeleteFile { get { return errorMsgDeleteFile; } }
        public string ErrorMsgReadFile { get { return errorMsgReadFile; } }



        public void ClearVariables()
        {   //After a particular file is processed, clear it's memory to reduce app memory usage
            this.fileAddress = "";
            this.fileName = "";
            this.fileExt = "";
            this.fileCopyAddress = "";
            this.fileCopyName = "";
            this.isExtOK = false;
            this.dataTable = new DataTable();
        }


        private void AddDataTableColumns(DataTable Table, string[] ColumnArray, bool nullable, bool unique, string defaultVal)
        {   //Add all columns listed in list of columns
            for (int i = 0; i < ColumnArray.Length; i++)
            {
                DataColumn columnUID = Table.Columns.Add(dataColumns[i], typeof(string));
                columnUID.AllowDBNull = nullable;
                columnUID.Unique = unique;
                columnUID.DefaultValue = defaultVal;
            }
        }


        private void AddDataTableRow(DataTable Table, string[] TableColumns, string[] rowData)
        {   //Add all row data for a single row
            if (rowData.Length == TableColumns.Length)
            {   //number of elemnts of row data must equal number of columns
                DataRow RowUID = Table.NewRow();
                for (int i = 0; i < TableColumns.Length; i++)
                {
                    RowUID[TableColumns[i]] = rowData[i];
                }
                Table.Rows.Add(RowUID);
            }
        }


        private string FileFormatCheck()
        {
            string fileRow = "";
            string retFormat = "";
            try
            {
                using (StreamReader SR = new StreamReader(fileAddress))
                {
                    while (!SR.EndOfStream)
                    {
                        fileRow = SR.ReadLine();

                        if (retFormat == "")
                        {   //If a format has not yet been determined, try all of them against the data row in question
                            for (int i = 0; i < DataManager.FileFormats.Tables[0].Rows.Count; i++)
                            {
                                if (FileFormatCheck(fileRow, DataManager.FileFormats.Tables[0].Rows[i]["FileDataFormat"].ToString()))
                                {
                                    retFormat = DataManager.FileFormats.Tables[0].Rows[i]["FileType"].ToString();
                                    this.fileDataFormat = DataManager.FileFormats.Tables[0].Rows[i]["FileDataFormat"].ToString();
                                    DataManager.TxtLog(this.fileName + ": Valid Format match - " + retFormat + " - " + DataManager.FileFormats.Tables[0].Rows[i]["FileDataFormat"].ToString());
                                    totalGoodRecords++;
                                    i = DataManager.FileFormats.Tables[0].Rows.Count;
                                }
                                else
                                {
                                    totalBadRecords++;
                                }
                            }
                        }
                        else
                        {   //if a format has been determined, 
                            for (int i = 0; i < DataManager.FileFormats.Tables[0].Rows.Count; i++)
                            {
                                if (retFormat == DataManager.FileFormats.Tables[0].Rows[i]["FileType"].ToString())
                                {
                                    if (FileFormatCheck(fileRow, DataManager.FileFormats.Tables[0].Rows[i]["FileDataFormat"].ToString()))
                                    {
                                        totalGoodRecords++;
                                    }
                                    else
                                    {
                                        totalBadRecords++;
                                    }
                                    i = DataManager.FileFormats.Tables[0].Rows.Count;
                                }
                            }
                        }

                        totalRecords++;
                    }   //while
                }
                if (retFormat != "")
                {
                    return retFormat;
                }
                else
                {
                    errorMsgFileFormat = "Data in selected file not in recognized format";
                    return retFormat;
                }
            }   //try
            catch (IOException Ex)
            {
                MessageBox.Show(Ex.Message + "Could not find .OK file: " + fileAddress);
                return "";
            }   //catch
        }

        private bool FileFormatCheck(string fileRecord, string fileFormat)
        {
            bool passfail = false;

            if (!(fileRecord.Length == fileFormat.Length))
            {   //If the legnth of the data record and the file format dont match, just return false
                return passfail;
            }

            for (int i = 0; i < fileFormat.Length; i++)
            {
                if (fileFormat[i] == 'H')
                {   //If format indicates this char should be Hex (H), check that char is in Hex range
                    if ((fileRecord[i] >= '0' && fileRecord[i] <= '9') ||
                        (fileRecord[i] >= 'A' && fileRecord[i] <= 'F'))
                    {
                        passfail = true;
                    }
                    else
                    {   //If this char does not follow format, exit immediately
                        passfail = false;
                        return passfail;
                    }
                }
                else if (fileFormat[i] == 'A')
                {   //If format indicates this char should be Alpha (A), check that char is in Alpha range
                    if (fileRecord[i] >= 'A' && fileRecord[i] <= 'Z')
                    {
                        passfail = true;
                    }
                    else
                    {   //If this char does not follow format, exit immediately
                        passfail = false;
                        return passfail;
                    }
                }
                else if (fileFormat[i] == 'N')
                {   //If format indicates this char should be Numeric (N), check that char is in Numeric range
                    if (fileRecord[i] >= '0' && fileRecord[i] <= '9')
                    {
                        passfail = true;
                    }
                    else
                    {   //If this char does not follow format, exit immediately
                        passfail = false;
                        return passfail;
                    }
                }
                else if (fileFormat[i] == 'P')
                {   //If format indicates this char should be AlphaNumeric (P), check that char is in AlphaNumeric range
                    if ((fileRecord[i] >= '0' && fileRecord[i] <= '9') ||
                        (fileRecord[i] >= 'A' && fileRecord[i] <= 'Z'))
                    {
                        passfail = true;
                    }
                    else
                    {   //If this char does not follow format, exit immediately
                        passfail = false;
                        return passfail;
                    }
                }
                else if (fileFormat[i] == 'X')
                {   //If format indicates to ignore this char, skip and assume is correct
                    passfail = true;
                }
                else
                {   //If format does not indicate one of the above, simply compare the char to actual value in the format
                    if (fileRecord[i] == fileFormat[i])
                    {
                        passfail = true;
                    }
                    else
                    {   //If this char does not follow format, exit immediately
                        passfail = false;
                        return passfail;
                    }
                }
            }   //for

            return passfail;
        }

        public bool CopyFile()
        {
            try
            {
                System.IO.Directory.CreateDirectory(Path.GetDirectoryName(this.fileCopyName));
                System.IO.File.Copy(fileAddress, fileCopyName, true);
            }
            catch (System.IO.IOException e)
            {
                MessageBox.Show("An Issue occured while copying file: " + fileName + '\n' +
                                "Copying to: " + fileCopyName + '\n' +
                                e.Message);
                errorMsgCopyFile = fileName + ": File backup failed" + '\n';
                DataManager.TxtLog("File failed to copy to the OK File Backup: " + this.fileCopyName);
            }
            catch (System.UnauthorizedAccessException e)
            {
                MessageBox.Show("An Issue occured while copying file: " + fileName + '\n' +
                                "Copying to: " + fileCopyName + '\n' +
                                e.Message);
                errorMsgCopyFile = fileName + ": File backup failed" + '\n';
                DataManager.TxtLog("File failed to copy to the OK File Backup: " + this.fileCopyName);
            }

            return File.Exists(fileCopyName);
        }

        public bool ReadFile()
        {
            string fileRow = "";
            string[] rowData = new string[0];
            try
            {
                using (StreamReader SR = new StreamReader(fileAddress))
                {
                    while (!SR.EndOfStream)
                    {
                        fileRow = SR.ReadLine();
                        if (fileType == "PrincessULC" && FileFormatCheck(fileRow, this.fileDataFormat))
                        {   //Check to make sure each row is of the correct format
                            rowData = new string[dataColumns.Length];
                            rowData[0] = fileRow.Substring(0, 14);
                            AddDataTableRow(this.dataTable, this.dataColumns, rowData);
                        }
                        else if (fileType == "InCommULC" && FileFormatCheck(fileRow, this.fileDataFormat))
                        {
                            rowData = new string[dataColumns.Length];
                            rowData[0] = fileRow.Substring(0, 9);
                            rowData[1] = fileRow.Substring(10, 16);
                            rowData[2] = fileRow.Substring(27, 16);
                            AddDataTableRow(this.dataTable, this.dataColumns, rowData);
                        }
                        else if (fileType == "DisneyInfinityLogULC" && FileFormatCheck(fileRow, this.fileDataFormat))
                        {
                            rowData = new string[dataColumns.Length];
                            rowData[0] = fileRow.Substring(0, 6);
                            rowData[1] = fileRow.Substring(7, 10);
                            rowData[2] = fileRow.Substring(18, 24);
                            rowData[3] = fileRow.Substring(43, 12);
                            rowData[4] = fileRow.Substring(56, 6);
                            rowData[5] = fileRow.Substring(63, 16);
                            rowData[6] = fileRow.Substring(80, 8);
                            rowData[7] = fileRow.Substring(89, 5);
                            rowData[8] = fileRow.Substring(95, 8);
                            rowData[9] = fileRow.Substring(104, 16);
                            rowData[10] = fileRow.Substring(121, 75);
                            rowData[11] = fileRow.Substring(197, 11);
                            rowData[12] = fileRow.Substring(209, 17);
                            rowData[13] = fileRow.Substring(227, 11);
                            rowData[14] = fileRow.Substring(239, 6);
                            rowData[15] = fileRow.Substring(246, 52);
                            rowData[16] = fileRow;
                            AddDataTableRow(this.dataTable, this.dataColumns, rowData);
                        }
                        else
                        {
                            DataManager.TxtLog("File Row has incorrect format: " + fileRow);
                            continue;
                        }
                    }   //while (!SR.EndOfStream)
                    SR.Close();
                }
                return true;
            }   //try
            catch (IOException Ex)
            {
                MessageBox.Show(Ex.Message + "Could not find file: " + fileAddress);
                return false;
            }   //catch
        }



        public bool WriteFileToDB()
        {
            if (DataManager.IsTableExist(this.DBTableName))
            {   //Check if DB table related to FileToProcess exists
                if(DataManager.DatabaseWrite(this.dataTable, this.DBTableName))
                {
                    return true;
                }
                else
                {
                    DataManager.TxtLog("There was an error when writing data to the database: " + this.DBTableName);
                    this.errorMsgWriteDB = "There was an error when writing data to the database: " + this.DBTableName + '\n';
                    return false;
                }

            }
            else
            {   //Related DB table does not exist
                DataManager.TxtLog("Princess table is not connected or does not exist");
                this.errorMsgWriteDB = "DB table is unavailable: " + this.DBTableName + '\n';
                return false;
            }

        }

        public bool RunStoredProcedure()
        {
            if (this.sproc.Equals(""))
            {
                return true;
            }
            else
            {
                int result = 0;
                if (this.fileType.Equals("PrincessULC"))
                {
                    result = DataManager.RunSproc_PrincessULC(this.refDataRow);
                }
                else if (this.fileType.Equals("InCommULC"))
                {   //Shouldn't make it to this point because InComm doesn't have a sproc define yet
                    result = 0;
                }
                else if (this.fileType.Equals("DisneyInfinityLogULC"))
                {   //Shouldn't make it to this point because DisneyInfinityLog doesn't have a sproc define yet
                    result = 0;
                }
                else
                {
                    DataManager.TxtLog("This sproc is not handled in the program yet. Code changes will be need to be made");
                    return false;
                }

                if(result == 0)
                {
                    return true;
                }
                else
                {   
                    MessageBox.Show("SQL Error " + result + ": Stored Procedure has failed - " + this.sproc);
                    return false;
                }
            }
        }

        public bool DeleteFile()
        {
            if (File.Exists(fileAddress))
            {
                try
                {
                    System.IO.File.Delete(fileAddress);
                    DataManager.TxtLog(this.FileName + ": File delete succeeded");
                }
                catch (System.IO.IOException e)
                {
                    MessageBox.Show("An Issue occured while deleting file: " + fileName + '\n' +
                                    "Deleting from: " + fileAddress + '\n' +
                                    "File has already been exported to database." + '\n' +
                                    e.Message);
                    DataManager.TxtLog(this.FileName + ": File delete failed");
                    this.errorMsgDeleteFile = this.FileName + ": File delete failed" + '\n';
                }

                return !File.Exists(fileAddress);
            }
            else
            {
                return true;
            }
        }
    }   //class FileToProcess
}   //Namespace AZ_Princess_UID_Export
