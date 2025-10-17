using MaterialSkin.Controls;

namespace SmartDNSProxy_VPN_Client
{
    partial class About
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
            this.AppTitle = new System.Windows.Forms.Label();
            this.Logo = new System.Windows.Forms.PictureBox();
            this.Version = new System.Windows.Forms.Label();
            this.ContactUs = new MaterialSkin.Controls.MaterialFlatButton();
            this.GrayBarHidder = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.Logo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GrayBarHidder)).BeginInit();
            this.SuspendLayout();
            // 
            // AppTitle
            // 
            this.AppTitle.AutoSize = true;
            this.AppTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.AppTitle.Location = new System.Drawing.Point(191, 45);
            this.AppTitle.Name = "AppTitle";
            this.AppTitle.Size = new System.Drawing.Size(73, 25);
            this.AppTitle.TabIndex = 2;
            this.AppTitle.Text = "Getflix";
            // 
            // Logo
            // 
            this.Logo.BackColor = System.Drawing.Color.White;
            this.Logo.Image = global::SmartDNSProxy_VPN_Client.Properties.Resources.SmartDNSProxyLogoOnly;
            this.Logo.Location = new System.Drawing.Point(12, 45);
            this.Logo.Name = "Logo";
            this.Logo.Size = new System.Drawing.Size(80, 85);
            this.Logo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.Logo.TabIndex = 3;
            this.Logo.TabStop = false;
            // 
            // Version
            // 
            this.Version.AutoSize = true;
            this.Version.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.Version.ForeColor = System.Drawing.SystemColors.GrayText;
            this.Version.Location = new System.Drawing.Point(174, 79);
            this.Version.Name = "Version";
            this.Version.Size = new System.Drawing.Size(106, 20);
            this.Version.TabIndex = 4;
            this.Version.Text = "Version: 0.0.0";
            // 
            // ContactUs
            // 
            this.ContactUs.AutoSize = true;
            this.ContactUs.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ContactUs.BackColor = System.Drawing.Color.White;
            this.ContactUs.Depth = 0;
            this.ContactUs.Icon = null;
            this.ContactUs.Location = new System.Drawing.Point(134, 105);
            this.ContactUs.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.ContactUs.MouseState = MaterialSkin.MouseState.HOVER;
            this.ContactUs.Name = "ContactUs";
            this.ContactUs.Primary = false;
            this.ContactUs.Size = new System.Drawing.Size(179, 38);
            this.ContactUs.TabIndex = 5;
            this.ContactUs.Text = "Contact Us";
            this.ContactUs.UseVisualStyleBackColor = false;
            this.ContactUs.Click += new System.EventHandler(this.ContactUs_Click);
            // 
            // GrayBarHidder
            // 
            this.GrayBarHidder.Location = new System.Drawing.Point(0, 25);
            this.GrayBarHidder.Name = "GrayBarHidder";
            this.GrayBarHidder.Size = new System.Drawing.Size(350, 40);
            this.GrayBarHidder.TabIndex = 1;
            this.GrayBarHidder.TabStop = false;
            // 
            // About
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(350, 160);
            this.Controls.Add(this.ContactUs);
            this.Controls.Add(this.Version);
            this.Controls.Add(this.Logo);
            this.Controls.Add(this.AppTitle);
            this.Controls.Add(this.GrayBarHidder);
            this.Name = "About";
            this.Text = "About";
            ((System.ComponentModel.ISupportInitialize)(this.Logo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GrayBarHidder)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label AppTitle;
        private System.Windows.Forms.PictureBox Logo;
        private System.Windows.Forms.Label Version;
        private MaterialFlatButton ContactUs;
        private System.Windows.Forms.PictureBox GrayBarHidder;
    }
}