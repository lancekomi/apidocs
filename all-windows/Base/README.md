# Base (VpnBase) - Shared VPN Client Library

## Overview

**Base** is a shared C# library project (`VpnBase.shproj`) that provides core VPN client functionality for multiple white-label VPN applications. It implements a complete VPN connection management system supporting multiple protocols (OpenVPN, PPTP, L2TP, SSTP) with features like auto-reconnection, DNS management, authentication, and automatic updates.

### Purpose

This shared project serves as the foundation for several VPN client applications:
- Getflix VPN
- Pizza VPN
- Smart DNS Proxy VPN
- TrickByte VPN

By centralizing core VPN functionality, it enables code reuse and consistency across multiple branded VPN clients while reducing maintenance overhead.

## Architecture

### Project Type
- **Shared Project** (`.shproj`): Code is compiled directly into each consuming project rather than as a separate assembly.
- **Namespace**: `SmartDNSProxy_VPN_Client`

### Core Dependencies
- **DotRas**: Windows RAS (Remote Access Service) API wrapper for PPTP, L2TP, SSTP protocols
- **NetFwTypeLib**: Windows Firewall management
- **System.Management**: Network interface management

## Key Components

### 1. VPNConnectionManager
**File**: `VPNConnectionManager.cs`

The central orchestrator for all VPN operations.

#### Responsibilities:
- Protocol selection and connection management
- DNS configuration and restoration
- Connection monitoring and statistics
- Network stack reset operations
- Integration with both RAS and OpenVPN drivers

#### Key Methods:

```csharp
// Connect to VPN with specified protocol
void connectToVPN(string selectedProtocol, ServerInformation selectedServer, 
                  string entryName, string login, string password, string L2TPpassword)

// Disconnect from VPN
void VPNDisconnect(string protocol, string entryName, bool notifyUi = true)

// Check if OpenVPN is connected
bool isOpenVPNConnected()

// Check if standard protocol connection is active
bool IsConnectionActive()

// Get connection statistics
void UpdateConnectionStatistics()

// Check if connection is broken
bool isConnectionBroken(string startIp, string currentIp)
```

#### Usage Example:

```csharp
// Initialize
var view = new YourViewImplementation(); // implements IVPNView
var openVPN = new OpenVPN();
var networkMgmt = new NetworkManagment();
var connectionManager = new VPNConnectionManager(view, openVPN, networkMgmt);

// Connect to OpenVPN
var server = new ServerInformation { 
    DnsAddress = "vpn.example.com",
    ip = new[] { "8.8.8.8", "8.8.4.4" }
};
connectionManager.connectToVPN("OpenVPN", server, "MyVPN", "user", "pass", null);

// Disconnect
connectionManager.VPNDisconnect("OpenVPN", "MyVPN");
```

### 2. OpenVPN
**File**: `OpenVPN.clsss.cs`

Complete OpenVPN protocol implementation with management interface.

#### Features:
- Process management for OpenVPN executable
- Socket-based management interface (localhost:12343)
- Real-time connection state monitoring
- Certificate and credential file handling
- Automatic cleanup on disconnect

#### Key Methods:

```csharp
// Connect to OpenVPN server
void connect(OpenVPNConnectionInfo connectionInfo, Action OnOpenVPnConnectedCallback)

// Disconnect from OpenVPN
void disconnect()

// Check connection status
bool isConnected()
```

#### Connection Info Structure:

```csharp
var connectionInfo = new OpenVPNConnectionInfo
{
    host = "vpn.example.com",
    username = "user",
    password = "password",
    protocol = "tcp",  // or "udp"
    port = 443
};
```

#### Internal Architecture:
- **Socket Management**: Dedicated thread for monitoring OpenVPN management interface
- **State Tracking**: Parses `>STATE:` messages to determine connection status
- **Certificate Handling**: Embeds CA certificate and generates temporary credential files
- **Process Control**: Manages OpenVPN process lifecycle

### 3. Ras (RAS Driver)
**File**: `Ras.class.cs`

Wrapper for Windows Remote Access Service, handling PPTP, L2TP, and SSTP protocols.

#### Supported Protocols:
- **PPTP**: Point-to-Point Tunneling Protocol
- **L2TP**: Layer 2 Tunneling Protocol (with pre-shared key support)
- **SSTP**: Secure Socket Tunneling Protocol

#### Key Methods:

