namespace SmartDNSProxy_VPN_Client
{
    partial class SplashScreen
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplashScreen));
            this.splashScreenImg = new System.Windows.Forms.PictureBox();
            this.startButton = new MaterialSkin.Controls.MaterialFlatButton();
            this.splashScreenPassword = new MaterialSkin.Controls.MaterialSingleLineTextField();
            this.splashScreenLogin = new MaterialSkin.Controls.MaterialSingleLineTextField();
            this.getVPNAcc = new System.Windows.Forms.LinkLabel();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.splashScreenImg)).BeginInit();
            this.SuspendLayout();
            // 
            // splashScreenImg
            // 
            this.splashScreenImg.Image = global::SmartDNSProxy_VPN_Client.Properties.Resources.splashScreenImg;
            this.splashScreenImg.Location = new System.Drawing.Point(0, 25);
            this.splashScreenImg.Name = "splashScreenImg";
            this.splashScreenImg.Size = new System.Drawing.Size(412, 385);
            this.splashScreenImg.TabIndex = 4;
            this.splashScreenImg.TabStop = false;
            // 
            // startButton
            // 
            this.startButton.AutoSize = true;
            this.startButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.startButton.Depth = 0;
            this.startButton.Icon = null;
            this.startButton.Location = new System.Drawing.Point(131, 540);
            this.startButton.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.startButton.MouseState = MaterialSkin.MouseState.HOVER;
            this.startButton.Name = "startButton";
            this.startButton.Primary = false;
            this.startButton.Size = new System.Drawing.Size(138, 38);
            this.startButton.TabIndex = 41;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            // 
            // splashScreenPassword
            // 
            this.splashScreenPassword.Depth = 0;
            this.splashScreenPassword.Hint = "";
            this.splashScreenPassword.Location = new System.Drawing.Point(35, 490);
            this.splashScreenPassword.MaxLength = 32767;
            this.splashScreenPassword.MouseState = MaterialSkin.MouseState.HOVER;
            this.splashScreenPassword.Name = "splashScreenPassword";
            this.splashScreenPassword.PasswordChar = '\0';
            this.splashScreenPassword.SelectedText = "";
            this.splashScreenPassword.SelectionLength = 0;
            this.splashScreenPassword.SelectionStart = 0;
            this.splashScreenPassword.Size = new System.Drawing.Size(342, 23);
            this.splashScreenPassword.TabIndex = 40;
            this.splashScreenPassword.TabStop = false;
            this.splashScreenPassword.UseSystemPasswordChar = true;
            // 
            // splashScreenLogin
            // 
            this.splashScreenLogin.BackColor = System.Drawing.Color.White;
            this.splashScreenLogin.Depth = 0;
            this.splashScreenLogin.Hint = "";
            this.splashScreenLogin.Location = new System.Drawing.Point(35, 435);
            this.splashScreenLogin.MaxLength = 32767;
            this.splashScreenLogin.MouseState = MaterialSkin.MouseState.HOVER;
            this.splashScreenLogin.Name = "splashScreenLogin";
            this.splashScreenLogin.PasswordChar = '\0';
            this.splashScreenLogin.SelectedText = "";
            this.splashScreenLogin.SelectionLength = 0;
            this.splashScreenLogin.SelectionStart = 0;
            this.splashScreenLogin.Size = new System.Drawing.Size(342, 23);
            this.splashScreenLogin.TabIndex = 38;
            this.splashScreenLogin.TabStop = false;
            this.splashScreenLogin.UseSystemPasswordChar = false;
            // 
            // getVPNAcc
            // 
            this.getVPNAcc.AutoSize = true;
            this.getVPNAcc.LinkColor = System.Drawing.Color.OrangeRed;
            this.getVPNAcc.Location = new System.Drawing.Point(140, 604);
            this.getVPNAcc.Name = "getVPNAcc";
            this.getVPNAcc.Size = new System.Drawing.Size(120, 13);
            this.getVPNAcc.TabIndex = 42;
            this.getVPNAcc.TabStop = true;
            this.getVPNAcc.Text = "Click here to create one";
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.ForeColor = System.Drawing.Color.Gray;
            this.linkLabel1.LinkArea = new System.Windows.Forms.LinkArea(0, 0);
            this.linkLabel1.Location = new System.Drawing.Point(96, 584);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.linkLabel1.Size = new System.Drawing.Size(209, 13);
            this.linkLabel1.TabIndex = 42;
            this.linkLabel1.Text = "Don\'t have Smart DNS Proxy account yet?";
            // 
            // SplashScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(412, 644);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.getVPNAcc);
            this.Controls.Add(this.splashScreenPassword);
            this.Controls.Add(this.splashScreenLogin);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.splashScreenImg);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(412, 644);
            this.MinimumSize = new System.Drawing.Size(412, 644);
            this.Name = "SplashScreen";
            this.Sizable = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Smart DNS Proxy VPN Client";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SplashScreen_FormClosed);
            this.Load += new System.EventHandler(this.SplashScreen_Load);
            this.Shown += new System.EventHandler(this.SplashScreen_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.splashScreenImg)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.PictureBox splashScreenImg;
        private MaterialSkin.Controls.MaterialFlatButton startButton;
        private MaterialSkin.Controls.MaterialSingleLineTextField splashScreenPassword;
        private MaterialSkin.Controls.MaterialSingleLineTextField splashScreenLogin;
        private System.Windows.Forms.LinkLabel getVPNAcc;
        private System.Windows.Forms.LinkLabel linkLabel1;
    }
}