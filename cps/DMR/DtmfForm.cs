using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace DMR
{
    public class DtmfForm : DockContent, IDisp
    {
        private enum DtmfDecResp
        {
            None,
            Reminder,
            Reply,
            ReminderAndReply
        }

        private enum DtmfDelimiter
        {
            A = 10,
            B,
            C,
            D,
            E,
            F
        }

        private enum DtmfGroupCode
        {
            None = 9,
            A,
            B,
            C,
            D,
            E,
            F
        }

        private enum DtmfKillType
        {
            DisableTx,
            DisableRxTx,
            Kill
        }

        private enum DtmfKillWakeDec
        {
            Off,
            On
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class Dtmf : IVerify<Dtmf>
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            private byte[] selfId;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            private byte[] killCode;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            private byte[] wakeCode;

            private byte delimiter;

            private byte groupCode;

            private byte decodeResp;

            private byte autoResetTimer;

            private byte flag1;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            private byte[] reserve1;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
            private byte[] pttidUpCode;

            private ushort reserve2;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
            private byte[] pttidDownCode;

            private ushort reserve3;

            private byte respHoldTime;

            private byte decTime;

            private byte fstDigitDly;

            private byte fstDur;

            private byte otherDur;

            private byte rate;

            private byte tail; // Was flag2

            private byte reserve4;

            public string SelfId
            {
                get
                {
                    int i = 0;
                    int num = 0;
                    StringBuilder stringBuilder = new StringBuilder(8);
                    for (; i < 8; i++)
                    {
                        num = this.selfId[i];
                        if (num >= "0123456789ABCD*#\b".Length)
                        {
                            break;
                        }
                        stringBuilder.Append("0123456789ABCD*#\b"[num]);
                    }
                    return stringBuilder.ToString();
                }
                set
                {
                    for (int i = 0; i < this.selfId.Length; i++)
                    {
                        this.selfId[i] = 255;
                    }
                    for (int i = 0; i < value.Length; i++)
                    {
                        int num = "0123456789ABCD*#\b".IndexOf(value[i]);
                        if (num < 0)
                        {
                            break;
                        }
                        this.selfId[i] = Convert.ToByte(num);
                    }
                }
            }

            public string KillCode
            {
                get
                {
                    int i = 0;
                    int num = 0;
                    StringBuilder stringBuilder = new StringBuilder(16);
                    for (; i < 16; i++)
                    {
                        num = this.killCode[i];
                        if (num >= "0123456789ABCD*#\b".Length)
                        {
                            break;
                        }
                        stringBuilder.Append("0123456789ABCD*#\b"[num]);
                    }
                    return stringBuilder.ToString();
                }
                set
                {
                    for (int i = 0; i < this.killCode.Length; i++)
                    {
                        this.killCode[i] = 255;
                    }
                    for (int i = 0; i < value.Length; i++)
                    {
                        int num = "0123456789ABCD*#\b".IndexOf(value[i]);
                        if (num < 0)
                        {
                            break;
                        }
                        this.killCode[i] = Convert.ToByte(num);
                    }
                }
            }

            public string WakeCode
            {
                get
                {
                    int i = 0;
                    int num = 0;
                    StringBuilder stringBuilder = new StringBuilder(16);
                    for (; i < 16; i++)
                    {
                        num = this.wakeCode[i];
                        if (num >= "0123456789ABCD*#\b".Length)
                        {
                            break;
                        }
                        stringBuilder.Append("0123456789ABCD*#\b"[num]);
                    }
                    return stringBuilder.ToString();
                }
                set
                {
                    for (int i = 0; i < this.wakeCode.Length; i++)
                    {
                        this.wakeCode[i] = 255;
                    }
                    for (int i = 0; i < value.Length; i++)
                    {
                        int num = "0123456789ABCD*#\b".IndexOf(value[i]);
                        if (num < 0)
                        {
                            break;
                        }
                        this.wakeCode[i] = Convert.ToByte(num);
                    }
                }
            }

            public int Delimiter
            {
                get
                {
                    int num = this.delimiter;
                    if (Enum.IsDefined(typeof(DtmfDelimiter), num))
                    {
                        return num;
                    }
                    return 13;
                }
                set
                {
                    if (Enum.IsDefined(typeof(DtmfDelimiter), value))
                    {
                        this.delimiter = (byte)value;
                    }
                    else
                    {
                        this.delimiter = 13;
                    }
                }
            }

            public int GroupCode
            {
                get
                {
                    int num = this.groupCode;
                    if (Enum.IsDefined(typeof(DtmfGroupCode), num))
                    {
                        return num;
                    }
                    return 14;
                }
                set
                {
                    if (Enum.IsDefined(typeof(DtmfGroupCode), value))
                    {
                        this.groupCode = (byte)value;
                    }
                    else
                    {
                        this.groupCode = 14;
                    }
                }
            }

            public int DecodeResp
            {
                get
                {
                    int num = this.decodeResp;
                    if (Enum.IsDefined(typeof(DtmfDecResp), num))
                    {
                        return num;
                    }
                    return Convert.ToInt32(DtmfDecResp.ReminderAndReply);
                }
                set
                {
                    if (Enum.IsDefined(typeof(DtmfDecResp), value))
                    {
                        this.decodeResp = Convert.ToByte(value);
                    }
                    else
                    {
                        this.decodeResp = Convert.ToByte(DtmfDecResp.ReminderAndReply);
                    }
                }
            }

            public decimal AutoResetTimer
            {
                get
                {
                    if (this.autoResetTimer >= 5 && this.autoResetTimer <= 60)
                    {
                        return this.autoResetTimer;
                    }
                    return 10m;
                }
                set
                {
                    byte b = Convert.ToByte(value);
                    if (b >= 5 && b <= 60)
                    {
                        this.autoResetTimer = b;
                    }
                    else
                    {
                        this.autoResetTimer = 10;
                    }
                }
            }

            public int KillWakeDec
            {
                get
                {
                    return (this.flag1 & 0x80) >> 7;
                }
                set
                {
                    value = (value << 7 & 0x80);
                    this.flag1 &= 127;
                    this.flag1 |= (byte)value;
                }
            }

            public int KillType
            {
                get
                {
                    return (this.flag1 & 0x60) >> 5;
                }
                set
                {
                    value = (value << 5 & 0x60);
                    this.flag1 &= 159;
                    this.flag1 |= (byte)value;
                }
            }

            public string PttidUpCode
            {
                get
                {
                    int i = 0;
                    int num = 0;
                    StringBuilder stringBuilder = new StringBuilder(30);
                    for (; i < 30; i++)
                    {
                        num = this.pttidUpCode[i];
                        if (num >= "0123456789ABCD*#\b".Length)
                        {
                            break;
                        }
                        stringBuilder.Append("0123456789ABCD*#\b"[num]);
                    }
                    return stringBuilder.ToString();
                }
                set
                {
                    int num = 0;
                    this.pttidUpCode.Fill((byte)255);
                    for (num = 0; num < value.Length; num++)
                    {
                        int num2 = "0123456789ABCD*#\b".IndexOf(value[num]);
                        if (num2 < 0)
                        {
                            break;
                        }
                        this.pttidUpCode[num] = Convert.ToByte(num2);
                    }
                }
            }

            public string PttidDownCode
            {
                get
                {
                    int i = 0;
                    int num = 0;
                    StringBuilder stringBuilder = new StringBuilder(30);
                    for (; i < 30; i++)
                    {
                        num = this.pttidDownCode[i];
                        if (num >= "0123456789ABCD*#\b".Length)
                        {
                            break;
                        }
                        stringBuilder.Append("0123456789ABCD*#\b"[num]);
                    }
                    return stringBuilder.ToString();
                }
                set
                {
                    for (int i = 0; i < this.pttidDownCode.Length; i++)
                    {
                        this.pttidDownCode[i] = 255;
                    }
                    for (int i = 0; i < value.Length; i++)
                    {
                        int num = "0123456789ABCD*#\b".IndexOf(value[i]);
                        if (num < 0)
                        {
                            break;
                        }
                        this.pttidDownCode[i] = Convert.ToByte(num);
                    }
                }
            }

            public decimal RespHoldTime
            {
                get
                {
                    if (this.respHoldTime >= 5 && this.respHoldTime <= 100)
                    {
                        return (decimal)this.respHoldTime * 0.1m;
                    }
                    return 5.0m;
                }
                set
                {
                    byte b = Convert.ToByte(value / 0.1m);
                    if (b >= 5 && b <= 100)
                    {
                        this.respHoldTime = b;
                    }
                    else
                    {
                        this.respHoldTime = b;
                    }
                }
            }

            public decimal DecTime
            {
                get
                {
                    if (this.decTime >= 5 && this.decTime <= 50)
                    {
                        return (decimal)this.decTime * 0.1m;
                    }
                    return 2.5m;
                }
                set
                {
                    byte b = Convert.ToByte(value / 0.1m);
                    if (b >= 5 && b <= 50)
                    {
                        this.decTime = b;
                    }
                    else
                    {
                        this.decTime = 25;
                    }
                }
            }

            public decimal FstDigitDly
            {
                get
                {
                    if (this.fstDigitDly >= 1 && this.fstDigitDly <= 10)
                    {
                        return this.fstDigitDly * 100;
                    }
                    return 100m;
                }
                set
                {
                    byte b = Convert.ToByte(value / 100m);
                    if (b >= 1 && b <= 10)
                    {
                        this.fstDigitDly = b;
                    }
                    else
                    {
                        this.fstDigitDly = 1;
                    }
                }
            }

            public decimal FstDur
            {
                get
                {
                    if (this.fstDur >= 0 && this.fstDur <= 10)
                    {
                        return this.fstDur * 100;
                    }
                    return 100m;
                }
                set
                {
                    byte b = Convert.ToByte(value / 100m);
                    if (b >= 0 && b <= 10)
                    {
                        this.fstDur = b;
                    }
                    else
                    {
                        this.fstDur = 1;
                    }
                }
            }

            public decimal OtherDur
            {
                get
                {
                    if (this.otherDur >= 0 && this.otherDur <= 10)
                    {
                        return this.otherDur * 100;
                    }
                    return 100m;
                }
                set
                {
                    byte b = Convert.ToByte(value / 100m);
                    if (b >= 0 && b <= 10)
                    {
                        this.otherDur = b;
                    }
                    else
                    {
                        this.otherDur = 1;
                    }
                }
            }

            public decimal Tail
            {
                get
                {
                    if (this.tail >= 0 && this.tail <= 10)
                    {
                        return this.tail * 100;
                    }
                    return 100m;
                }
                set
                {
                    byte b = Convert.ToByte(value / 100m);
                    if (b >= 0 && b <= 10)
                    {
                        this.tail = b;
                    }
                    else
                    {
                        this.tail = 5;
                    }
                }
            }


            public decimal Rate
            {
                get
                {
                    if (this.rate >= 1 && this.rate <= 10)
                    {
                        return this.rate;
                    }
                    return 5m;
                }
                set
                {
                    byte b = Convert.ToByte(value);
                    if (b >= 1 && b <= 10)
                    {
                        this.rate = b;
                    }
                    else
                    {
                        this.rate = 5;
                    }
                }
            }

            public Dtmf()
            {

                //base._002Ector();
                this.selfId = new byte[8];
                this.killCode = new byte[16];
                this.wakeCode = new byte[16];
                this.reserve1 = new byte[3];
                this.pttidUpCode = new byte[30];
                this.pttidDownCode = new byte[30];
            }

            public void Verify(Dtmf def)
            {
                if (!Enum.IsDefined(typeof(DtmfDelimiter), (int)this.delimiter))
                {
                    this.delimiter = def.delimiter;
                }
                if (!Enum.IsDefined(typeof(DtmfGroupCode), (int)this.groupCode))
                {
                    this.groupCode = def.groupCode;
                }
                if (!Enum.IsDefined(typeof(DtmfDecResp), (int)this.decodeResp))
                {
                    this.decodeResp = def.decodeResp;
                }
                Settings.smethod_11(ref this.autoResetTimer, (byte)5, (byte)60, def.autoResetTimer);
                if (!Enum.IsDefined(typeof(DtmfKillType), this.KillType))
                {
                    this.KillType = def.KillType;
                }
                Settings.smethod_11(ref this.respHoldTime, (byte)5, (byte)100, def.respHoldTime);
                Settings.smethod_11(ref this.decTime, (byte)5, (byte)50, def.decTime);
                Settings.smethod_11(ref this.fstDigitDly, (byte)1, (byte)10, def.fstDigitDly);
                Settings.smethod_11(ref this.fstDur, (byte)0, (byte)10, def.fstDur);
                Settings.smethod_11(ref this.otherDur, (byte)0, (byte)10, def.otherDur);
                Settings.smethod_11(ref this.tail, (byte)0, (byte)250, def.tail);
                Settings.smethod_11(ref this.rate, (byte)1, (byte)10, def.rate);
            }
        }

        private const string SZ_SELF_ID = "0123456789\b";

        private const string SZ_DTMF_CODE = "0123456789ABCD*#\b";

        public const string SZ_DECODE_RESP_NAME = "DecodeResp";

        private const string SZ_DELIMITER = "ABCD*#";

        public const string SZ_GROUP_CODE_NAME = "GroupCode";

        public const string SZ_KILL_TYPE_NAME = "KillType";

        public const string SZ_KILL_WAKE_DEC_NAME = "KillWakeDec";

        private const int LEN_SELF_ID = 8;

        private const int LEN_KILL_CODE = 16;

        private const int LEN_WAKE_CODE = 16;

        private const int MIN_AUTO_RESET_TIMER = 5;

        private const int MAX_AUTO_RESET_TIMER = 60;

        private const int INC_AUTO_RESET_TIMER = 1;

        private const int SCL_AUTO_RESET_TIMER = 1;

        private const int LEN_AUTO_RESET_TIMER = 2;

        private const int DEF_AUTO_RESET_TIMER = 10;

        private const int LEN_PTTID_UP_CODE = 30;

        private const int LEN_PTTID_DOWN_CODE = 30;

        private const int MIN_RESP_HOLD_TIME = 5;

        private const int MAX_RESP_HOLD_TIME = 100;

        private const int INC_RESP_HOLD_TIME = 1;

        /* Roger Clark Removed unused cnstants to prevent warnings
         private const decimal SCL_RESP_HOLD_TIME = 0.1m;
        */
        private const int DEF_RESP_HOLD_TIME = 50;

        private const int LEN_RESP_HOLD_TIME = 4;

        private const int MIN_DECODE_TIME = 5;

        private const int MAX_DECODE_TIME = 50;

        private const int INC_DECODE_TIME = 1;
        /* Roger Clark Removed unused cnstants to prevent warnings
		private const decimal SCL_DECODE_TIME = 0.1m;
        */
        private const int DEF_DECODE_TIME = 25;

        private const int LEN_DECODE_TIME = 3;

        private const int MIN_FST_DIGIT_DLY = 1;

        private const int MAX_FST_DIGIT_DLY = 10;

        private const int INC_FST_DIGIT_DLY = 1;

        private const int SCL_FST_DIGIT_DLY = 100;

        private const int DEF_FST_DIGIT_DLY = 1;

        private const int LEN_FST_DIGIT_DLY = 4;

        private const int MIN_FST_DUR = 0;

        private const int MAX_FST_DUR = 10;

        private const int INC_FST_DUR = 1;

        private const int SCL_FST_DUR = 100;

        private const int DEF_FST_DUR = 1;

        private const int LEN_FST_DUR = 4;

        private const int MIN_OTHER_DUR = 0;

        private const int MAX_OTHER_DUR = 10;

        private const int INC_OTHER_DUR = 1;

        private const int SCL_OTHER_DUR = 100;

        private const int DEF_OTHER_DUR = 1;

        private const int LEN_OTHER_DUR = 4;

        private static readonly string[] SZ_DECODE_RESP;

        private static readonly string[] SZ_GROUP_CODE;

        private static readonly string[] SZ_KILL_TYPE;

        private static readonly string[] SZ_KILL_WAKE_DEC;

        public static Dtmf DefaultDtmf;

        public static Dtmf data;

        //private IContainer components;

        private Label lblUpCode;

        private Label lblDownCode;

        private Label lblRespHoldTime;

        private Label lblDecTime;

        private Label lblFstDlyTime;

        private Label lblFstDur;

        private Label lblOtherDur;

        private Label lblTail;

        private Label lblRate;

        private SGTextBox txtUpCode;

        private SGTextBox txtDownCode;

        private CustomNumericUpDown nudRespHoldTime;

        private CustomNumericUpDown nudDecTime;

        private CustomNumericUpDown nudFstDlyTime;

        private CustomNumericUpDown nudFstDur;

        private CustomNumericUpDown nudOtherDur;

        private CustomNumericUpDown nudRate;

        private CustomNumericUpDown nudTailDur;

        private Label label_0;

        private SGTextBox sgtextBox_0;

        private Label lblKillCode;

        private SGTextBox txtKillCode;

        private Label lblWakeCode;

        private SGTextBox txtWakeCode;

        private Label lblDelimiter;

        private Label lblGrpCallCode;

        private Label lblAutoResetTime;

        private CustomNumericUpDown nudAutoResetTimer;

        private Label lblDecResp;

        private CustomCombo cmbDecResp;

        private Label lblKillWakeDec;

        private CustomCombo cmbKillWakeDec;

        private Label lblKillType;

        private CustomCombo cmbKillType;

        private CustomCombo cmbDelimiter;

        private CustomCombo cmbGrpCallCode;

        private CustomPanel pnlDtmf;

        public TreeNode Node
        {
            get;
            set;
        }

        public void SaveData()
        {
            try
            {
                DtmfForm.data.SelfId = this.sgtextBox_0.Text;
                DtmfForm.data.KillCode = this.txtKillCode.Text;
                DtmfForm.data.WakeCode = this.txtWakeCode.Text;
                DtmfForm.data.Delimiter = this.cmbDelimiter.method_3();
                DtmfForm.data.GroupCode = this.cmbGrpCallCode.method_3();
                DtmfForm.data.DecodeResp = this.cmbDecResp.method_3();
                DtmfForm.data.AutoResetTimer = this.nudAutoResetTimer.Value;
                DtmfForm.data.KillWakeDec = this.cmbKillWakeDec.method_3();
                DtmfForm.data.KillType = this.cmbKillType.method_3();
                DtmfForm.data.PttidUpCode = this.txtUpCode.Text;
                DtmfForm.data.PttidDownCode = this.txtDownCode.Text;
                DtmfForm.data.RespHoldTime = this.nudRespHoldTime.Value;
                DtmfForm.data.DecTime = this.nudDecTime.Value;
                DtmfForm.data.FstDigitDly = this.nudFstDlyTime.Value;
                DtmfForm.data.FstDur = this.nudFstDur.Value;
                DtmfForm.data.OtherDur = this.nudOtherDur.Value;
                DtmfForm.data.Tail = this.nudTailDur.Value;
                DtmfForm.data.Rate = this.nudRate.Value;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void DispData()
        {
            try
            {
                this.method_0();
                this.sgtextBox_0.Text = DtmfForm.data.SelfId;
                this.txtKillCode.Text = DtmfForm.data.KillCode;
                this.txtWakeCode.Text = DtmfForm.data.WakeCode;
                this.cmbDelimiter.method_2(DtmfForm.data.Delimiter);
                this.cmbGrpCallCode.method_2(DtmfForm.data.GroupCode);
                this.cmbDecResp.method_2(DtmfForm.data.DecodeResp);
                this.nudAutoResetTimer.Value = DtmfForm.data.AutoResetTimer;
                this.cmbKillWakeDec.method_2(DtmfForm.data.KillWakeDec);
                this.cmbKillType.method_2(DtmfForm.data.KillType);
                this.txtUpCode.Text = DtmfForm.data.PttidUpCode;
                this.txtDownCode.Text = DtmfForm.data.PttidDownCode;
                this.nudRespHoldTime.Value = DtmfForm.data.RespHoldTime;
                this.nudDecTime.Value = DtmfForm.data.DecTime;
                this.nudFstDlyTime.Value = DtmfForm.data.FstDigitDly;
                this.nudFstDur.Value = DtmfForm.data.FstDur;
                this.nudOtherDur.Value = DtmfForm.data.OtherDur;
                this.nudTailDur.Value = DtmfForm.data.Tail;
                this.nudRate.Value = DtmfForm.data.Rate;
                this.RefreshByUserMode();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void RefreshByUserMode()
        {
            bool flag = Settings.getUserExpertSettings() == Settings.UserMode.Expert;
            this.lblKillCode.Enabled &= flag;
            this.txtKillCode.Enabled &= flag;
            this.lblWakeCode.Enabled &= flag;
            this.txtWakeCode.Enabled &= flag;
            this.lblDelimiter.Enabled &= flag;
            this.cmbDelimiter.Enabled &= flag;
            this.lblKillWakeDec.Enabled &= flag;
            this.cmbKillWakeDec.Enabled &= flag;
            this.lblKillType.Enabled &= flag;
            this.cmbKillType.Enabled &= flag;
            this.lblFstDlyTime.Enabled &= flag;
            this.nudFstDlyTime.Enabled &= flag;
            this.lblFstDur.Enabled &= flag;
            this.nudFstDur.Enabled &= flag;
            this.lblOtherDur.Enabled &= flag;
            this.nudOtherDur.Enabled &= flag;
            this.nudRate.Enabled &= flag;
            this.nudTailDur.Enabled &= flag;
        }

        public void RefreshName()
        {
        }

        public DtmfForm()
        {

            //base._002Ector();
            this.InitializeComponent();
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);// Roger Clark. Added correct icon on main form!
            base.Scale(Settings.smethod_6());
        }

        private void method_0()
        {
            int num = 0;
            this.sgtextBox_0.MaxLength = 8;
            this.sgtextBox_0.InputString = "0123456789\b";
            this.txtKillCode.MaxLength = 16;
            this.txtKillCode.InputString = "0123456789ABCD*#\b";
            this.txtWakeCode.MaxLength = 16;
            this.txtWakeCode.InputString = "0123456789ABCD*#\b";
            num = 0;
            this.cmbDelimiter.Items.Clear();
            foreach (int value in Enum.GetValues(typeof(DtmfDelimiter)))
            {
                this.cmbDelimiter.method_1("ABCD*#".Substring(num++, 1), value);
            }
            num = 0;
            this.cmbGrpCallCode.Items.Clear();
            foreach (int value2 in Enum.GetValues(typeof(DtmfGroupCode)))
            {
                this.cmbGrpCallCode.method_1(DtmfForm.SZ_GROUP_CODE[num++], value2);
            }
            num = 0;
            this.cmbDecResp.Items.Clear();
            foreach (int value3 in Enum.GetValues(typeof(DtmfDecResp)))
            {
                this.cmbDecResp.method_1(DtmfForm.SZ_DECODE_RESP[num++], value3);
            }
            Settings.smethod_36(this.nudAutoResetTimer, new Class13(5, 60, 1, 1m, 2));
            num = 0;
            this.cmbKillWakeDec.Items.Clear();
            foreach (int value4 in Enum.GetValues(typeof(DtmfKillWakeDec)))
            {
                this.cmbKillWakeDec.method_1(DtmfForm.SZ_KILL_WAKE_DEC[num++], value4);
            }
            num = 0;
            this.cmbKillType.Items.Clear();
            foreach (int value5 in Enum.GetValues(typeof(DtmfKillType)))
            {
                this.cmbKillType.method_1(DtmfForm.SZ_KILL_TYPE[num++], value5);
            }
            this.txtUpCode.MaxLength = 30;
            this.txtUpCode.InputString = "0123456789ABCD*#\b";
            this.txtDownCode.MaxLength = 30;
            this.txtDownCode.InputString = "0123456789ABCD*#\b";
            Settings.smethod_36(this.nudRespHoldTime, new Class13(5, 100, 1, 0.1m, 4));
            Settings.smethod_36(this.nudDecTime, new Class13(5, 50, 1, 0.1m, 3));
            Settings.smethod_36(this.nudFstDlyTime, new Class13(1, 10, 1, 100m, 4));
            Settings.smethod_36(this.nudFstDur, new Class13(0, 10, 1, 100m, 4));
            Settings.smethod_36(this.nudOtherDur, new Class13(0, 10, 1, 100m, 4));
            Settings.smethod_36(this.nudTailDur, new Class13(0, 10, 1, 100m, 4));
            Settings.smethod_36(this.nudRate, new Class13(1, 10, 1, 1m, 2));
        }

        public static void RefreshCommonLang()
        {
            string name = typeof(DtmfForm).Name;
            Settings.smethod_78("DecodeResp", DtmfForm.SZ_DECODE_RESP, name);
            Settings.smethod_78("GroupCode", DtmfForm.SZ_GROUP_CODE, name);
            Settings.smethod_78("KillType", DtmfForm.SZ_KILL_TYPE, name);
            Settings.smethod_78("KillWakeDec", DtmfForm.SZ_KILL_WAKE_DEC, name);
        }

        private void DtmfForm_Load(object sender, EventArgs e)
        {
            Settings.smethod_59(base.Controls);
            Settings.UpdateComponentTextsFromLanguageXmlData(this);
            this.DispData();
        }

        private void DtmfForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.SaveData();
        }

        private void sgtextBox_0_Validating(object sender, CancelEventArgs e)
        {
            if (this.sgtextBox_0.Text.Length < 3)
            {
                this.sgtextBox_0.Text = this.sgtextBox_0.Text.PadLeft(3, '0');
            }
        }

        private void txtKillCode_Validating(object sender, CancelEventArgs e)
        {
            if (this.txtKillCode.Text.Length < 3)
            {
                this.txtKillCode.Text = this.txtKillCode.Text.PadLeft(3, '0');
            }
        }

        private void txtWakeCode_Validating(object sender, CancelEventArgs e)
        {
            if (this.txtWakeCode.Text.Length < 3)
            {
                this.txtWakeCode.Text = this.txtWakeCode.Text.PadLeft(3, '0');
            }
        }

        protected override void Dispose(bool disposing)
        {
            /*
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
             * */
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlDtmf = new CustomPanel();
            this.lblUpCode = new System.Windows.Forms.Label();
            this.cmbKillType = new CustomCombo();
            this.lblDownCode = new System.Windows.Forms.Label();
            this.cmbKillWakeDec = new CustomCombo();
            this.label_0 = new System.Windows.Forms.Label();
            this.cmbGrpCallCode = new CustomCombo();
            this.lblRespHoldTime = new System.Windows.Forms.Label();
            this.cmbDelimiter = new CustomCombo();
            this.lblKillCode = new System.Windows.Forms.Label();
            this.cmbDecResp = new CustomCombo();
            this.lblAutoResetTime = new System.Windows.Forms.Label();
            this.nudOtherDur = new CustomNumericUpDown();
            this.nudTailDur = new CustomNumericUpDown();
            this.nudRate = new CustomNumericUpDown();
            this.lblDecTime = new System.Windows.Forms.Label();
            this.nudFstDur = new CustomNumericUpDown();
            this.lblWakeCode = new System.Windows.Forms.Label();
            this.nudFstDlyTime = new CustomNumericUpDown();
            this.lblDelimiter = new System.Windows.Forms.Label();
            this.nudDecTime = new CustomNumericUpDown();
            this.lblGrpCallCode = new System.Windows.Forms.Label();
            this.nudAutoResetTimer = new CustomNumericUpDown();
            this.lblDecResp = new System.Windows.Forms.Label();
            this.nudRespHoldTime = new CustomNumericUpDown();
            this.lblFstDlyTime = new System.Windows.Forms.Label();
            this.txtDownCode = new DMR.SGTextBox();
            this.lblKillWakeDec = new System.Windows.Forms.Label();
            this.txtWakeCode = new DMR.SGTextBox();
            this.lblFstDur = new System.Windows.Forms.Label();
            this.txtKillCode = new DMR.SGTextBox();
            this.lblKillType = new System.Windows.Forms.Label();
            this.sgtextBox_0 = new DMR.SGTextBox();
            this.lblOtherDur = new System.Windows.Forms.Label();
            this.txtUpCode = new DMR.SGTextBox();
            this.lblTail = new System.Windows.Forms.Label();
            this.lblRate = new System.Windows.Forms.Label();
            this.pnlDtmf.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudOtherDur)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTailDur)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFstDur)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFstDlyTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDecTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudAutoResetTimer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRespHoldTime)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlDtmf
            // 
            this.pnlDtmf.AutoScroll = true;
            this.pnlDtmf.AutoSize = true;
            this.pnlDtmf.Controls.Add(this.lblUpCode);
            this.pnlDtmf.Controls.Add(this.cmbKillType);
            this.pnlDtmf.Controls.Add(this.lblDownCode);
            this.pnlDtmf.Controls.Add(this.cmbKillWakeDec);
            this.pnlDtmf.Controls.Add(this.label_0);
            this.pnlDtmf.Controls.Add(this.cmbGrpCallCode);
            this.pnlDtmf.Controls.Add(this.lblRespHoldTime);
            this.pnlDtmf.Controls.Add(this.cmbDelimiter);
            this.pnlDtmf.Controls.Add(this.lblKillCode);
            this.pnlDtmf.Controls.Add(this.cmbDecResp);
            this.pnlDtmf.Controls.Add(this.lblAutoResetTime);
            this.pnlDtmf.Controls.Add(this.nudOtherDur);
            this.pnlDtmf.Controls.Add(this.nudTailDur);
            this.pnlDtmf.Controls.Add(this.nudRate);
            this.pnlDtmf.Controls.Add(this.lblDecTime);
            this.pnlDtmf.Controls.Add(this.nudFstDur);
            this.pnlDtmf.Controls.Add(this.lblWakeCode);
            this.pnlDtmf.Controls.Add(this.nudFstDlyTime);
            this.pnlDtmf.Controls.Add(this.lblDelimiter);
            this.pnlDtmf.Controls.Add(this.nudDecTime);
            this.pnlDtmf.Controls.Add(this.lblGrpCallCode);
            this.pnlDtmf.Controls.Add(this.nudAutoResetTimer);
            this.pnlDtmf.Controls.Add(this.lblDecResp);
            this.pnlDtmf.Controls.Add(this.nudRespHoldTime);
            this.pnlDtmf.Controls.Add(this.lblFstDlyTime);
            this.pnlDtmf.Controls.Add(this.txtDownCode);
            this.pnlDtmf.Controls.Add(this.lblKillWakeDec);
            this.pnlDtmf.Controls.Add(this.txtWakeCode);
            this.pnlDtmf.Controls.Add(this.lblFstDur);
            this.pnlDtmf.Controls.Add(this.txtKillCode);
            this.pnlDtmf.Controls.Add(this.lblKillType);
            this.pnlDtmf.Controls.Add(this.sgtextBox_0);
            this.pnlDtmf.Controls.Add(this.lblOtherDur);
            this.pnlDtmf.Controls.Add(this.txtUpCode);
            this.pnlDtmf.Controls.Add(this.lblTail);
            this.pnlDtmf.Controls.Add(this.lblRate);
            this.pnlDtmf.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDtmf.Location = new System.Drawing.Point(0, 0);
            this.pnlDtmf.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlDtmf.Name = "pnlDtmf";
            this.pnlDtmf.Size = new System.Drawing.Size(383, 247);
            this.pnlDtmf.TabIndex = 0;
            // 
            // lblUpCode
            // 
            this.lblUpCode.Location = new System.Drawing.Point(381, 52);
            this.lblUpCode.Name = "lblUpCode";
            this.lblUpCode.Size = new System.Drawing.Size(156, 24);
            this.lblUpCode.TabIndex = 21;
            this.lblUpCode.Text = "PTTID Up Code";
            this.lblUpCode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblUpCode.Visible = false;
            // 
            // cmbKillType
            // 
            this.cmbKillType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbKillType.FormattingEnabled = true;
            this.cmbKillType.Items.AddRange(new object[] {
            "发射禁止",
            "收发禁止",
            "遥毙"});
            this.cmbKillType.Location = new System.Drawing.Point(547, 454);
            this.cmbKillType.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cmbKillType.Name = "cmbKillType";
            this.cmbKillType.Size = new System.Drawing.Size(139, 24);
            this.cmbKillType.TabIndex = 17;
            this.cmbKillType.Visible = false;
            // 
            // lblDownCode
            // 
            this.lblDownCode.Location = new System.Drawing.Point(381, 93);
            this.lblDownCode.Name = "lblDownCode";
            this.lblDownCode.Size = new System.Drawing.Size(156, 24);
            this.lblDownCode.TabIndex = 23;
            this.lblDownCode.Text = "PTTID Down Code";
            this.lblDownCode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblDownCode.Visible = false;
            // 
            // cmbKillWakeDec
            // 
            this.cmbKillWakeDec.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbKillWakeDec.FormattingEnabled = true;
            this.cmbKillWakeDec.Items.AddRange(new object[] {
            "关",
            "开"});
            this.cmbKillWakeDec.Location = new System.Drawing.Point(547, 413);
            this.cmbKillWakeDec.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cmbKillWakeDec.Name = "cmbKillWakeDec";
            this.cmbKillWakeDec.Size = new System.Drawing.Size(139, 24);
            this.cmbKillWakeDec.TabIndex = 15;
            this.cmbKillWakeDec.Visible = false;
            // 
            // label_0
            // 
            this.label_0.Location = new System.Drawing.Point(402, 134);
            this.label_0.Name = "label_0";
            this.label_0.Size = new System.Drawing.Size(131, 24);
            this.label_0.TabIndex = 0;
            this.label_0.Text = "Self ID";
            this.label_0.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label_0.Visible = false;
            // 
            // cmbGrpCallCode
            // 
            this.cmbGrpCallCode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbGrpCallCode.FormattingEnabled = true;
            this.cmbGrpCallCode.Items.AddRange(new object[] {
            "None",
            "A",
            "B",
            "C",
            "D",
            "*",
            "#"});
            this.cmbGrpCallCode.Location = new System.Drawing.Point(547, 294);
            this.cmbGrpCallCode.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cmbGrpCallCode.Name = "cmbGrpCallCode";
            this.cmbGrpCallCode.Size = new System.Drawing.Size(139, 24);
            this.cmbGrpCallCode.TabIndex = 9;
            this.cmbGrpCallCode.Visible = false;
            // 
            // lblRespHoldTime
            // 
            this.lblRespHoldTime.Location = new System.Drawing.Point(24, 342);
            this.lblRespHoldTime.Name = "lblRespHoldTime";
            this.lblRespHoldTime.Size = new System.Drawing.Size(156, 24);
            this.lblRespHoldTime.TabIndex = 33;
            this.lblRespHoldTime.Text = "Response Hold [s]";
            this.lblRespHoldTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblRespHoldTime.Visible = false;
            // 
            // cmbDelimiter
            // 
            this.cmbDelimiter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDelimiter.FormattingEnabled = true;
            this.cmbDelimiter.Items.AddRange(new object[] {
            "A",
            "B",
            "C",
            "D",
            "*",
            "#"});
            this.cmbDelimiter.Location = new System.Drawing.Point(547, 253);
            this.cmbDelimiter.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cmbDelimiter.Name = "cmbDelimiter";
            this.cmbDelimiter.Size = new System.Drawing.Size(139, 24);
            this.cmbDelimiter.TabIndex = 7;
            this.cmbDelimiter.Visible = false;
            // 
            // lblKillCode
            // 
            this.lblKillCode.Location = new System.Drawing.Point(402, 173);
            this.lblKillCode.Name = "lblKillCode";
            this.lblKillCode.Size = new System.Drawing.Size(131, 24);
            this.lblKillCode.TabIndex = 2;
            this.lblKillCode.Text = "Kill Code";
            this.lblKillCode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblKillCode.Visible = false;
            // 
            // cmbDecResp
            // 
            this.cmbDecResp.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDecResp.FormattingEnabled = true;
            this.cmbDecResp.Items.AddRange(new object[] {
            "None",
            "提醒",
            "回复",
            "提醒并回复"});
            this.cmbDecResp.Location = new System.Drawing.Point(547, 333);
            this.cmbDecResp.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cmbDecResp.Name = "cmbDecResp";
            this.cmbDecResp.Size = new System.Drawing.Size(139, 24);
            this.cmbDecResp.TabIndex = 11;
            this.cmbDecResp.Visible = false;
            // 
            // lblAutoResetTime
            // 
            this.lblAutoResetTime.Location = new System.Drawing.Point(402, 374);
            this.lblAutoResetTime.Name = "lblAutoResetTime";
            this.lblAutoResetTime.Size = new System.Drawing.Size(131, 24);
            this.lblAutoResetTime.TabIndex = 12;
            this.lblAutoResetTime.Text = "Auto Reset Time [s]";
            this.lblAutoResetTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblAutoResetTime.Visible = false;
            // 
            // nudOtherDur
            // 
            this.nudOtherDur.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudOtherDur.Location = new System.Drawing.Point(190, 99);
            this.nudOtherDur.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.nudOtherDur.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudOtherDur.Name = "nudOtherDur";
            this.nudOtherDur.Size = new System.Drawing.Size(140, 23);
            this.nudOtherDur.TabIndex = 30;
            // 
            // nudTailDur
            // 
            this.nudTailDur.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudTailDur.Location = new System.Drawing.Point(190, 140);
            this.nudTailDur.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.nudTailDur.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudTailDur.Name = "nudTailDur";
            this.nudTailDur.Size = new System.Drawing.Size(140, 23);
            this.nudTailDur.TabIndex = 31;
            // 
            // nudRate
            // 
            this.nudRate.Location = new System.Drawing.Point(190, 182);
            this.nudRate.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.nudRate.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudRate.Name = "nudRate";
            this.nudRate.Size = new System.Drawing.Size(140, 23);
            this.nudRate.TabIndex = 32;
            // 
            // lblDecTime
            // 
            this.lblDecTime.Location = new System.Drawing.Point(402, 492);
            this.lblDecTime.Name = "lblDecTime";
            this.lblDecTime.Size = new System.Drawing.Size(131, 24);
            this.lblDecTime.TabIndex = 18;
            this.lblDecTime.Text = "Decode Time [s]";
            this.lblDecTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblDecTime.Visible = false;
            // 
            // nudFstDur
            // 
            this.nudFstDur.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudFstDur.Location = new System.Drawing.Point(190, 60);
            this.nudFstDur.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.nudFstDur.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudFstDur.Name = "nudFstDur";
            this.nudFstDur.Size = new System.Drawing.Size(140, 23);
            this.nudFstDur.TabIndex = 28;
            // 
            // lblWakeCode
            // 
            this.lblWakeCode.Location = new System.Drawing.Point(402, 214);
            this.lblWakeCode.Name = "lblWakeCode";
            this.lblWakeCode.Size = new System.Drawing.Size(131, 24);
            this.lblWakeCode.TabIndex = 4;
            this.lblWakeCode.Text = "Wake Code";
            this.lblWakeCode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblWakeCode.Visible = false;
            // 
            // nudFstDlyTime
            // 
            this.nudFstDlyTime.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudFstDlyTime.Location = new System.Drawing.Point(190, 19);
            this.nudFstDlyTime.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.nudFstDlyTime.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudFstDlyTime.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudFstDlyTime.Name = "nudFstDlyTime";
            this.nudFstDlyTime.Size = new System.Drawing.Size(140, 23);
            this.nudFstDlyTime.TabIndex = 26;
            this.nudFstDlyTime.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // lblDelimiter
            // 
            this.lblDelimiter.Location = new System.Drawing.Point(402, 253);
            this.lblDelimiter.Name = "lblDelimiter";
            this.lblDelimiter.Size = new System.Drawing.Size(131, 24);
            this.lblDelimiter.TabIndex = 6;
            this.lblDelimiter.Text = "Delimiter";
            this.lblDelimiter.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblDelimiter.Visible = false;
            // 
            // nudDecTime
            // 
            this.nudDecTime.DecimalPlaces = 1;
            this.nudDecTime.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nudDecTime.Location = new System.Drawing.Point(547, 492);
            this.nudDecTime.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.nudDecTime.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            65536});
            this.nudDecTime.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.nudDecTime.Name = "nudDecTime";
            this.nudDecTime.Size = new System.Drawing.Size(140, 23);
            this.nudDecTime.TabIndex = 19;
            this.nudDecTime.Value = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.nudDecTime.Visible = false;
            // 
            // lblGrpCallCode
            // 
            this.lblGrpCallCode.Location = new System.Drawing.Point(402, 294);
            this.lblGrpCallCode.Name = "lblGrpCallCode";
            this.lblGrpCallCode.Size = new System.Drawing.Size(131, 24);
            this.lblGrpCallCode.TabIndex = 8;
            this.lblGrpCallCode.Text = "Group Code";
            this.lblGrpCallCode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblGrpCallCode.Visible = false;
            // 
            // nudAutoResetTimer
            // 
            this.nudAutoResetTimer.Location = new System.Drawing.Point(547, 374);
            this.nudAutoResetTimer.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.nudAutoResetTimer.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.nudAutoResetTimer.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.nudAutoResetTimer.Name = "nudAutoResetTimer";
            this.nudAutoResetTimer.Size = new System.Drawing.Size(140, 23);
            this.nudAutoResetTimer.TabIndex = 13;
            this.nudAutoResetTimer.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.nudAutoResetTimer.Visible = false;
            // 
            // lblDecResp
            // 
            this.lblDecResp.Location = new System.Drawing.Point(402, 333);
            this.lblDecResp.Name = "lblDecResp";
            this.lblDecResp.Size = new System.Drawing.Size(131, 24);
            this.lblDecResp.TabIndex = 10;
            this.lblDecResp.Text = "Decode Response";
            this.lblDecResp.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblDecResp.Visible = false;
            // 
            // nudRespHoldTime
            // 
            this.nudRespHoldTime.DecimalPlaces = 1;
            this.nudRespHoldTime.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nudRespHoldTime.Location = new System.Drawing.Point(190, 342);
            this.nudRespHoldTime.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.nudRespHoldTime.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            65536});
            this.nudRespHoldTime.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.nudRespHoldTime.Name = "nudRespHoldTime";
            this.nudRespHoldTime.Size = new System.Drawing.Size(140, 23);
            this.nudRespHoldTime.TabIndex = 34;
            this.nudRespHoldTime.Value = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.nudRespHoldTime.Visible = false;
            // 
            // lblFstDlyTime
            // 
            this.lblFstDlyTime.Location = new System.Drawing.Point(24, 19);
            this.lblFstDlyTime.Name = "lblFstDlyTime";
            this.lblFstDlyTime.Size = new System.Drawing.Size(156, 24);
            this.lblFstDlyTime.TabIndex = 25;
            this.lblFstDlyTime.Text = "First Digit Delay [ms]";
            this.lblFstDlyTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtDownCode
            // 
            this.txtDownCode.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtDownCode.InputString = null;
            this.txtDownCode.Location = new System.Drawing.Point(547, 93);
            this.txtDownCode.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtDownCode.MaxByteLength = 0;
            this.txtDownCode.MaxLength = 30;
            this.txtDownCode.Name = "txtDownCode";
            this.txtDownCode.Size = new System.Drawing.Size(139, 23);
            this.txtDownCode.TabIndex = 24;
            this.txtDownCode.Visible = false;
            // 
            // lblKillWakeDec
            // 
            this.lblKillWakeDec.Location = new System.Drawing.Point(402, 413);
            this.lblKillWakeDec.Name = "lblKillWakeDec";
            this.lblKillWakeDec.Size = new System.Drawing.Size(131, 24);
            this.lblKillWakeDec.TabIndex = 14;
            this.lblKillWakeDec.Text = "Kill/Wake Code";
            this.lblKillWakeDec.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblKillWakeDec.Visible = false;
            // 
            // txtWakeCode
            // 
            this.txtWakeCode.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtWakeCode.InputString = null;
            this.txtWakeCode.Location = new System.Drawing.Point(547, 214);
            this.txtWakeCode.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtWakeCode.MaxByteLength = 0;
            this.txtWakeCode.MaxLength = 16;
            this.txtWakeCode.Name = "txtWakeCode";
            this.txtWakeCode.Size = new System.Drawing.Size(139, 23);
            this.txtWakeCode.TabIndex = 5;
            this.txtWakeCode.Visible = false;
            this.txtWakeCode.Validating += new System.ComponentModel.CancelEventHandler(this.txtWakeCode_Validating);
            // 
            // lblFstDur
            // 
            this.lblFstDur.Location = new System.Drawing.Point(24, 60);
            this.lblFstDur.Name = "lblFstDur";
            this.lblFstDur.Size = new System.Drawing.Size(156, 24);
            this.lblFstDur.TabIndex = 27;
            this.lblFstDur.Text = "First Digit Duration [ms]";
            this.lblFstDur.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtKillCode
            // 
            this.txtKillCode.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtKillCode.InputString = null;
            this.txtKillCode.Location = new System.Drawing.Point(547, 173);
            this.txtKillCode.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtKillCode.MaxByteLength = 0;
            this.txtKillCode.MaxLength = 16;
            this.txtKillCode.Name = "txtKillCode";
            this.txtKillCode.Size = new System.Drawing.Size(139, 23);
            this.txtKillCode.TabIndex = 3;
            this.txtKillCode.Visible = false;
            this.txtKillCode.Validating += new System.ComponentModel.CancelEventHandler(this.txtKillCode_Validating);
            // 
            // lblKillType
            // 
            this.lblKillType.Location = new System.Drawing.Point(402, 454);
            this.lblKillType.Name = "lblKillType";
            this.lblKillType.Size = new System.Drawing.Size(131, 24);
            this.lblKillType.TabIndex = 16;
            this.lblKillType.Text = "Kill Type";
            this.lblKillType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblKillType.Visible = false;
            // 
            // sgtextBox_0
            // 
            this.sgtextBox_0.InputString = null;
            this.sgtextBox_0.Location = new System.Drawing.Point(547, 134);
            this.sgtextBox_0.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.sgtextBox_0.MaxByteLength = 0;
            this.sgtextBox_0.MaxLength = 8;
            this.sgtextBox_0.Name = "sgtextBox_0";
            this.sgtextBox_0.Size = new System.Drawing.Size(139, 23);
            this.sgtextBox_0.TabIndex = 1;
            this.sgtextBox_0.Visible = false;
            this.sgtextBox_0.Validating += new System.ComponentModel.CancelEventHandler(this.sgtextBox_0_Validating);
            // 
            // lblOtherDur
            // 
            this.lblOtherDur.Location = new System.Drawing.Point(24, 99);
            this.lblOtherDur.Name = "lblOtherDur";
            this.lblOtherDur.Size = new System.Drawing.Size(156, 24);
            this.lblOtherDur.TabIndex = 29;
            this.lblOtherDur.Text = "*# Duration [ms]";
            this.lblOtherDur.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtUpCode
            // 
            this.txtUpCode.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtUpCode.InputString = null;
            this.txtUpCode.Location = new System.Drawing.Point(547, 52);
            this.txtUpCode.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtUpCode.MaxByteLength = 0;
            this.txtUpCode.MaxLength = 30;
            this.txtUpCode.Name = "txtUpCode";
            this.txtUpCode.Size = new System.Drawing.Size(139, 23);
            this.txtUpCode.TabIndex = 22;
            this.txtUpCode.Visible = false;
            // 
            // lblTail
            // 
            this.lblTail.Location = new System.Drawing.Point(24, 140);
            this.lblTail.Name = "lblTail";
            this.lblTail.Size = new System.Drawing.Size(156, 24);
            this.lblTail.TabIndex = 31;
            this.lblTail.Text = "DTMF Tail [ms]";
            this.lblTail.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblRate
            // 
            this.lblRate.Location = new System.Drawing.Point(24, 181);
            this.lblRate.Name = "lblRate";
            this.lblRate.Size = new System.Drawing.Size(156, 24);
            this.lblRate.TabIndex = 31;
            this.lblRate.Text = "DTMF Rate";
            this.lblRate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // DtmfForm
            // 
            this.ClientSize = new System.Drawing.Size(383, 247);
            this.Controls.Add(this.pnlDtmf);
            this.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DtmfForm";
            this.Text = "DTMF";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DtmfForm_FormClosing);
            this.Load += new System.EventHandler(this.DtmfForm_Load);
            this.pnlDtmf.ResumeLayout(false);
            this.pnlDtmf.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudOtherDur)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTailDur)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFstDur)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFstDlyTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDecTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudAutoResetTimer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRespHoldTime)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        static DtmfForm()
        {

            DtmfForm.SZ_DECODE_RESP = new string[4]
            {
                "None",
                "Reminder",
                "Reply",
                "Reminder & Reply"
            };
            DtmfForm.SZ_GROUP_CODE = new string[7]
            {
                Settings.SZ_NONE,
                "A",
                "B",
                "C",
                "D",
                "*",
                "#"
            };
            DtmfForm.SZ_KILL_TYPE = new string[3]
            {
                "Tx Disable",
                "Tx&Rx Disable",
                "Kill"
            };
            DtmfForm.SZ_KILL_WAKE_DEC = new string[2]
            {
                "Off",
                "On"
            };

            DtmfForm.data = new Dtmf();
        }
    }
}
