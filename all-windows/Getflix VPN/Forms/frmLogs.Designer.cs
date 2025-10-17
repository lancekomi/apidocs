namespace SmartDNSProxy_VPN_Client
{
    partial class frmLogs
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLogs));
            this.tabControlLog = new MetroFramework.Controls.MetroTabControl();
            this.metroTabPage1 = new MetroFramework.Controls.MetroTabPage();
            this.txtPPTPL2TPSSTPLog = new System.Windows.Forms.TextBox();
            this.metroTabPage2 = new MetroFramework.Controls.MetroTabPage();
            this.txtOpenVPNLog = new System.Windows.Forms.TextBox();
            this.copyToClipboardBtn = new MetroFramework.Controls.MetroButton();
            this.tabControlLog.SuspendLayout();
            this.metroTabPage1.SuspendLayout();
            this.metroTabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControlLog
            // 
            this.tabControlLog.Controls.Add(this.metroTabPage1);
            this.tabControlLog.Controls.Add(this.metroTabPage2);
            this.tabControlLog.Location = new System.Drawing.Point(13, 64);
            this.tabControlLog.Name = "tabControlLog";
            this.tabControlLog.SelectedIndex = 0;
            this.tabControlLog.Size = new System.Drawing.Size(382, 298);
            this.tabControlLog.TabIndex = 25;
            this.tabControlLog.UseSelectable = true;
            // 
            // metroTabPage1
            // 
            this.metroTabPage1.Controls.Add(this.txtPPTPL2TPSSTPLog);
            this.metroTabPage1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.metroTabPage1.HorizontalScrollbarBarColor = true;
            this.metroTabPage1.HorizontalScrollbarHighlightOnWheel = false;
            this.metroTabPage1.HorizontalScrollbarSize = 10;
            this.metroTabPage1.Location = new System.Drawing.Point(4, 38);
            this.metroTabPage1.Name = "metroTabPage1";
            this.metroTabPage1.Size = new System.Drawing.Size(374, 256);
            this.metroTabPage1.TabIndex = 0;
            this.metroTabPage1.Text = "PPTP && L2TP && SSTP";
            this.metroTabPage1.VerticalScrollbarBarColor = true;
            this.metroTabPage1.VerticalScrollbarHighlightOnWheel = false;
            this.metroTabPage1.VerticalScrollbarSize = 10;
            // 
            // txtPPTPL2TPSSTPLog
            // 
            this.txtPPTPL2TPSSTPLog.BackColor = System.Drawing.Color.White;
            this.txtPPTPL2TPSSTPLog.Location = new System.Drawing.Point(0, 3);
            this.txtPPTPL2TPSSTPLog.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtPPTPL2TPSSTPLog.Multiline = true;
            this.txtPPTPL2TPSSTPLog.Name = "txtPPTPL2TPSSTPLog";
            this.txtPPTPL2TPSSTPLog.ReadOnly = true;
            this.txtPPTPL2TPSSTPLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtPPTPL2TPSSTPLog.Size = new System.Drawing.Size(372, 256);
            this.txtPPTPL2TPSSTPLog.TabIndex = 2;
            // 
            // metroTabPage2
            // 
            this.metroTabPage2.Controls.Add(this.txtOpenVPNLog);
            this.metroTabPage2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.metroTabPage2.HorizontalScrollbarBarColor = true;
            this.metroTabPage2.HorizontalScrollbarHighlightOnWheel = false;
            this.metroTabPage2.HorizontalScrollbarSize = 10;
            this.metroTabPage2.Location = new System.Drawing.Point(4, 38);
            this.metroTabPage2.Name = "metroTabPage2";
            this.metroTabPage2.Size = new System.Drawing.Size(374, 256);
            this.metroTabPage2.TabIndex = 1;
            this.metroTabPage2.Text = "OpenVPN";
            this.metroTabPage2.VerticalScrollbarBarColor = true;
            this.metroTabPage2.VerticalScrollbarHighlightOnWheel = false;
            this.metroTabPage2.VerticalScrollbarSize = 10;
            // 
            // txtOpenVPNLog
            // 
            this.txtOpenVPNLog.BackColor = System.Drawing.Color.White;
            this.txtOpenVPNLog.Location = new System.Drawing.Point(0, 3);
            this.txtOpenVPNLog.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtOpenVPNLog.Multiline = true;
            this.txtOpenVPNLog.Name = "txtOpenVPNLog";
            this.txtOpenVPNLog.ReadOnly = true;
            this.txtOpenVPNLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtOpenVPNLog.Size = new System.Drawing.Size(372, 256);
            this.txtOpenVPNLog.TabIndex = 2;
            // 
            // copyToClipboardBtn
            // 
            this.copyToClipboardBtn.Location = new System.Drawing.Point(276, 386);
            this.copyToClipboardBtn.Name = "copyToClipboardBtn";
            this.copyToClipboardBtn.Size = new System.Drawing.Size(115, 23);
            this.copyToClipboardBtn.TabIndex = 26;
            this.copyToClipboardBtn.Text = "Copy to clipboard";
            this.copyToClipboardBtn.UseSelectable = true;
            this.copyToClipboardBtn.Click += new System.EventHandler(this.copyToClipboardBtn_Click);
            // 
            // frmLogs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(419, 440);
            this.Controls.Add(this.copyToClipboardBtn);
            this.Controls.Add(this.tabControlLog);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmLogs";
            this.Text = "Getflix VPN Client - Logs";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmLogs_FormClosing);
            this.tabControlLog.ResumeLayout(false);
            this.metroTabPage1.ResumeLayout(false);
            this.metroTabPage1.PerformLayout();
            this.metroTabPage2.ResumeLayout(false);
            this.metroTabPage2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private MetroFramework.Controls.MetroTabControl tabControlLog;
        private MetroFramework.Controls.MetroTabPage metroTabPage1;
        private System.Windows.Forms.TextBox txtPPTPL2TPSSTPLog;
        private MetroFramework.Controls.MetroTabPage metroTabPage2;
        private System.Windows.Forms.TextBox txtOpenVPNLog;
        private MetroFramework.Controls.MetroButton copyToClipboardBtn;
    }
}