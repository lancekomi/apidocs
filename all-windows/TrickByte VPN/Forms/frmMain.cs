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

        private const string UpdateUrl = "http://network.glbls.net/app/tb/windows_update.txt";

        private const string UpdateAvailableTitle = "TrickByte VPN Client - new version";

        private const string UpdateAvailableMessage =
            "There is a new version of TrickByte VPN Client available. Do you want to update?";

        private const string AutostartTaskName = "TrickByte VPN Client";
        private const string AutostartTaskDescription = "Windows autostart - TrickByte VPN Client";

        private const string ClientAlreadyRunningMessage = "TrickByte VPN Client is already running";

        private const string KillSwitchNotificationTitle = "TrickByte VPN Client Notification";

        private const string TrayIconTitle = "TrickByte VPN Client";
        private const string TrayIconNotificationTitle = "TrickByte VPN Client information:";

        private void InitializeMaterialSkin()
        {
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.ColorScheme = new ColorScheme(Primary.VpnGreen, Primary.VpnGreen, Primary.VpnGreen,
                Accent.VpnBlack, TextShade.BLACK);

            connectingSpinner.Style = MetroColorStyle.Green;
            checkboxRunAtWindowsStart.Style = MetroColorStyle.Green;
            checkBoxKillSwitch.Style = MetroColorStyle.Green;
            checkboxAutoReconnect.Style = MetroColorStyle.Green;
        }
    }
}