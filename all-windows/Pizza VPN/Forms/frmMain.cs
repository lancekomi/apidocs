using System.Drawing;
using MaterialSkin;
using MetroFramework;

namespace SmartDNSProxy_VPN_Client
{
    public partial class frmMain
    {
        private const string VpnIntroductionLink =
            "https://pizzavpn.com/setup";

        private const string VpnKillSwitchSupportLink =
            "http://support.smartdnsproxy.com/customer/portal/articles/2905592-vpn-kill-switch";

        private const string UpdateUrl = "http://network.glbls.net/app/pv/windows_update.txt";

        private const string UpdateAvailableTitle = "Pizza VPN Client - new version";

        private const string UpdateAvailableMessage =
            "There is a new version of Pizza VPN Client available. Do you want to update?";

        private const string AutostartTaskName = "Pizza VPN Client";
        private const string AutostartTaskDescription = "Windows autostart - Pizza VPN Client";

        private const string ClientAlreadyRunningMessage = "Pizza VPN Client is already running";

        private const string KillSwitchNotificationTitle = "Pizza VPN Client Notification";

        private const string TrayIconTitle = "Pizza VPN Client";
        private const string TrayIconNotificationTitle = "Pizza VPN Client information:";

        private void InitializeMaterialSkin()
        {
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.ColorScheme = new ColorScheme(Primary.Grey900, Primary.Grey900, Primary.Grey900,
                Accent.VpnWhite, TextShade.WHITE);

            connectingSpinner.Style = MetroColorStyle.Orange;
            checkboxRunAtWindowsStart.Style = MetroColorStyle.Orange;
            checkBoxKillSwitch.Style = MetroColorStyle.Orange;
            checkboxAutoReconnect.Style = MetroColorStyle.Orange;
        }
    }
}