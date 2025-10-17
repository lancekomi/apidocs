# Base VPN Library - Quick Reference Guide

## Table of Contents
- [Quick Start](#quick-start)
- [API Reference](#api-reference)
- [Common Recipes](#common-recipes)
- [Error Codes](#error-codes)

## Quick Start

### Minimal Working Example

```csharp
// 1. Create your form implementing IVPNView
public class MyVPNForm : Form, IVPNView
{
    private VPNConnectionManager manager;
    
    public MyVPNForm()
    {
        var openVPN = new OpenVPN();
        var netMgmt = new NetworkManagment();
        manager = new VPNConnectionManager(this, openVPN, netMgmt);
    }
    
    // 2. Authenticate and connect
    private async void ConnectButton_Click(object sender, EventArgs e)
    {
        var auth = await AuthApi.Authorize("user", "pass");
        if (auth == AuthApiResponse.Success)
        {
            var server = new ServerInformation { DnsAddress = "vpn.example.com" };
            manager.connectToVPN("OpenVPN", server, "MyVPN", "user", "pass", null);
        }
    }
    
    // 3. Implement IVPNView interface
    public void updateState(string state) => statusLabel.Text = state;
    public void addToLog(string log, string msg) => logBox.AppendText($"{log}: {msg}\n");
    public void UpdateConnectionStatistics(long rx, long tx) { }
    public bool isInvokeRequired() => InvokeRequired;
    public object Invoke(Delegate method) => base.Invoke(method);
}
```

## API Reference

### VPNConnectionManager

| Method | Parameters | Description |
|--------|------------|-------------|
| `connectToVPN()` | protocol, server, entryName, login, password, L2TPpassword | Initiate VPN connection |
| `VPNDisconnect()` | protocol, entryName, notifyUi | Disconnect from VPN |
| `isOpenVPNConnected()` | - | Check if OpenVPN is connected |
| `IsConnectionActive()` | - | Check if RAS connection is active |
| `UpdateConnectionStatistics()` | - | Refresh connection statistics |
| `isConnectionBroken()` | startIp, currentIp | Check if connection dropped |

### OpenVPN

| Method | Parameters | Description |
|--------|------------|-------------|
| `connect()` | OpenVPNConnectionInfo, callback | Connect to OpenVPN server |
| `disconnect()` | - | Disconnect OpenVPN |
| `isConnected()` | - | Check connection status |

### Ras

| Method | Parameters | Description |
|--------|------------|-------------|
| `Connect()` | entryName, host, username, password, callback, protocol, preSharedKey | Connect via PPTP/L2TP/SSTP |
| `disconnect()` | entryName | Disconnect connection |
| `isConnected()` | entryName | Check if connected |
| `deleteVPNEntry()` | entryName | Remove phonebook entry |

### AuthApi

| Method | Parameters | Return | Description |
|--------|------------|--------|-------------|
| `Authorize()` | username, password | `AuthApiResponse` | Authenticate user credentials |

**Response Values:**
- `AuthApiResponse.Success` - Login successful
- `AuthApiResponse.InvalidCredentials` - Wrong username/password
- `AuthApiResponse.AccountDisabled` - Account is disabled
- `AuthApiResponse.ServerError` - Server unreachable

### ServerListClient

| Method | Parameters | Return | Description |
|--------|------------|--------|-------------|
| `GetServerList()` | - | `ServerInformation[]` | Get list of available VPN servers |

### NetworkManagment

| Method | Parameters | Description |
|--------|------------|-------------|
| `setDNS()` | adapterName, primary, secondary, dhcp | Configure DNS servers |
| `disableInternetConnections()` | - | Block all outbound traffic (kill switch) |
| `enableInternetConnections()` | - | Allow outbound traffic |
| `allowSmartDNSProxyApp()` | - | Add firewall exception for VPN app |
| `removeSmartDNSRule()` | - | Remove firewall exception |
| `isFirewallEnabled()` | - | Check if Windows Firewall is enabled |

### AutoReconnect

| Method | Parameters | Return | Description |
|--------|------------|--------|-------------|
| `getConnectedIPAddress()` | - | `string` | Get current public IP address |

**Returns:** IP address as string, or `"broken"` if failed

### AutostartManager

| Method | Parameters | Description |
|--------|------------|-------------|
| `AddApplicationToStartup()` | - | Add to Windows startup (registry) |
| `RemoveApplicationFromStartup()` | - | Remove from Windows startup |

### CheckUpdates

```csharp
var checker = new CheckUpdates("https://example.com/update.txt");
// Properties: appname, version, newdownloadlink
```

### Encrypter

| Method | Parameters | Return | Description |
|--------|------------|--------|-------------|
| `Encrypt()` | plainText, passPhrase | `string` | Encrypt string (AES-256) |
| `Decrypt()` | cipherText, passPhrase | `string` | Decrypt string |

## Common Recipes

### Recipe 1: Connect to Best Server

```csharp
var client = new ServerListClient();
var servers = client.GetServerList();

// Find server with OpenVPN and torrent support
var bestServer = servers.FirstOrDefault(s => 
    s.protocols.Contains("OpenVPN") && 
    s.torrentP2P.ToLower() == "yes"
);

manager.connectToVPN("OpenVPN", bestServer, "VPN", user, pass, null);
```

### Recipe 2: Implement Auto-Reconnect

```csharp
private Timer reconnectTimer;
private string initialIp;

private void EnableAutoReconnect()
{
    initialIp = new AutoReconnect().getConnectedIPAddress();
    
    reconnectTimer = new Timer { Interval = 30000 }; // 30 seconds
    reconnectTimer.Tick += (s, e) =>
    {
        var autoRecon = new AutoReconnect();
        string currentIp = autoRecon.getConnectedIPAddress();
        
        if (manager.isConnectionBroken(initialIp, currentIp))
        {
            addToLog("Info", "Connection broken - reconnecting");
            manager.connectToVPN(protocol, server, entry, user, pass, key);
        }
    };
    reconnectTimer.Start();
}
```

### Recipe 3: Enable Kill Switch

```csharp
private void EnableKillSwitch()
{
    var netMgmt = new NetworkManagment();
    
    if (!netMgmt.isFirewallEnabled())
    {
        MessageBox.Show("Windows Firewall must be enabled!");
        return;
    }
    
    // Block all traffic except VPN
    netMgmt.disableInternetConnections();
    netMgmt.allowSmartDNSProxyApp();
    
    addToLog("Security", "Kill Switch activated");
}

private void DisableKillSwitch()
{
    var netMgmt = new NetworkManagment();
    netMgmt.enableInternetConnections();
    netMgmt.removeSmartDNSRule();
    
    addToLog("Security", "Kill Switch deactivated");
}
```

### Recipe 4: Store Credentials Securely

```csharp
// Save credentials
private void SaveCredentials(string username, string password)
{
    string key = Environment.MachineName + Environment.UserName;
    string encrypted = Encrypter.Encrypt(password, key);
    
    Settings.Default.Username = username;
    Settings.Default.Password = encrypted;
    Settings.Default.Save();
}

// Load credentials
private void LoadCredentials()
{
    string username = Settings.Default.Username;
    string encrypted = Settings.Default.Password;
    
    if (!string.IsNullOrEmpty(encrypted))
    {
        string key = Environment.MachineName + Environment.UserName;
        string password = Encrypter.Decrypt(encrypted, key);
        
        usernameBox.Text = username;
        passwordBox.Text = password;
    }
}
```

### Recipe 5: Protocol Switching

```csharp
private async Task SwitchProtocol(string newProtocol)
{
    // Disconnect current
    manager.VPNDisconnect(currentProtocol, currentEntry, false);
    
    // Wait for cleanup
    await Task.Delay(2000);
    
    // Connect with new protocol
    currentProtocol = newProtocol;
    currentEntry = $"VPN_{newProtocol}";
    
    manager.connectToVPN(
        newProtocol, 
        currentServer, 
        currentEntry,
        username, 
        password,
        newProtocol == "L2TP" ? l2tpKey : null
    );
}
```

### Recipe 6: Monitor Connection Statistics

```csharp
private Timer statsTimer;

private void StartStatsMonitoring()
{
    statsTimer = new Timer { Interval = 1000 }; // Every second
    statsTimer.Tick += (s, e) => manager.UpdateConnectionStatistics();
    statsTimer.Start();
}

// In IVPNView implementation
public void UpdateConnectionStatistics(long bytesReceived, long bytesTransmitted)
{
    downloadLabel.Text = $"↓ {FormatBytes(bytesReceived)}";
    uploadLabel.Text = $"↑ {FormatBytes(bytesTransmitted)}";
}

private string FormatBytes(long bytes)
{
    string[] sizes = { "B", "KB", "MB", "GB", "TB" };
    double len = bytes;
    int order = 0;
    while (len >= 1024 && order < sizes.Length - 1)
    {
        order++;
        len /= 1024;
    }
    return $"{len:0.##} {sizes[order]}";
}
```

### Recipe 7: Check for Updates

```csharp
private async Task CheckForUpdates()
{
    try
    {
        var checker = new CheckUpdates("https://example.com/version.txt");
        var currentVersion = new Version(Application.ProductVersion);
        
        if (checker.version > currentVersion)
        {
            var result = MessageBox.Show(
                $"New version {checker.version} is available!\nWould you like to download it?",
                "Update Available",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information
            );
            
            if (result == DialogResult.Yes)
            {
                Process.Start(checker.newdownloadlink);
            }
        }
    }
    catch (Exception ex)
    {
        addToLog("Update", $"Failed to check for updates: {ex.Message}");
    }
}
```

### Recipe 8: Proper Application Cleanup

```csharp
protected override void OnFormClosing(FormClosingEventArgs e)
{
    // Ask user if connected
    if (manager.IsConnectionActive() || manager.isOpenVPNConnected())
    {
        var result = MessageBox.Show(
            "VPN is still connected. Disconnect and exit?",
            "Confirm Exit",
            MessageBoxButtons.YesNo
        );
        
        if (result == DialogResult.No)
        {
            e.Cancel = true;
            return;
        }
    }
    
    // Stop all timers
    reconnectTimer?.Stop();
    statsTimer?.Stop();
    
    // Disconnect VPN
    try
    {
        manager.VPNDisconnect(currentProtocol, currentEntry, false);
    }
    catch { }
    
    // Restore DNS
    var netMgmt = new NetworkManagment();
    netMgmt.changeDNSOnExit(this, EventArgs.Empty);
    
    // Disable kill switch
    if (killSwitchEnabled)
    {
        netMgmt.enableInternetConnections();
        netMgmt.removeSmartDNSRule();
    }
    
    base.OnFormClosing(e);
}
```

## Error Codes

### RAS Error Codes

| Code | Description | Solution |
|------|-------------|----------|
| 691 | Access denied | Wrong username/password |
| 800 | Unable to establish connection | Server unreachable |
| 806 | GRE blocked | Firewall blocking GRE protocol (PPTP) |
| 809 | Network unreachable | L2TP/IPsec port blocked |
| 812 | Connection prevented by policy | Windows policy blocking VPN |

### HTTP Status Codes (Auth API)

| Code | Meaning | AuthApiResponse |
|------|---------|-----------------|
| 200 | OK | `Success` |
| 401 | Unauthorized | `AccountDisabled` |
| 404 | Not Found | `InvalidCredentials` |
| 405 | Method Not Allowed | `InvalidCredentials` |
| 500 | Server Error | `ServerError` |

### Common Exceptions

```csharp
try
{
    manager.connectToVPN(...);
}
catch (InvalidOperationException)
{
    // Already connected or busy
}
catch (UnauthorizedAccessException)
{
    // Need administrator privileges
}
catch (SocketException)
{
    // OpenVPN management socket error
}
catch (Win32Exception)
{
    // RAS operation failed
}
```

## Configuration Values

### Default Intervals (in milliseconds)

```csharp
Configuration.autoreconnectTimeInterval = 10000;    // 10 seconds
Configuration.connectionTimerInterval = 30000;     // 30 seconds
Configuration.vpnTimerInterval = 10000;            // 10 seconds
```

### OpenVPN Management Interface

```csharp
Configuration.socketAddress = "localhost";
Configuration.socketPort = 12343;
```

### Recommended Timer Intervals

```csharp
// Auto-reconnect check
reconnectTimer.Interval = 30000;  // 30 seconds

// Statistics update
statsTimer.Interval = 1000;       // 1 second

// Update check
updateTimer.Interval = 3600000;   // 1 hour
```

## Connection States

### OpenVPN States

| State | Meaning |
|-------|---------|
| `CONNECTING` | Establishing connection |
| `WAIT` | Waiting for server response |
| `AUTH` | Authenticating |
| `GET_CONFIG` | Retrieving configuration |
| `ASSIGN_IP` | Assigning IP address |
| `ADD_ROUTES` | Adding routes |
| `CONNECTED` | Successfully connected |
| `RECONNECTING` | Attempting to reconnect |
| `EXITING` | Shutting down |

### RAS States

| State | Meaning |
|-------|---------|
| `OpenPort` | Opening port |
| `PortOpened` | Port opened |
| `ConnectDevice` | Connecting device |
| `DeviceConnected` | Device connected |
| `Authenticate` | Authenticating |
| `Authenticated` | Authentication successful |
| `Connected` | Connection established |

## DNS Servers

### Common Public DNS

```csharp
// Google DNS
setDNS(adapter, "8.8.8.8", "8.8.4.4");

// Cloudflare DNS
setDNS(adapter, "1.1.1.1", "1.0.0.1");

// OpenDNS
setDNS(adapter, "208.67.222.222", "208.67.220.220");

// Quad9
setDNS(adapter, "9.9.9.9", "149.112.112.112");
```

## Debugging Tips

### Enable Verbose Logging

```csharp
public void addToLog(string category, string message)
{
    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
    string logLine = $"[{timestamp}] [{category}] {message}";
    
    // Display in UI
    logTextBox.AppendText(logLine + "\r\n");
    
    // Also write to file
    File.AppendAllText("vpn-debug.log", logLine + Environment.NewLine);
}
```

### Monitor Network Interfaces

```csharp
foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
{
    if (ni.OperationalStatus == OperationalStatus.Up)
    {
        addToLog("Network", $"{ni.Name}: {ni.NetworkInterfaceType}");
        
        var props = ni.GetIPProperties();
        foreach (var dns in props.DnsAddresses)
        {
            addToLog("DNS", $"  {dns}");
        }
    }
}
```

### Test IP Leak

```csharp
private async Task<bool> CheckIPLeak()
{
    var autoRecon = new AutoReconnect();
    string vpnIp = autoRecon.getConnectedIPAddress();
    
    // Wait a moment
    await Task.Delay(5000);
    
    string currentIp = autoRecon.getConnectedIPAddress();
    
    if (vpnIp != currentIp)
    {
        addToLog("Security", "WARNING: IP leak detected!");
        return true;
    }
    
    return false;
}
```

## Performance Tips

1. **Cache server list**: Only call `GetServerList()` once, reuse the array
2. **Throttle statistics**: Update max every 500ms, not every 100ms
3. **Use async/await**: Keep UI responsive during connection operations
4. **Dispose timers**: Always stop and dispose timers to prevent leaks
5. **Limit log size**: Trim or clear logs periodically to prevent memory issues

## Thread Safety

All VPN operations use `Invoke()` to ensure thread-safe UI updates:

```csharp
// Internal implementation already handles this
private void updateViewState(string state)
{
    view.Invoke(new Action(() => view.updateState(state)));
}

// You just need to implement isInvokeRequired() correctly
public bool isInvokeRequired() => InvokeRequired;
public object Invoke(Delegate method) => base.Invoke(method);
```

---

**Quick Reference Version**: 1.0  
**Last Updated**: October 2025

