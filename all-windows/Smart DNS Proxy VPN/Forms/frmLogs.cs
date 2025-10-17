using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MaterialSkin.Controls;
using MetroFramework;

namespace SmartDNSProxy_VPN_Client
{
    public partial class frmLogs : MaterialForm
    {
        public frmLogs()
        {
            InitializeComponent();
            MaximumSize = new Size(419, 440);
            MinimumSize = new Size(419, 440);
            SizeGripStyle = SizeGripStyle.Hide;
        }

        public void addToLog(string protocol, string log)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new System.Action(delegate ()
                {
                    this.addToLog(protocol, log);
                }
                ));
            }
            else
            {
                switch (protocol)
                {
                    case "PPTPL2TPSSTP":
                        txtPPTPL2TPSSTPLog.Text += log + Environment.NewLine;
                        txtPPTPL2TPSSTPLog.SelectionStart = txtPPTPL2TPSSTPLog.Text.Length;
                        txtPPTPL2TPSSTPLog.ScrollToCaret();
                        break;
                    case "OpenVPN":
                        txtOpenVPNLog.Text = log;
                        txtOpenVPNLog.SelectionStart = txtOpenVPNLog.Text.Length;
                        txtOpenVPNLog.ScrollToCaret();
                        break;
                    default:
                        txtPPTPL2TPSSTPLog.Text = "No protocol (frmLogs.cs)";
                        txtOpenVPNLog.Text = "No protocol (frmLogs.cs)";
                        break;
                }
            }
        }
        public void selectTabIndex(int index)
        {
            selectedIndex(index);
        }

        void selectedIndex(int index)
        {
            if (tabControlLog.InvokeRequired)
            {
                tabControlLog.Invoke(new Action<int>(selectedIndex), index);
            }
            else
            {
                tabControlLog.SelectedIndex = index;
            }
        }
        

        private void frmLogs_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
            }
        }

        private void copyToClipboardBtn_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();
            var selectedTab = tabControlLog.SelectedTab;
            if (selectedTab.Text.IndexOf("PPTP") != -1)
            {
                Clipboard.SetText(txtPPTPL2TPSSTPLog.Text);
            }
            else
            {
                Clipboard.SetText(txtOpenVPNLog.Text);
            }
            MetroMessageBox.Show(this,"Log text was coppied to clipboard.", "Log copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