```csharp
// Connect using standard VPN protocol
RasHandle Connect(string entryName, string host, string username, 
                  string password, Action<string> returnDialerState, 
                  StandardVpnProtocol vpnProtocol, string preSharedKey = null)

// Disconnect active connection
void disconnect(string entryName)

// Check if specific entry is connected
bool isConnected(string entryName)

// Get connection by entry name
bool getConnectionByEntryName(string entryName)

// Delete VPN phonebook entry
bool deleteVPNEntry(string entryName)
```

#### Usage Example:

```csharp
var ras = new Ras();

// Connect using L2TP
ras.Connect(
    entryName: "MyL2TP", 
    host: "vpn.example.com",
    username: "user",
    password: "password",
    returnDialerState: (state) => Console.WriteLine(state),
    vpnProtocol: StandardVpnProtocol.L2TP,
    preSharedKey: "sharedSecret"
);

// Check connection status
var stats = ras.connection.GetConnectionStatistics();
Console.WriteLine($"Received: {stats.BytesReceived} bytes");

// Disconnect
ras.disconnect("MyL2TP");
ras.deleteVPNEntry("MyL2TP");
```

### 4. AuthApi
**File**: `AuthApi.cs`

Centralized authentication service for user credential verification.

#### Features:
- RESTful API integration
- MD5 hash-based authentication
- Multiple response states (Success, InvalidCredentials, AccountDisabled, ServerError)
- Automatic retry and error handling

#### API Endpoints:
- **Production**: `https://auth-api.glbls.net:5000/auth/`
- **Debug**: `http://199.241.146.241:5000/auth/`

#### Usage:

```csharp
var response = await AuthApi.Authorize("username", "password");

switch (response)
{
    case AuthApiResponse.Success:
        // Proceed with VPN connection
        break;
    case AuthApiResponse.InvalidCredentials:
        // Show error: Invalid username or password
        break;
    case AuthApiResponse.AccountDisabled:
        // Show error: Account has been disabled
        break;
    case AuthApiResponse.ServerError:
        // Show error: Cannot reach authentication server
        break;
}
```

#### Authentication Flow:
1. Generates MD5 hash of `username + password`
2. Sends POST request to `{AuthApiUrl}/{username}`
3. Includes hash in JSON body: `{"hash": "..."}`
4. Interprets HTTP status codes:
   - 200 OK → Success
   - 404/405 → Invalid credentials
   - 401 → Account disabled
   - Other → Server error

### 5. ServerListClient
**File**: `ServerListClient.cs`

Downloads and parses VPN server list from remote CSV file.

#### Features:
- Automatic server list updates
- Local caching with backup
- CSV parsing with error handling
- Server categorization (SmartVPN, Torrent, Standard)
- Port extraction and sorting

#### Key Methods:

```csharp
// Get server list (downloads if updated, otherwise uses cache)
ServerInformation[] GetServerList()
```

#### Server Information Fields:
- Country and city
- DNS address
- Supported protocols
- Torrent/P2P support
- SmartVPN capability
- IP addresses
- Available ports

#### CSV Format:
```
Country,City,Note,Protocols,DnsAddress,TorrentP2P,SmartVPN,IPs,Ports
US,New York,Fast,"PPTP;L2TP;OpenVPN",us-ny.vpn.example.com,Yes,Yes,1.2.3.4;5.6.7.8,443;1194
```

#### Usage:

```csharp
var client = new ServerListClient();
var servers = client.GetServerList();

foreach (var server in servers)
{
    Console.WriteLine($"{server.name} - {server.DnsAddress}");
    Console.WriteLine($"  Protocols: {string.Join(", ", server.protocols)}");
    Console.WriteLine($"  Torrent: {server.torrentP2P}");
    Console.WriteLine($"  SmartVPN: {server.smartVPN}");
}
```

### 6. NetworkManagment
**File**: `NetworkManagment.cs`

Handles DNS configuration and Windows Firewall rules.

#### DNS Management:

```csharp
var netMgmt = new NetworkManagment();

// Set custom DNS servers
netMgmt.setDNS("Ethernet", "8.8.8.8", "8.8.4.4");

// Set single DNS server
netMgmt.setDNS("Wi-Fi", "1.1.1.1", "");

// Enable DHCP for DNS
netMgmt.setDNS("Ethernet", "", "", dhcp: true);

// Restore DNS on application exit
netMgmt.changeDNSOnExit(this, EventArgs.Empty);
```

#### Firewall Management (Kill Switch):

```csharp
var netMgmt = new NetworkManagment();

// Check if firewall is enabled
if (netMgmt.isFirewallEnabled())
{
    // Block all internet traffic
    netMgmt.disableInternetConnections();
    
    // Allow VPN application and OpenVPN
    netMgmt.allowSmartDNSProxyApp();
    
    // Later: restore normal operation
    netMgmt.enableInternetConnections();
    netMgmt.removeSmartDNSRule();
}
```

