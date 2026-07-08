using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace DMR
{
    public class RxGroupListForm : DockContent, IDisp
    {
        //private IContainer components;

        private ToolStrip tsrGrpList;
        private ToolStripButton tsbtnFirst;
        private ToolStripButton tsbtnPrev;
        private ToolStripButton tsbtnNext;
        private ToolStripButton tsbtnLast;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton tsbtnAdd;
        private ToolStripButton tsbtnDel;
        private ToolStripMenuItem tsmiCh;
        private ToolStripMenuItem tsmiFirst;
        private ToolStripMenuItem tsmiPrev;
        private ToolStripMenuItem tsmiNext;
        private ToolStripMenuItem tsmiLast;
        private ToolStripMenuItem tsmiAdd;
        private ToolStripMenuItem tsmiDel;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripLabel tslblInfo;


        private Button btnDel;

        private Button btnAdd;
        private ListBox lstSelected;
        private ListBox lstUnselected;
        private SGTextBox txtName;
        private Label lblName;
        private GroupBox grpUnselected;
        private GroupBox grpSelected;
        private Button btnUp;
        private Button btnDown;
        private CustomPanel pnlRxGrpList;

        public static RxListData data;

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
            this.tsmiCh = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiFirst = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiPrev = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiNext = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiLast = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDel = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlRxGrpList = new CustomPanel();
            this.tsrGrpList = new System.Windows.Forms.ToolStrip();
            this.tslblInfo = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbtnFirst = new System.Windows.Forms.ToolStripButton();
            this.tsbtnPrev = new System.Windows.Forms.ToolStripButton();
            this.tsbtnNext = new System.Windows.Forms.ToolStripButton();
            this.tsbtnLast = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbtnAdd = new System.Windows.Forms.ToolStripButton();
            this.tsbtnDel = new System.Windows.Forms.ToolStripButton();
            this.btnDown = new System.Windows.Forms.Button();
            this.btnUp = new System.Windows.Forms.Button();
            this.txtName = new DMR.SGTextBox();
            this.grpSelected = new System.Windows.Forms.GroupBox();
            this.lstSelected = new System.Windows.Forms.ListBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.grpUnselected = new System.Windows.Forms.GroupBox();
            this.lstUnselected = new System.Windows.Forms.ListBox();
            this.btnDel = new System.Windows.Forms.Button();
            this.lblName = new System.Windows.Forms.Label();
            this.pnlRxGrpList.SuspendLayout();
            this.tsrGrpList.SuspendLayout();
            this.grpSelected.SuspendLayout();
            this.grpUnselected.SuspendLayout();
            this.SuspendLayout();
            // 
            // tsmiCh
            // 
            this.tsmiCh.Name = "tsmiCh";
            this.tsmiCh.Size = new System.Drawing.Size(32, 19);
            // 
            // tsmiFirst
            // 
            this.tsmiFirst.Name = "tsmiFirst";
            this.tsmiFirst.Size = new System.Drawing.Size(32, 19);
            // 
            // tsmiPrev
            // 
            this.tsmiPrev.Name = "tsmiPrev";
            this.tsmiPrev.Size = new System.Drawing.Size(32, 19);
            // 
            // tsmiNext
            // 
            this.tsmiNext.Name = "tsmiNext";
            this.tsmiNext.Size = new System.Drawing.Size(32, 19);
            // 
            // tsmiLast
            // 
            this.tsmiLast.Name = "tsmiLast";
            this.tsmiLast.Size = new System.Drawing.Size(32, 19);
            // 
            // tsmiAdd
            // 
            this.tsmiAdd.Name = "tsmiAdd";
            this.tsmiAdd.Size = new System.Drawing.Size(32, 19);
            // 
            // tsmiDel
            // 
            this.tsmiDel.Name = "tsmiDel";
            this.tsmiDel.Size = new System.Drawing.Size(32, 19);
            // 
            // pnlRxGrpList
            // 
            this.pnlRxGrpList.AutoScroll = true;
            this.pnlRxGrpList.AutoSize = true;
            this.pnlRxGrpList.Controls.Add(this.tsrGrpList);
            this.pnlRxGrpList.Controls.Add(this.btnDown);
            this.pnlRxGrpList.Controls.Add(this.btnUp);
            this.pnlRxGrpList.Controls.Add(this.txtName);
            this.pnlRxGrpList.Controls.Add(this.grpSelected);
            this.pnlRxGrpList.Controls.Add(this.btnAdd);
            this.pnlRxGrpList.Controls.Add(this.grpUnselected);
            this.pnlRxGrpList.Controls.Add(this.btnDel);
            this.pnlRxGrpList.Controls.Add(this.lblName);
            this.pnlRxGrpList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRxGrpList.Location = new System.Drawing.Point(0, 0);
            this.pnlRxGrpList.Name = "pnlRxGrpList";
            this.pnlRxGrpList.Size = new System.Drawing.Size(693, 567);
            this.pnlRxGrpList.TabIndex = 8;
            // 
            // tsrGrpList
            // 
            this.tsrGrpList.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tslblInfo,
            this.toolStripSeparator2,
            this.tsbtnFirst,
            this.tsbtnPrev,
            this.tsbtnNext,
            this.tsbtnLast,
            this.toolStripSeparator1,
            this.tsbtnAdd,
            this.tsbtnDel});
            this.tsrGrpList.Location = new System.Drawing.Point(0, 0);
            this.tsrGrpList.Name = "tsrGrpList";
            this.tsrGrpList.Size = new System.Drawing.Size(693, 25);
            this.tsrGrpList.TabIndex = 0;
            // 
            // tslblInfo
            // 
            this.tslblInfo.Name = "tslblInfo";
            this.tslblInfo.Size = new System.Drawing.Size(0, 22);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbtnFirst
            // 
            this.tsbtnFirst.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbtnFirst.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbtnFirst.Name = "tsbtnFirst";
            this.tsbtnFirst.Size = new System.Drawing.Size(23, 22);
            this.tsbtnFirst.Text = "First";
            this.tsbtnFirst.Click += new System.EventHandler(this.tsmiFirst_Click);
            // 
            // tsbtnPrev
            // 
            this.tsbtnPrev.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbtnPrev.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbtnPrev.Name = "tsbtnPrev";
            this.tsbtnPrev.Size = new System.Drawing.Size(23, 22);
            this.tsbtnPrev.Text = "Previous";
            this.tsbtnPrev.Click += new System.EventHandler(this.tsmiPrev_Click);
            // 
            // tsbtnNext
            // 
            this.tsbtnNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbtnNext.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbtnNext.Name = "tsbtnNext";
            this.tsbtnNext.Size = new System.Drawing.Size(23, 22);
            this.tsbtnNext.Text = "Next";
            this.tsbtnNext.Click += new System.EventHandler(this.tsmiNext_Click);
            // 
            // tsbtnLast
            // 
            this.tsbtnLast.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbtnLast.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbtnLast.Name = "tsbtnLast";
            this.tsbtnLast.Size = new System.Drawing.Size(23, 22);
            this.tsbtnLast.Text = "Last";
            this.tsbtnLast.Click += new System.EventHandler(this.tsmiLast_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbtnAdd
            // 
            this.tsbtnAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbtnAdd.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbtnAdd.Name = "tsbtnAdd";
            this.tsbtnAdd.Size = new System.Drawing.Size(23, 22);
            this.tsbtnAdd.Text = "Add..";
            this.tsbtnAdd.Click += new System.EventHandler(this.tsmiAdd_Click);
            // 
            // tsbtnDel
            // 
            this.tsbtnDel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbtnDel.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbtnDel.Name = "tsbtnDel";
            this.tsbtnDel.Size = new System.Drawing.Size(23, 22);
            this.tsbtnDel.Text = "Delete";
            this.tsbtnDel.Click += new System.EventHandler(this.tsmiDel_Click);
            // 
            // btnDown
            // 
            this.btnDown.Location = new System.Drawing.Point(598, 276);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(75, 23);
            this.btnDown.TabIndex = 9;
            this.btnDown.Text = "Down";
            this.btnDown.UseVisualStyleBackColor = true;
            this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
            // 
            // btnUp
            // 
            this.btnUp.Location = new System.Drawing.Point(598, 224);
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new System.Drawing.Size(75, 23);
            this.btnUp.TabIndex = 8;
            this.btnUp.Text = "Up";
            this.btnUp.UseVisualStyleBackColor = true;
            this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
            // 
            // txtName
            // 
            this.txtName.InputString = null;
            this.txtName.Location = new System.Drawing.Point(100, 39);
            this.txtName.MaxByteLength = 0;
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(115, 23);
            this.txtName.TabIndex = 1;
            this.txtName.Leave += new System.EventHandler(this.txtName_Leave);
            // 
            // grpSelected
            // 
            this.grpSelected.Controls.Add(this.lstSelected);
            this.grpSelected.Location = new System.Drawing.Point(353, 110);
            this.grpSelected.Name = "grpSelected";
            this.grpSelected.Size = new System.Drawing.Size(230, 433);
            this.grpSelected.TabIndex = 7;
            this.grpSelected.TabStop = false;
            this.grpSelected.Text = "Member";
            // 
            // lstSelected
            // 
            this.lstSelected.FormattingEnabled = true;
            this.lstSelected.ItemHeight = 16;
            this.lstSelected.Location = new System.Drawing.Point(25, 25);
            this.lstSelected.Name = "lstSelected";
            this.lstSelected.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstSelected.Size = new System.Drawing.Size(180, 388);
            this.lstSelected.TabIndex = 5;
            this.lstSelected.SelectedIndexChanged += new System.EventHandler(this.lstSelected_SelectedIndexChanged);
            this.lstSelected.DoubleClick += new System.EventHandler(this.lstSelected_DoubleClick);
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(258, 224);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 23);
            this.btnAdd.TabIndex = 3;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // grpUnselected
            // 
            this.grpUnselected.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.grpUnselected.Controls.Add(this.lstUnselected);
            this.grpUnselected.Location = new System.Drawing.Point(10, 97);
            this.grpUnselected.Name = "grpUnselected";
            this.grpUnselected.Size = new System.Drawing.Size(230, 446);
            this.grpUnselected.TabIndex = 6;
            this.grpUnselected.TabStop = false;
            this.grpUnselected.Text = "Available";
            // 
            // lstUnselected
            // 
            this.lstUnselected.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lstUnselected.FormattingEnabled = true;
            this.lstUnselected.ItemHeight = 16;
            this.lstUnselected.Location = new System.Drawing.Point(25, 25);
            this.lstUnselected.Name = "lstUnselected";
            this.lstUnselected.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstUnselected.Size = new System.Drawing.Size(180, 388);
            this.lstUnselected.TabIndex = 2;
            // 
            // btnDel
            // 
            this.btnDel.Location = new System.Drawing.Point(258, 276);
            this.btnDel.Name = "btnDel";
            this.btnDel.Size = new System.Drawing.Size(75, 23);
            this.btnDel.TabIndex = 4;
            this.btnDel.Text = "Delete";
            this.btnDel.UseVisualStyleBackColor = true;
            this.btnDel.Click += new System.EventHandler(this.btnDel_Click);
            // 
            // lblName
            // 
            this.lblName.Location = new System.Drawing.Point(7, 39);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(86, 23);
            this.lblName.TabIndex = 0;
            this.lblName.Text = "Name";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // RxGroupListForm
            // 
            this.ClientSize = new System.Drawing.Size(693, 567);
            this.Controls.Add(this.pnlRxGrpList);
            this.Font = new System.Drawing.Font("Arial", 10F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RxGroupListForm";
            this.Text = "Rx Group List";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RxGroupListForm_FormClosing);
            this.Load += new System.EventHandler(this.RxGroupListForm_Load);
            this.pnlRxGrpList.ResumeLayout(false);
            this.pnlRxGrpList.PerformLayout();
            this.tsrGrpList.ResumeLayout(false);
            this.tsrGrpList.PerformLayout();
            this.grpSelected.ResumeLayout(false);
            this.grpUnselected.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        public void SaveData()
        {
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            try
            {
                num3 = Convert.ToInt32(base.Tag);
                if (num3 == -1)
                {
                    return;
                }
                if (this.txtName.Focused)
                {
                    this.txtName_Leave(this.txtName, null);
                }
                RxListOneData value = new RxListOneData(num3);
                value.Name = this.txtName.Text;
                num2 = this.lstSelected.Items.Count;
                ushort[] array = new ushort[num2];
                RxGroupListForm.data.SetIndex(num3, num2 + 1);
                foreach (SelectedItemUtils item in this.lstSelected.Items)
                {
                    array[num++] = (ushort)item.Value;
                }
                value.ContactList = array;
                RxGroupListForm.data[num3] = value;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void DispData()
        {
            int num = 0;
            int num2 = 0;
            ushort num3 = 0;
            string text = "";
            int num4 = 0;
            int num5 = 0;
            try
            {
                num2 = Convert.ToInt32(base.Tag);
                if (num2 == -1)
                {
                    this.Close();
                    return;
                }
                this.txtName.Text = RxGroupListForm.data[num2].Name;
                this.lstSelected.Items.Clear();
                num4 = RxGroupListForm.data.GetContactCntByIndex(num2);
                for (num = 0; num < num4; num++)
                {
                    num3 = RxGroupListForm.data[num2].ContactList[num];
                    if (ContactForm.data.DataIsValid(num3 - 1))
                    {
                        if (ContactForm.data.IsGroupCall(num3 - 1) || ContactForm.data.IsPrivateCall(num3 - 1))
                        {
                            text = ContactForm.data[num3 - 1].Name;
                            this.lstSelected.Items.Add(new SelectedItemUtils(num, num3, text));
                        }
                        num5++;
                    }
                }
                if (num4 != num5 && num5 > 0)
                {
                    RxGroupListForm.data.SetIndex(num2, num5 + 1);
                }
                if (this.lstSelected.Items.Count > 0)
                {
                    this.lstSelected.SelectedIndex = 0;
                }
                this.lstUnselected.Items.Clear();
                for (num = 0; num < 1024; num++)
                {
                    if (!RxGroupListForm.data[num2].ContactList.Contains((ushort)(num + 1)) && ContactForm.data.DataIsValid(num) &&
                        (ContactForm.data[num].CallType == (int)CallTypeE.GroupCall || ContactForm.data[num].CallType == (int)CallTypeE.PrivateCall)
                        )
                    {
                        this.lstUnselected.Items.Add(new SelectedItemUtils(-1, num + 1, ContactForm.data[num].Name));
                    }
                }
                if (this.lstUnselected.Items.Count > 0)
                {
                    this.lstUnselected.SelectedIndex = 0;
                }
                this.method_4();
                this.method_6();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void RefreshName()
        {
            int index = Convert.ToInt32(base.Tag);
            this.txtName.Text = RxGroupListForm.data[index].Name;
        }

        private ComponentResourceManager componentResourceManager;
        public RxGroupListForm()
        {
            componentResourceManager = new ComponentResourceManager(typeof(RxGroupListForm));
            this.InitializeComponent();
            this.tsbtnDel.Image = (Image)componentResourceManager.GetObject("tsbtnDel.Image");
            this.tsbtnFirst.Image = (Image)componentResourceManager.GetObject("tsbtnFirst.Image");
            this.tsbtnLast.Image = (Image)componentResourceManager.GetObject("tsbtnLast.Image");
            this.tsbtnPrev.Image = (Image)componentResourceManager.GetObject("tsbtnPrev.Image");
            this.tsbtnAdd.Image = (Image)componentResourceManager.GetObject("tsbtnAdd.Image");
            this.tsbtnNext.Image = (Image)componentResourceManager.GetObject("tsbtnNext.Image");
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);// Roger Clark. Added correct icon on main form!
            base.Scale(Settings.smethod_6());
        }

        private void method_1()
        {
            this.txtName.MaxByteLength = 15;
            this.txtName.KeyPress += new KeyPressEventHandler(Settings.smethod_54);
        }

        private void RxGroupListForm_Load(object sender, EventArgs e)
        {
            Settings.smethod_59(base.Controls);
            Settings.UpdateComponentTextsFromLanguageXmlData(this);
            this.method_1();
            this.DispData();
        }

        private void RxGroupListForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.SaveData();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            int num = 0;
            int count = this.lstUnselected.SelectedIndices.Count;
            int num2 = this.lstUnselected.SelectedIndices[count - 1];
            int num3 = 0;
            this.lstSelected.SelectedItems.Clear();
            while (this.lstUnselected.SelectedItems.Count > 0 && this.lstSelected.Items.Count < RxListOneData.CNT_CONTACT_PER_RX_LIST)
            {
                num = this.lstSelected.Items.Count;
                SelectedItemUtils @class = (SelectedItemUtils)this.lstUnselected.SelectedItems[0];
                @class.method_1(num);
                num3 = this.lstSelected.Items.Add(@class);
                this.lstSelected.SetSelected(num3, true);
                this.lstUnselected.Items.RemoveAt(this.lstUnselected.SelectedIndices[0]);
            }
            if (this.lstUnselected.SelectedItems.Count == 0)
            {
                int num4 = num2 - count + 1;
                if (num4 >= this.lstUnselected.Items.Count)
                {
                    num4 = this.lstUnselected.Items.Count - 1;
                }
                this.lstUnselected.SelectedIndex = num4;
            }
            this.method_3();
            this.method_4();
            if (!this.btnAdd.Enabled)
            {
                this.lstSelected.Focus();
            }
        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            int num = 0;
            int count = this.lstSelected.SelectedIndices.Count;
            int num2 = this.lstSelected.SelectedIndices[count - 1];
            this.lstUnselected.SelectedItems.Clear();
            while (this.lstSelected.SelectedItems.Count > 0)
            {
                SelectedItemUtils @class = (SelectedItemUtils)this.lstSelected.SelectedItems[0];
                num = this.method_2(@class);
                @class.method_1(-1);
                this.lstUnselected.Items.Insert(num, @class);
                this.lstUnselected.SetSelected(num, true);
                this.lstSelected.Items.RemoveAt(this.lstSelected.SelectedIndices[0]);
            }
            int num3 = num2 - count + 1;
            if (num3 >= this.lstSelected.Items.Count)
            {
                num3 = this.lstSelected.Items.Count - 1;
            }
            this.lstSelected.SelectedIndex = num3;
            this.method_3();
            this.method_4();
        }



        private void btnUp_Click(object sender = null, EventArgs e = null)
        {
            int num = 0;
            int num2 = 0;
            int count = this.lstSelected.SelectedIndices.Count;
            int num3 = this.lstSelected.SelectedIndices[count - 1];
            for (num = 0; num < count; num++)
            {
                num2 = this.lstSelected.SelectedIndices[num];
                if (num != num2)
                {
                    object value = this.lstSelected.Items[num2];
                    this.lstSelected.Items[num2] = this.lstSelected.Items[num2 - 1];
                    this.lstSelected.Items[num2 - 1] = value;
                    this.lstSelected.SetSelected(num2, false);
                    this.lstSelected.SetSelected(num2 - 1, true);
                }
            }
            this.method_3();
        }

        private void btnDown_Click(object sender = null, EventArgs e = null)
        {
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            int count = this.lstSelected.Items.Count;
            int count2 = this.lstSelected.SelectedIndices.Count;
            int num4 = this.lstSelected.SelectedIndices[count2 - 1];
            num = count2 - 1;
            while (num >= 0)
            {
                num3 = this.lstSelected.SelectedIndices[num];
                if (count - 1 - num2 != num3)
                {
                    object value = this.lstSelected.Items[num3];
                    this.lstSelected.Items[num3] = this.lstSelected.Items[num3 + 1];
                    this.lstSelected.Items[num3 + 1] = value;
                    this.lstSelected.SetSelected(num3, false);
                    this.lstSelected.SetSelected(num3 + 1, true);
                }
                num--;
                num2++;
            }
            this.method_3();
        }

        private int method_2(SelectedItemUtils class14_0)
        {
            int num = 0;
            num = 0;
            while (true)
            {
                if (num < this.lstUnselected.Items.Count)
                {
                    SelectedItemUtils @class = (SelectedItemUtils)this.lstUnselected.Items[num];
                    if (class14_0.Value < @class.Value)
                    {
                        break;
                    }
                    num++;
                    continue;
                }
                return num;
            }
            return num;
        }

        private void method_3()
        {
            int num = 0;
            bool flag = false;
            this.lstSelected.BeginUpdate();
            for (num = 0; num < this.lstSelected.Items.Count; num++)
            {
                SelectedItemUtils @class = (SelectedItemUtils)this.lstSelected.Items[num];
                if (@class.method_0() != num)
                {
                    @class.method_1(num);
                    flag = this.lstSelected.GetSelected(num);
                    this.lstSelected.Items[num] = @class;
                    if (flag)
                    {
                        this.lstSelected.SetSelected(num, true);
                    }
                }
            }
            this.lstSelected.EndUpdate();
        }

        private void method_4()
        {
            this.btnAdd.Enabled = (this.lstUnselected.Items.Count > 0 && this.lstSelected.Items.Count < RxListOneData.CNT_CONTACT_PER_RX_LIST);
            this.btnDel.Enabled = (this.lstSelected.Items.Count > 0);
            int count = this.lstSelected.Items.Count;
            int count2 = this.lstSelected.SelectedIndices.Count;
            this.btnUp.Enabled = (this.lstSelected.SelectedItems.Count > 0 && this.lstSelected.Items.Count > 0 && this.lstSelected.SelectedIndices[count2 - 1] != count2 - 1);
            this.btnDown.Enabled = (this.lstSelected.Items.Count > 0 && this.lstSelected.SelectedItems.Count > 0 && this.lstSelected.SelectedIndices[0] != count - count2);
        }

        private void txtName_Leave(object sender, EventArgs e)
        {
            this.txtName.Text = this.txtName.Text.Trim();
            if (this.Node.Text != this.txtName.Text)
            {
                if (Settings.smethod_50(this.Node, this.txtName.Text))
                {
                    this.txtName.Text = this.Node.Text;
                }
                else
                {
                    this.Node.Text = this.txtName.Text;
                }
            }
        }

        private void lstSelected_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.method_4();
        }

        private void lstSelected_DoubleClick(object sender, EventArgs e)
        {
            if (this.lstSelected.SelectedItem != null)
            {
                SelectedItemUtils @class = this.lstSelected.SelectedItem as SelectedItemUtils;
                MainForm mainForm = base.MdiParent as MainForm;
                if (mainForm != null)
                {
                    mainForm.DispChildForm(typeof(ContactForm), @class.Value - 1);
                }
            }
        }

        static RxGroupListForm()
        {

            RxGroupListForm.data = new RxListData();
        }

        private void tsmiFirst_Click(object sender, EventArgs e)
        {
            this.SaveData();
            this.Node = this.Node.Parent.FirstNode;
            TreeNodeItem treeNodeItem = this.Node.Tag as TreeNodeItem;
            base.Tag = treeNodeItem.Index;
            this.DispData();
        }


        private void handlePreviousClick()
        {
            this.SaveData();
            if (this.Node.PrevNode != null)
            {
                this.Node = this.Node.PrevNode;
                TreeNodeItem treeNodeItem = this.Node.Tag as TreeNodeItem;
                base.Tag = treeNodeItem.Index;
                this.DispData();
            }
        }

        private void tsmiPrev_Click(object sender, EventArgs e)
        {
            handlePreviousClick();
        }

        private void handleNextClick()
        {

            this.SaveData();
            if (this.Node.NextNode != null)
            {
                this.Node = this.Node.NextNode;
                TreeNodeItem treeNodeItem = this.Node.Tag as TreeNodeItem;
                base.Tag = treeNodeItem.Index;
                this.DispData();
            }
        }

        private void tsmiNext_Click(object sender, EventArgs e)
        {
            handleNextClick();
        }

        private void tsmiLast_Click(object sender, EventArgs e)
        {
            this.SaveData();
            this.Node = this.Node.Parent.LastNode;
            TreeNodeItem treeNodeItem = this.Node.Tag as TreeNodeItem;
            base.Tag = treeNodeItem.Index;
            this.DispData();
        }

        private void handleInsertClick()
        {

            if (this.Node.Parent.Nodes.Count < RxListData.CNT_RX_LIST)
            {
                this.SaveData();
                TreeNodeItem treeNodeItem = this.Node.Tag as TreeNodeItem;
                int minIndex = RxGroupListForm.data.GetMinIndex();
                string minName = RxGroupListForm.data.GetMinName(this.Node);
                RxGroupListForm.data.SetIndex(minIndex, 1);
                TreeNodeItem tag = new TreeNodeItem(treeNodeItem.Cms, treeNodeItem.Type, null, 0, minIndex, treeNodeItem.ImageIndex, treeNodeItem.Data);
                TreeNode treeNode = new TreeNode(minName);
                treeNode.Tag = tag;
                treeNode.ImageIndex = 19;
                treeNode.SelectedImageIndex = 19;
                this.Node.Parent.Nodes.Insert(minIndex, treeNode);
                RxGroupListForm.data.SetName(minIndex, minName);
                this.Node = treeNode;
                base.Tag = minIndex;
                this.DispData();
                this.method_7();
            }
        }

        private void tsmiAdd_Click(object sender, EventArgs e)
        {
            handleInsertClick();
        }

        private void handleDeleteClick()
        {
            if (this.Node.Parent.Nodes.Count > 1 && this.Node.Index != 0)
            {
                this.SaveData();
                TreeNode node = this.Node.NextNode ?? this.Node.PrevNode;
                TreeNodeItem treeNodeItem = this.Node.Tag as TreeNodeItem;
                RxGroupListForm.data.ClearIndex(treeNodeItem.Index);
                this.Node.Remove();
                this.Node = node;
                TreeNodeItem treeNodeItem2 = this.Node.Tag as TreeNodeItem;
                base.Tag = treeNodeItem2.Index;
                this.DispData();
                this.method_7();
            }
            else
            {
                MessageBox.Show(Settings.dicCommon["FirstNotDelete"]);
            }
        }

        private void tsmiDel_Click(object sender, EventArgs e)
        {
            handleDeleteClick();
        }

        private void method_6()
        {
            this.tsbtnAdd.Enabled = (this.Node.Parent.Nodes.Count != RxListData.CNT_RX_LIST);
            this.tsbtnDel.Enabled = (this.Node.Parent.Nodes.Count != 1 && this.Node.Index != 0);
            this.tsbtnFirst.Enabled = (this.Node != this.Node.Parent.FirstNode);
            this.tsbtnPrev.Enabled = (this.Node != this.Node.Parent.FirstNode);
            this.tsbtnNext.Enabled = (this.Node != this.Node.Parent.LastNode);
            this.tsbtnLast.Enabled = (this.Node != this.Node.Parent.LastNode);
            //this.tslblInfo.Text = string.Format(" {0} / {1}", RxGroupListForm.data.GetDispIndex(Convert.ToInt32(base.Tag)), RxGroupListForm.data.ValidCount);
        }

        private void method_7()
        {
            MainForm mainForm = base.MdiParent as MainForm;
            if (mainForm != null)
            {
                mainForm.RefreshRelatedForm(typeof(RxGroupListForm));
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.Right))
            {
                handleNextClick();
                return true;
            }
            if (keyData == (Keys.Control | Keys.Left))
            {
                handlePreviousClick();
                return true;
            }

            if ((keyData == (Keys.Control | Keys.Insert)) || (keyData == (Keys.Control | Keys.I)))
            {
                handleInsertClick();
                return true;
            }

            if (keyData == (Keys.Control | Keys.Delete))
            {
                handleDeleteClick();
                return true;
            }

            if (keyData == (Keys.Control | Keys.Up))
            {
                btnUp_Click();
                return true;
            }

            if (keyData == (Keys.Control | Keys.Down))
            {
                btnDown_Click();
                return true;
            }



            return base.ProcessCmdKey(ref msg, keyData);
        }

    }
}
