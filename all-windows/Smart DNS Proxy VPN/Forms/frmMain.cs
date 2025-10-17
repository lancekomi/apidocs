using MaterialSkin;

namespace SmartDNSProxy_VPN_Client
{
    public partial class frmMain
    {
        private const string VpnIntroductionLink =
            "http://support.smartdnsproxy.com/customer/en/portal/articles/1905934-vpn-introduction";

        private const string VpnKillSwitchSupportLink =
            "http://support.smartdnsproxy.com/customer/portal/articles/2905592-vpn-kill-switch";

        private const string UpdateUrl = "http://network.glbls.net/app/sdp/windows_update.txt";

        private const string UpdateAvailableTitle = "SmartyDNSProxy - VPN Client - new version";

        private const string UpdateAvailableMessage =
            "There is a new version of SmartDNSProxy available. Do you want to update?";

        private const string AutostartTaskName = "Smart DNS Proxy VPN Client";
        private const string AutostartTaskDescription = "Windows autostart - Smart DNS Proxy VPN Client";

        private const string ClientAlreadyRunningMessage = "Smart DNS Proxy VPN Client is already running";

        private const string KillSwitchNotificationTitle = "Smart DNS Proxy VPN Client Notification";

        private const string TrayIconTitle = "Smart DNS Proxy VPN Client";
        private const string TrayIconNotificationTitle = "Smart DNS Proxy VPN Client information:";

        private void InitializeMaterialSkin()
        {
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.ColorScheme = new ColorScheme(Primary.VpnBlue, Primary.VpnBlue, Primary.VpnBlueLight,
                Accent.VpnOrange, TextShade.WHITE);
        }
    }
}