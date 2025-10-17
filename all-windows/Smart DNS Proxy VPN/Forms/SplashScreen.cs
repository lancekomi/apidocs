using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetroFramework;
using MaterialSkin;
using MaterialSkin.Controls;
using SmartDNSProxy_VPN_Client.Properties;

namespace SmartDNSProxy_VPN_Client
{
    public partial class SplashScreen : MaterialForm
    {
        private bool isTryingToConnect = false;
        public static SplashScreen Instance { get; private set; }

        public SplashScreen()
        {
            InitializeComponent();
            Instance = this;
            startButton.Click += startButton_Click;
            getVPNAcc.Click += getVPNAcc_Click;
            this.AcceptButton = startButton;
            this.splashScreenLogin.Text = "VPN Username";
            this.splashScreenPassword.UseSystemPasswordChar = false;
            this.splashScreenPassword.Text = "VPN Password";
            this.splashScreenLogin.LostFocus += placeholderAdd;
            this.splashScreenLogin.GotFocus += SplashScreenLoginOnGotFocus;
            this.splashScreenPassword.LostFocus += placeholderAdd;
            this.splashScreenPassword.GotFocus += SplashScreenPasswordOnGotFocus;
            this.splashScreenLogin.ForeColor = Color.FromArgb(240, 240, 245);
            this.splashScreenPassword.ForeColor = Color.FromArgb(240, 240, 245);            
        }
        private void SplashScreenLoginOnGotFocus(object sender, EventArgs eventArgs)
        {
            if (splashScreenLogin.Text == "VPN Username")
                splashScreenLogin.Text = string.Empty;
        }

        private void SplashScreenPasswordOnGotFocus(object sender, EventArgs eventArgs)
        {
            if (splashScreenPassword.Text == "VPN Password")
            {
                splashScreenPassword.UseSystemPasswordChar = true;
                splashScreenPassword.Text = string.Empty;
            }
        }

        private void SplashScreen_Shown(object sender, EventArgs e)
        {
            var mf = frmMain.Instance ?? new frmMain();
            if (mf.areSettingsSet())
            {
                startButton.Hide();
                mf.Show();
                Hide();
            } else
            {

            }        
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            isTryingToConnect = true;
            string username = splashScreenLogin.Text;
            string password = splashScreenPassword.Text;
            splashScreenLogin.Enabled = false;
            splashScreenPassword.Enabled = false;

            var valid = true;
            if (string.IsNullOrWhiteSpace(username) || username == "VPN Username")
            {
                MetroMessageBox.Show(this, "Login field cannot be empty", "Login empty!", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                valid = false;
            }
            if (splashScreenLogin.Text.Contains('@'))
            {
                MetroMessageBox.Show(this, "Login field cannot contain '@' sign", "Login invalid!",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                valid = false;
            }
            if (string.IsNullOrWhiteSpace(password) || password == "VPN Password")
            {
                MetroMessageBox.Show(this, "Password field cannot be empty", "Password empty!", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                valid = false;
            }
            if (!valid)
            {
                isTryingToConnect = false;
                splashScreenLogin.Enabled = true;
                splashScreenPassword.Enabled = true;
                return;
            }

            startButton.Enabled = false;

            Task.Run(async () =>
            {
                var apiResponse = await AuthApi.Authorize(username, password);

                Invoke(new Action(() =>
                {
                    switch (apiResponse)
                    {
                        case AuthApiResponse.Success:
                            saveUserAndPassword(username, password);
                            var newMain = new frmMain();
                            newMain.Show();
                            Hide();
                            break;
                        case AuthApiResponse.InvalidCredentials:
                            MetroMessageBox.Show(this, "Login and/or password is invalid", "Invalid credentials!",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        case AuthApiResponse.AccountDisabled:
                            MetroMessageBox.Show(this, "Your account has been disabled", "Account disabled!",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        case AuthApiResponse.ServerError:
                            MetroMessageBox.Show(this, "An error has occurred, please try again later",
                                "Internal error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                    }
                    startButton.Enabled = true;
                    isTryingToConnect = false;
                    splashScreenLogin.Enabled = true;
                    splashScreenPassword.Enabled = true;
                }));
            });
        }

        private void saveUserAndPassword(string username, string password)
        {
            string login = username;
            string pass = password;
            string encryptedPassword = Encrypter.Encrypt(pass, "EENGINEEFEKEYasdasdasdasdasdas");
            Settings.Default.userLoginTXT = login;
            Settings.Default.userPasswordTXT = encryptedPassword;
            Settings.Default.userL2TPPassphraseTXT = "s3CuREpaSs412";
            Settings.Default.Save();
        }

        private void placeholderAdd(object sender, EventArgs e)
        {
            if (isTryingToConnect)
                return;

            if (string.IsNullOrWhiteSpace(splashScreenLogin.Text))
            {
                splashScreenLogin.Text = "VPN Username";
            }
            if (string.IsNullOrWhiteSpace(splashScreenPassword.Text))
            {
                splashScreenPassword.UseSystemPasswordChar = false;
                splashScreenPassword.Text = "VPN Password";
            }
        }

        private void getVPNAcc_Click(object sender, EventArgs e)
        {
            ProcessStartInfo getAcc = new ProcessStartInfo("https://www.smartdnsproxy.com/SignUp");
            Process.Start(getAcc);
        }

        private void SplashScreen_Load(object sender, EventArgs e)
        {
            Process[] theProcesses = System.Diagnostics.Process.GetProcessesByName(Application.ProductName);
            if (theProcesses.Length > 1)
            {
                MetroMessageBox.Show(this,"The application is already running.", Application.ProductName,
                   MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Application.Exit();
            }
        }

        private void SplashScreen_FormClosed(object sender, FormClosedEventArgs e)
        {
            Invoke(new Action(() => Environment.Exit(0)));
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_SHOWME && !frmMain.Instance.areSettingsSet())
            {
                ShowMe();
            }
            base.WndProc(ref m);
        }

        private void ShowMe()
        {
            if (Instance != this)
                return;
            SplashScreen.Instance.Show();
            if (WindowState == FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Normal;
            }
            TopMost = true;
            TopMost = false;
            MetroMessageBox.Show(this, "Smart DNS Proxy VPN Client is already running");
        }
    }
}