#### Kill Switch Implementation:
The `disableInternetConnections()` method blocks all outbound traffic, then `allowSmartDNSProxyApp()` creates firewall rules to allow only:
1. The VPN client application
2. OpenVPN.exe process

This ensures that if VPN disconnects, no traffic can leak outside the tunnel.

### 7. AutoReconnect
**File**: `AutoReconnect.cs`

IP address monitoring for detecting VPN disconnections.

#### Purpose:
Periodically checks external IP address to verify VPN is working correctly.

#### Usage:

```csharp
var autoReconnect = new AutoReconnect();

// Get current external IP
string currentIp = autoReconnect.getConnectedIPAddress();

// Check if IP changed (VPN dropped)
if (startIp != currentIp)
{
    // Reconnect to VPN
    connectionManager.connectToVPN(...);
}
```

#### Configuration:
- Uses `https://api.ipify.org` for IP detection
- 5-second timeout
- Up to 10 retry attempts
- Returns `"broken"` if all attempts fail

#### Typical Implementation Pattern:

```csharp
// In timer tick event (e.g., every 30 seconds)
private void CheckConnectionTimer_Tick(object sender, EventArgs e)
{
    string currentIp = autoReconnect.getConnectedIPAddress();
    
    if (connectionManager.isConnectionBroken(startIp, currentIp))
    {
        // Connection lost - reconnect
        if (Properties.Settings.Default.autoReconnect)
        {
            connectionManager.connectToVPN(protocol, server, entry, user, pass, l2tpKey);
        }
    }
}
```

### 8. AutostartManager
**File**: `AutostartManager.cs`

Windows startup integration for auto-launching VPN on boot.

#### Features:
- Registry-based autostart (preferred)
- Shortcut-based autostart (legacy)
- Removal of autostart entries

#### Methods:

```csharp
// Add application to Windows startup
AutostartManager.AddApplicationToStartup();

// Remove from startup
AutostartManager.RemoveApplicationFromStartup();

// Shortcut-based methods (legacy)
AutostartManager.saveFileInAutostart();
AutostartManager.removeFileInAutostart();
```

#### Registry Approach (Recommended):

```csharp
// Adds to: HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Run
AutostartManager.AddApplicationToStartup();

// Removes from registry
AutostartManager.RemoveApplicationFromStartup();
```

#### Shortcut Approach (Legacy):
Creates a `.lnk` file in the Windows Startup folder. Note: The `CreateShortcut()` method has the `.Save()` call commented out, suggesting this approach is deprecated.

### 9. CheckUpdates
**File**: `CheckUpdates.cs`

Simple update checker that downloads version information from remote text file.

#### Text File Format:
```
AppName;1.2.3.4;https://example.com/download.exe
```

#### Usage:

```csharp
var updateChecker = new CheckUpdates("https://example.com/update.txt");

if (updateChecker.version > currentVersion)
{
    // Show update notification
    MessageBox.Show($"New version available: {updateChecker.version}");
    Process.Start(updateChecker.newdownloadlink);
}
```

#### Properties:
- `appname`: Application name from file
- `version`: Parsed version number
- `newdownloadlink`: Download URL for new version

### 10. Configuration
**File**: `Configuration.cs`

Static configuration values for the VPN system.

```csharp
public class Configuration
{
    // Auto-reconnect check interval (10 seconds)
    public static int autoreconnectTimeInterval = 10000;
    
    // Connection status check interval (30 seconds)
    public static int connectionTimerInterval = 30000;
    
    // VPN state monitoring interval (10 seconds)
    public static int vpnTimerInterval = 10000;
    
    // OpenVPN management interface
    public static string socketAddress = "localhost";
    public static int socketPort = 12343;
}
```

### 11. Encrypter
**File**: `Utilities/Encrypter.cs`

AES-256 encryption utility for storing sensitive data (passwords, credentials).

#### Features:
- **AES-256** encryption with CBC mode
- **PKCS7** padding
- **Random salt and IV** for each encryption
- **1000 iterations** for key derivation (PBKDF2)

#### Usage:

```csharp
// Encrypt sensitive data
string encrypted = Encrypter.Encrypt("myPassword123", "masterKey");
// Store encrypted string in settings/database

// Decrypt when needed
string decrypted = Encrypter.Decrypt(encrypted, "masterKey");
```

