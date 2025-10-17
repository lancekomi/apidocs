namespace SmartDNSProxy_VPN_Client
{
    partial class frmUpdater
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmUpdater));
            this.saveDiag1 = new System.Windows.Forms.SaveFileDialog();
            this.progressBar1 = new MetroFramework.Controls.MetroProgressBar();
            this.metroLabel1 = new MetroFramework.Controls.MetroLabel();
            this.statusLabel = new MetroFramework.Controls.MetroLabel();
            this.statusText = new MetroFramework.Controls.MetroLabel();
            this.SuspendLayout();
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(27, 82);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.ProgressBarStyle = System.Windows.Forms.ProgressBarStyle.Blocks;
            this.progressBar1.Size = new System.Drawing.Size(216, 23);
            this.progressBar1.TabIndex = 20;
            this.progressBar1.UseWaitCursor = true;
            // 
            // metroLabel1
            // 
            this.metroLabel1.AutoSize = true;
            this.metroLabel1.Location = new System.Drawing.Point(23, 60);
            this.metroLabel1.Name = "metroLabel1";
            this.metroLabel1.Size = new System.Drawing.Size(123, 19);
            this.metroLabel1.TabIndex = 21;
            this.metroLabel1.Text = "Download progress";
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(23, 108);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(46, 19);
            this.statusLabel.TabIndex = 22;
            this.statusLabel.Text = "Status:";
            // 
            // statusText
            // 
            this.statusText.AutoSize = true;
            this.statusText.Location = new System.Drawing.Point(65, 108);
            this.statusText.Name = "statusText";
            this.statusText.Size = new System.Drawing.Size(76, 19);
            this.statusText.TabIndex = 23;
            this.statusText.Text = "Updating ...";
            // 
            // frmUpdater
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(282, 147);
            this.Controls.Add(this.statusText);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.metroLabel1);
            this.Controls.Add(this.progressBar1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmUpdater";
            this.Resizable = false;
            this.ShowInTaskbar = false;
            this.Text = "Getflix VPN Client - Update";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmUpdater_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SaveFileDialog saveDiag1;
        private MetroFramework.Controls.MetroProgressBar progressBar1;
        private MetroFramework.Controls.MetroLabel metroLabel1;
        private MetroFramework.Controls.MetroLabel statusLabel;
        private MetroFramework.Controls.MetroLabel statusText;
    }
}