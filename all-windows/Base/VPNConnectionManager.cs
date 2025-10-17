using DotRas;
using SmartDNSProxy_VPN_Client.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace SmartDNSProxy_VPN_Client
{
    class VPNConnectionManager
    {
        private readonly IVPNView view;
        private Ras rasDriver;
        private readonly OpenVPN openVPNDriver;
        private readonly NetworkManagment networkManagment;
        private bool openVPNConnected;
        private bool setNewDNS;
        private Dictionary<string, string[]> userCustomDnsSettings;

        public VPNConnectionManager(IVPNView view, OpenVPN openVPNDriver, NetworkManagment networkManagment) {
            this.view = view;
            this.openVPNDriver = openVPNDriver;
            this.networkManagment = networkManagment;
            userCustomDnsSettings = new Dictionary<string, string[]>();
        }

        public bool isOpenVPNConnected()
        {
            return openVPNDriver.isConnected();
        }

        public void connectToVPN(string selectedProtocol, ServerInformation selectedServer, string entryName,
            string login, string password, string L2TPpassword)
        {
            Task.Run(() =>
            {
                updateState("connecting");

                switch (selectedProtocol)
                {
                    case "PPTP":
                    case "L2TP":
                    case "SSTP":
                        if (rasDriver != null)
                        {
                            rasDriver.disconnect(entryName);
                            rasDriver.deleteVPNEntry(entryName);
                        }
                        resetNetworkStack();
                        rasDriver = new Ras();
                        Enum.TryParse(selectedProtocol, out StandardVpnProtocol protocol);
                        ConnectUsingStandardProcotol(selectedServer, entryName, login, password, protocol,
                            L2TPpassword);
                        setDNSAddress(selectedServer, true);
                        setNewDNS = true;
                        break;
                    case "OpenVPN":
                        resetNetworkStack();
                        openVPNDriver.disconnect();
                        OpenVPNCon(selectedServer, login, password);
                        break;
                    default:
                        VPNDisconnect(selectedProtocol, entryName); //TODO nie mam pojącia po co to
                        break;
                }
            });
        }

        private void OpenVPNCon(ServerInformation selectedServer, string login, string password)
        {
            Task.Run(() =>
            {
                var connectionInfo = new OpenVPNConnectionInfo
                {
                    username = login,
                    password = password,
                    protocol = Settings.Default.selectSettingsProtocol != ""
                        ? Settings.Default.selectSettingsProtocol
                        : "tcp",
                    port = Settings.Default.selectOptionPort != ""
                        ? int.Parse(Settings.Default.selectOptionPort)
                        : 443,
                    host = !string.IsNullOrWhiteSpace(Settings.Default.selectedServerDns)
                        ? Settings.Default.selectedServerDns
                        : selectedServer.DnsAddress
                };
                openVPNDriver.connect(connectionInfo, () => afterOpenVPNConnected(selectedServer));
            });
        }
        private void afterOpenVPNConnected(ServerInformation selectedServer)
        {
            string openVPNLogPath = string.Format(@"{0}\{1}", openVPNDriver.openVPNPath, openVPNDriver.logFileName);
            if (File.Exists(openVPNLogPath))
            {
                Stream stream = File.Open(openVPNLogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader streamReader = new StreamReader(stream);
                string str = streamReader.ReadToEnd();
                view.addToLog("OpenVPN", str);
                streamReader.Dispose();
                stream.Dispose();
            }
            if (Process.GetProcessesByName("openvpn").Length > 0)
            {
                openVPNConnected = true;

                if (!setNewDNS)
                {
                    setDNSAddress(selectedServer, true);
                    setNewDNS = true;
                }
            }
            else
            {
                if (openVPNConnected)
                {
                    try
                    {
                        File.Delete(openVPNDriver.certificateFile);
                        File.Delete(openVPNDriver.passwordFile);
                        //checkInternetConnection = true;
                    }
                    catch (Exception ex)
                    {
                        var error = ex;
                    }
                }
                openVPNConnected = false;
                updateState("connecting");
            }
        }

        private void setDNSWithOpenVPN(bool connection)
        {
            if (connection)
            {
                foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.NetworkInterfaceType != NetworkInterfaceType.Wireless80211 &&
                        ni.NetworkInterfaceType != NetworkInterfaceType.Ethernet)
                        continue;

                    var adapterProperties = ni.GetIPProperties();

                    try
                    {
                        var key = Registry.GetValue(
                            $"HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces\\{ni.Id}",
                            "NameServer", null);
                        if (string.IsNullOrEmpty((string) key))
                            continue;
                    }
                    catch
                    {
                        continue;
                    }

                    if (!userCustomDnsSettings.ContainsKey(ni.Id))
                    {
                        var dnsServers = adapterProperties.DnsAddresses;

                        var ipv4Dns = (from ip in dnsServers
                            where ip.AddressFamily == AddressFamily.InterNetwork
                            select ip.ToString()).ToArray();

                        if (ipv4Dns.Length == 0)
                        {
                            ipv4Dns = null;
                        }
                        else if (ipv4Dns.Length == 1)
                        {
                            var temp = ipv4Dns.ToList();
                            temp.Add(ipv4Dns[0]);
                            ipv4Dns = temp.ToArray();
                        }

                        userCustomDnsSettings.Add(ni.Id, ipv4Dns);
                    }

                    networkManagment.setDNS(ni.Name, "0.0.0.0", "0.0.0.0");
                }
            }
            else
            {
                foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.NetworkInterfaceType != NetworkInterfaceType.Wireless80211 &&
                        ni.NetworkInterfaceType != NetworkInterfaceType.Ethernet)
                        continue;

                    if (userCustomDnsSettings.ContainsKey(ni.Id) && userCustomDnsSettings[ni.Id] != null)
                    {
                        var dnsIps = userCustomDnsSettings[ni.Id];
                        networkManagment.setDNS(ni.Name, dnsIps[0], dnsIps[1]);
                    }
                    else
                    {
                        networkManagment.setDNS(ni.Name, "8.8.8.8", "8.8.4.4", true);
                    }
                }
            }
        }
        private void setDNSAddress(ServerInformation selectedServer, bool setOpenVPN)
        {
            var dnsAddress = selectedServer.ip;
            var smartyVPN = selectedServer.smartVPN;
            if (setOpenVPN)
            {
                setDNSWithOpenVPN(true);

                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.NetworkInterfaceType != NetworkInterfaceType.Wireless80211 && nic.NetworkInterfaceType != NetworkInterfaceType.Ethernet)
                        continue;

                    if (smartyVPN.ToLower() == "no")
                    {
                        networkManagment.setDNS(nic.Name, "208.67.222.222", "208.67.220.220");
                    }
                    else if (!dnsAddress.Any(x => x.Length > 0))
                    {
                        networkManagment.setDNS(nic.Name, "8.8.8.8", "8.8.4.4");
                    }
                    else if (dnsAddress[0].Length > 0 && dnsAddress[1].Length > 0)
                    {
                        networkManagment.setDNS(nic.Name, dnsAddress[0], dnsAddress[1]);
                    }
                    else
                    {
                        networkManagment.setDNS(nic.Name, dnsAddress[0], "");
                    }
                }
            }
        }

        private void resetNetworkStack()
        {
            var resetProcess =
                new Process
                {
                    StartInfo =
                    {
                        WindowStyle = ProcessWindowStyle.Hidden,
                        FileName = Environment.ExpandEnvironmentVariables("%SystemRoot%") + @"\System32\cmd.exe",
                        Verb = "runas",
                        Arguments = "netsh winsock reset catalog & netsh int ipv4 reset reset.log"
                    }
                };
            resetProcess.Start();
        }

        private void ConnectUsingStandardProcotol(ServerInformation selectedServer, string entry, string login,
            string password, StandardVpnProtocol vpnProtocol, string L2TPPassword = null)
        {
            rasDriver.Connect(entry,
                selectedServer.DnsAddress,
                login,
                password,
                dialerStateChanged,
                vpnProtocol,
                L2TPPassword);
        }

        private void dialerStateChanged(string dialerState)
        {
            Debug.WriteLine(dialerState);
            view.Invoke(new Action(() => view.addToLog("PPTPL2TPSSTP", dialerState)));
            view.Invoke(new Action(() => view.updateState(dialerState)));
        }

        public void VPNDisconnect(string protocol, string entryName, bool notifyUi = true)
        {
            Task.Run(() =>
            {
                switch (protocol)
                {
                    case "SSTP":
                    case "L2TP":
                    case "PPTP":
                        rasDriver?.disconnect(entryName);
                        rasDriver?.deleteVPNEntry(entryName);
                        rasDriver = null;
                        setNewDNS = false;
                        setDNSWithOpenVPN(false);
                        break;
                    case "OpenVPN":
                        openVPNDriver.disconnect();
                        setNewDNS = false;
                        setDNSWithOpenVPN(false);
                        openVPNConnected = false;
                        break;
                }
                if (notifyUi)
                    view.Invoke(new Action(() => { updateState("disconnected"); }));
            });
        }

        public void UpdateConnectionStatistics()
        {
            Task.Run(() =>
            {
                try
                {
                    if (rasDriver == null || rasDriver.connectionHandle == null ||
                        rasDriver.connectionHandle.IsInvalid || rasDriver.connection == null ||
                        rasDriver.connection.GetConnectionStatus().ConnectionState !=
                        RasConnectionState.Connected)
                        return;

                    var bytesReceived = rasDriver.connection.GetConnectionStatistics().BytesReceived;
                    var bytesTransmitted = rasDriver.connection.GetConnectionStatistics().BytesTransmitted;

                    view.UpdateConnectionStatistics(bytesReceived, bytesTransmitted);
                }
                catch
                {
                    Debug.WriteLine("Unable to get connection statistics");
                }
            });
        }

        public bool GetConnectionByEntryName(string entryName)
        {
            return rasDriver?.getConnectionByEntryName(entryName) ?? false;
        }

        public bool IsConnectionActive()
        {
            try
            {
                return rasDriver?.connection != null && !rasDriver.connectionHandle.IsInvalid &&
                       rasDriver.connection.GetConnectionStatus().ConnectionState == RasConnectionState.Connected;
            }
            catch
            {
                return false;
            }
        }

        public bool isConnectionBroken(string startIp, string currentIp)
        {
            if (currentIp == "broken")
            {
                return true;
            }
            try
            {
                var vpnConnected = openVPNConnected;
                var rasConnected = rasDriver?.connection != null &&
                                   rasDriver.connection.GetConnectionStatus().ConnectionState ==
                                   RasConnectionState.Connected;
                var areIpSame = startIp == currentIp;
                return !((vpnConnected || rasConnected) && areIpSame);
            }
            catch (Exception e)
            {
                addToLog("OpenVPN", e.ToString());
                return true;
            }
        }

        private void updateState(string state)
        {
            updateViewState(state);
        }

        private void updateViewState(string state)
        {
            view.Invoke(new Action(() => view.updateState(state)));
        }

        private void addToLog(string log, string message)
        {
            view.Invoke(new Action(() => view.addToLog(log, message)));
        }
    }
}