#### Security Notes:
- Salt and IV are prepended to ciphertext (no separate storage needed)
- Uses `RijndaelManaged` with 256-bit block size
- Master key should be stored securely (not hardcoded)

### 12. Interfaces

#### IVPNView
**File**: `IVPNView.cs`

Interface that UI forms must implement to receive updates from connection manager.

```csharp
interface IVPNView
{
    bool isInvokeRequired();
    object Invoke(Delegate method);
    void updateState(string state);
    void addToLog(string log, string message);
    void UpdateConnectionStatistics(long bytesReceived, long bytesTransmitted);
}
```

**States**:
- `"connecting"`: Connection in progress
- `"connected"`: Successfully connected
- `"disconnected"`: Not connected
- RAS states: `"Authenticating"`, `"Authenticated"`, `"DeviceConnected"`, etc.

#### IOpenVPN
**File**: `IOpenVPN.cs`

Callback interface for OpenVPN state updates.

```csharp
interface IOpenVPN
{
    void updateOpenVPNState(string state);
}
```

**OpenVPN States**:
- `"CONNECTING"`: Establishing connection
- `"WAIT"`: Waiting for server
- `"AUTH"`: Authenticating
- `"GET_CONFIG"`: Retrieving configuration
- `"ASSIGN_IP"`: Getting IP address
- `"ADD_ROUTES"`: Adding routes
- `"CONNECTED"`: Fully connected
- `"RECONNECTING"`: Reconnecting after failure

## Data Structures

### ServerInformation
**File**: `ServerInformation.cs`

```csharp
class ServerInformation
{
    public string DnsAddress;        // vpn.example.com
    public string country;           // "United States"
    public string city;              // "New York"
    public string[] protocols;       // ["PPTP", "L2TP", "OpenVPN"]
    public string name;              // Display name
    public string note;              // Additional information
    public string torrentP2P;        // "Yes" or "No"
    public string smartVPN;          // "Yes" or "No"
    public string[] ip;              // DNS server IPs
}
```

### OpenVPNConnectionInfo
**File**: `OpenVPNConnectionInfo.cs`

```csharp
class OpenVPNConnectionInfo
{
    public string host;              // Server hostname
    public string username;          // Username
    public string password;          // Password
    public string protocol;          // "tcp" or "udp"
    public int port;                 // 443, 1194, etc.
}
```

### StandardVpnProtocol
**File**: `StandardVpnProtocol.cs`

```csharp
public enum StandardVpnProtocol
{
    PPTP,   // Point-to-Point Tunneling Protocol
    L2TP,   // Layer 2 Tunneling Protocol
    SSTP    // Secure Socket Tunneling Protocol
}
```

### AuthApiResponse
**File**: `AuthApi.cs`

```csharp
public enum AuthApiResponse
{
    Success,              // Authentication successful
    InvalidCredentials,   // Wrong username or password
    AccountDisabled,      // Account has been disabled
    ServerError           // Cannot reach authentication server
}
```

## Complete Integration Example

Here's a comprehensive example showing how to implement a VPN client using the Base library:

