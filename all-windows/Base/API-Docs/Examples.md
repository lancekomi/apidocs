# Base VPN Library - Code Examples

## Overview

This document provides complete, working code examples for common scenarios using the Base VPN Library.

---

## Table of Contents

1. [Complete Minimal VPN Client](#complete-minimal-vpn-client)
2. [Advanced VPN Client with All Features](#advanced-vpn-client-with-all-features)
3. [Authentication Examples](#authentication-examples)
4. [Connection Management](#connection-management)
5. [Kill Switch Implementation](#kill-switch-implementation)
6. [Auto-Reconnect Implementation](#auto-reconnect-implementation)
7. [Credential Storage](#credential-storage)
8. [Server Management](#server-management)
9. [Statistics and Monitoring](#statistics-and-monitoring)
10. [Error Handling](#error-handling)

---

## Complete Minimal VPN Client

A minimal but functional VPN client in under 200 lines:

```csharp
using System;
using System.Windows.Forms;
using SmartDNSProxy_VPN_Client;

namespace MinimalVPN
{
    public partial class MainForm : Form, IVPNView
    {
        private VPNConnectionManager manager;
        private string username, password;
        
        public MainForm()
        {
            InitializeComponent();
            manager = new VPNConnectionManager(
                this, 
                new OpenVPN(), 
                new NetworkManagment()
            );
        }
        
        // IVPNView Implementation
        public bool isInvokeRequired() => InvokeRequired;
        public object Invoke(Delegate method) => base.Invoke(method);
        
        public void updateState(string state)
        {
            statusLabel.Text = state;
            connectButton.Enabled = state.ToLower() != "connecting";
            disconnectButton.Enabled = state.ToLower() == "connected";
        }
        
        public void addToLog(string cat, string msg)
        {
            logBox.AppendText($"[{cat}] {msg}\r\n");
        }
        
        public void UpdateConnectionStatistics(long rx, long tx)
        {
            statsLabel.Text = $"↓{rx/1024}KB ↑{tx/1024}KB";
        }
        
        // Connect Button
        private async void btnConnect_Click(object sender, EventArgs e)
        {
            username = txtUsername.Text;
            password = txtPassword.Text;
            
            // Authenticate
            var auth = await AuthApi.Authorize(username, password);
            if (auth != AuthApiResponse.Success)
            {
                MessageBox.Show($"Auth failed: {auth}");
                return;
            }
            
            // Connect
            var server = new ServerInformation 
            { 
                DnsAddress = txtServer.Text 
            };
            
            manager.connectToVPN(
                "OpenVPN", server, "VPN", 
                username, password, null
            );
        }
        
        // Disconnect Button
        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            manager.VPNDisconnect("OpenVPN", "VPN");
        }
        
        // Cleanup
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            manager.VPNDisconnect("OpenVPN", "VPN", false);
            base.OnFormClosing(e);
        }
    }
}
```

---

## Advanced VPN Client with All Features

Complete implementation with all features:

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SmartDNSProxy_VPN_Client;

namespace AdvancedVPN
{
    public partial class MainForm : Form, IVPNView
    {
        // Components
        private VPNConnectionManager connectionManager;
        private OpenVPN openVPN;
        private NetworkManagment networkMgmt;
        private ServerListClient serverClient;
        private AutoReconnect autoReconnect;
        
        // Timers
        private Timer reconnectTimer;
        private Timer statsTimer;
        private Timer updateCheckTimer;
        
        // State
        private string currentProtocol;
        private string currentEntry;
        private ServerInformation currentServer;
        private string username, password;
        private string initialIP;
        private bool killSwitchEnabled;
        
        public MainForm()
        {
            InitializeComponent();
            InitializeVPN();
            LoadSettings();
        }
        
        private void InitializeVPN()
        {
            openVPN = new OpenVPN();
            networkMgmt = new NetworkManagment();
            connectionManager = new VPNConnectionManager(this, openVPN, networkMgmt);
            serverClient = new ServerListClient();
            autoReconnect = new AutoReconnect();
            
            InitializeTimers();
        }
        
        private void InitializeTimers()
        {
            // Reconnect timer
            reconnectTimer = new Timer { Interval = 30000 };
            reconnectTimer.Tick += ReconnectTimer_Tick;
            
            // Statistics timer
            statsTimer = new Timer { Interval = 1000 };
            statsTimer.Tick += (s, e) => connectionManager.UpdateConnectionStatistics();
            
            // Update check timer
            updateCheckTimer = new Timer { Interval = 3600000 }; // 1 hour
            updateCheckTimer.Tick += async (s, e) => await CheckForUpdates();
        }
        
        private async void MainForm_Load(object sender, EventArgs e)
        {
            await LoadServers();
            LoadCredentials();
            await CheckForUpdates();
        }
        
        private async Task LoadServers()
        {
            try
            {
                addToLog("Servers", "Loading server list...");
                var servers = await Task.Run(() => serverClient.GetServerList());
                
                // Bind to UI
                cmbServers.DataSource = servers;
                cmbServers.DisplayMember = "ServerName";
                
                // Populate protocol dropdown
                if (servers.Length > 0)
                {
                    var protocols = servers[0].protocols;
                    cmbProtocol.Items.AddRange(protocols);
                    cmbProtocol.SelectedIndex = 0;
                }
                
                addToLog("Servers", $"Loaded {servers.Length} servers");
            }
            catch (Exception ex)
            {
                addToLog("Error", $"Failed to load servers: {ex.Message}");
                MessageBox.Show("Failed to load server list", "Error");
            }
        }
        
        private async void btnLogin_Click(object sender, EventArgs e)
        {
            username = txtUsername.Text;
            password = txtPassword.Text;
            
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter credentials");
                return;
            }
            
            btnLogin.Enabled = false;
            progressBar.Visible = true;
            
            try
            {
                var result = await AuthApi.Authorize(username, password);
                
                switch (result)
                {
                    case AuthApiResponse.Success:
                        SaveCredentials();
                        ShowVPNPanel();
                        break;
                    case AuthApiResponse.InvalidCredentials:
                        MessageBox.Show("Invalid credentials", "Error");
                        break;
                    case AuthApiResponse.AccountDisabled:
                        MessageBox.Show("Account disabled", "Error");
                        break;
                    case AuthApiResponse.ServerError:
                        MessageBox.Show("Server error", "Error");
                        break;
                }
            }
            finally
            {
                btnLogin.Enabled = true;
                progressBar.Visible = false;
            }
        }
        
        private void btnConnect_Click(object sender, EventArgs e)
        {
            currentServer = (ServerInformation)cmbServers.SelectedItem;
            currentProtocol = cmbProtocol.SelectedItem.ToString();
            currentEntry = $"VPN_{currentProtocol}_{DateTime.Now.Ticks}";
            
            if (currentServer == null)
            {
                MessageBox.Show("Select a server");
                return;
            }
            
            // Get initial IP
            initialIP = autoReconnect.getConnectedIPAddress();
            addToLog("Info", $"Initial IP: {initialIP}");
            
            // Connect
            string l2tpKey = currentProtocol == "L2TP" ? txtL2TPKey.Text : null;
            connectionManager.connectToVPN(
                currentProtocol, currentServer, currentEntry,
                username, password, l2tpKey
            );
            
            // Start monitoring
            reconnectTimer.Start();
            if (currentProtocol != "OpenVPN")
                statsTimer.Start();
        }
        
        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            reconnectTimer.Stop();
            statsTimer.Stop();
            connectionManager.VPNDisconnect(currentProtocol, currentEntry);
        }
        
        private void chkKillSwitch_CheckedChanged(object sender, EventArgs e)
        {
            killSwitchEnabled = chkKillSwitch.Checked;
            
            if (killSwitchEnabled)
            {
                if (!networkMgmt.isFirewallEnabled())
                {
                    MessageBox.Show("Firewall must be enabled");
                    chkKillSwitch.Checked = false;
                    return;
                }
                
                networkMgmt.disableInternetConnections();
                networkMgmt.allowSmartDNSProxyApp();
                addToLog("Security", "Kill switch enabled");
            }
            else
            {
                networkMgmt.enableInternetConnections();
                networkMgmt.removeSmartDNSRule();
                addToLog("Security", "Kill switch disabled");
            }
        }
        
        private void chkAutostart_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAutostart.Checked)
                AutostartManager.AddApplicationToStartup();
            else
                AutostartManager.RemoveApplicationFromStartup();
        }
        
        private void ReconnectTimer_Tick(object sender, EventArgs e)
        {
            string currentIP = autoReconnect.getConnectedIPAddress();
            
            if (connectionManager.isConnectionBroken(initialIP, currentIP))
            {
                addToLog("Warning", "Connection broken - reconnecting");
                
                connectionManager.VPNDisconnect(currentProtocol, currentEntry, false);
                
                Task.Delay(2000).ContinueWith(_ =>
                {
                    Invoke(new Action(() =>
                    {
                        btnConnect_Click(null, null);
                    }));
                });
            }
        }
        
        private async Task CheckForUpdates()
        {
            try
            {
                var checker = new CheckUpdates("https://example.com/update.txt");
                var currentVersion = new Version(Application.ProductVersion);
                
                if (checker.version > currentVersion)
                {
                    var result = MessageBox.Show(
                        $"Version {checker.version} available. Update?",
                        "Update Available",
                        MessageBoxButtons.YesNo
                    );
                    
                    if (result == DialogResult.Yes)
                        System.Diagnostics.Process.Start(checker.newdownloadlink);
                }
            }
            catch { }
        }
        
        private void SaveCredentials()
        {
            string key = Environment.MachineName;
            string encrypted = Encrypter.Encrypt(password, key);
            
            Properties.Settings.Default.Username = username;
            Properties.Settings.Default.Password = encrypted;
            Properties.Settings.Default.Save();
        }
        
        private void LoadCredentials()
        {
            string username = Properties.Settings.Default.Username;
            string encrypted = Properties.Settings.Default.Password;
            
            if (!string.IsNullOrEmpty(encrypted))
            {
                string key = Environment.MachineName;
                string password = Encrypter.Decrypt(encrypted, key);
                
                txtUsername.Text = username;
                txtPassword.Text = password;
            }
        }
        
        private void LoadSettings()
        {
            chkAutostart.Checked = Properties.Settings.Default.Autostart;
            chkAutoReconnect.Checked = Properties.Settings.Default.AutoReconnect;
        }
        
        private void SaveSettings()
        {
            Properties.Settings.Default.Autostart = chkAutostart.Checked;
            Properties.Settings.Default.AutoReconnect = chkAutoReconnect.Checked;
            Properties.Settings.Default.Save();
        }
        
        private void ShowVPNPanel()
        {
            loginPanel.Visible = false;
            vpnPanel.Visible = true;
        }
        
        // IVPNView Implementation
        public bool isInvokeRequired() => InvokeRequired;
        public object Invoke(Delegate method) => base.Invoke(method);
        
        public void updateState(string state)
        {
            lblStatus.Text = $"Status: {state}";
            
            switch (state.ToLower())
            {
                case "connected":
                    btnConnect.Enabled = false;
                    btnDisconnect.Enabled = true;
                    pnlStatus.BackColor = Color.Green;
                    
                    string newIP = autoReconnect.getConnectedIPAddress();
                    addToLog("Success", $"Connected! IP: {newIP}");
                    break;
                    
                case "disconnected":
                    btnConnect.Enabled = true;
                    btnDisconnect.Enabled = false;
                    pnlStatus.BackColor = Color.Red;
                    break;
                    
                case "connecting":
                    btnConnect.Enabled = false;
                    btnDisconnect.Enabled = false;
                    pnlStatus.BackColor = Color.Yellow;
                    break;
            }
        }
        
        public void addToLog(string category, string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string entry = $"[{timestamp}] [{category}] {message}";
            
            txtLog.AppendText(entry + Environment.NewLine);
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
            
            // Trim if too large
            if (txtLog.Lines.Length > 1000)
            {
                var lines = txtLog.Lines.Skip(500).ToArray();
                txtLog.Lines = lines;
            }
        }
        
        public void UpdateConnectionStatistics(long rx, long tx)
        {
            lblDownload.Text = $"↓ {FormatBytes(rx)}";
            lblUpload.Text = $"↑ {FormatBytes(tx)}";
        }
        
        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
        
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (connectionManager.IsConnectionActive() || connectionManager.isOpenVPNConnected())
            {
                var result = MessageBox.Show(
                    "VPN is connected. Disconnect and exit?",
                    "Confirm",
                    MessageBoxButtons.YesNo
                );
                
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
            
            reconnectTimer?.Stop();
            statsTimer?.Stop();
            updateCheckTimer?.Stop();
            
            if (currentProtocol != null && currentEntry != null)
                connectionManager.VPNDisconnect(currentProtocol, currentEntry, false);
            
            networkMgmt.changeDNSOnExit(this, EventArgs.Empty);
            
            if (killSwitchEnabled)
            {
                networkMgmt.enableInternetConnections();
                networkMgmt.removeSmartDNSRule();
            }
            
            SaveSettings();
            
            base.OnFormClosing(e);
        }
    }
}
```

---

## Authentication Examples

### Example 1: Simple Authentication

```csharp
private async Task<bool> Authenticate()
{
    var result = await AuthApi.Authorize(username, password);
    return result == AuthApiResponse.Success;
}
```

### Example 2: Authentication with Retry

```csharp
private async Task<AuthApiResponse> AuthenticateWithRetry(int maxAttempts = 3)
{
    for (int i = 0; i < maxAttempts; i++)
    {
        var result = await AuthApi.Authorize(username, password);
        
        if (result != AuthApiResponse.ServerError)
            return result;
        
        await Task.Delay(2000 * (i + 1)); // Exponential backoff
    }
    
    return AuthApiResponse.ServerError;
}
```

### Example 3: Offline Mode with Cached Credentials

```csharp
private async Task<bool> AuthenticateWithOfflineMode()
{
    var result = await AuthApi.Authorize(username, password);
    
    if (result == AuthApiResponse.Success)
    {
        // Cache successful auth
        Properties.Settings.Default.LastSuccessfulAuth = DateTime.Now;
        Properties.Settings.Default.Save();
        return true;
    }
    else if (result == AuthApiResponse.ServerError)
    {
        // Server unavailable - check cache
        var lastAuth = Properties.Settings.Default.LastSuccessfulAuth;
        if ((DateTime.Now - lastAuth).TotalHours < 24)
        {
            MessageBox.Show("Using cached authentication (offline mode)");
            return true;
        }
    }
    
    return false;
}
```

---

## Connection Management

### Example 1: Protocol Selection Logic

```csharp
private string SelectBestProtocol(ServerInformation server)
{
    // Prefer OpenVPN for security
    if (server.protocols.Contains("OpenVPN"))
        return "OpenVPN";
    
    // SSTP for firewall-restricted networks
    if (server.protocols.Contains("SSTP"))
        return "SSTP";
    
    // L2TP for compatibility
    if (server.protocols.Contains("L2TP"))
        return "L2TP";
    
    // Fallback
    return server.protocols.FirstOrDefault() ?? "OpenVPN";
}
```

### Example 2: Smart Reconnection

```csharp
private async Task SmartReconnect()
{
    // Disconnect
    connectionManager.VPNDisconnect(currentProtocol, currentEntry, false);
    
    await Task.Delay(2000);
    
    // Try alternate protocol if current failed
    var protocols = currentServer.protocols.Where(p => p != currentProtocol).ToArray();
    if (protocols.Any())
    {
        currentProtocol = protocols[0];
        addToLog("Info", $"Trying alternate protocol: {currentProtocol}");
    }
    
    // Reconnect
    connectionManager.connectToVPN(
        currentProtocol, currentServer, currentEntry,
        username, password, null
    );
}
```

---

## Kill Switch Implementation

### Example 1: Toggle Kill Switch

```csharp
private void ToggleKillSwitch(bool enable)
{
    var netMgmt = new NetworkManagment();
    
    if (enable)
    {
        if (!netMgmt.isFirewallEnabled())
        {
            MessageBox.Show("Please enable Windows Firewall");
            return;
        }
        
        // Block all traffic
        netMgmt.disableInternetConnections();
        
        // Allow VPN app
        netMgmt.allowSmartDNSProxyApp();
        
        MessageBox.Show("Kill Switch enabled. Only VPN traffic allowed.");
    }
    else
    {
        // Restore normal connectivity
        netMgmt.enableInternetConnections();
        netMgmt.removeSmartDNSRule();
        
        MessageBox.Show("Kill Switch disabled.");
    }
}
```

### Example 2: Kill Switch with Connection Monitoring

```csharp
private Timer killSwitchMonitor;

private void EnableKillSwitchMonitoring()
{
    killSwitchMonitor = new Timer { Interval = 5000 };
    killSwitchMonitor.Tick += (s, e) =>
    {
        bool connected = connectionManager.IsConnectionActive() || 
                        connectionManager.isOpenVPNConnected();
        
        if (!connected && killSwitchEnabled)
        {
            // Connection lost with kill switch on
            ShowWarning("VPN disconnected - all traffic blocked!");
        }
    };
    killSwitchMonitor.Start();
}
```

---

## Auto-Reconnect Implementation

### Example 1: Basic Auto-Reconnect

```csharp
private Timer autoReconnectTimer;
private string startIP;
private int reconnectAttempts = 0;
private const int MAX_RECONNECT_ATTEMPTS = 5;

private void EnableAutoReconnect()
{
    startIP = new AutoReconnect().getConnectedIPAddress();
    
    autoReconnectTimer = new Timer { Interval = 30000 };
    autoReconnectTimer.Tick += AutoReconnect_Tick;
    autoReconnectTimer.Start();
}

private void AutoReconnect_Tick(object sender, EventArgs e)
{
    var checker = new AutoReconnect();
    string currentIP = checker.getConnectedIPAddress();
    
    if (connectionManager.isConnectionBroken(startIP, currentIP))
    {
        if (reconnectAttempts < MAX_RECONNECT_ATTEMPTS)
        {
            reconnectAttempts++;
            addToLog("Reconnect", $"Attempt {reconnectAttempts}/{MAX_RECONNECT_ATTEMPTS}");
            
            connectionManager.VPNDisconnect(currentProtocol, currentEntry, false);
            Task.Delay(2000).ContinueWith(_ => Reconnect());
        }
        else
        {
            autoReconnectTimer.Stop();
            MessageBox.Show("Max reconnection attempts reached");
        }
    }
    else
    {
        reconnectAttempts = 0; // Reset on successful check
    }
}
```

### Example 2: Smart Auto-Reconnect with Server Failover

```csharp
private void SmartAutoReconnect()
{
    // Try same server first
    connectionManager.connectToVPN(
        currentProtocol, currentServer, currentEntry,
        username, password, null
    );
    
    // If still fails after 30 seconds, try next server
    Task.Delay(30000).ContinueWith(_ =>
    {
        if (!connectionManager.IsConnectionActive() && 
            !connectionManager.isOpenVPNConnected())
        {
            // Switch to next server
            var servers = (ServerInformation[])cmbServers.DataSource;
            int currentIndex = Array.IndexOf(servers, currentServer);
            int nextIndex = (currentIndex + 1) % servers.Length;
            
            currentServer = servers[nextIndex];
            addToLog("Failover", $"Switching to {currentServer.name}");
            
            connectionManager.connectToVPN(
                currentProtocol, currentServer, currentEntry,
                username, password, null
            );
        }
    });
}
```

---

## Credential Storage

### Example 1: Secure Credential Storage

```csharp
public class CredentialManager
{
    private static string GetMasterKey()
    {
        return Environment.MachineName + Environment.UserName;
    }
    
    public static void SaveCredentials(string username, string password)
    {
        try
        {
            string encrypted = Encrypter.Encrypt(password, GetMasterKey());
            
            Properties.Settings.Default.Username = username;
            Properties.Settings.Default.EncryptedPassword = encrypted;
            Properties.Settings.Default.Save();
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to save credentials", ex);
        }
    }
    
    public static (string username, string password) LoadCredentials()
    {
        try
        {
            string username = Properties.Settings.Default.Username;
            string encrypted = Properties.Settings.Default.EncryptedPassword;
            
            if (string.IsNullOrEmpty(encrypted))
                return (username, null);
            
            string password = Encrypter.Decrypt(encrypted, GetMasterKey());
            return (username, password);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to load credentials", ex);
        }
    }
    
    public static void ClearCredentials()
    {
        Properties.Settings.Default.Username = "";
        Properties.Settings.Default.EncryptedPassword = "";
        Properties.Settings.Default.Save();
    }
}
```

---

## Server Management

### Example 1: Filter and Sort Servers

```csharp
private void FilterServers()
{
    var servers = serverClient.GetServerList();
    
    // Filter by features
    var torrentServers = servers.Where(s => s.torrentP2P == "Yes").ToArray();
    var smartVPNServers = servers.Where(s => s.smartVPN == "Yes").ToArray();
    var openVPNServers = servers.Where(s => s.protocols.Contains("OpenVPN")).ToArray();
    
    // Filter by region
    var usServers = servers.Where(s => s.country == "United States").ToArray();
    
    // Sort by name
    var sortedServers = servers.OrderBy(s => s.name).ToArray();
    
    cmbServers.DataSource = sortedServers;
}
```

### Example 2: Server Ping Test

```csharp
private async Task<long> PingServer(ServerInformation server)
{
    try
    {
        using (var ping = new System.Net.NetworkInformation.Ping())
        {
            var reply = await ping.SendPingAsync(server.DnsAddress, 5000);
            return reply.Status == IPStatus.Success ? reply.RoundtripTime : -1;
        }
    }
    catch
    {
        return -1;
    }
}

private async Task TestServerLatency()
{
    var servers = (ServerInformation[])cmbServers.DataSource;
    
    foreach (var server in servers)
    {
        long latency = await PingServer(server);
        addToLog("Ping", $"{server.name}: {latency}ms");
    }
}
```

---

## Statistics and Monitoring

### Example 1: Real-time Statistics Display

```csharp
private DateTime lastStatsUpdate = DateTime.MinValue;
private long lastRX = 0;
private long lastTX = 0;

public void UpdateConnectionStatistics(long bytesReceived, long bytesTransmitted)
{
    // Total traffic
    lblTotalDownload.Text = $"Total ↓: {FormatBytes(bytesReceived)}";
    lblTotalUpload.Text = $"Total ↑: {FormatBytes(bytesTransmitted)}";
    
    // Calculate speed
    if (lastStatsUpdate != DateTime.MinValue)
    {
        var elapsed = (DateTime.Now - lastStatsUpdate).TotalSeconds;
        if (elapsed > 0)
        {
            var dlSpeed = (bytesReceived - lastRX) / elapsed;
            var ulSpeed = (bytesTransmitted - lastTX) / elapsed;
            
            lblDownloadSpeed.Text = $"↓ {FormatBytes((long)dlSpeed)}/s";
            lblUploadSpeed.Text = $"↑ {FormatBytes((long)ulSpeed)}/s";
            
            // Update graph
            UpdateSpeedGraph(dlSpeed, ulSpeed);
        }
    }
    
    lastRX = bytesReceived;
    lastTX = bytesTransmitted;
    lastStatsUpdate = DateTime.Now;
}
```

### Example 2: Connection Duration Tracker

```csharp
private DateTime connectionStartTime;
private Timer durationTimer;

private void StartConnectionTracking()
{
    connectionStartTime = DateTime.Now;
    
    durationTimer = new Timer { Interval = 1000 };
    durationTimer.Tick += (s, e) =>
    {
        var duration = DateTime.Now - connectionStartTime;
        lblDuration.Text = $"Connected: {duration:hh\\:mm\\:ss}";
    };
    durationTimer.Start();
}

private void StopConnectionTracking()
{
    durationTimer?.Stop();
    lblDuration.Text = "Disconnected";
}
```

---

## Error Handling

### Example 1: Comprehensive Error Handler

```csharp
private void HandleVPNError(Exception ex)
{
    string errorMsg = "Unknown error";
    string suggestion = "Please try again";
    
    if (ex is UnauthorizedAccessException)
    {
        errorMsg = "Access denied";
        suggestion = "Please run as administrator";
    }
    else if (ex is System.Net.Sockets.SocketException)
    {
        errorMsg = "Network error";
        suggestion = "Check your internet connection";
    }
    else if (ex is System.ComponentModel.Win32Exception)
    {
        errorMsg = "Windows VPN service error";
        suggestion = "Check if RAS service is running";
    }
    
    addToLog("Error", $"{errorMsg}: {ex.Message}");
    MessageBox.Show($"{errorMsg}\n\n{suggestion}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
}
```

### Example 2: Connection State Error Detection

```csharp
public void updateState(string state)
{
    // Detect error states
    if (state.Contains("Error") || state.Contains("Failed"))
    {
        HandleConnectionError(state);
        return;
    }
    
    // Handle specific RAS errors
    if (state.Contains("691")) // Invalid credentials
    {
        MessageBox.Show("Invalid VPN credentials");
        btnConnect.Enabled = true;
        return;
    }
    
    if (state.Contains("800")) // Server unreachable
    {
        MessageBox.Show("Cannot reach VPN server");
        btnConnect.Enabled = true;
        return;
    }
    
    // Normal state handling
    lblStatus.Text = state;
}
```

---

## Related Documentation

- [Getting Started Guide](GettingStarted.md)
- [API Reference Index](INDEX.md)
- [Best Practices](BestPractices.md)

---

**Examples Version**: 1.0  
**Last Updated**: October 2025

