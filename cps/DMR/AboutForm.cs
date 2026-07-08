using System;
using System.Drawing;
using System.Windows.Forms;

namespace DMR
{
    public class AboutForm : Form
    {
        private Label lblVersion;
        private Label lblTranslationCredit;
        private Button btnClose;

        public AboutForm()
        {
            this.InitializeComponent();
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);// Roger Clark. Added correct icon on main form!
            base.Scale(Settings.smethod_6());
        }

        private void AboutForm_Load(object sender, EventArgs e)
        {
            Settings.UpdateComponentTextsFromLanguageXmlData(this);
            this.lblVersion.Text = "OpenGD77 CPS";
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            base.Close();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblVersion = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblTranslationCredit = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblVersion
            // 
            this.lblVersion.Location = new System.Drawing.Point(31, 20);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(351, 20);
            this.lblVersion.TabIndex = 0;
            this.lblVersion.Text = "v1.0.0";
            this.lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(175, 60);
            this.btnClose.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(64, 27);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "OK";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lblTranslationCredit
            // 
            this.lblTranslationCredit.Location = new System.Drawing.Point(31, 204);
            this.lblTranslationCredit.Name = "lblTranslationCredit";
            this.lblTranslationCredit.Size = new System.Drawing.Size(351, 20);
            this.lblTranslationCredit.TabIndex = 0;
            this.lblTranslationCredit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // AboutForm
            // 
            this.ClientSize = new System.Drawing.Size(409, 104);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lblTranslationCredit);
            this.Controls.Add(this.lblVersion);
            this.Font = new System.Drawing.Font("Arial", 10F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MinimizeBox = false;
            this.Name = "AboutForm";
            this.Text = "About";
            this.Load += new System.EventHandler(this.AboutForm_Load);
            this.ResumeLayout(false);
        }
    }
}
