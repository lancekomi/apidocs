using System.Linq;

using DotRas;
using System;
using System.Diagnostics;
using System.Net;

#pragma warning disable CS0618 // Type or member is obsolete
namespace SmartDNSProxy_VPN_Client
{
    class Ras
    {
        RasPhoneBook _phoneBook;
        RasConnection _connection;
        RasHandle _connectionHandle;


        public class VPNType
        {
            public RasVpnStrategy VPNStrategy { get; set; }

            public string VPNText { get; set; }
        }

        public Ras()
        {
            dialer = new RasDialer();

            _phoneBook = new RasPhoneBook();
            _phoneBook.Open();
        }

        public RasDialer dialer { get; }

        public RasHandle connectionHandle
        {
            get
            {
                return _connectionHandle;
            }
            set
            {
                _connectionHandle = value;
            }
        }

        public RasConnection connection
        {
            get
            {
                try
                {
                    return _connectionHandle != null ? RasConnection.GetActiveConnectionByHandle(_connectionHandle) : null;
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                _connection = value;
            }
        }

        private RasEntry createVpnEntry(string entryName, string host, StandardVpnProtocol vpnProtocol,
            string preSharedKey = null)
        {
            if (_phoneBook.Entries.Contains(entryName))
                return null;

            RasEntry entry = null;
            switch (vpnProtocol)
            {
                case StandardVpnProtocol.PPTP:
                    entry = RasEntry.CreateVpnEntry(entryName, host, RasVpnStrategy.PptpOnly,
                        RasDevice.GetDeviceByName("(PPTP)", RasDeviceType.Vpn));
                    break;
                case StandardVpnProtocol.L2TP:
                    entry = RasEntry.CreateVpnEntry(entryName, host, RasVpnStrategy.L2tpOnly,
                        RasDevice.GetDeviceByName("(L2TP)", RasDeviceType.Vpn));
                    entry.Options.UsePreSharedKey = true;
                    break;
                case StandardVpnProtocol.SSTP:
                    entry = RasEntry.CreateVpnEntry(entryName, host, RasVpnStrategy.Default,
                        RasDevice.GetDeviceByName("(SSTP)", RasDeviceType.Vpn));
                    break;
            }

            if (_phoneBook.Entries.Contains(entry))
                return null;

            _phoneBook.Entries.Add(entry);
            if (vpnProtocol == StandardVpnProtocol.L2TP)
                entry.UpdateCredentials(RasPreSharedKey.Client, preSharedKey);

            return entry;
        }

        // Disconnect current connection
        public void disconnect()
        {
            dialer.DialAsyncCancel();
            connection?.HangUp();
        }

        // Get connection by entry name then disconnect
        public void disconnect(string entryName)
        {
            if (!isConnected(entryName))
                return;

            dialer.DialAsyncCancel();
            connection?.HangUp();
        }

        public bool getConnectionByEntryName(string entryName)
        {
            _connection =
                RasConnection.GetActiveConnectionByName(entryName,
                    RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.AllUsers));
            if (_connection != null)
                _connectionHandle = _connection.Handle;
            return _connection != null;
        }

        public bool isConnected(string entryName)
        {
            getConnectionByEntryName(entryName);

            if (_connectionHandle == null)
                return false;
            return !_connectionHandle.IsClosed;
        }

        public bool deleteVPNEntry(string entryName)
        {
            try
            {
                return _phoneBook.Entries.Contains(entryName) && !dialer.IsBusy &&
                    _phoneBook.Entries.First(x => x.Name == entryName).Remove();
            }
            catch
            {
                _phoneBook = new RasPhoneBook();
                _phoneBook.Open();
                return false;
            }
        }

        public RasHandle Connect(string entryName, string host, string username, string password,
            Action<string> returnDialerState, StandardVpnProtocol vpnProtocol, string preSharedKey = null)
        {
            createVpnEntry(entryName, host, vpnProtocol, preSharedKey);

            return connectToStandardVPN(entryName, username, password, returnDialerState);
        }

        private RasHandle connectToStandardVPN(string entryName, string username, string password,
            Action<string> returnDialerState)
        {
            dialer.EntryName = entryName;
            dialer.StateChanged += (sender, eventArgs) => returnDialerState(eventArgs.State.ToString());
            dialer.PhoneBookPath = RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.AllUsers);
            try
            {
                dialer.Credentials = new NetworkCredential(username, password);
                _connectionHandle = dialer.DialAsync();
                return _connectionHandle;
            }
            catch
            {
                return null;
            }
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete
}