```csharp
using SmartDNSProxy_VPN_Client;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

public class VPNClientForm : Form, IVPNView
{
    private VPNConnectionManager connectionManager;
    private OpenVPN openVPN;
    private NetworkManagment networkMgmt;
    private ServerListClient serverClient;
    private AutoReconnect autoReconnect;
    private Timer reconnectTimer;
    private Timer statsTimer;
    
    private string startIp;
    private string currentProtocol;
    private string currentEntryName;
    private ServerInformation currentServer;
    
    public VPNClientForm()
    {
        InitializeComponents();
        InitializeVPN();
    }
    
    private void InitializeVPN()
    {
        // Initialize components
        openVPN = new OpenVPN();
        networkMgmt = new NetworkManagment();
        connectionManager = new VPNConnectionManager(this, openVPN, networkMgmt);
        serverClient = new ServerListClient();
        autoReconnect = new AutoReconnect();
        
        // Load server list
        LoadServers();
        
        // Setup timers
        reconnectTimer = new Timer();
        reconnectTimer.Interval = Configuration.autoreconnectTimeInterval;
        reconnectTimer.Tick += ReconnectTimer_Tick;
        
        statsTimer = new Timer();
        statsTimer.Interval = 1000; // Update every second
        statsTimer.Tick += StatsTimer_Tick;
    }
    
    private async void LoadServers()
    {
        try
        {
            var servers = await Task.Run(() => serverClient.GetServerList());
            serverListBox.DataSource = servers;
            serverListBox.DisplayMember = "ServerName";
        }
        catch (Exception ex)
        {
            addToLog("Error", $"Failed to load servers: {ex.Message}");
        }
    }
    
    private async void ConnectButton_Click(object sender, EventArgs e)
    {
        try
        {
            // Authenticate user
            var authResult = await AuthApi.Authorize(
                usernameTextBox.Text, 
                passwordTextBox.Text
            );
            
            if (authResult != AuthApiResponse.Success)
            {
                MessageBox.Show($"Authentication failed: {authResult}");
                return;
            }
            
            // Get selected server and protocol
            currentServer = (ServerInformation)serverListBox.SelectedItem;
            currentProtocol = protocolComboBox.SelectedItem.ToString();
            currentEntryName = $"VPN_{currentProtocol}";
            
            // Get current IP before connecting
            startIp = autoReconnect.getConnectedIPAddress();
            addToLog("Info", $"Initial IP: {startIp}");
            
            // Connect to VPN
            connectionManager.connectToVPN(
                currentProtocol,
                currentServer,
                currentEntryName,
                usernameTextBox.Text,
                passwordTextBox.Text,
                l2tpKeyTextBox.Text // Only used for L2TP
            );
            
            // Start monitoring
            reconnectTimer.Start();
            statsTimer.Start();
        }
        catch (Exception ex)
        {
            addToLog("Error", $"Connection failed: {ex.Message}");
        }
    }
    
    private void DisconnectButton_Click(object sender, EventArgs e)
    {
        reconnectTimer.Stop();
        statsTimer.Stop();
        
        connectionManager.VPNDisconnect(currentProtocol, currentEntryName);
        
        addToLog("Info", "Disconnected from VPN");
    }
    
    private void ReconnectTimer_Tick(object sender, EventArgs e)
    {
        string currentIp = autoReconnect.getConnectedIPAddress();
        
        if (connectionManager.isConnectionBroken(startIp, currentIp))
        {
            addToLog("Warning", "Connection broken - attempting to reconnect");
            
            connectionManager.VPNDisconnect(currentProtocol, currentEntryName, false);
            
            // Wait a moment then reconnect
            Task.Delay(2000).ContinueWith(_ =>
            {
                connectionManager.connectToVPN(
                    currentProtocol,
                    currentServer,
                    currentEntryName,
                    usernameTextBox.Text,
                    passwordTextBox.Text,
                    l2tpKeyTextBox.Text
                );
            });
        }
    }
    
    private void StatsTimer_Tick(object sender, EventArgs e)
    {
        connectionManager.UpdateConnectionStatistics();
    }
    
    private void KillSwitchCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        if (killSwitchCheckBox.Checked)
        {
            if (!networkMgmt.isFirewallEnabled())
            {
                MessageBox.Show("Firewall must be enabled for Kill Switch");
                killSwitchCheckBox.Checked = false;
                return;
            }
            
            networkMgmt.disableInternetConnections();
            networkMgmt.allowSmartDNSProxyApp();
            addToLog("Security", "Kill Switch enabled");
        }
        else
        {
            networkMgmt.enableInternetConnections();
            networkMgmt.removeSmartDNSRule();
            addToLog("Security", "Kill Switch disabled");
        }
    }
    
    // IVPNView implementation
    public bool isInvokeRequired() => InvokeRequired;
    
    public void updateState(string state)
    {
        statusLabel.Text = $"Status: {state}";
        
        if (state.ToLower() == "connected")
        {
            connectButton.Enabled = false;
            disconnectButton.Enabled = true;
            
            string newIp = autoReconnect.getConnectedIPAddress();
            addToLog("Info", $"Connected! New IP: {newIp}");
        }
        else if (state.ToLower() == "disconnected")
        {
            connectButton.Enabled = true;
            disconnectButton.Enabled = false;
        }
    }
    
    public void addToLog(string category, string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        logTextBox.AppendText($"[{timestamp}] [{category}] {message}\r\n");
    }
    
    public void UpdateConnectionStatistics(long bytesReceived, long bytesTransmitted)
    {
        receivedLabel.Text = $"Downloaded: {FormatBytes(bytesReceived)}";
        transmittedLabel.Text = $"Uploaded: {FormatBytes(bytesTransmitted)}";
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
        // Cleanup on exit
        if (connectionManager.IsConnectionActive() || 
            connectionManager.isOpenVPNConnected())
        {
            connectionManager.VPNDisconnect(currentProtocol, currentEntryName);
        }
        
        // Restore DNS settings
        networkMgmt.changeDNSOnExit(this, EventArgs.Empty);
        
        // Disable kill switch if enabled
        if (killSwitchCheckBox.Checked)
        {
            networkMgmt.enableInternetConnections();
            networkMgmt.removeSmartDNSRule();
        }
        
        base.OnFormClosing(e);
    }
}
```

