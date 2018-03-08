namespace AZ_OK_Processor
{
    partial class AZ_OKForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblExport = new System.Windows.Forms.Label();
            this.btnImport = new System.Windows.Forms.Button();
            this.txtFilesProcessed = new System.Windows.Forms.RichTextBox();
            this.lblFilesProcessed = new System.Windows.Forms.Label();
            this.folderPath = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.btnTestFile = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.dataGridColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // lblExport
            // 
            this.lblExport.Location = new System.Drawing.Point(0, 0);
            this.lblExport.Name = "lblExport";
            this.lblExport.Size = new System.Drawing.Size(100, 23);
            this.lblExport.TabIndex = 11;
            // 
            // btnImport
            // 
            this.btnImport.Location = new System.Drawing.Point(12, 223);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(75, 23);
            this.btnImport.TabIndex = 2;
            this.btnImport.Text = "Upload";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // txtFilesProcessed
            // 
            this.txtFilesProcessed.Location = new System.Drawing.Point(106, 223);
            this.txtFilesProcessed.Name = "txtFilesProcessed";
            this.txtFilesProcessed.Size = new System.Drawing.Size(468, 286);
            this.txtFilesProcessed.TabIndex = 4;
            this.txtFilesProcessed.Text = "";
            // 
            // lblFilesProcessed
            // 
            this.lblFilesProcessed.AutoSize = true;
            this.lblFilesProcessed.Location = new System.Drawing.Point(103, 207);
            this.lblFilesProcessed.Name = "lblFilesProcessed";
            this.lblFilesProcessed.Size = new System.Drawing.Size(81, 13);
            this.lblFilesProcessed.TabIndex = 5;
            this.lblFilesProcessed.Text = "Files Processed";
            // 
            // folderPath
            // 
            this.folderPath.Location = new System.Drawing.Point(12, 12);
            this.folderPath.Name = "folderPath";
            this.folderPath.Size = new System.Drawing.Size(75, 23);
            this.folderPath.TabIndex = 6;
            this.folderPath.Text = "Browse";
            this.folderPath.UseVisualStyleBackColor = true;
            this.folderPath.Click += new System.EventHandler(this.folderPath_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Multiselect = true;
            // 
            // btnTestFile
            // 
            this.btnTestFile.Location = new System.Drawing.Point(549, 199);
            this.btnTestFile.Name = "btnTestFile";
            this.btnTestFile.Size = new System.Drawing.Size(25, 28);
            this.btnTestFile.TabIndex = 8;
            this.btnTestFile.UseVisualStyleBackColor = true;
            this.btnTestFile.Click += new System.EventHandler(this.button1_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridColumn1,
            this.dataGridColumn2});
            this.dataGridView1.Location = new System.Drawing.Point(106, 12);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowTemplate.DefaultCellStyle.BackColor = System.Drawing.Color.Red;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridView1.Size = new System.Drawing.Size(468, 192);
            this.dataGridView1.TabIndex = 9;
            // 
            // dataGridColumn1
            // 
            this.dataGridColumn1.HeaderText = "File";
            this.dataGridColumn1.Name = "dataGridColumn1";
            this.dataGridColumn1.Width = 200;
            // 
            // dataGridColumn2
            // 
            this.dataGridColumn2.HeaderText = "Status";
            this.dataGridColumn2.Name = "dataGridColumn2";
            this.dataGridColumn2.Width = 259;
            // 
            // AZ_OKForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(586, 521);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.btnTestFile);
            this.Controls.Add(this.folderPath);
            this.Controls.Add(this.lblFilesProcessed);
            this.Controls.Add(this.txtFilesProcessed);
            this.Controls.Add(this.btnImport);
            this.Controls.Add(this.lblExport);
            this.Name = "AZ_OKForm";
            this.Text = "AZ OK Processor 1.1";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblExport;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.RichTextBox txtFilesProcessed;
        private System.Windows.Forms.Label lblFilesProcessed;
        private System.Windows.Forms.Button folderPath;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button btnTestFile;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridColumn2;

    }
}

