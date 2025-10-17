using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using NetFwTypeLib;
using System.Net;
using System.Net.NetworkInformation;

namespace SmartDNSProxy_VPN_Client
{
    class NetworkManagment
    {
        public void setDNS(string entryname, string dnsPrimary, string dnsSecondary, bool dhcp = false)
        {
            string[] arguments = { };
            if (dhcp)
            {
                arguments = new string[] {
                "interface ip set dns name=\"" + entryname + "\" source=dhcp"};
            }
            else if (dnsPrimary.Length > 0 && dnsSecondary.Length > 0)
            {
                arguments = new string[] {
                "interface ip set dns name=\"" + entryname + "\" source=static addr=none" ,
                "interface ip add dns name=\"" + entryname + "\" addr=" + dnsPrimary + " index=1" ,
                "interface ip add dns name=\"" + entryname + "\" addr=" + dnsSecondary + " index=2"};
            } else
            {
                arguments = new string[] {
                "interface ip set dns name=\"" + entryname + "\" source=static addr=none" ,
                "interface ip add dns name=\"" + entryname + "\" addr=" + dnsPrimary + " index=1"};
            }

            foreach (string arg in arguments)
            {
                ProcessStartInfo procStartInfo = new ProcessStartInfo("netsh", arg);
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;
                var setDnsProcess = Process.Start(procStartInfo);
                setDnsProcess.WaitForExit();
            }
            ProcessStartInfo procFlushDns = new ProcessStartInfo("ipconfig", "/flushdns");
            procFlushDns.RedirectStandardOutput = true;
            procFlushDns.UseShellExecute = false;
            procFlushDns.CreateNoWindow = true;
            var flush = Process.Start(procFlushDns);
            flush.WaitForExit();
           
        }
        public void disableInternetConnections()
        {
            INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(
            Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));            
            firewallPolicy.DefaultOutboundAction[NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_DOMAIN] = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
            firewallPolicy.DefaultOutboundAction[NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PRIVATE] = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
            firewallPolicy.DefaultOutboundAction[NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PUBLIC] = NET_FW_ACTION_.NET_FW_ACTION_BLOCK; 
        }
        public void enableInternetConnections()
        {
            INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(
            Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
            firewallPolicy.DefaultOutboundAction[NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_DOMAIN] = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
            firewallPolicy.DefaultOutboundAction[NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PRIVATE] = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
            firewallPolicy.DefaultOutboundAction[NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PUBLIC] = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
        }
        public void allowSmartDNSProxyApp()
        {
            var allowSmartDNSRule = (INetFwRule) Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
            allowSmartDNSRule.Name = "Allow SmartDNS Proxy";
            allowSmartDNSRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
            allowSmartDNSRule.Description = "SmartDNS Proxy Killswitch monitor connection";
            allowSmartDNSRule.ApplicationName = AppDomain.CurrentDomain.BaseDirectory + @"SmartDNSProxy VPN Client.exe";
            allowSmartDNSRule.InterfaceTypes = "All";
            allowSmartDNSRule.Enabled = true;
            allowSmartDNSRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT;

            var allowOpenVPNRule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
            allowOpenVPNRule.Name = "Allow OpenVPN Client";
            allowOpenVPNRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
            allowOpenVPNRule.Description = "SmartDNS Proxy Killswitch monitor connection";
            allowOpenVPNRule.ApplicationName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OpenVPN", "openvpn.exe");
            allowOpenVPNRule.InterfaceTypes = "All";
            allowOpenVPNRule.Enabled = true;
            allowOpenVPNRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT;

            var firewallPolicy = (INetFwPolicy2) Activator.CreateInstance(
                Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
            firewallPolicy.Rules.Add(allowSmartDNSRule);
            firewallPolicy.Rules.Add(allowOpenVPNRule);
        }
        public void removeSmartDNSRule()
        {
            INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(
            Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));            
            firewallPolicy.Rules.Remove("Allow SmartDNS Proxy");
            firewallPolicy.Rules.Remove("Allow OpenVPN Client");
        }

        public bool isFirewallEnabled()
        {
            try
            {
                var mgr = (INetFwMgr) Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwMgr"));
                return mgr.LocalPolicy.CurrentProfile.FirewallEnabled;
            }
            catch
            {
                return false;
            }
        }

        public void changeDNSOnExit(object sender, EventArgs e)
        {
            NetworkManagment netManagement = new NetworkManagment();
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    netManagement.setDNS(ni.Name, "", "", true);
                }
            }
        }
    }
}
