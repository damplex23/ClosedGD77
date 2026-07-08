using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace DMR
{
    public class BootItemForm : DockContent, IDisp
    {
        private enum BootScreenE
        {
            DefaultImage,
            CustomText
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class BootItem : IVerify<BootItem>
        {
            private byte bootScreen;

            private byte bootPwdEnable;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            private byte[] reserve;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            private byte[] bootPwd;

            private byte reserve1;

            public bool BootScreen
            {
                get
                {
                    return Convert.ToBoolean(this.bootScreen);
                }
                set
                {
                    this.bootScreen = Convert.ToByte(value);
                }
            }

            public int BootScreenType
            {
                get
                {
                    if (Enum.IsDefined(typeof(BootScreenE), (int)this.bootScreen))
                    {
                        return this.bootScreen;
                    }
                    return 0;
                }
                set
                {
                    if (Enum.IsDefined(typeof(BootScreenE), value))
                    {
                        this.bootScreen = Convert.ToByte(value);
                    }
                    else
                    {
                        this.bootScreen = 0;
                    }
                }
            }

            public bool BootPwdEnable
            {
                get
                {
                    return Convert.ToBoolean(this.bootPwdEnable);
                }
                set
                {
                    this.bootPwdEnable = Convert.ToByte(value);
                }
            }

            public string BootPwd
            {
                get
                {
                    int num = 0;
                    StringBuilder stringBuilder = new StringBuilder(6);
                    for (num = 0; num < 3 && this.bootPwd[num] != 255; num++)
                    {
                        stringBuilder.Append(this.bootPwd[num].ToString("X2"));
                    }
                    return stringBuilder.ToString().TrimEnd('F');
                }
                set
                {
                    int num = 0;
                    this.bootPwd.Fill((byte)255);
                    string text = value.PadRight(6, 'F');
                    for (num = 0; num < 3; num++)
                    {
                        this.bootPwd[num] = Convert.ToByte(text.Substring(num * 2, 2), 16);
                    }
                }
            }

            public BootItem()
            {

                //base._002Ector();
                this.reserve = new byte[2];
                this.bootPwd = new byte[3];
            }

            public void Verify(BootItem def)
            {
                Settings.smethod_11(ref this.bootScreen, BootItemForm.MIN_BOOT_SCREEN, BootItemForm.MAX_BOOT_SCREEN, def.bootScreen);
                Settings.smethod_11(ref this.bootPwdEnable, (byte)0, (byte)1, def.bootPwdEnable);
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class BootContent
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            private byte[] bootLine1;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            private byte[] bootLine2;

            public string BootLine1
            {
                get
                {
                    return Settings.smethod_25(this.bootLine1);
                }
                set
                {
                    byte[] array = Settings.smethod_23(value);
                    this.bootLine1.Fill((byte)255);
                    Array.Copy(array, 0, this.bootLine1, 0, Math.Min(array.Length, this.bootLine1.Length));
                }
            }

            public string BootLine2
            {
                get
                {
                    return Settings.smethod_25(this.bootLine2);
                }
                set
                {
                    byte[] array = Settings.smethod_23(value);
                    this.bootLine2.Fill((byte)255);
                    Array.Copy(array, 0, this.bootLine2, 0, Math.Min(array.Length, this.bootLine2.Length));
                }
            }

            public BootContent()
            {

                //base._002Ector();
                this.bootLine1 = new byte[16];
                this.bootLine2 = new byte[16];
            }
        }

        private const byte MIN_BOOT_PWD_ENABLE = 0;

        private const byte MAX_BOOT_PWD_ENABLE = 1;

        private const int LEN_BOOT_PWD = 6;

        private const int SPACE_BOOT_PWD = 3;

        private const string SZ_BOOT_PWD = "0123456789\b";

        private const int LEN_BOOT_LINE = 16;

        private const int SPACE_BOOT_LINE = 16;

        private const string SZ_BOOT_SCREEN_NAME = "BootScreen";

        //private IContainer components;

        private CheckBox chkBootScreen;

        private CheckBox chkBootPwdEnable;

        private Label lblBootPwd;

        private SGTextBox txtBootPwd;

        private SGTextBox txtLine1;

        private Label lblLine1;

        private SGTextBox txtLine2;

        private Label lblLine2;

        private Label lblBootScreen;

        private ComboBox cmbBootScreen;

        private GroupBox grpBootScreen;

        private static readonly byte MIN_BOOT_SCREEN;

        private static readonly byte MAX_BOOT_SCREEN;

        private static readonly string[] SZ_BOOT_SCREEN;

        public static BootItem DefaultBootItem;

        public static BootItem data;

        public static BootContent dataContent;

        public TreeNode Node
        {
            get;
            set;
        }

        protected override void Dispose(bool disposing)
        {
            /*
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}*/
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.chkBootScreen = new System.Windows.Forms.CheckBox();
            this.chkBootPwdEnable = new System.Windows.Forms.CheckBox();
            this.lblBootPwd = new System.Windows.Forms.Label();
            this.lblLine1 = new System.Windows.Forms.Label();
            this.lblLine2 = new System.Windows.Forms.Label();
            this.lblBootScreen = new System.Windows.Forms.Label();
            this.cmbBootScreen = new System.Windows.Forms.ComboBox();
            this.grpBootScreen = new System.Windows.Forms.GroupBox();
            this.txtLine2 = new DMR.SGTextBox();
            this.txtLine1 = new DMR.SGTextBox();
            this.txtBootPwd = new DMR.SGTextBox();
            this.grpBootScreen.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkBootScreen
            // 
            this.chkBootScreen.AutoSize = true;
            this.chkBootScreen.Location = new System.Drawing.Point(24, 273);
            this.chkBootScreen.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkBootScreen.Name = "chkBootScreen";
            this.chkBootScreen.Size = new System.Drawing.Size(104, 20);
            this.chkBootScreen.TabIndex = 0;
            this.chkBootScreen.Text = "Intro Screen";
            this.chkBootScreen.UseVisualStyleBackColor = true;
            this.chkBootScreen.Visible = false;
            // 
            // chkBootPwdEnable
            // 
            this.chkBootPwdEnable.AutoSize = true;
            this.chkBootPwdEnable.Location = new System.Drawing.Point(12, 193);
            this.chkBootPwdEnable.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkBootPwdEnable.Name = "chkBootPwdEnable";
            this.chkBootPwdEnable.Size = new System.Drawing.Size(202, 20);
            this.chkBootPwdEnable.TabIndex = 2;
            this.chkBootPwdEnable.Text = "Power On Password Enable";
            this.chkBootPwdEnable.UseVisualStyleBackColor = true;
            this.chkBootPwdEnable.CheckedChanged += new System.EventHandler(this.chkBootPwdEnable_CheckedChanged);
            // 
            // lblBootPwd
            // 
            this.lblBootPwd.Location = new System.Drawing.Point(8, 221);
            this.lblBootPwd.Name = "lblBootPwd";
            this.lblBootPwd.Size = new System.Drawing.Size(166, 24);
            this.lblBootPwd.TabIndex = 3;
            this.lblBootPwd.Text = "Power On Password";
            this.lblBootPwd.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblLine1
            // 
            this.lblLine1.Location = new System.Drawing.Point(12, 74);
            this.lblLine1.Name = "lblLine1";
            this.lblLine1.Size = new System.Drawing.Size(104, 24);
            this.lblLine1.TabIndex = 2;
            this.lblLine1.Text = "Line 1";
            this.lblLine1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblLine2
            // 
            this.lblLine2.Location = new System.Drawing.Point(12, 116);
            this.lblLine2.Name = "lblLine2";
            this.lblLine2.Size = new System.Drawing.Size(104, 24);
            this.lblLine2.TabIndex = 4;
            this.lblLine2.Text = "Line 2";
            this.lblLine2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblBootScreen
            // 
            this.lblBootScreen.Location = new System.Drawing.Point(12, 32);
            this.lblBootScreen.Name = "lblBootScreen";
            this.lblBootScreen.Size = new System.Drawing.Size(104, 24);
            this.lblBootScreen.TabIndex = 0;
            this.lblBootScreen.Text = "Intro Screen";
            this.lblBootScreen.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbBootScreen
            // 
            this.cmbBootScreen.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBootScreen.ForeColor = System.Drawing.SystemColors.WindowText;
            this.cmbBootScreen.FormattingEnabled = true;
            this.cmbBootScreen.Items.AddRange(new object[] {
            "",
            ""});
            this.cmbBootScreen.Location = new System.Drawing.Point(122, 32);
            this.cmbBootScreen.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cmbBootScreen.Name = "cmbBootScreen";
            this.cmbBootScreen.Size = new System.Drawing.Size(126, 24);
            this.cmbBootScreen.TabIndex = 1;
            this.cmbBootScreen.SelectedIndexChanged += new System.EventHandler(this.cmbBootScreen_SelectedIndexChanged);
            // 
            // grpBootScreen
            // 
            this.grpBootScreen.Controls.Add(this.lblBootScreen);
            this.grpBootScreen.Controls.Add(this.cmbBootScreen);
            this.grpBootScreen.Controls.Add(this.txtLine2);
            this.grpBootScreen.Controls.Add(this.lblLine2);
            this.grpBootScreen.Controls.Add(this.txtLine1);
            this.grpBootScreen.Controls.Add(this.lblLine1);
            this.grpBootScreen.Location = new System.Drawing.Point(12, 13);
            this.grpBootScreen.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpBootScreen.Name = "grpBootScreen";
            this.grpBootScreen.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpBootScreen.Size = new System.Drawing.Size(288, 172);
            this.grpBootScreen.TabIndex = 1;
            this.grpBootScreen.TabStop = false;
            this.grpBootScreen.Text = "Intro Screen";
            // 
            // txtLine2
            // 
            this.txtLine2.InputString = null;
            this.txtLine2.Location = new System.Drawing.Point(122, 116);
            this.txtLine2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtLine2.MaxByteLength = 0;
            this.txtLine2.Name = "txtLine2";
            this.txtLine2.Size = new System.Drawing.Size(126, 23);
            this.txtLine2.TabIndex = 5;
            this.txtLine2.Leave += new System.EventHandler(this.txtLine2_Leave);
            // 
            // txtLine1
            // 
            this.txtLine1.InputString = null;
            this.txtLine1.Location = new System.Drawing.Point(122, 74);
            this.txtLine1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtLine1.MaxByteLength = 0;
            this.txtLine1.Name = "txtLine1";
            this.txtLine1.Size = new System.Drawing.Size(126, 23);
            this.txtLine1.TabIndex = 3;
            this.txtLine1.Leave += new System.EventHandler(this.txtLine1_Leave);
            // 
            // txtBootPwd
            // 
            this.txtBootPwd.InputString = null;
            this.txtBootPwd.Location = new System.Drawing.Point(180, 221);
            this.txtBootPwd.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtBootPwd.MaxByteLength = 0;
            this.txtBootPwd.Name = "txtBootPwd";
            this.txtBootPwd.Size = new System.Drawing.Size(120, 23);
            this.txtBootPwd.TabIndex = 4;
            // 
            // BootItemForm
            // 
            this.ClientSize = new System.Drawing.Size(350, 262);
            this.Controls.Add(this.grpBootScreen);
            this.Controls.Add(this.txtBootPwd);
            this.Controls.Add(this.lblBootPwd);
            this.Controls.Add(this.chkBootPwdEnable);
            this.Controls.Add(this.chkBootScreen);
            this.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BootItemForm";
            this.Text = "Boot Item";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BootItemForm_FormClosing);
            this.Load += new System.EventHandler(this.BootItemForm_Load);
            this.grpBootScreen.ResumeLayout(false);
            this.grpBootScreen.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        public void SaveData()
        {
            BootItemForm.data.BootScreen = this.chkBootScreen.Checked;
            BootItemForm.data.BootScreenType = this.cmbBootScreen.SelectedIndex;
            BootItemForm.dataContent.BootLine1 = this.txtLine1.Text;
            BootItemForm.dataContent.BootLine2 = this.txtLine2.Text;
            BootItemForm.data.BootPwdEnable = this.chkBootPwdEnable.Checked;
            BootItemForm.data.BootPwd = this.txtBootPwd.Text;
        }

        public void DispData()
        {
            this.chkBootScreen.Checked = BootItemForm.data.BootScreen;
            this.cmbBootScreen.SelectedIndex = BootItemForm.data.BootScreenType;
            this.txtLine1.Text = BootItemForm.dataContent.BootLine1;
            this.txtLine2.Text = BootItemForm.dataContent.BootLine2;
            this.chkBootPwdEnable.Checked = BootItemForm.data.BootPwdEnable;
            this.txtBootPwd.Text = BootItemForm.data.BootPwd;
            this.chkBootPwdEnable_CheckedChanged(null, null);
            this.RefreshByUserMode();
        }

        public void RefreshByUserMode()
        {
            bool flag = Settings.getUserExpertSettings() == Settings.UserMode.Expert;
            this.grpBootScreen.Enabled &= flag;
            this.chkBootPwdEnable.Enabled &= flag;
            this.lblBootPwd.Enabled &= flag;
            this.txtBootPwd.Enabled &= flag;
        }

        public void RefreshName()
        {
        }

        public BootItemForm()
        {

            //base._002Ector();
            this.InitializeComponent();
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);// Roger Clark. Added correct icon on main form!
            base.Scale(Settings.smethod_6());
        }

        private void method_1()
        {
            this.txtBootPwd.MaxLength = 6;
            this.txtBootPwd.InputString = "0123456789\b";
            Settings.smethod_37(this.cmbBootScreen, BootItemForm.SZ_BOOT_SCREEN);
            this.txtLine1.MaxByteLength = 16;
            this.txtLine2.MaxByteLength = 16;
        }

        public static void RefreshCommonLang()
        {
            string name = typeof(BootItemForm).Name;
            Settings.smethod_78("BootScreen", BootItemForm.SZ_BOOT_SCREEN, name);
        }

        private void BootItemForm_Load(object sender, EventArgs e)
        {
            Settings.smethod_59(base.Controls);
            Settings.UpdateComponentTextsFromLanguageXmlData(this);
            this.method_1();
            this.DispData();
        }

        private void BootItemForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.SaveData();
        }

        private void chkBootPwdEnable_CheckedChanged(object sender, EventArgs e)
        {
            this.lblBootPwd.Enabled = this.chkBootPwdEnable.Checked;
            this.txtBootPwd.Enabled = this.chkBootPwdEnable.Checked;
        }

        private void cmbBootScreen_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void txtLine1_Leave(object sender, EventArgs e)
        {
            BootItemForm.dataContent.BootLine1 = this.txtLine1.Text;
        }

        private void txtLine2_Leave(object sender, EventArgs e)
        {
            BootItemForm.dataContent.BootLine2 = this.txtLine2.Text;
        }

        static BootItemForm()
        {

            BootItemForm.MIN_BOOT_SCREEN = 0;
            BootItemForm.MAX_BOOT_SCREEN = 1;
            BootItemForm.SZ_BOOT_SCREEN = new string[2]
            {
                "Picture",
                "Char String"
            };
            BootItemForm.DefaultBootItem = new BootItem();
            BootItemForm.data = new BootItem();
            BootItemForm.dataContent = new BootContent();
        }
    }
}
