using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace DMR
{
    public class WebClientAsync : WebClient
    {
        private int timeoutMS;
        private System.Timers.Timer timer;


        public WebClientAsync(int timeoutSeconds)
        {
            timeoutMS = timeoutSeconds * 1000;

            timer = new System.Timers.Timer(timeoutMS);
            System.Timers.ElapsedEventHandler handler = null;

            handler = ((sender, args) =>
            {
                this.CancelAsync();
                timer.Stop();
                timer.Elapsed -= handler;
            });

            timer.Elapsed += handler;
            timer.Enabled = true;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            request.Timeout = timeoutMS;
            ((HttpWebRequest)request).ReadWriteTimeout = timeoutMS;

            return request;
        }

        protected override void OnDownloadProgressChanged(DownloadProgressChangedEventArgs e)
        {
            base.OnDownloadProgressChanged(e);
            timer.Stop();
            timer.Start();
        }
    }

    public partial class FirmwareLoaderUI : Form
    {
        public bool IsLoading = false;
        private static String tempFile = "";
        private WebClientAsync wc = null;
        private static bool _saveDownloadedFile = false;
        public static Dictionary<string, string> StringsDict = new Dictionary<string, string>();

        public FirmwareLoaderUI()
        {
            InitializeComponent();
            this.lblMessage.Text = "";
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);// Roger Clark. Added correct icon on main form!

            FirmwareLoader.outputType = FirmwareLoader.OutputType.OutputType_GD77;// FirmwareLoader.OutputType.OutputType_UNKNOWN;// FirmwareLoader.probeModel();
            /*
                        if ((FirmwareLoader.outputType < FirmwareLoader.OutputType.OutputType_GD77) || (FirmwareLoader.outputType > FirmwareLoader.OutputType.OutputType_DM1801))
                        {
                            MessageBox.Show("Error: Unable to detect your radio.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            FirmwareLoader.outputType = FirmwareLoader.OutputType.OutputType_GD77;
                        }
                        this.rbModels[(int)FirmwareLoader.outputType].Checked = true;
            * */
        }


        public void SetLabel(string txt)
        {
            if (this.lblMessage.InvokeRequired)
            {
                this.Invoke(new Action(() => SetLabel(txt)));
            }
            else
            {
                this.lblMessage.Text = txt;
            }
        }
        public void SetProgressPercentage(int perc)
        {
            if (this.progressBar1.InvokeRequired)
            {
                this.Invoke(new Action(() => SetProgressPercentage(perc)));
            }
            else
            {
                this.progressBar1.Value = perc;
            }
        }

        public void SetLoadingState(bool loading)
        {
            if (loading)
            {
                Invoke((MethodInvoker)delegate
                {
                    this.grpboxModel.Enabled = false;
                    this.btnClose.Enabled = false;
                    this.grpboxProgress.Enabled = true;
                });

            }
            else
            {
                Invoke((MethodInvoker)delegate
                {
                    this.grpboxModel.Enabled = true;
                    this.btnClose.Enabled = true;
                    this.grpboxProgress.Enabled = false;
                    this.progressBar1.Value = 0;
                });
            }
        }


        private void FirmwareLoaderUI_Load(object sender, EventArgs e)
        {
            Settings.UpdateComponentTextsFromLanguageXmlData(this);
            Settings.ReadCommonsForSectionIntoDictionary(StringsDict, this.Name);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (!IsLoading)
            {
                this.Close();
            }
            else
            {
                MessageBox.Show(StringsDict["CantInterrupt"]);
            }
        }


        private void rbModel_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb != null)
            {
                if (rb.Checked)
                {
                    FirmwareLoader.outputType = (FirmwareLoader.OutputType)rb.Tag;
                    this.btnDownloadFirmware.Enabled = true;
                    this.btnUploadFirmware.Enabled = true;
                }
            }
        }

        private DialogResult DialogBox(String title, String message, String btn1Label = "&Yes", String btn2Label = "&No", String btn3Label = "&Cancel")
        {
            int buttonX = 10;
            int buttonY = 120 - 25 - 5;
            Form form = new System.Windows.Forms.Form();
            Label label = new System.Windows.Forms.Label();
            Button button1 = new System.Windows.Forms.Button();
            Button button2 = new System.Windows.Forms.Button();
            Button button3 = new System.Windows.Forms.Button();
            form.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);// Roger Clark. Added correct icon on main form!

            form.SuspendLayout();

            if (btn1Label.Length <= 0)
            {
                button1.Visible = false;
                button1.Enabled = false;
            }

            if (btn2Label.Length <= 0)
            {
                button2.Visible = false;
                button2.Enabled = false;
            }

            if (btn1Label.Length <= 0 || btn2Label.Length <= 0)
            {
                buttonX += 120 + 10;
            }

            form.Text = title;

            // Label
            label.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            label.Location = new System.Drawing.Point(13, 13);
            label.Name = "LblMessage";
            label.Size = new System.Drawing.Size(380 - (13 * 2), (120 - 24 - 13 - 13));
            label.Text = message;
            label.TextAlign = ContentAlignment.MiddleCenter;

            // Button 1
            button1.Text = btn1Label ?? string.Empty;
            button1.Name = "btnYes";
            button1.Location = new System.Drawing.Point(buttonX, buttonY);
            button1.Size = new System.Drawing.Size(120, 24);
            button1.UseVisualStyleBackColor = true;

            if (button1.Visible)
            {
                buttonX += 120 + 10;
            }

            // Button 2
            button2.Text = btn2Label ?? string.Empty;
            button2.Name = "btnNo";
            button2.Location = new System.Drawing.Point(buttonX, buttonY);
            button2.Size = new System.Drawing.Size(120, 24);
            button2.UseVisualStyleBackColor = true;

            // Button 3
            button3.Text = btn3Label ?? string.Empty;
            button3.Location = new System.Drawing.Point((380 - 100 - 10), buttonY);
            button3.Name = "btnCancel";
            button3.Size = new System.Drawing.Size(100, 24);
            button3.UseVisualStyleBackColor = true;

            // Assign results
            button1.DialogResult = DialogResult.Yes;
            button2.DialogResult = DialogResult.No;
            button3.DialogResult = DialogResult.Cancel;

            form.ClientSize = new System.Drawing.Size(396, 107);
            form.Controls.Add(label);

            if (button1.Visible)
            {
                form.Controls.Add(button1);
            }

            if (button2.Visible)
            {
                form.Controls.Add(button2);
            }

            form.Controls.Add(button3);

            form.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            form.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            form.ClientSize = new System.Drawing.Size(380, 120);
            form.KeyPreview = true;
            form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = (button1.Visible == false ? button2 : button1);
            form.CancelButton = button3;
            form.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            form.ResumeLayout(false);

            DialogResult dialogResult = form.ShowDialog();

            return dialogResult;
        }
        private void downloadProgressChangedCallback(object sender, DownloadProgressChangedEventArgs ev)
        {
            this.progressBarDwnl.Value = ev.ProgressPercentage;
        }

        private void downloadStringCompletedCallback(object sender, DownloadStringCompletedEventArgs ev)
        {
            if (ev.Cancelled)
            {
                MessageBox.Show(StringsDict["DownloadCancelled"], StringsDict["Timeout"], MessageBoxButtons.OK, MessageBoxIcon.Error);

                SetLoadingState(false);
                this.progressBarDwnl.Visible = false;
                return;
            }
            else if (ev.Error != null)
            {
                MessageBox.Show(ev.Error.Message, StringsDict["Error"], MessageBoxButtons.OK, MessageBoxIcon.Error);

                SetLoadingState(false);
                this.progressBarDwnl.Visible = false;
                return;
            }

            String result = ev.Result;

            this.progressBarDwnl.Visible = false;

            FirmwareLoaderReleasesList flrl = new FirmwareLoaderReleasesList(result);
            if (DialogResult.Cancel != flrl.ShowDialog())
            {
                if (_saveDownloadedFile)
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.Filter = "firmware files|*.sgl";
                    saveFileDialog.InitialDirectory = IniFileUtils.getProfileStringWithDefault("Setup", "LastFirmwareLocation", null);
                    saveFileDialog.FileName = FirmwareLoader.getModelSaveFileString(FirmwareLoader.outputType) +"_"+ flrl.SelectedVersion + ".sgl";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK && saveFileDialog.FileName != null)
                    {
                        tempFile = saveFileDialog.FileName;
                    }
                    else
                    {
                        
                        MessageBox.Show(StringsDict["No_file_location_specified"]);
                        SetLoadingState(false);
                        IsLoading = false;
                        return;
                    }
                }
                else
                {
                    tempFile = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".sgl";
                }
                // Download the firmware binary to a temporary file
                try
                {
                    Application.DoEvents();
                    this.progressBarDwnl.Value = 0;
                    this.progressBarDwnl.Visible = true;
                    wc.DownloadFileAsync(new Uri(flrl.SelectedURL), tempFile);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(StringsDict["Error"] + ": " + ex.Message, StringsDict["Error"], MessageBoxButtons.OK, MessageBoxIcon.Error);

                    if (File.Exists(tempFile))
                        File.Delete(tempFile);

                    SetLoadingState(false);
                    this.progressBarDwnl.Visible = false;
                    return;
                }


            }
            else
            {
                SetLoadingState(false);
            }
            return;
        }

        private void downloadFileCompletedCallback(object sender, AsyncCompletedEventArgs ev)
        {
            this.progressBarDwnl.Visible = false;
            this.progressBarDwnl.Value = 0;

            if (ev.Cancelled)
            {
                MessageBox.Show(StringsDict["DownloadCancelled"], StringsDict["Timeout"], MessageBoxButtons.OK, MessageBoxIcon.Error);

                SetLoadingState(false);
                IsLoading = false;
                return;
            }
            else if (ev.Error != null)
            {
                MessageBox.Show(ev.Error.Message, StringsDict["Error"], MessageBoxButtons.OK, MessageBoxIcon.Error);

                SetLoadingState(false);
                IsLoading = false;
                return;
            }

            // Now flash the downloaded firmware
            Action<object> action = (object obj) =>
            {

                IsLoading = true;
                SetLoadingState(true);
                if (!_saveDownloadedFile)
                {
                    FirmwareLoader.UploadFirmare(tempFile, this);
                    SetLoadingState(false);
                    IsLoading = false;
                    // Cleanup
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }
                else
                {
                    SetLoadingState(false);
                    IsLoading = false;
                }
            };
            try
            {
                Task t1 = new Task(action, "LoaderUSB");
                t1.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(StringsDict["Error"] + ": " + ex.Message, StringsDict["Error"], MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetLoadingState(false);
                IsLoading = false;
                // Cleanup
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        private void btnDetectModel_Click(object sender, EventArgs e)
        {
            this.btnDetectModel.Enabled = false;

            FirmwareLoader.outputType = FirmwareLoader.OutputType.OutputType_UNKNOWN;//FirmwareLoader.probeModel();

            if ((FirmwareLoader.outputType < FirmwareLoader.OutputType.OutputType_GD77) || (FirmwareLoader.outputType > FirmwareLoader.OutputType.OutputType_RD5R))
            {
                MessageBox.Show(StringsDict["Error:_Unable_to_detect_your_radio."], StringsDict["Error"], MessageBoxButtons.OK, MessageBoxIcon.Error);
                FirmwareLoader.outputType = FirmwareLoader.OutputType.OutputType_GD77;
            }
            
            this.rbModels[(int)FirmwareLoader.outputType].Checked = true;
            this.btnDetectModel.Enabled = true;
        }

        private void btnDownloadFirmware_Click(object sender, EventArgs e)
        {

            Uri uri = new Uri("https://api.github.com/repos/libredmr/opengd77_releases/releases");//https://api.github.com/repos/rogerclarkmelbourne/opengd77/releases");

            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                _saveDownloadedFile = true;
            }
            else
            {
                _saveDownloadedFile = false;
            }

            this.lblMessage.Text = "";
            wc = new WebClientAsync(40);
            wc.Headers.Add(HttpRequestHeader.Accept, "application/json");
            wc.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            wc.Headers.Add(HttpRequestHeader.UserAgent, "request");

            ServicePointManager.Expect100Continue = true;
            // If you have .Net 4.5
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            // otherwise
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            this.progressBarDwnl.Value = 0;

            wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(downloadProgressChangedCallback);
            wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(downloadStringCompletedCallback);
            wc.DownloadFileCompleted += new AsyncCompletedEventHandler(downloadFileCompletedCallback);

            this.progressBarDwnl.Visible = true;
            SetLoadingState(true);

            // Retrieve release webpage
            try
            {
                Application.DoEvents();
                wc.DownloadStringAsync(uri);
            }
            catch (WebException ex)
            {
                MessageBox.Show(StringsDict["Error"] + ": " + ex.Message, StringsDict["Error"], MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetLoadingState(false);
                this.progressBarDwnl.Visible = false;
                return;
            }
        }

        private void btnUploadFirmware_Click(object sender, EventArgs e)
        {
            if (IsLoading)
            {
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "firmware files|*.sgl";
            openFileDialog.InitialDirectory = IniFileUtils.getProfileStringWithDefault("Setup", "LastFirmwareLocation", null);

            if (openFileDialog.ShowDialog() == DialogResult.OK && openFileDialog.FileName != null)
            {
                IniFileUtils.WriteProfileString("Setup", "LastFirmwareLocation", Path.GetDirectoryName(openFileDialog.FileName));

                this.lblMessage.Text = "";

                Action<object> action = (object obj) =>
                {
                    IsLoading = true;
                    SetLoadingState(true);
                    FirmwareLoader.UploadFirmare(openFileDialog.FileName, this);
                    SetLoadingState(false);
                    IsLoading = false;
                };
                try
                {
                    Task t1 = new Task(action, "LoaderUSB");
                    t1.Start();
                }
                catch (Exception)
                {
                    IsLoading = false;
                    SetLoadingState(false);
                }

            }
        }

        private void FirmwareLoaderUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsLoading)
            {
                MessageBox.Show(StringsDict["CantInterrupt"]);
            }
            e.Cancel = IsLoading;
        }
    }
}

