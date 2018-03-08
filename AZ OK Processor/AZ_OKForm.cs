using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Configuration;

namespace AZ_OK_Processor
{
    public partial class AZ_OKForm : Form
    {
        private string OKFileDirectory = ConfigurationManager.AppSettings["OKFileDirectory"];
        private List<FileToProcess> Files = new List<FileToProcess>();
        private string[] selectedFiles;

        public AZ_OKForm()
        {
            InitializeComponent();
        }

        #region GUI update functions
        public void Update_DataGrid(int row, string column1Text, string column2Text, bool passFail)
        {
            this.dataGridView1.Rows.Add(new object[] 
                    {
                        column1Text, 
                        column2Text
                    });
            if (passFail)
            {
                this.dataGridView1.Rows[row].Cells["dataGridColumn1"].Style.BackColor = Color.LightGreen;
                this.dataGridView1.Rows[row].Cells["dataGridColumn2"].Style.BackColor = Color.LightGreen;
            }
            this.dataGridView1.ClearSelection();

            this.dataGridView1.Refresh();
        }   //Update_DataGrid()


        public void Update_TxtFilesProcessed(string Msg)
        {
            txtFilesProcessed.Text += Msg + '\n';
        }


        public void Change_TxtFilesProcessed(string Msg)
        {
            txtFilesProcessed.Text = Msg;
            txtFilesProcessed.Refresh();
        }


        public void Clear_TxtFilesProcessed()
        {
            txtFilesProcessed.Text  = "";
        }
        #endregion

        private void folderPath_Click(object sender, EventArgs e)
        {
            this.txtFilesProcessed.Text = "";
            this.dataGridView1.Rows.Clear();

            if (!DataManager.IsServerConnected())
            {   //Check if Database is connected
                MessageBox.Show("Database not connected");
                return;
            }

            DataManager.GetRefData();
            openFileDialog1.InitialDirectory = OKFileDirectory;
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                selectedFiles = openFileDialog1.FileNames;
                Files = new List<FileToProcess>();
                for(int i = 0; i < selectedFiles.Length; i++)
                {
                    Files.Add(new FileToProcess(selectedFiles[i]));
                    DataManager.TxtLog("Successfully added " + Files[i].FileName + " to the list of files to process");
                    if (!Files[i].IsDataFormatOK)
                    {   //If current file does not match a stored format, then do not run this file
                        DataManager.TxtLog(Files[i].FileName + ": " + Files[i].ErrorMsgeFileFormat);
                        this.Update_DataGrid(i, Files[i].FileName, Files[i].ErrorMsgeFileFormat, Files[i].IsDataFormatOK);
                    }
                    else if (!Files[i].IsExtOK)
                    {   //If current file is not a .OK file, do not run this file
                        DataManager.TxtLog(Files[i].FileName + ": " + Files[i].ErrorMsgInvalidExt);
                        this.Update_DataGrid(i, Files[i].FileName, Files[i].ErrorMsgInvalidExt, Files[i].IsExtOK);
                    }
                    else
                    {   //If current file passes previous tests, then make this file available to run
                        DataManager.TxtLog(Files[i].FileName + ": " + Files[i].TotalGoodRecords + " of " + Files[i].TotalRecords + " records passed (" + Files[i].PercentGoodRecords + "%)");
                        this.Update_DataGrid(i, Files[i].FileName, Files[i].TotalGoodRecords + " of " + Files[i].TotalRecords + " records passed (" + Files[i].PercentGoodRecords + "%)", Files[i].IsExtOK && Files[i].IsDataFormatOK);
                    }
                }   //for

                for (int i = 0; i < Files.Count; i++ )
                {   //Remove all files that failed previous tests from the List of files to process
                    if (!Files[i].IsExtOK || !Files[i].IsDataFormatOK)
                    {
                        DataManager.TxtLog(Files[i].FileName + " is being removed from files to process due to an error with the file");
                        Files.RemoveAt(i);
                        i--;
                    }
                }   //for
            }   //if
        }   //folderPath_Click()


        private void button1_Click(object sender, EventArgs e)
        {
            DataManager.CreateTestOKFile();
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            Clear_TxtFilesProcessed();

            if (!DataManager.IsServerConnected())
            {   //Check if Database is connected
                MessageBox.Show("Database not connected");
                return;
            }

            for (int i = 0; i < Files.Count; i++)
            {
                DataManager.TxtLog(Files[i].FileName + ": Beginning to process");

                if (!Files[i].ReadFile())
                {   //Read File and store UIDs
                    DataManager.TxtLog(Files[i].FileName + ": File read failed");
                    txtFilesProcessed.Text += Files[i].FileName + ": File read failed" + '\n';
                    continue;
                }
                else
                {
                    DataManager.TxtLog(Files[i].FileName + ": File Read has succeeded");
                }

                if (!Files[i].WriteFileToDB())
                {
                    txtFilesProcessed.Text += Files[i].ErrorMsgWriteDB;
                    continue;
                }
                else
                {
                    //txtFilesProcessed.Text += "Duplicate Cleanup successful" + '\n';
                    this.Update_TxtFilesProcessed(Files[i].FileName + ": File written to DB successfully");
                }

                if (!Files[i].RunStoredProcedure())
                {
                    this.Update_TxtFilesProcessed(Files[i].FileName + ": Stored Procedure Failed");
                    DataManager.TxtLog(Files[i].FileName + ": Stored Procedure failed - " + Files[i].Sproc);
                }
                else
                {
                    DataManager.TxtLog(Files[i].FileName + ": Stored Procedure succeeded - " + Files[i].Sproc);
                }
                //string result = DataManager.RunSproc_PrincessULC(Files[i].RefDataRow);

                if (!Files[i].CopyFile())
                {   //Copy file to a backup directory to keep
                    DataManager.TxtLog(Files[i].FileName + ": " + Files[i].ErrorMsgCopyFile + " - File copy failed");
                    txtFilesProcessed.Text += Files[i].ErrorMsgCopyFile + '\n';
                    continue;
                }
                else
                {
                    DataManager.TxtLog(Files[i].FileName + ": File successfully copied to the corresponding OK File Backup: " + Files[i].FileCopyAddress);
                }

                if (!Files[i].DeleteFile())
                {   //Delete File from original folder
                    txtFilesProcessed.Text += Files[i].ErrorMsgDeleteFile;
                }
                else
                {
                    DataManager.TxtLog(Files[i].FileName + ": File delete successful");
                }
            }   //for

            //DataManager.DuplicateCleanup();
            //for (int i = 0; i < DataManager.FileFormats.Tables[0].Rows.Count; i++ )
            //{
            //    string result = DataManager.RunSproc_PrincessULC(DataManager.FileFormats.Tables[0].Rows[i]);
            //}
            Files = new List<FileToProcess>();
        }   //btnImport_Click()
    }   //class AZ_OKForm : Form
}   //namespace AZ_OK_Processor
