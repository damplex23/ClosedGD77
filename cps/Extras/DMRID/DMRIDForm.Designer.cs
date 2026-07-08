namespace DMR
{
    partial class DMRIDForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DMRIDForm));
            this.btnWriteToGD77 = new System.Windows.Forms.Button();
            this.txtRegionId = new System.Windows.Forms.TextBox();
            this.btnClear = new System.Windows.Forms.Button();
            this.lblMessage = new System.Windows.Forms.Label();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.lblRegionId = new System.Windows.Forms.Label();
            this.cmbStringLen = new System.Windows.Forms.ComboBox();
            this.lblEnhancedLength = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.btnDownloadFromRadioId = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.cmbRadioType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.chkUseVPMemory = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnWriteToGD77
            // 
            this.btnWriteToGD77.Location = new System.Drawing.Point(457, 387);
            this.btnWriteToGD77.Name = "btnWriteToGD77";
            this.btnWriteToGD77.Size = new System.Drawing.Size(123, 28);
            this.btnWriteToGD77.TabIndex = 2;
            this.btnWriteToGD77.Text = "Write to GD-77";
            this.btnWriteToGD77.UseVisualStyleBackColor = true;
            this.btnWriteToGD77.Click += new System.EventHandler(this.btnWriteToGD77_Click);
            // 
            // txtRegionId
            // 
            this.txtRegionId.Location = new System.Drawing.Point(538, 70);
            this.txtRegionId.Name = "txtRegionId";
            this.txtRegionId.Size = new System.Drawing.Size(42, 20);
            this.txtRegionId.TabIndex = 3;
            this.txtRegionId.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(12, 387);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(71, 28);
            this.btnClear.TabIndex = 4;
            this.btnClear.Text = "Clear list";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // lblMessage
            // 
            this.lblMessage.Location = new System.Drawing.Point(12, 9);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(405, 23);
            this.lblMessage.TabIndex = 5;
            this.lblMessage.Text = "lblMessage";
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToResizeColumns = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(9, 207);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(571, 174);
            this.dataGridView1.TabIndex = 6;
            this.dataGridView1.SortCompare += new System.Windows.Forms.DataGridViewSortCompareEventHandler(this.dataGridView1_SortCompare);
            // 
            // lblRegionId
            // 
            this.lblRegionId.Location = new System.Drawing.Point(383, 70);
            this.lblRegionId.Name = "lblRegionId";
            this.lblRegionId.Size = new System.Drawing.Size(149, 16);
            this.lblRegionId.TabIndex = 7;
            this.lblRegionId.Text = "Region filter number";
            this.lblRegionId.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // cmbStringLen
            // 
            this.cmbStringLen.FormattingEnabled = true;
            this.cmbStringLen.Items.AddRange(new object[] {
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18",
            "19",
            "20",
            "21",
            "22",
            "23",
            "24",
            "25",
            "26",
            "27",
            "28",
            "29",
            "30",
            "31",
            "32"});
            this.cmbStringLen.Location = new System.Drawing.Point(15, 19);
            this.cmbStringLen.Name = "cmbStringLen";
            this.cmbStringLen.Size = new System.Drawing.Size(56, 21);
            this.cmbStringLen.TabIndex = 10;
            this.cmbStringLen.SelectedIndexChanged += new System.EventHandler(this.cmbStringLen_SelectedIndexChanged);
            // 
            // lblEnhancedLength
            // 
            this.lblEnhancedLength.AutoSize = true;
            this.lblEnhancedLength.Location = new System.Drawing.Point(85, 27);
            this.lblEnhancedLength.Name = "lblEnhancedLength";
            this.lblEnhancedLength.Size = new System.Drawing.Size(109, 13);
            this.lblEnhancedLength.TabIndex = 11;
            this.lblEnhancedLength.Text = "Number of characters";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(15, 35);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(565, 17);
            this.progressBar1.TabIndex = 12;
            // 
            // btnDownloadFromRadioId
            // 
            this.btnDownloadFromRadioId.Location = new System.Drawing.Point(15, 67);
            this.btnDownloadFromRadioId.Name = "btnDownloadFromRadioId";
            this.btnDownloadFromRadioId.Size = new System.Drawing.Size(203, 23);
            this.btnDownloadFromRadioId.TabIndex = 0;
            this.btnDownloadFromRadioId.Text = "Download from RadioID.net";
            this.btnDownloadFromRadioId.UseVisualStyleBackColor = true;
            this.btnDownloadFromRadioId.Click += new System.EventHandler(this.btnDownloadFromRadioId_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cmbStringLen);
            this.groupBox1.Controls.Add(this.lblEnhancedLength);
            this.groupBox1.Location = new System.Drawing.Point(18, 145);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 56);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Data record length";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(15, 96);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(203, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Import CSV";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.btnImportCSV_Click);
            // 
            // cmbRadioType
            // 
            this.cmbRadioType.FormattingEnabled = true;
            this.cmbRadioType.Items.AddRange(new object[] {
            "GD-77 / GD-77S / MD-760",
            "DM-1801",
            "RD-5R",
            "Custom 8Mb"});
            this.cmbRadioType.Location = new System.Drawing.Point(422, 172);
            this.cmbRadioType.Name = "cmbRadioType";
            this.cmbRadioType.Size = new System.Drawing.Size(158, 21);
            this.cmbRadioType.TabIndex = 14;
            this.cmbRadioType.SelectedIndexChanged += new System.EventHandler(this.cmbRadioType_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(325, 175);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 19);
            this.label1.TabIndex = 7;
            this.label1.Text = "Radio";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // chkUseVPMemory
            // 
            this.chkUseVPMemory.AutoSize = true;
            this.chkUseVPMemory.Location = new System.Drawing.Point(409, 145);
            this.chkUseVPMemory.Name = "chkUseVPMemory";
            this.chkUseVPMemory.Size = new System.Drawing.Size(150, 17);
            this.chkUseVPMemory.TabIndex = 15;
            this.chkUseVPMemory.Text = "Use Voice Prompt memory";
            this.chkUseVPMemory.UseVisualStyleBackColor = true;
            this.chkUseVPMemory.CheckedChanged += new System.EventHandler(this.chkUseVPMemory_CheckedChanged);
            // 
            // DMRIDForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(606, 422);
            this.Controls.Add(this.chkUseVPMemory);
            this.Controls.Add(this.cmbRadioType);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblRegionId);
            this.Controls.Add(this.txtRegionId);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnWriteToGD77);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnDownloadFromRadioId);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimizeBox = false;
            this.Name = "DMRIDForm";
            this.Text = "DMR ID";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DMRIDFormNew_FormClosing);
            this.Load += new System.EventHandler(this.DMRIDForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnWriteToGD77;
        private System.Windows.Forms.TextBox txtRegionId;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Label lblRegionId;
        private System.Windows.Forms.ComboBox cmbStringLen;
        private System.Windows.Forms.Label lblEnhancedLength;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button btnDownloadFromRadioId;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox cmbRadioType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkUseVPMemory;
    }
}