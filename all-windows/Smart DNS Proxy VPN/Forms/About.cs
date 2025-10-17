using System;
using System.Diagnostics;
using System.Reflection;
using MaterialSkin.Controls;
namespace SmartDNSProxy_VPN_Client
{
    public partial class About : MaterialForm
    {
        public About()
        {
            InitializeComponent();
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            Version.Text = $"Version: {version.ToString(3)}";
        }

        private void ContactUs_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.smartdnsproxy.com/Contactus");
        }
    }
}