## Common Usage Patterns

### Pattern 1: Basic VPN Connection Flow

```csharp
// 1. Authenticate
var authResult = await AuthApi.Authorize(username, password);
if (authResult != AuthApiResponse.Success) return;

// 2. Get servers
var servers = serverClient.GetServerList();
var selectedServer = servers.First();

// 3. Connect
connectionManager.connectToVPN("OpenVPN", selectedServer, "MyVPN", 
                               username, password, null);

// 4. Monitor connection
while (isConnected)
{
    await Task.Delay(10000);
    string ip = autoReconnect.getConnectedIPAddress();
    if (connectionManager.isConnectionBroken(startIp, ip))
    {
        // Reconnect
    }
}

// 5. Disconnect
connectionManager.VPNDisconnect("OpenVPN", "MyVPN");
```

### Pattern 2: Protocol Switching

```csharp
private void SwitchProtocol(string newProtocol)
{
    // Disconnect current
    connectionManager.VPNDisconnect(currentProtocol, currentEntry);
    
    // Small delay
    await Task.Delay(1000);
    
    // Connect with new protocol
    currentProtocol = newProtocol;
    connectionManager.connectToVPN(newProtocol, server, 
                                   $"VPN_{newProtocol}", 
                                   user, pass, l2tpKey);
}
```

### Pattern 3: Auto-Reconnect with Exponential Backoff

```csharp
private int reconnectAttempts = 0;
private const int maxReconnectAttempts = 5;

private async void SmartReconnect()
{
    if (reconnectAttempts >= maxReconnectAttempts)
    {
        addToLog("Error", "Max reconnection attempts reached");
        return;
    }
    
    reconnectAttempts++;
    int delay = (int)Math.Pow(2, reconnectAttempts) * 1000; // Exponential backoff
    
    addToLog("Info", $"Reconnecting in {delay/1000} seconds (attempt {reconnectAttempts})");
    await Task.Delay(delay);
    
    connectionManager.connectToVPN(protocol, server, entry, user, pass, key);
}
```

### Pattern 4: Secure Credential Storage

```csharp
// Saving credentials
private void SaveCredentials(string username, string password)
{
    string masterKey = Environment.MachineName + Environment.UserName;
    string encryptedPassword = Encrypter.Encrypt(password, masterKey);
    
    Properties.Settings.Default.Username = username;
    Properties.Settings.Default.EncryptedPassword = encryptedPassword;
    Properties.Settings.Default.Save();
}

// Loading credentials
private (string username, string password) LoadCredentials()
{
    string username = Properties.Settings.Default.Username;
    string encrypted = Properties.Settings.Default.EncryptedPassword;
    
    if (string.IsNullOrEmpty(encrypted))
        return (username, "");
    
    string masterKey = Environment.MachineName + Environment.UserName;
    string password = Encrypter.Decrypt(encrypted, masterKey);
    
    return (username, password);
}
```

## Configuration and Settings

### Required Application Settings

Your consuming application should define these settings:

```xml
<!-- app.config or Settings.settings -->
<configuration>
  <applicationSettings>
    <YourApp.Properties.Settings>
      <!-- Protocol selection -->
      <setting name="selectSettingsProtocol" serializeAs="String">
        <value>tcp</value>
      </setting>
      
      <!-- Port selection -->
      <setting name="selectOptionPort" serializeAs="String">
        <value>443</value>
      </setting>
      
      <!-- Custom server DNS (optional) -->
      <setting name="selectedServerDns" serializeAs="String">
        <value></value>
      </setting>
      
      <!-- Auto-reconnect feature -->
      <setting name="autoReconnect" serializeAs="String">
        <value>True</value>
      </setting>
      
      <!-- CSV last modified timestamp -->
      <setting name="CSVLastModified" serializeAs="String">
        <value></value>
      </setting>
    </YourApp.Properties.Settings>
  </applicationSettings>
</configuration>
```

### OpenVPN Directory Structure

The Base library expects OpenVPN to be installed in a specific directory:

```
YourApp.exe
└── OpenVPN/
    ├── openvpn.exe       (OpenVPN client executable)
    ├── log.txt           (Generated during connection)
    ├── ca.txt            (Temporary certificate file)
    ├── cert.txt          (Temporary certificate file)
    └── key.txt           (Temporary key file)
```

The `OpenVPN` class automatically handles certificate and credential file creation/deletion.

