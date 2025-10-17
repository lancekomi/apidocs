namespace SmartDNSProxy_VPN_Client
{
    partial class frmMain
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.timerOpenVPN = new System.Windows.Forms.Timer(this.components);
            this.timerDataCount = new System.Windows.Forms.Timer(this.components);
            this.VPNClientBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.labelStatus = new MaterialSkin.Controls.MaterialLabel();
            this.materialLabel11 = new MaterialSkin.Controls.MaterialLabel();
            this.selectSettingsPort = new MetroFramework.Controls.MetroComboBox();
            this.selectProtocol = new MetroFramework.Controls.MetroComboBox();
            this.checkboxAutoReconnect = new MetroFramework.Controls.MetroToggle();
            this.materialLabel10 = new MaterialSkin.Controls.MaterialLabel();
            this.checkBoxKillSwitch = new MetroFramework.Controls.MetroToggle();
            this.materialLabel9 = new MaterialSkin.Controls.MaterialLabel();
            this.checkboxRunAtWindowsStart = new MetroFramework.Controls.MetroToggle();
            this.materialLabel8 = new MaterialSkin.Controls.MaterialLabel();
            this.materialLabel6 = new MaterialSkin.Controls.MaterialLabel();
            this.DnsProxyStatus = new MaterialSkin.Controls.MaterialLabel();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.selectServerList = new System.Windows.Forms.ComboBox();
            this.selectCountryList = new System.Windows.Forms.ComboBox();
            this.connectingSpinner = new MetroFramework.Controls.MetroProgressSpinner();
            this.btnLogs = new MaterialSkin.Controls.MaterialFlatButton();
            this.labelPPTPL2TPDataInOut = new MaterialSkin.Controls.MaterialLabel();
            this.materialLabel13 = new MaterialSkin.Controls.MaterialLabel();
            this.btnConnectVPN = new SmartDNSProxy_VPN_Client.RoundButton();
            this.selectorFrame1 = new System.Windows.Forms.PictureBox();
            this.selectorFrame2 = new System.Windows.Forms.PictureBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.killSwitchLink = new System.Windows.Forms.LinkLabel();
            this.procotolChoseLink = new System.Windows.Forms.LinkLabel();
            this.LogOut = new MaterialSkin.Controls.MaterialFlatButton();
            this.killswitchInfo = new System.Windows.Forms.Label();
            this.materialLabel12 = new MaterialSkin.Controls.MaterialLabel();
            this.selectSettingsProtocol = new MetroFramework.Controls.MetroComboBox();
            this.saveSettings = new MaterialSkin.Controls.MaterialFlatButton();
            this.materialLabel7 = new MaterialSkin.Controls.MaterialLabel();
            this.saveButtonFrame = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.materialTabControl1 = new MaterialSkin.Controls.MaterialTabControl();
            this.materialTabSelector1 = new MaterialSkin.Controls.MaterialTabSelector();
            this.logoBox = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.selectorFrame1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.selectorFrame2)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.saveButtonFrame)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.materialTabControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logoBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // VPNClientBackgroundWorker
            // 
            this.VPNClientBackgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.VPNClientBackgroundWorker_DoWork);
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Depth = 0;
            this.labelStatus.Font = new System.Drawing.Font("Roboto", 10F);
            this.labelStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.labelStatus.Location = new System.Drawing.Point(144, 51);
            this.labelStatus.MouseState = MaterialSkin.MouseState.HOVER;
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(101, 18);
            this.labelStatus.TabIndex = 37;
            this.labelStatus.Text = "Not Connected";
            // 
            // materialLabel11
            // 
            this.materialLabel11.AutoSize = true;
            this.materialLabel11.Depth = 0;
            this.materialLabel11.Font = new System.Drawing.Font("Roboto", 10F);
            this.materialLabel11.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialLabel11.Location = new System.Drawing.Point(104, 21);
            this.materialLabel11.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel11.Name = "materialLabel11";
            this.materialLabel11.Size = new System.Drawing.Size(159, 18);
            this.materialLabel11.TabIndex = 35;
            this.materialLabel11.Text = "Encrypted VPN is turned";
            // 
            // selectSettingsPort
            // 
            this.selectSettingsPort.FormattingEnabled = true;
            this.selectSettingsPort.ItemHeight = 23;
            this.selectSettingsPort.Location = new System.Drawing.Point(27, 174);
            this.selectSettingsPort.Name = "selectSettingsPort";
            this.selectSettingsPort.Size = new System.Drawing.Size(349, 29);
            this.selectSettingsPort.TabIndex = 33;
            this.selectSettingsPort.UseSelectable = true;
            // 
            // selectProtocol
            // 
            this.selectProtocol.FormattingEnabled = true;
            this.selectProtocol.ItemHeight = 23;
            this.selectProtocol.Location = new System.Drawing.Point(27, 36);
            this.selectProtocol.Name = "selectProtocol";
            this.selectProtocol.Size = new System.Drawing.Size(349, 29);
            this.selectProtocol.TabIndex = 34;
            this.selectProtocol.UseSelectable = true;
            // 
            // checkboxAutoReconnect
            // 
            this.checkboxAutoReconnect.AutoSize = true;
            this.checkboxAutoReconnect.DisplayStatus = false;
            this.checkboxAutoReconnect.Location = new System.Drawing.Point(326, 362);
            this.checkboxAutoReconnect.Name = "checkboxAutoReconnect";
            this.checkboxAutoReconnect.Size = new System.Drawing.Size(50, 17);
            this.checkboxAutoReconnect.TabIndex = 27;
            this.checkboxAutoReconnect.Text = "Off";
            this.checkboxAutoReconnect.UseSelectable = true;
            // 
            // materialLabel10
            // 
            this.materialLabel10.AutoSize = true;
            this.materialLabel10.Depth = 0;
            this.materialLabel10.Font = new System.Drawing.Font("Roboto", 10F);
            this.materialLabel10.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialLabel10.Location = new System.Drawing.Point(30, 359);
            this.materialLabel10.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel10.Name = "materialLabel10";
            this.materialLabel10.Size = new System.Drawing.Size(128, 18);
            this.materialLabel10.TabIndex = 26;
            this.materialLabel10.Text = "AUTO RECONNECT";
            // 
            // checkBoxKillSwitch
            // 
            this.checkBoxKillSwitch.AutoSize = true;
            this.checkBoxKillSwitch.DisplayStatus = false;
            this.checkBoxKillSwitch.Location = new System.Drawing.Point(326, 326);
            this.checkBoxKillSwitch.Name = "checkBoxKillSwitch";
            this.checkBoxKillSwitch.Size = new System.Drawing.Size(50, 17);
            this.checkBoxKillSwitch.TabIndex = 25;
            this.checkBoxKillSwitch.Text = "Off";
            this.checkBoxKillSwitch.UseSelectable = true;
            // 
            // materialLabel9
            // 
            this.materialLabel9.AutoSize = true;
            this.materialLabel9.Depth = 0;
            this.materialLabel9.Font = new System.Drawing.Font("Roboto", 10F);
            this.materialLabel9.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialLabel9.Location = new System.Drawing.Point(30, 326);
            this.materialLabel9.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel9.Name = "materialLabel9";
            this.materialLabel9.Size = new System.Drawing.Size(159, 18);
            this.materialLabel9.TabIndex = 24;
            this.materialLabel9.Text = "INTERNET KILL SWITCH";
            // 
            // checkboxRunAtWindowsStart
            // 
            this.checkboxRunAtWindowsStart.AutoSize = true;
            this.checkboxRunAtWindowsStart.DisplayStatus = false;
            this.checkboxRunAtWindowsStart.Location = new System.Drawing.Point(326, 293);
            this.checkboxRunAtWindowsStart.Name = "checkboxRunAtWindowsStart";
            this.checkboxRunAtWindowsStart.Size = new System.Drawing.Size(50, 17);
            this.checkboxRunAtWindowsStart.TabIndex = 23;
            this.checkboxRunAtWindowsStart.Text = "Off";
            this.checkboxRunAtWindowsStart.UseSelectable = true;
            // 
            // materialLabel8
            // 
            this.materialLabel8.AutoSize = true;
            this.materialLabel8.Depth = 0;
            this.materialLabel8.Font = new System.Drawing.Font("Roboto", 10F);
            this.materialLabel8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialLabel8.Location = new System.Drawing.Point(30, 293);
            this.materialLabel8.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel8.Name = "materialLabel8";
            this.materialLabel8.Size = new System.Drawing.Size(102, 18);
            this.materialLabel8.TabIndex = 22;
            this.materialLabel8.Text = "AUTO LAUNCH";
            // 
            // materialLabel6
            // 
            this.materialLabel6.AutoSize = true;
            this.materialLabel6.Depth = 0;
            this.materialLabel6.Font = new System.Drawing.Font("Roboto", 10F);
            this.materialLabel6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialLabel6.Location = new System.Drawing.Point(26, 15);
            this.materialLabel6.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel6.Name = "materialLabel6";
            this.materialLabel6.Size = new System.Drawing.Size(132, 18);
            this.materialLabel6.TabIndex = 19;
            this.materialLabel6.Text = "CONNECTION TYPE";
            // 
            // DnsProxyStatus
            // 
            this.DnsProxyStatus.AutoSize = true;
            this.DnsProxyStatus.Depth = 0;
            this.DnsProxyStatus.Font = new System.Drawing.Font("Roboto", 10F);
            this.DnsProxyStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.DnsProxyStatus.Location = new System.Drawing.Point(269, 21);
            this.DnsProxyStatus.MouseState = MaterialSkin.MouseState.HOVER;
            this.DnsProxyStatus.Name = "DnsProxyStatus";
            this.DnsProxyStatus.Size = new System.Drawing.Size(34, 18);
            this.DnsProxyStatus.TabIndex = 36;
            this.DnsProxyStatus.Text = "OFF";
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.tabPage1.Controls.Add(this.selectServerList);
            this.tabPage1.Controls.Add(this.selectCountryList);
            this.tabPage1.Controls.Add(this.connectingSpinner);
            this.tabPage1.Controls.Add(this.btnLogs);
            this.tabPage1.Controls.Add(this.labelPPTPL2TPDataInOut);
            this.tabPage1.Controls.Add(this.materialLabel13);
            this.tabPage1.Controls.Add(this.btnConnectVPN);
            this.tabPage1.Controls.Add(this.labelStatus);
            this.tabPage1.Controls.Add(this.DnsProxyStatus);
            this.tabPage1.Controls.Add(this.materialLabel11);
            this.tabPage1.Controls.Add(this.selectorFrame1);
            this.tabPage1.Controls.Add(this.selectorFrame2);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(404, 514);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Status";
            // 
            // selectServerList
            // 
            this.selectServerList.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.selectServerList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selectServerList.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.selectServerList.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.selectServerList.FormattingEnabled = true;
            this.selectServerList.ItemHeight = 23;
            this.selectServerList.Location = new System.Drawing.Point(105, 412);
            this.selectServerList.Name = "selectServerList";
            this.selectServerList.Size = new System.Drawing.Size(195, 29);
            this.selectServerList.TabIndex = 49;
            // 
            // selectCountryList
            // 
            this.selectCountryList.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.selectCountryList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selectCountryList.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.selectCountryList.FormattingEnabled = true;
            this.selectCountryList.ItemHeight = 23;
            this.selectCountryList.Location = new System.Drawing.Point(108, 362);
            this.selectCountryList.Name = "selectCountryList";
            this.selectCountryList.Size = new System.Drawing.Size(195, 29);
            this.selectCountryList.TabIndex = 48;
            // 
            // connectingSpinner
            // 
            this.connectingSpinner.Location = new System.Drawing.Point(272, 44);
            this.connectingSpinner.Maximum = 100;
            this.connectingSpinner.Name = "connectingSpinner";
            this.connectingSpinner.Size = new System.Drawing.Size(28, 25);
            this.connectingSpinner.TabIndex = 47;
            this.connectingSpinner.UseSelectable = true;
            // 
            // btnLogs
            // 
            this.btnLogs.AutoSize = true;
            this.btnLogs.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnLogs.Depth = 0;
            this.btnLogs.Icon = null;
            this.btnLogs.Location = new System.Drawing.Point(266, 467);
            this.btnLogs.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnLogs.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnLogs.Name = "btnLogs";
            this.btnLogs.Primary = false;
            this.btnLogs.Size = new System.Drawing.Size(129, 38);
            this.btnLogs.TabIndex = 46;
            this.btnLogs.Text = "Logs";
            this.btnLogs.UseVisualStyleBackColor = true;
            // 
            // labelPPTPL2TPDataInOut
            // 
            this.labelPPTPL2TPDataInOut.AutoSize = true;
            this.labelPPTPL2TPDataInOut.Depth = 0;
            this.labelPPTPL2TPDataInOut.Font = new System.Drawing.Font("Roboto", 10F);
            this.labelPPTPL2TPDataInOut.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.labelPPTPL2TPDataInOut.Location = new System.Drawing.Point(212, 337);
            this.labelPPTPL2TPDataInOut.MouseState = MaterialSkin.MouseState.HOVER;
            this.labelPPTPL2TPDataInOut.Name = "labelPPTPL2TPDataInOut";
            this.labelPPTPL2TPDataInOut.Size = new System.Drawing.Size(33, 18);
            this.labelPPTPL2TPDataInOut.TabIndex = 45;
            this.labelPPTPL2TPDataInOut.Text = "N/A";
            // 
            // materialLabel13
            // 
            this.materialLabel13.AutoSize = true;
            this.materialLabel13.Depth = 0;
            this.materialLabel13.Font = new System.Drawing.Font("Roboto", 10F);
            this.materialLabel13.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialLabel13.Location = new System.Drawing.Point(95, 337);
            this.materialLabel13.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel13.Name = "materialLabel13";
            this.materialLabel13.Size = new System.Drawing.Size(82, 18);
            this.materialLabel13.TabIndex = 44;
            this.materialLabel13.Text = "Data In/Out";
            // 
            // btnConnectVPN
            // 
            this.btnConnectVPN.AutoSize = true;
            this.btnConnectVPN.Enabled = false;
            this.btnConnectVPN.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConnectVPN.Image = global::SmartDNSProxy_VPN_Client.Properties.Resources.connectBtn;
            this.btnConnectVPN.Location = new System.Drawing.Point(98, 104);
            this.btnConnectVPN.Name = "btnConnectVPN";
            this.btnConnectVPN.Size = new System.Drawing.Size(230, 230);
            this.btnConnectVPN.TabIndex = 40;
            this.btnConnectVPN.TabStop = false;
            this.btnConnectVPN.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            // 
            // selectorFrame1
            // 
            this.selectorFrame1.Image = global::SmartDNSProxy_VPN_Client.Properties.Resources.comboboxBg;
            this.selectorFrame1.Location = new System.Drawing.Point(98, 358);
            this.selectorFrame1.Name = "selectorFrame1";
            this.selectorFrame1.Size = new System.Drawing.Size(215, 38);
            this.selectorFrame1.TabIndex = 41;
            this.selectorFrame1.TabStop = false;
            // 
            // selectorFrame2
            // 
            this.selectorFrame2.Image = global::SmartDNSProxy_VPN_Client.Properties.Resources.comboboxBg;
            this.selectorFrame2.Location = new System.Drawing.Point(98, 408);
            this.selectorFrame2.Name = "selectorFrame2";
            this.selectorFrame2.Size = new System.Drawing.Size(215, 38);
            this.selectorFrame2.TabIndex = 42;
            this.selectorFrame2.TabStop = false;
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.tabPage2.Controls.Add(this.killSwitchLink);
            this.tabPage2.Controls.Add(this.procotolChoseLink);
            this.tabPage2.Controls.Add(this.LogOut);
            this.tabPage2.Controls.Add(this.killswitchInfo);
            this.tabPage2.Controls.Add(this.materialLabel12);
            this.tabPage2.Controls.Add(this.selectSettingsProtocol);
            this.tabPage2.Controls.Add(this.saveSettings);
            this.tabPage2.Controls.Add(this.selectSettingsPort);
            this.tabPage2.Controls.Add(this.selectProtocol);
            this.tabPage2.Controls.Add(this.checkboxAutoReconnect);
            this.tabPage2.Controls.Add(this.materialLabel10);
            this.tabPage2.Controls.Add(this.checkBoxKillSwitch);
            this.tabPage2.Controls.Add(this.materialLabel9);
            this.tabPage2.Controls.Add(this.checkboxRunAtWindowsStart);
            this.tabPage2.Controls.Add(this.materialLabel8);
            this.tabPage2.Controls.Add(this.materialLabel7);
            this.tabPage2.Controls.Add(this.materialLabel6);
            this.tabPage2.Controls.Add(this.saveButtonFrame);
            this.tabPage2.Controls.Add(this.pictureBox2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(404, 514);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Settings";
            // 
            // killSwitchLink
            // 
            this.killSwitchLink.AutoSize = true;
            this.killSwitchLink.LinkColor = System.Drawing.Color.OrangeRed;
            this.killSwitchLink.Location = new System.Drawing.Point(134, 266);
            this.killSwitchLink.Name = "killSwitchLink";
            this.killSwitchLink.Size = new System.Drawing.Size(145, 13);
            this.killSwitchLink.TabIndex = 44;
            this.killSwitchLink.TabStop = true;
            this.killSwitchLink.Text = "What does Kill Switch mean?";
            this.killSwitchLink.Click += new System.EventHandler(this.killSwitchLink_Click);
            // 
            // procotolChoseLink
            // 
            this.procotolChoseLink.AutoSize = true;
            this.procotolChoseLink.LinkColor = System.Drawing.Color.OrangeRed;
            this.procotolChoseLink.Location = new System.Drawing.Point(126, 239);
            this.procotolChoseLink.Name = "procotolChoseLink";
            this.procotolChoseLink.Size = new System.Drawing.Size(163, 13);
            this.procotolChoseLink.TabIndex = 43;
            this.procotolChoseLink.TabStop = true;
            this.procotolChoseLink.Text = "Which protocol should I choose?";
            this.procotolChoseLink.Click += new System.EventHandler(this.procotolChoseLink_Click);
            // 
            // LogOut
            // 
            this.LogOut.AutoSize = true;
            this.LogOut.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.LogOut.Depth = 0;
            this.LogOut.Icon = null;
            this.LogOut.Location = new System.Drawing.Point(117, 457);
            this.LogOut.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.LogOut.MouseState = MaterialSkin.MouseState.HOVER;
            this.LogOut.Name = "LogOut";
            this.LogOut.Primary = false;
            this.LogOut.Size = new System.Drawing.Size(196, 38);
            this.LogOut.TabIndex = 35;
            this.LogOut.Text = "                Logout                  ";
            this.LogOut.UseVisualStyleBackColor = true;
            this.LogOut.Click += new System.EventHandler(this.LogOut_Click);
            // 
            // killswitchInfo
            // 
            this.killswitchInfo.AutoSize = true;
            this.killswitchInfo.BackColor = System.Drawing.Color.Transparent;
            this.killswitchInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.killswitchInfo.ForeColor = System.Drawing.Color.Red;
            this.killswitchInfo.Location = new System.Drawing.Point(30, 216);
            this.killswitchInfo.Name = "killswitchInfo";
            this.killswitchInfo.Size = new System.Drawing.Size(347, 13);
            this.killswitchInfo.TabIndex = 39;
            this.killswitchInfo.Text = "Killswitch function disabled due to firewall settings (FW must be enabled)";
            // 
            // materialLabel12
            // 
            this.materialLabel12.AutoSize = true;
            this.materialLabel12.Depth = 0;
            this.materialLabel12.Font = new System.Drawing.Font("Roboto", 10F);
            this.materialLabel12.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialLabel12.Location = new System.Drawing.Point(24, 85);
            this.materialLabel12.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel12.Name = "materialLabel12";
            this.materialLabel12.Size = new System.Drawing.Size(81, 18);
            this.materialLabel12.TabIndex = 37;
            this.materialLabel12.Text = "PROTOCOL";
            // 
            // selectSettingsProtocol
            // 
            this.selectSettingsProtocol.FormattingEnabled = true;
            this.selectSettingsProtocol.ItemHeight = 23;
            this.selectSettingsProtocol.Location = new System.Drawing.Point(27, 106);
            this.selectSettingsProtocol.Name = "selectSettingsProtocol";
            this.selectSettingsProtocol.Size = new System.Drawing.Size(349, 29);
            this.selectSettingsProtocol.TabIndex = 36;
            this.selectSettingsProtocol.UseSelectable = true;
            // 
            // saveSettings
            // 
            this.saveSettings.AutoSize = true;
            this.saveSettings.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.saveSettings.Depth = 0;
            this.saveSettings.Icon = null;
            this.saveSettings.Location = new System.Drawing.Point(116, 399);
            this.saveSettings.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.saveSettings.MouseState = MaterialSkin.MouseState.HOVER;
            this.saveSettings.Name = "saveSettings";
            this.saveSettings.Primary = false;
            this.saveSettings.Size = new System.Drawing.Size(196, 38);
            this.saveSettings.TabIndex = 35;
            this.saveSettings.Text = "Save settings";
            this.saveSettings.UseVisualStyleBackColor = true;
            // 
            // materialLabel7
            // 
            this.materialLabel7.AutoSize = true;
            this.materialLabel7.Depth = 0;
            this.materialLabel7.Font = new System.Drawing.Font("Roboto", 10F);
            this.materialLabel7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialLabel7.Location = new System.Drawing.Point(24, 153);
            this.materialLabel7.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel7.Name = "materialLabel7";
            this.materialLabel7.Size = new System.Drawing.Size(44, 18);
            this.materialLabel7.TabIndex = 21;
            this.materialLabel7.Text = "PORT";
            // 
            // saveButtonFrame
            // 
            this.saveButtonFrame.Image = global::SmartDNSProxy_VPN_Client.Properties.Resources.saveBtnBorder;
            this.saveButtonFrame.Location = new System.Drawing.Point(115, 397);
            this.saveButtonFrame.Name = "saveButtonFrame";
            this.saveButtonFrame.Size = new System.Drawing.Size(200, 44);
            this.saveButtonFrame.TabIndex = 38;
            this.saveButtonFrame.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::SmartDNSProxy_VPN_Client.Properties.Resources.saveBtnBorder;
            this.pictureBox2.Location = new System.Drawing.Point(116, 455);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(200, 44);
            this.pictureBox2.TabIndex = 40;
            this.pictureBox2.TabStop = false;
            // 
            // materialTabControl1
            // 
            this.materialTabControl1.Controls.Add(this.tabPage1);
            this.materialTabControl1.Controls.Add(this.tabPage2);
            this.materialTabControl1.Depth = 0;
            this.materialTabControl1.Location = new System.Drawing.Point(0, 104);
            this.materialTabControl1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialTabControl1.Name = "materialTabControl1";
            this.materialTabControl1.SelectedIndex = 0;
            this.materialTabControl1.Size = new System.Drawing.Size(412, 540);
            this.materialTabControl1.TabIndex = 3;
            // 
            // materialTabSelector1
            // 
            this.materialTabSelector1.BaseTabControl = this.materialTabControl1;
            this.materialTabSelector1.Depth = 0;
            this.materialTabSelector1.Location = new System.Drawing.Point(0, 62);
            this.materialTabSelector1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialTabSelector1.Name = "materialTabSelector1";
            this.materialTabSelector1.Padding = new System.Windows.Forms.Padding(50, 0, 0, 0);
            this.materialTabSelector1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.materialTabSelector1.Size = new System.Drawing.Size(412, 46);
            this.materialTabSelector1.TabIndex = 2;
            this.materialTabSelector1.Text = "materialTabSelector1";
            // 
            // logoBox
            // 
            this.logoBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(87)))), ((int)(((byte)(131)))));
            this.logoBox.Image = global::SmartDNSProxy_VPN_Client.Properties.Resources.smartDnsProxyLogo;
            this.logoBox.InitialImage = null;
            this.logoBox.Location = new System.Drawing.Point(119, 2);
            this.logoBox.Name = "logoBox";
            this.logoBox.Size = new System.Drawing.Size(174, 57);
            this.logoBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.logoBox.TabIndex = 4;
            this.logoBox.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::SmartDNSProxy_VPN_Client.Properties.Resources.MainFormTextHider;
            this.pictureBox1.Location = new System.Drawing.Point(0, 25);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(412, 31);
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(412, 644);
            this.Controls.Add(this.logoBox);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.materialTabSelector1);
            this.Controls.Add(this.materialTabControl1);
            this.DataBindings.Add(new System.Windows.Forms.Binding("Location", global::SmartDNSProxy_VPN_Client.Properties.Settings.Default, "Location", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Location = global::SmartDNSProxy_VPN_Client.Properties.Settings.Default.Location;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(412, 644);
            this.MinimumSize = new System.Drawing.Size(412, 644);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Smart DNS Proxy VPN Client";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.Resize += new System.EventHandler(this.frmMain_Resize);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.selectorFrame1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.selectorFrame2)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.saveButtonFrame)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.materialTabControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.logoBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Timer timerOpenVPN;
        private System.Windows.Forms.Timer timerDataCount;
        private System.ComponentModel.BackgroundWorker VPNClientBackgroundWorker;
        private MaterialSkin.Controls.MaterialLabel labelStatus;
        private MaterialSkin.Controls.MaterialLabel materialLabel11;
        private MetroFramework.Controls.MetroComboBox selectSettingsPort;
        private MetroFramework.Controls.MetroComboBox selectProtocol;
        private MetroFramework.Controls.MetroToggle checkboxAutoReconnect;
        private MaterialSkin.Controls.MaterialLabel materialLabel10;
        private MetroFramework.Controls.MetroToggle checkBoxKillSwitch;
        private MaterialSkin.Controls.MaterialLabel materialLabel9;
        private MetroFramework.Controls.MetroToggle checkboxRunAtWindowsStart;
        private MaterialSkin.Controls.MaterialLabel materialLabel8;
        private MaterialSkin.Controls.MaterialLabel materialLabel6;
        private MaterialSkin.Controls.MaterialLabel DnsProxyStatus;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private MaterialSkin.Controls.MaterialLabel materialLabel7;
        private MaterialSkin.Controls.MaterialTabControl materialTabControl1;
        private MaterialSkin.Controls.MaterialTabSelector materialTabSelector1;
        private System.Windows.Forms.PictureBox logoBox;
        private MaterialSkin.Controls.MaterialFlatButton saveSettings;
        private System.Windows.Forms.PictureBox selectorFrame1;
        private System.Windows.Forms.PictureBox selectorFrame2;
        private MetroFramework.Controls.MetroComboBox selectSettingsProtocol;
        private MaterialSkin.Controls.MaterialLabel materialLabel12;
        private MaterialSkin.Controls.MaterialLabel materialLabel13;
        private MaterialSkin.Controls.MaterialLabel labelPPTPL2TPDataInOut;
        private MaterialSkin.Controls.MaterialFlatButton btnLogs;
        private System.Windows.Forms.PictureBox saveButtonFrame;
        private MetroFramework.Controls.MetroProgressSpinner connectingSpinner;
        private System.Windows.Forms.Label killswitchInfo;
        private System.Windows.Forms.ComboBox selectCountryList;
        private System.Windows.Forms.ComboBox selectServerList;
        private RoundButton btnConnectVPN;
        private System.Windows.Forms.PictureBox pictureBox1;
        private MaterialSkin.Controls.MaterialFlatButton LogOut;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.LinkLabel procotolChoseLink;
        private System.Windows.Forms.LinkLabel killSwitchLink;
    }
}
