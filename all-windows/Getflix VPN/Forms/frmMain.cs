using System.Drawing;
using MaterialSkin;
using MetroFramework;

namespace SmartDNSProxy_VPN_Client
{
    public partial class frmMain
    {
        private const string VpnIntroductionLink =
            "http://support.smartdnsproxy.com/customer/en/portal/articles/1905934-vpn-introduction";

        private const string VpnKillSwitchSupportLink =
            "http://support.smartdnsproxy.com/customer/portal/articles/2905592-vpn-kill-switch";

        private const string UpdateUrl = "http://network.glbls.net/app/gf/windows_update.txt";

        private const string UpdateAvailableTitle = "Getflix VPN Client - new version";

        private const string UpdateAvailableMessage =
            "There is a new version of Getflix VPN Client available. Do you want to update?";

        private const string AutostartTaskName = "Getflix VPN Client";
        private const string AutostartTaskDescription = "Windows autostart - Getflix VPN Client";

        private const string ClientAlreadyRunningMessage = "Getflix VPN Client is already running";

        private const string KillSwitchNotificationTitle = "Getflix VPN Client Notification";

        private const string TrayIconTitle = "Getflix VPN Client";
        private const string TrayIconNotificationTitle = "Getflix VPN Client information:";

        private void InitializeMaterialSkin()
        {
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.ACTION_BAR_TEXT_SECONDARY = Color.FromArgb(153, 0, 0, 0);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.ColorScheme = new ColorScheme(Primary.VpnGray, Primary.VpnGray, Primary.VpnGray,
                Accent.VpnOrange2, TextShade.BLACK);

            connectingSpinner.Style = MetroColorStyle.Orange;
            checkboxRunAtWindowsStart.Style = MetroColorStyle.Orange;
            checkBoxKillSwitch.Style = MetroColorStyle.Orange;
            checkboxAutoReconnect.Style = MetroColorStyle.Orange;
        }
    }
}