## Error Handling

### Common Error Scenarios

1. **Authentication Failure**
```csharp
var result = await AuthApi.Authorize(user, pass);
if (result == AuthApiResponse.InvalidCredentials)
{
    MessageBox.Show("Invalid username or password");
}
else if (result == AuthApiResponse.AccountDisabled)
{
    MessageBox.Show("Your account has been disabled. Please contact support.");
}
else if (result == AuthApiResponse.ServerError)
{
    MessageBox.Show("Cannot reach authentication server. Check your internet connection.");
}
```

2. **Connection Failures**
```csharp
// Monitor connection state
void updateState(string state)
{
    if (state.ToLower().Contains("error") || state.ToLower().Contains("failed"))
    {
        addToLog("Error", $"Connection failed: {state}");
        
        // Show specific error messages
        if (state.Contains("691")) // Invalid credentials
        {
            MessageBox.Show("Invalid VPN credentials");
        }
        else if (state.Contains("800")) // No answer
        {
            MessageBox.Show("VPN server not responding");
        }
    }
}
```

3. **Network Stack Issues**
```csharp
try
{
    connectionManager.connectToVPN(...);
}
catch (Exception ex)
{
    if (ex.Message.Contains("winsock"))
    {
        // Network stack might be corrupted
        // VPNConnectionManager.resetNetworkStack() is called automatically
        MessageBox.Show("Network stack reset required. Please try again.");
    }
}
```

4. **OpenVPN Process Failures**
```csharp
// OpenVPN class automatically detects process termination
private void afterOpenVPNConnected(ServerInformation selectedServer)
{
    if (Process.GetProcessesByName("openvpn").Length == 0)
    {
        // Process not running - connection failed
        updateState("disconnected");
        addToLog("Error", "OpenVPN process terminated unexpectedly");
    }
}
```

## Security Considerations

### 1. Credential Protection
- Always use `Encrypter` class for storing passwords
- Never store passwords in plain text
- Use machine-specific master keys

### 2. Kill Switch Implementation
```csharp
// Prevent IP leaks when VPN drops
if (enableKillSwitch)
{
    networkMgmt.disableInternetConnections();  // Block all traffic
    networkMgmt.allowSmartDNSProxyApp();       // Allow only VPN app
    
    // Application crash/force close will keep firewall rules active
    // This is intentional - protects even if app crashes
}

// Proper cleanup when disabling
networkMgmt.enableInternetConnections();
networkMgmt.removeSmartDNSRule();
```

### 3. DNS Leak Prevention
```csharp
// VPNConnectionManager automatically manages DNS
// - Stores original DNS settings
// - Sets VPN DNS on connect
// - Restores original DNS on disconnect

// Manual DNS restoration if app crashes
protected override void OnApplicationExit(EventArgs e)
{
    networkMgmt.changeDNSOnExit(this, e);
}
```

### 4. Certificate Management
The OpenVPN certificate is embedded in code. For production:
- Consider loading from encrypted resource
- Implement certificate pinning
- Add certificate expiration checking

## Performance Optimization

### 1. Reduce Server List Downloads
```csharp
// ServerListClient automatically caches server list
// Only downloads if remote file is newer
// Keeps backup in case of download failure

// Manual cache refresh
Properties.Settings.Default.CSVLastModified = DateTime.MinValue;
var servers = serverClient.GetServerList(); // Forces download
```

### 2. Connection Statistics Throttling
```csharp
// Don't update UI too frequently
private DateTime lastStatsUpdate = DateTime.MinValue;

void UpdateConnectionStatistics(long rx, long tx)
{
    if ((DateTime.Now - lastStatsUpdate).TotalMilliseconds < 500)
        return; // Update max every 500ms
        
    lastStatsUpdate = DateTime.Now;
    // Update UI
}
```

### 3. Async Operations
```csharp
// All connection operations use Task.Run internally
// But for UI responsiveness, also use async/await in caller:

private async Task ConnectAsync()
{
    await Task.Run(() => {
        connectionManager.connectToVPN(...);
    });
    
    // UI thread free during connection
}
```

## Troubleshooting

### Issue: VPN connects but no internet
**Solution**: Check DNS settings
```csharp
// Force DNS refresh
networkMgmt.setDNS(adapterName, "8.8.8.8", "8.8.4.4");
```

### Issue: OpenVPN won't connect
**Solution**: Check management port
```csharp
// Ensure nothing else is using port 12343
// Change Configuration.socketPort if needed
Configuration.socketPort = 12344;
```

