using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using DotRas;
using MaterialSkin.Controls;
using MetroFramework;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using SmartDNSProxy_VPN_Client.Properties;
using Action = System.Action;
using Task = System.Threading.Tasks.Task;
using Timer = System.Timers.Timer;

namespace SmartDNSProxy_VPN_Client
{
    public partial class frmMain : MaterialForm, IOpenVPN, IVPNView
    {
        private string entryName = "";
        public NotifyIcon trayIcon = new NotifyIcon();
        private readonly NotifyIcon notifyIcon = new NotifyIcon();
        private readonly NetworkManagment networkManagment = new NetworkManagment();
        public ContextMenu trayMenu = new ContextMenu();
        public string connectedAddressIp;
        public List<Country> countries;
        private OpenVPN openVPN;
        private ServerInformation[] serverList;
        private frmUpdater frmUpdater = new frmUpdater();
        private readonly frmLogs frmLogs = new frmLogs();
        private readonly Drivers drivers = new Drivers();
        private About about;
        private readonly AutoReconnect AReconnect = new AutoReconnect();
        private readonly Font font = new Font("Roboto", 10F);
        private readonly Timer timerConnectionAttempt = new Timer(1000);
        private string vpnState = "Disconnected";
        public int connCounter;
        private Timer checkConTimer;
        private List<string> ports = new List<string>();
        private readonly List<string> settingsProtocols = new List<string>();
        private readonly List<string> protocols = new List<string>();
        public bool shouldConnectionBeEstablished;
        private RasConnectionWatcher connectionWatcher;
        private string selectedProtocol;
        public bool isConnected;
        private string VPNConnectionState;
        private VPNConnectionManager connector;
        private bool isOpenVPNTryingToReconnect;
        private bool killSwitchNotificationShown;
        private bool hasConnectionBeenEstablished;
        private bool isConnectionWatchDogTickInProgress;
        private Timer connectionWatchDog;
        public static frmMain Instance { get; private set; }

        public frmMain()
        {
            InitializeComponent();
            Instance = this;
            InitializeMaterialSkin();
            loadToList();

            killswitchInfo.Hide();
            if (!networkManagment.isFirewallEnabled())
            {
                checkBoxKillSwitch.Enabled = false;
                killswitchInfo.Show();
            }

            notifyIcon.BalloonTipText = " ";
            notifyIcon.BalloonTipTitle = " ";
        }

        private void setKillSwitchEnabled(bool enabled)
        {
            if(!enabled)
            {
                networkManagment.disableInternetConnections();
                networkManagment.allowSmartDNSProxyApp();
            }
            else
            {
                networkManagment.enableInternetConnections();
                networkManagment.removeSmartDNSRule();
            }
        } 

        private void addConnectionWatcher()
        {
            if (connectionWatcher != null)
            {
                connectionWatcher.EnableRaisingEvents = false;
                connectionWatcher.Dispose();
            }
            connectionWatcher = new RasConnectionWatcher();
            connectionWatcher.BeginInit();
            connectionWatcher.Disconnected += (sender, e) =>
            {
                isConnected = false;
                if (shouldConnectionBeEstablished && Settings.Default.checkboxAutoReconnect &&
                    !Settings.Default.checkboxKillSwitch)
                    reconnectAfterGivenTime(Configuration.autoreconnectTimeInterval);
            };
            connectionWatcher.Connected += (sender, e) => { isConnected = true; };
            connectionWatcher.EnableRaisingEvents = true;
            connectionWatcher.EndInit();
        }

        private void loadToList()
        {
            settingsProtocols.Add("tcp");
            settingsProtocols.Add("udp");

            protocols.Add("OpenVPN");
            protocols.Add("L2TP");
            protocols.Add("PPTP");
            protocols.Add("SSTP");
        }

        private void setDefaultSettings()
        {
            foreach (Country item in selectCountryList.Items)
            {
                if (item.Name == Settings.Default.lastCountry) selectCountryList.SelectedIndex = selectCountryList.Items.IndexOf(item);
            }
            selectServerList.SelectedIndex = selectServerList.FindStringExact(Settings.Default.lastServer);
            selectProtocol.SelectedIndex = selectProtocol.FindStringExact(Settings.Default.lastConnectionMethod);

            selectSettingsPort.SelectedItem = Settings.Default.selectOptionPort;
            checkboxAutoReconnect.Checked = Settings.Default.checkboxAutoReconnect;
            checkBoxKillSwitch.Checked = Settings.Default.checkboxKillSwitch;
            checkboxRunAtWindowsStart.Checked = Settings.Default.checkboxRunAtWindowsStart;

            setKillSwitchEnabled(!checkBoxKillSwitch.Checked);
            selectProtocol_SelectedIndexChanged(null, null);
        }