### Issue: L2TP connection fails
**Solution**: Verify pre-shared key
```csharp
// L2TP requires pre-shared key
connectionManager.connectToVPN("L2TP", server, entry, 
                               user, pass, 
                               "YourPreSharedKey"); // Don't pass null
```

### Issue: Connection statistics not updating
**Solution**: Check timer and connection type
```csharp
// Statistics only work for RAS connections (PPTP, L2TP, SSTP)
// OpenVPN doesn't provide statistics through DotRas

if (currentProtocol != "OpenVPN")
{
    connectionManager.UpdateConnectionStatistics();
}
```

### Issue: Auto-reconnect not working
**Solution**: Verify IP checking
```csharp
// Ensure startIp is captured BEFORE connecting
startIp = autoReconnect.getConnectedIPAddress();
connectionManager.connectToVPN(...);

// Then in timer:
string currentIp = autoReconnect.getConnectedIPAddress();
bool broken = connectionManager.isConnectionBroken(startIp, currentIp);
```

## Best Practices

### 1. Always Implement IVPNView
Your main form must implement this interface to receive connection updates:

```csharp
public class MainForm : Form, IVPNView
{
    // Implement all interface methods
    // Use Invoke() for thread-safe UI updates
}
```

### 2. Use Try-Catch for All VPN Operations
```csharp
try
{
    connectionManager.connectToVPN(...);
}
catch (Exception ex)
{
    addToLog("Error", ex.Message);
    // Handle gracefully
}
```

### 3. Clean Up Resources
```csharp
protected override void OnFormClosing(FormClosingEventArgs e)
{
    // Disconnect VPN
    connectionManager.VPNDisconnect(protocol, entry);
    
    // Restore DNS
    networkMgmt.changeDNSOnExit(this, EventArgs.Empty);
    
    // Remove firewall rules
    if (killSwitchEnabled)
    {
        networkMgmt.enableInternetConnections();
        networkMgmt.removeSmartDNSRule();
    }
    
    // Stop timers
    reconnectTimer?.Stop();
    statsTimer?.Stop();
    
    base.OnFormClosing(e);
}
```

### 4. Log Everything
```csharp
// Comprehensive logging helps with debugging
addToLog("Auth", "Authenticating user");
addToLog("Server", "Selected server: " + server.name);
addToLog("Connection", "Connecting via " + protocol);
addToLog("DNS", "DNS set to " + dns1 + ", " + dns2);
addToLog("Status", "Connected successfully");
```

### 5. Handle Network Changes
```csharp
// Monitor network adapter changes
NetworkChange.NetworkAddressChanged += (s, e) =>
{
    addToLog("Network", "Network adapter changed");
    
    // May need to reconnect or update DNS
    if (isConnected)
    {
        // Reapply DNS settings
        setDNSAddress(currentServer, true);
    }
};
```

## Testing Checklist

Before deploying your VPN client:

- [ ] Test all four protocols (OpenVPN, PPTP, L2TP, SSTP)
- [ ] Verify authentication with valid and invalid credentials
- [ ] Test auto-reconnect functionality
- [ ] Verify kill switch blocks all traffic when enabled
- [ ] Check DNS leak protection
- [ ] Test server list download and caching
- [ ] Verify connection statistics display
- [ ] Test application startup integration
- [ ] Check memory leaks during long connections
- [ ] Verify proper cleanup on application exit
- [ ] Test on different Windows versions (7, 8, 10, 11)
- [ ] Check behavior with multiple network adapters
- [ ] Test reconnection after network interruption
- [ ] Verify proper error messages display
- [ ] Check logs for sensitive information

## Dependencies and Prerequisites

### Required NuGet Packages
- **DotRas** (RAS API wrapper)
- **Newtonsoft.Json** (JSON serialization)

### System Requirements
- Windows 7 or later
- .NET Framework 4.5 or later
- Administrator privileges (for VPN connection, DNS changes, firewall rules)
- OpenVPN executable (for OpenVPN protocol)

### Required Windows Features
- Remote Access Service (RAS)
- Windows Firewall (for kill switch)
- Network interface management permissions

## License and Credits

This shared library is part of the Global Stealth VPNs project and is used across multiple branded VPN clients.

**Authentication API**: `https://auth-api.glbls.net`  
**Server List**: `https://network.glbls.net/vpnnetwork/VPNServerList.csv`

## Support and Contribution

For issues, questions, or contributions related to the Base shared library, please contact the development team or create an issue in the project repository.

---

**Last Updated**: October 2025  
**Version**: Based on current project state  
**Maintained by**: Global Stealth VPNs Development Team