        public void checkUpdates()
        {
            CheckUpdates checkupdate = new CheckUpdates(UpdateUrl);
            var currentVersion = new Version(getCurrentAssemblyVersion());
            var updateVersion = new Version(checkupdate.version.ToString());
            var result = currentVersion.CompareTo(updateVersion);
            if (result < 0)
            {
                Invoke(new Action(() =>
                {
                    var dialogResult = MetroMessageBox.Show(this, UpdateAvailableMessage, UpdateAvailableTitle,
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    if (dialogResult == DialogResult.OK)
                    {
                        var downloadLink = checkupdate.newdownloadlink;
                        frmUpdater = new frmUpdater();
                        frmUpdater.downloadedPath = downloadLink;
                        frmUpdater.Show();
                        frmUpdater.runUpdate();
                    }
                    else
                    {
                        trayIcon.Dispose();
                        Application.Exit();
                    }
                }));
            }
        }
       
        public void requestReconnect()
        {
            setKillSwitchEnabled(true);
            this.Invoke(new Action(VPNConnect));
        }

        public void reconnectAfterGivenTime(int givenTime)
        {
            var isAutoReconnectEnabled = Settings.Default.checkboxAutoReconnect;
            if (!isAutoReconnectEnabled)
            {
                Invoke(new Action(() => updateState("disconnected")));
                return;
            }

            requestReconnect();
            Thread.Sleep(1000);
            Stopwatch sw = Stopwatch.StartNew();                
            while (true)
            {
                if (sw.ElapsedMilliseconds >= (givenTime - 500))
                {
                    break;
                }
                Thread.Sleep(500);
            }
            sw.Stop();

            if (!isConnected)
                reconnectAfterGivenTime(givenTime);
        }

        private void runOpenVPNAutoReconnect()
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                if (isOpenVPNTryingToReconnect)
                    return;

                isOpenVPNTryingToReconnect = true;
                if (VPNConnectionState != "connected")
                {
                    connector.VPNDisconnect("OpenVPN", entryName, false);
                    VPNConnect();
                }
                Stopwatch vpnAutoreconnectStopwatch = Stopwatch.StartNew();
                while (true)
                {
                    if (vpnAutoreconnectStopwatch.ElapsedMilliseconds >= 10000)
                    {
                        break;
                    }
                }
                vpnAutoreconnectStopwatch.Stop();

                isOpenVPNTryingToReconnect = false;
            });
        }

        private void autoReconnectProtocol(string protocol)
        {
            connector.VPNDisconnect(protocol, entryName, false);
            var countDownEvent = new CountdownEvent(10);
            for (var i = 1; i <= 10; i++)
            {
                countDownEvent.Signal(1);
                countDownEvent.Wait(1000);
            }
            if (countDownEvent.CurrentCount == 0)
            {
                setKillSwitchEnabled(true);
                VPNConnect();
            }
        }

        private string getCurrentAssemblyVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;
            return version;
        }

        private void loadPort()
        {
            selectSettingsPort.Items.Clear();
            foreach(string port in ports)
                selectSettingsPort.Items.Add(port);

            selectSettingsProtocol.Items.Clear();
            foreach(string setProtocol in settingsProtocols)
                selectSettingsProtocol.Items.Add(setProtocol);

            var hasFound = false;
            if (selectSettingsProtocol.Items.Count > 0)
            {
                foreach (var item in selectSettingsProtocol.Items)
                {
                    if (Settings.Default.selectSettingsProtocol == item.ToString())
                    {
                        selectSettingsProtocol.SelectedIndex = selectSettingsProtocol.Items.IndexOf(item);
                        hasFound = true;
                        break;
                    }
                }
                if (!hasFound)
                    selectSettingsProtocol.SelectedIndex = 0;
            }
            if (selectSettingsPort.Items.Count > 0)
            {
                hasFound = false;
                foreach (var item in selectSettingsPort.Items)
                {
                    if (Settings.Default.selectOptionPort == item.ToString())
                    {
                        selectSettingsPort.SelectedIndex = selectSettingsPort.Items.IndexOf(item);
                        hasFound = true;
                        break;
                    }
                }
                if (!hasFound)
                    selectSettingsPort.SelectedIndex = 0;
            }
        }

        private void loadOptionsPortAndProtocol()
        {
            loadPort();
            selectProtocol.Items.Clear();
            foreach(string protocol in protocols)
                selectProtocol.Items.Add(protocol);

            switch (Settings.Default.selectProtocol.ToLower())
            {
                default:
                //case "openvpn":
                    selectProtocol.SelectedIndex = 0;
                    break;
                case "l2tp":
                    selectProtocol.SelectedIndex = 1;
                    break;
                case "pptp":
                    selectProtocol.SelectedIndex = 2;
                    break;
                case "sstp":
                    selectProtocol.SelectedIndex = 3;
                    break;
            }
        }

        public void UpdateConnectionStatistics(long bytesReceived, long bytesTransmitted)
        {
            Invoke(new Action(() =>
            {
                var kbIn = bytesReceived / 1024;
                var mbIn = (bytesReceived / 1024) / 1024;
                var gbIn = ((bytesReceived / 1024) / 1024) / 1024;

                var kbOut = bytesTransmitted / 1024;
                var mbOut = (bytesTransmitted / 1024) / 1024;
                var gbOut = ((bytesTransmitted / 1024) / 1024) / 1024;

                labelPPTPL2TPDataInOut.Text = string.Format("{0} / {1}",
                    gbIn > 0 ? gbIn + " GB" : mbIn > 0 ? mbIn + " MB" : kbIn > 0 ? kbIn + " KB" : "0 KB",
                    gbOut > 0 ? gbOut + " GB" : mbOut > 0 ? mbOut + " MB" : kbOut > 0 ? kbOut + " KB" : "0 KB");
            }));
        }

        public void SetPorts(IEnumerable<string> portsToChose)
        {
            ports = new List<string>();
            ports.AddRange(portsToChose);
            Invoke(new Action(() => { loadPort(); }));
        }
        
        public bool areSettingsSet()
        {
            var settings = Settings.Default;
            return !(string.IsNullOrEmpty(settings.userLoginTXT) || string.IsNullOrEmpty(settings.userPasswordTXT));
        }

        public void updateOpenVPNState(string state)
        {
            VPNConnectionState = state;
            switch(state.ToLower())
            {
                case "connected":
                    Invoke((MethodInvoker)delegate {
                        updateState("connected");
                    });
                    checkConTimer?.Stop();
                    break;
                case "wait":
                    if (!Settings.Default.checkboxAutoReconnect)
                    {
                        checkConTimer?.Start();
                        Invoke((MethodInvoker)delegate {
                            updateState("connecting");
                        });
                    }
                    break;
                case "get_config":
                case "resolve":
                case "assign_ip":
                case "resolv":
                case "auth":
                case "add_routes":
                case "reconnecting":
                case "connecting":
                    Invoke((MethodInvoker)delegate {
                        updateState("connecting");
                    });
                    break;
                default:
                    Invoke((MethodInvoker)delegate {
                        updateState("disconnected");
                    });
                    break;

            }
            Debug.WriteLine("OpenVPN State(frmMain): " + state);
        }

        public void saveOpenVPNSettings()
        {
            using (TaskService ts = new TaskService())//Add to Windows Task Scheduler.
            {
                TaskDefinition td = ts.NewTask();
                td.RegistrationInfo.Description = AutostartTaskDescription;
                td.Principal.LogonType = TaskLogonType.S4U;
                string taskName = AutostartTaskName;
                td.Triggers.Add(new LogonTrigger());
                td.Principal.LogonType = TaskLogonType.InteractiveToken;
                td.Principal.RunLevel = TaskRunLevel.Highest;
                td.Actions.Add(new ExecAction(Application.ExecutablePath.ToString(), null, null));
                if (checkboxRunAtWindowsStart.Checked)
                {
                    Properties.Settings.Default.checkboxRunAtWindowsStart = true;
                    ts.RootFolder.RegisterTaskDefinition(taskName, td);
                }
                else
                {
                    try
                    {
                        ts.RootFolder.DeleteTask(taskName);
                    }
                    catch (Exception)
                    {
                        frmLogs.addToLog("OpenVPN", "Autostart was disabled");
                        frmLogs.addToLog("PPTPL2TPSSTP", "Autostart was disabled");
                    }
                    Properties.Settings.Default.checkboxRunAtWindowsStart = false;
                }
            }

            // end
            if (this.selectProtocol.SelectedItem.ToString() == "OpenVPN")
            {
                var protocol = this.selectSettingsProtocol.SelectedItem.ToString();
                var port = this.selectSettingsPort.SelectedItem.ToString();
                Properties.Settings.Default.selectSettingsProtocol = protocol;
                Properties.Settings.Default.selectOptionPort = port;
            }

            Properties.Settings.Default.checkboxRunAtWindowsStart = checkboxRunAtWindowsStart.Checked;
            Properties.Settings.Default.checkboxKillSwitch = checkBoxKillSwitch.Checked;
            Properties.Settings.Default.checkboxAutoReconnect = checkboxAutoReconnect.Checked;

            setKillSwitchEnabled(!checkBoxKillSwitch.Checked);

            Properties.Settings.Default.Save();

            //set registry key to start app whe windows startup
            string name = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            string path = Application.ExecutablePath.ToString();

            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (Properties.Settings.Default.checkboxRunAtWindowsStart)
            {
                AutostartManager.saveFileInAutostart();
                key.SetValue(name, path);
            }
            else
            {
                AutostartManager.removeFileInAutostart();
                key.DeleteValue(name, false);
            }
            // end
        }

        public bool isInvokeRequired()
        {
            return this.InvokeRequired;
        }

        private void VPNConnect()
        {
            Invoke(new Action(() =>
            {
                timerConnectionAttempt.Enabled = true;
                selectedProtocol = !string.IsNullOrWhiteSpace(Settings.Default.selectProtocol)
                    ? Settings.Default.selectProtocol
                    : selectProtocol.SelectedItem.ToString();
                var selectedServer = (ServerInformation) selectServerList.SelectedItem;
                entryName = !string.IsNullOrWhiteSpace(Settings.Default.selectedServerDns)
                    ? Settings.Default.selectedServerDns
                    : selectedServer.DnsAddress;
                connector.connectToVPN(selectedProtocol, selectedServer, entryName,
                    Settings.Default.userLoginTXT,
                    Encrypter.Decrypt(Settings.Default.userPasswordTXT, "EENGINEEFEKEYasdasdasdasdasdas"),
                    Settings.Default.userL2TPPassphraseTXT);
                prepareSystemTrayIcon();
            }));
        }

        private void HangUpAllExistingConnections()
        {
            string path = RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.AllUsers);
            var phoneBook = new RasPhoneBook();
            phoneBook.Open(path);
            foreach (var entry in phoneBook.Entries)
            {
#pragma warning disable 618
                var connection = RasConnection.GetActiveConnectionByName(entry.Name, path);
#pragma warning restore 618
                connection?.HangUp();
            }
        }

        private void LoadServerList()
        {
            ServerListClient serverListClient = new ServerListClient();
            serverList = serverListClient.GetServerList();
        }

        private string getSelectProtocol()
        {
            return selectProtocol.SelectedItem.ToString();
        }

        private void Disconect()
        {
            shouldConnectionBeEstablished = false;
            string protcol = getSelectProtocol();
            timerConnectionAttempt?.Stop();
            timerOpenVPN?.Stop();
            checkConTimer?.Stop();
            connectionWatchDog?.Stop();
            this.connector.VPNDisconnect(protcol, entryName);
        }

        public void addToLog(string log, string message)
        {
            frmLogs.addToLog(log, message);
        }

        public void updateState(string state)
        {
            switch (state.ToLower())
            {
                case "connected":
                    if (checkBoxKillSwitch.Checked)
                    {
                        Task.Run(() =>
                        {
                            var ip = AReconnect.getConnectedIPAddress();
                            while (string.IsNullOrEmpty(ip) || ip == "broken")
                                ip = AReconnect.getConnectedIPAddress();
                            connectedAddressIp = ip;

                            if (!connectionWatchDog.Enabled)
                                connectionWatchDog.Start();
                        });
                    }
                    else
                    {
                        connectionWatchDog.Start();
                    }
                    vpnState = "connected";
                    labelStatus.Left = 169;
                    DnsProxyStatus.Text = "ON";
                    labelStatus.ForeColor = Color.DarkGreen;
                    labelStatus.Text = "Connected";
                    connectingSpinner.Hide();
                    btnConnectVPN.Image = Properties.Resources.disconnectBtn;
                    shouldConnectionBeEstablished = true;
                    hasConnectionBeenEstablished = true;
                    isConnected = true;
                    frmLogs.selectTabIndex(this.selectedProtocol.ToString() == "OpenVPN" ? 1 : 0);
                    if (selectedProtocol != "OpenVPN")
                    {
                        timerDataCount.Start();
                        // ConnectionWatcher throws disconnect event only once, 
                        // so for each connection ConnectionWatcher needs to be recreated.
                        addConnectionWatcher();
                    }
                    timerConnectionAttempt.Stop();
                    networkManagment.enableInternetConnections();
                    networkManagment.removeSmartDNSRule();
                    break;
                case "disconnected":
                    LogOut.Enabled = true;
                    if (Settings.Default.checkboxKillSwitch && !connector.isOpenVPNConnected())
                    {
                        setKillSwitchEnabled(false);
                    }
                    vpnState = "disconnected";
                    labelStatus.Left = 160;
                    labelStatus.Text = "Disconnected";
                    labelStatus.ForeColor = Color.Maroon;
                    timerDataCount.Stop();
                    DnsProxyStatus.Text = "OFF";
                    connectingSpinner.Hide();
                    btnConnectVPN.Image = Properties.Resources.connectBtn;
                    hasConnectionBeenEstablished = false;
#if PIZZAVPN
                    pizzaConnectLabel.Text = "Connect now";
                    pizzaConnectLabel.Location = new Point(150, pizzaConnectLabel.Location.Y);
#endif
                    break;
                case "connecting":
                default:
                    vpnState = "connecting";
                    labelStatus.Text = "Connecting";
                    labelStatus.ForeColor = Color.ForestGreen;
                    connectingSpinner.Show();
                    btnConnectVPN.Image = Properties.Resources.disconnectBtn;
                    LogOut.Enabled = false;
#if PIZZAVPN
                    pizzaConnectLabel.Text = "Disconnect now";
                    pizzaConnectLabel.Location = new Point(140, pizzaConnectLabel.Location.Y);
#endif
                    break;
            }
        }

        #region Background Workers
        private void VPNClientBackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                LoadServerList();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void VPNClientBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            countries = new List<Country>();
            foreach (var serverInfo in serverList.Where(srv => srv.protocols.Contains(selectProtocol.SelectedItem.ToString())))
            {
                if (countries.Select(cn => cn.Name).Contains(serverInfo.country))
                    continue;

                var flag = (Image)Resources.ResourceManager.GetObject(serverInfo.country.Replace(" ", "_").Replace("-", "_"));
                countries.Add(new Country(flag ?? Resources.Empty, serverInfo.country, font));
            }
            selectCountryList.DisplayImagesAndText(countries);
            setDefaultSettings();
            var selectedServer = selectServerList.SelectedItem;

            entryName = selectedServer.ToString();
            if (connector.GetConnectionByEntryName(entryName))
                updateState("connected");
        }
        #endregion

        #region UI events
        private void frmMain_Load(object sender, EventArgs e)
        {
            loadOptionsPortAndProtocol();

            if (selectProtocol.SelectedItem.ToString() == "OpenVPN")
            {
                materialLabel13.Hide();
                labelPPTPL2TPDataInOut.Hide();
            }

            Settings.Default.trayIconStatus = false;
            Settings.Default.Save();
            prepareSystemTrayIcon();
            trayIcon.Visible = true;

            selectCountryList.SelectedIndexChanged += selectCountryList_SelectedIndexChanged;
            selectServerList.SelectedIndexChanged += selectServerList_SelectedIndexChanged;
            selectProtocol.SelectedIndexChanged += selectProtocol_SelectedIndexChanged;
            selectServerList.MeasureItem += selectServerList_MeasureItem;
            selectServerList.DrawItem += selectServerList_DrawItem;
            timerDataCount.Tick += timerDataCount_Tick;
            btnConnectVPN.Click += btnConnectVPN_Click;
            saveSettings.Click += saveSettings_Click;
            btnLogs.Click += btnLogs_Click;
            Move += frmMain_Move;

            Task.Run(() =>
            {
                openVPN = new OpenVPN();
                openVPN.openVPNDelegate = this;
                connector = new VPNConnectionManager(this, openVPN, networkManagment);

                var backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += VPNClientBackgroundWorker_DoWork;
                backgroundWorker.RunWorkerCompleted += (a, b) => Invoke(new Action(() => VPNClientBackgroundWorker_RunWorkerCompleted(a, b)));
                backgroundWorker.RunWorkerAsync();

                checkConTimer = new System.Timers.Timer();
                checkConTimer.Elapsed += checkConnectionTimer;
                checkConTimer.Interval = Configuration.connectionTimerInterval;

                connectionWatchDog = new System.Timers.Timer();
                connectionWatchDog.Enabled = false;
                connectionWatchDog.Interval = 5000;
                connectionWatchDog.Elapsed += (a, b) => Invoke(new Action(() => connectionWatchDog_Tick(a, b)));

                try
                {
                    checkUpdates();
                }
                catch
                {
                    // Network error
                }

                try
                {
                    drivers.installTAPDrivers();
                }
                catch
                {
                    MetroMessageBox.Show(this, "TAP-Driver folder damaged or non-existing", "Instalation error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                }

                HangUpAllExistingConnections();
                networkManagment.enableInternetConnections();
                networkManagment.removeSmartDNSRule();

                Invoke(new Action(() =>
                {
                    btnConnectVPN.Enabled = true;
                    connectingSpinner.Hide();
                    
                    if (checkboxRunAtWindowsStart.Checked)
                    {
                        btnConnectVPN_Click(null, null);
                    }
                }));
            });
        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal && !IsFormFullyVisible(this))
                CenterToActiveScreen(this);
        }

        private void selectServerList_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemWidth = 50;
            e.ItemHeight = 23;
        }

        private void selectServerList_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            const int MarginWidth = 4;
            const int MarginHeight = 4;

            if (e.Index >= 0)
            {
                float hgt = e.Bounds.Height - 2 * MarginHeight;
                var rect = new RectangleF(e.Bounds.X + MarginWidth, e.Bounds.Y + MarginHeight, e.Bounds.Width + 10, hgt);
                var serverName = this.selectServerList.Items[e.Index].ToString();
                using (StringFormat sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Near;
                    sf.LineAlignment = StringAlignment.Center;
                    e.Graphics.DrawString(serverName, font, Brushes.Black, rect, sf);
                }
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Horrible hack alert
            // If cursor is over "X" button on form hide it, else if
            // i.e. user has right clicked on menu start and selected close window close the app
            var rawMousePosition = Cursor.Position;
            var position = PointToClient(rawMousePosition);
            if (position.X > Width - 40 && position.X < Width + 40 && position.Y < 30)
            {
                e.Cancel = true;
                Settings.Default.trayIconStatus = true;
                Settings.Default.Save();
                prepareSystemTrayIcon();
                Hide();
            }
            else if (e.CloseReason == CloseReason.UserClosing)
            {
                trayExit(sender, null);
            }
        }

        private void frmMain_Move(object sender, EventArgs e)
        {
            Settings.Default.Location = Location;
            Settings.Default.Save();
        }

        private void selectServerList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedServer = (ServerInformation)this.selectServerList.SelectedItem;
            Settings.Default.selectedServerDns = selectedServer.DnsAddress;
            Settings.Default.lastServer = ((ServerInformation) selectServerList.SelectedItem).name;
            Settings.Default.Save();
        }

        private void selectCountryList_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.selectServerList.Items.Clear();
            var selectedCountry = (Country)selectCountryList.SelectedItem;
            var countryName = selectedCountry.Name;
            foreach (ServerInformation server in serverList.Where(srv => srv.country == countryName))
            {
                this.selectServerList.Items.Add(server);
            }
            selectServerList.SelectedIndex = Math.Max(selectServerList.FindStringExact(Settings.Default.lastServer), 0);
            Settings.Default.lastCountry = countryName;
            Settings.Default.Save();
        }

        private void selectProtocol_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (selectProtocol.SelectedItem.ToString() == "PPTP" || selectProtocol.SelectedItem.ToString() == "L2TP")
            {
                this.selectSettingsPort.Items.Clear();
                this.selectSettingsPort.Enabled = false;
                this.selectSettingsProtocol.Items.Clear();
                this.selectSettingsProtocol.Enabled = false;
            }
            else
            {
                this.selectSettingsProtocol.Enabled = true;
                this.selectSettingsPort.Enabled = true;
                loadPort();
            }

            countries.Clear();
            foreach (ServerInformation serverInfo in serverList.Where(srv => srv.protocols.Contains(selectProtocol.SelectedItem.ToString())))
            {
                if (!countries.Select(cn => cn.Name).Contains(serverInfo.country))
                {
                    var flag = (Image)Properties.Resources.ResourceManager.GetObject(serverInfo.country.Replace(" ", "_").Replace("-", "_"));
                    countries.Add(new Country(flag != null ? flag : Properties.Resources.Empty, serverInfo.country, this.font));
                }
            }
            this.selectCountryList.DisplayImagesAndText(countries);

            var hasCountryBeenFound = false;
            foreach (Country item in selectCountryList.Items)
            {
                if (item.Name == Settings.Default.lastCountry)
                {
                    selectCountryList.SelectedIndex = selectCountryList.Items.IndexOf(item);
                    hasCountryBeenFound = true;
                }
            }
            if (!hasCountryBeenFound)
                selectCountryList.SelectedIndex = 0;

            Settings.Default.lastConnectionMethod = selectProtocol.SelectedItem.ToString();
            Settings.Default.Save();
        }

        private void btnLogs_Click(object sender, EventArgs e)
        {
            frmLogs.Show();
            frmLogs.Focus();
        }

        private void LogOut_Click(object sender, EventArgs e)
        {
            Settings.Default.userLoginTXT = null;
            Settings.Default.userPasswordTXT = null;
            Settings.Default.userL2TPPassphraseTXT = null;
            Settings.Default.Save();

            setKillSwitchEnabled(true);

            var splashScreen = new SplashScreen();
            frmLogs.Hide();
            splashScreen.Show();
            splashScreen.Focus();
            trayIcon.Dispose();
            Hide();
        }

        private void btnConnectVPN_Click(object sender, EventArgs e)
        {
            bool isDisconnected = (vpnState.ToLower() == "disconnected");
            if (checkBoxKillSwitch.Checked)
            {
                setKillSwitchEnabled(isDisconnected);
            }
            if (isDisconnected)
            {
                checkConTimer.Start();
                VPNConnect();
            }
            else
            {
                Disconect();
            }
        }

        private void saveSettings_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.selectProtocol = this.selectProtocol.SelectedItem.ToString();
            if (this.selectSettingsProtocol.SelectedItem != null)
            {
                Properties.Settings.Default.selectSettingsProtocol = this.selectSettingsProtocol.SelectedItem.ToString();
            }
            Settings.Default.selectOptionPort = selectSettingsPort.SelectedItem.ToString();
            Properties.Settings.Default.Save();
            saveOpenVPNSettings();
            if (this.selectProtocol.Text == "OpenVPN")
            {
                this.labelPPTPL2TPDataInOut.Hide();
                this.materialLabel13.Hide();
            }
            else
            {
                this.labelPPTPL2TPDataInOut.Show();
                this.materialLabel13.Show();
            }
        }

        private void procotolChoseLink_Click(object sender, EventArgs e)
        {
            ProcessStartInfo getAcc = new ProcessStartInfo(VpnIntroductionLink);
            Process.Start(getAcc);
        }

        private void killSwitchLink_Click(object sender, EventArgs e)
        {
            ProcessStartInfo getAcc = new ProcessStartInfo(VpnKillSwitchSupportLink);
            Process.Start(getAcc);
        }
        #endregion

        #region UI

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_SHOWME && areSettingsSet())
            {
                ShowMe();
            }
            base.WndProc(ref m);
        }

        private void ShowMe()
        {
            if (Instance != this)
                return;
            frmMain.Instance.Show();
            if (WindowState == FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Normal;
            }
            TopMost = true;
            TopMost = false;
            Settings.Default.trayIconStatus = false;
            Settings.Default.Save();
            prepareSystemTrayIcon();
            MetroMessageBox.Show(this, ClientAlreadyRunningMessage);
        }

        private static bool IsFormFullyVisible(Form f)
        {
            return IsPointVisibleOnAScreen(new Point(f.Left, f.Top)) &&
                   IsPointVisibleOnAScreen(new Point(f.Right, f.Top)) &&
                   IsPointVisibleOnAScreen(new Point(f.Left, f.Bottom)) &&
                   IsPointVisibleOnAScreen(new Point(f.Right, f.Bottom));

            bool IsPointVisibleOnAScreen(Point p)
            {
                return Screen.AllScreens.Any(s =>
                    p.X < s.Bounds.Right && p.X > s.Bounds.Left && p.Y > s.Bounds.Top && p.Y < s.Bounds.Bottom);
            }
        }

        private static void CenterToActiveScreen(Form form)
        {
            var workingArea = Screen.FromControl(form).WorkingArea;

            // Some event is interrupting form.SetDesktopLocation, spawning another Task
            // is slow enough for that event to finish.
            Task.Run(() =>
            {
                form.Invoke(new Action(() =>
                {
                    form.SetDesktopLocation(
                        Math.Max(workingArea.X, workingArea.X + (workingArea.Width - form.Width) / 2),
                        Math.Max(workingArea.Y, workingArea.Y + (workingArea.Height - form.Height) / 2));
                }));
            });
        }

        #endregion

        #region Timers
        private void checkConnectionTimer(object sender, ElapsedEventArgs e)
        {
            if (vpnState.ToLower() == "connected")
            {
                connCounter = 0;
                checkConTimer.Stop();
            }
            else
            {
                if (connCounter == 4)
                    Invoke(new Action(() =>
                    {
                        checkConTimer.Stop();
                        connCounter = 0;
                        connector.VPNDisconnect(selectProtocol.SelectedItem.ToString(), entryName);
                        frmLogs.addToLog(selectProtocol.SelectedItem.ToString(), "No connection. Check the settings");
                    }));
                else
                    Invoke(new Action(() =>
                    {
                        connCounter++;
                        frmLogs.addToLog(selectProtocol.SelectedItem.ToString(), "Connection attempt: " + connCounter);
                        connector.VPNDisconnect(selectProtocol.SelectedItem.ToString(), entryName, false);
                        VPNConnect();
                    }));
            }
        }

        private void timerDataCount_Tick(object sender, EventArgs e)
        {
            connector.UpdateConnectionStatistics();
        }

        private void connectionWatchDog_Tick(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.checkboxKillSwitch)
            {
                if (hasConnectionBeenEstablished)
                {
                    if (isConnectionWatchDogTickInProgress)
                        return;

                    isConnectionWatchDogTickInProgress = true;
                    var isUsingOpenVpn = selectProtocol.SelectedItem.ToString() == "OpenVPN";

                    System.Threading.Tasks.Task.Run(() =>
                    {
                        if ((isUsingOpenVpn ? !openVPN.isConnected() : !connector.IsConnectionActive()) ||
                            connector.isConnectionBroken(connectedAddressIp, AReconnect.getConnectedIPAddress()))
                        {
                            if (killSwitchNotificationShown)
                                return;

                            killSwitchNotificationShown = true;
                            setKillSwitchEnabled(false);

                            Invoke((MethodInvoker) (() =>
                            {
                                updateState("disconnected");
                                checkConTimer?.Stop();
                                timerConnectionAttempt?.Stop();
                                timerOpenVPN?.Stop();
                                connectionWatchDog?.Stop();

                                notifyIcon.Visible = true;
                                notifyIcon.BalloonTipTitle = KillSwitchNotificationTitle;
                                notifyIcon.BalloonTipText = "Error: Connection was lost. The Internet was disabled.";
                                notifyIcon.ShowBalloonTip(5000);
                                var dialogResult = MetroMessageBox.Show(this,
                                    "We could not connect to SmartVPN. Do you want to enable Internet? Connection may expose your data to be captured!",
                                    "KillSwitch!", MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                                if (dialogResult == DialogResult.Yes)
                                {
                                    connector.VPNDisconnect(selectProtocol.SelectedItem.ToString(), entryName);

                                    Settings.Default.checkboxKillSwitch = checkBoxKillSwitch.Checked = false;
                                    Settings.Default.Save();

                                    Task.Run(() => { setKillSwitchEnabled(true); });
                                    killSwitchNotificationShown = false;
                                }
                                else
                                {
                                    if (Settings.Default.checkboxAutoReconnect)
                                    {
                                        updateState("connecting");
                                        string selectedItem = selectProtocol.SelectedItem.ToString();
                                        Task.Run(() => { autoReconnectProtocol(selectedItem); });
                                    }
                                    else
                                    {
                                        connector.VPNDisconnect(selectProtocol.SelectedItem.ToString(), entryName);
                                    }
                                    killSwitchNotificationShown = false;
                                }
                            }));
                        }
                        isConnectionWatchDogTickInProgress = false;
                    });
                }
            }
            else
            {
                bool isAutoreconnectEnabled = Settings.Default.checkboxAutoReconnect;
                if (selectProtocol.SelectedItem?.ToString() == "OpenVPN")
                {
                    if (isAutoreconnectEnabled && shouldConnectionBeEstablished && !openVPN.isConnected())
                    {
                        Task.Run(() =>
                        {
                            if (Process.GetProcessesByName("openvpn").Length == 0 || connector.isConnectionBroken(connectedAddressIp, AReconnect.getConnectedIPAddress()))
                                runOpenVPNAutoReconnect();
                        });
                    }
                }
                else
                {
                    if (isAutoreconnectEnabled && shouldConnectionBeEstablished)
                    {
                        var selectedProtocol = selectProtocol.SelectedItem;
                        Task.Run(() =>
                        {
                            if (connector.isConnectionBroken(connectedAddressIp, AReconnect.getConnectedIPAddress()))
                                connector.VPNDisconnect(selectedProtocol.ToString(), entryName, true);
                        });
                    }
                }
            }
        }
        #endregion

        #region Tray icon
        private void prepareSystemTrayIcon()
        {
            if (Settings.Default.trayIconStatus && trayMenu.MenuItems.Count != 5)
            {
                if (trayMenu.MenuItems.Count > 0)
                    trayMenu.MenuItems.Clear();
                trayMenu.MenuItems.Add("Show", trayShowApp);
                if(vpnState.ToLower() != "disconnected")
                    trayMenu.MenuItems.Add(new MenuItem("Stop", trayStopConnection));

                trayMenu.MenuItems.Add("Logs", trayLogs);
                trayMenu.MenuItems.Add("About", trayAbout);
                trayMenu.MenuItems.Add("Exit", trayExit);
                trayIcon.Text = TrayIconTitle;
                trayIcon.Icon = new Icon(Resources.Network_Vpn_icon, 40, 40);
                trayIcon.ContextMenu = trayMenu;
                trayIcon.DoubleClick += trayIcon_DoubleClick;

                trayIcon.BalloonTipText = "Application is still working in the background.";
                trayIcon.BalloonTipTitle = TrayIconNotificationTitle;
                trayIcon.ShowBalloonTip(1000);
            }
            else if (trayMenu.MenuItems.Count == 5 && vpnState.ToLower() == "disconnected")
                trayMenu.MenuItems.RemoveAt(1);

            trayIcon.Visible = Settings.Default.trayIconStatus;
        }

        private void trayIcon_DoubleClick(object Sender, EventArgs e)
        {
            this.Show();
            Settings.Default.trayIconStatus = false;
            Settings.Default.Save();
            prepareSystemTrayIcon();
        }

        private void trayExit(object sender, EventArgs e)
        {
            trayIcon.Dispose();
            Disconect();
            Environment.Exit(0);
        }

        private void trayShowApp(object sender, EventArgs e)
        {
            this.Show();
            Settings.Default.trayIconStatus = false;
            Settings.Default.Save();
            prepareSystemTrayIcon();
        }

        private void trayStopConnection(object sender, EventArgs e)
        {
            Disconect();
            trayMenu.MenuItems.RemoveAt(1);
        }

        private void trayLogs(object sender, EventArgs e)
        {
            frmLogs.Show();
        }

        private void trayAbout(object sender, EventArgs e)
        {
            if (about == null || about.IsDisposed)
                about = new About();
            about.Show();
            about.Focus();
        }
        #endregion
    }
}
