# Getting Started with Base VPN Library

## Introduction

This guide will walk you through integrating the Base VPN Library into your Windows application. By the end, you'll have a working VPN client with connection management, authentication, and monitoring capabilities.

**Time Required:** 30-45 minutes  
**Skill Level:** Intermediate C#/.NET knowledge

## Prerequisites

### Software Requirements
- Visual Studio 2015 or later
- .NET Framework 4.5 or later
- Windows 7 or later (for testing)

### Knowledge Requirements
- C# programming
- Windows Forms or WPF
- Basic networking concepts
- Async/await patterns

### System Requirements
- Administrator privileges (for VPN operations)
- OpenVPN executable (for OpenVPN protocol)
- TAP-Windows driver (for OpenVPN)

## Step 1: Add Shared Project Reference

### Add the Base Shared Project

1. Right-click your solution in Solution Explorer
2. Select **Add → Existing Project**
3. Browse to `all-windows/Base/VpnBase.shproj`
4. Click **Open**

### Reference the Shared Project

1. Right-click your application project
2. Select **Add → Reference**
3. Go to **Shared Projects** tab
4. Check `VpnBase`
5. Click **OK**

**Result:** All Base library code is now compiled directly into your project.

## Step 2: Install Dependencies

### Add NuGet Packages

Open Package Manager Console and run:

```powershell
Install-Package DotRas
Install-Package Newtonsoft.Json  # Optional, for JSON operations
```

### Add COM References

1. Right-click **References** in your project
2. Select **Add Reference**
3. Go to **COM** tab
4. Add:
   - **NetFwTypeLib** (Windows Firewall)
   - **IWshRuntimeLibrary** (Windows Script Host)

## Step 3: Setup OpenVPN (Optional)

If you plan to support OpenVPN protocol:

### Directory Structure

Create this folder structure in your application directory:

```
YourApp/
  ├─ YourApp.exe
  └─ OpenVPN/
      ├─ openvpn.exe
      ├─ libeay32.dll
      ├─ ssleay32.dll
      └─ (other OpenVPN files)
```

### Download OpenVPN

1. Download OpenVPN from [openvpn.net](https://openvpn.net/community-downloads/)
2. Extract `openvpn.exe` and DLL files
3. Copy to `OpenVPN/` folder in your application

### Install TAP Driver

Users must have TAP-Windows driver installed. Include in your installer or prompt user to install.

## Step 4: Implement IVPNView Interface

Your main form must implement `IVPNView`:

```csharp
using System;
using System.Windows.Forms;
using SmartDNSProxy_VPN_Client;

namespace YourVPNApp
{
    public partial class MainForm : Form, IVPNView
    {
        public MainForm()
        {
            InitializeComponent();
        }
        
        // IVPNView Implementation
        
        public bool isInvokeRequired()
        {
            return InvokeRequired;
        }
        
        public object Invoke(Delegate method)
        {
            return base.Invoke(method);
        }
        
        public void updateState(string state)
        {
            // Update UI based on connection state
            statusLabel.Text = $"Status: {state}";
            
            switch (state.ToLower())
            {
                case "connected":
                    connectButton.Enabled = false;
                    disconnectButton.Enabled = true;
                    break;
                case "disconnected":
                    connectButton.Enabled = true;
                    disconnectButton.Enabled = false;
                    break;
                case "connecting":
                    connectButton.Enabled = false;
                    disconnectButton.Enabled = false;
                    break;
            }
        }
        
        public void addToLog(string category, string message)
        {
            // Add to log display
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            logTextBox.AppendText($"[{timestamp}] [{category}] {message}\r\n");
        }
        
        public void UpdateConnectionStatistics(long bytesReceived, long bytesTransmitted)
        {
            // Update statistics display
            downloadLabel.Text = $"Downloaded: {FormatBytes(bytesReceived)}";
            uploadLabel.Text = $"Uploaded: {FormatBytes(bytesTransmitted)}";
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
    }
}
```

## Step 5: Initialize VPN Components

Add initialization code to your form:

```csharp
public partial class MainForm : Form, IVPNView
{
    private VPNConnectionManager connectionManager;
    private OpenVPN openVPN;
    private NetworkManagment networkManagement;
    private ServerListClient serverClient;
    private AutoReconnect autoReconnect;
    
    public MainForm()
    {
        InitializeComponent();
        InitializeVPN();
    }
    
    private void InitializeVPN()
    {
        try
        {
            // Initialize components
            openVPN = new OpenVPN();
            networkManagement = new NetworkManagment();
            connectionManager = new VPNConnectionManager(
                this, 
                openVPN, 
                networkManagement
            );
            
            serverClient = new ServerListClient();
            autoReconnect = new AutoReconnect();
            
            addToLog("Init", "VPN components initialized successfully");
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to initialize VPN: {ex.Message}",
                "Initialization Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }
}
```

## Step 6: Load Server List

Load available VPN servers:

```csharp
private async void MainForm_Load(object sender, EventArgs e)
{
    await LoadServers();
}

private async Task LoadServers()
{
    try
    {
        addToLog("Servers", "Loading server list...");
        
        // Load servers on background thread
        var servers = await Task.Run(() => serverClient.GetServerList());
        
        // Populate server list in UI
        serverListBox.DataSource = servers;
        serverListBox.DisplayMember = "ServerName";
        
        addToLog("Servers", $"Loaded {servers.Length} servers");
    }
    catch (Exception ex)
    {
        addToLog("Error", $"Failed to load servers: {ex.Message}");
        MessageBox.Show(
            "Failed to load server list. Check your internet connection.",
            "Error",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning
        );
    }
}
```

## Step 7: Implement Authentication

Add login functionality:

```csharp
private async void LoginButton_Click(object sender, EventArgs e)
{
    // Validate input
    if (string.IsNullOrWhiteSpace(usernameTextBox.Text) ||
        string.IsNullOrWhiteSpace(passwordTextBox.Text))
    {
        MessageBox.Show("Please enter username and password");
        return;
    }
    
    // Disable UI during authentication
    loginButton.Enabled = false;
    loginProgressBar.Visible = true;
    
    try
    {
        addToLog("Auth", "Authenticating user...");
        
        // Authenticate
        var result = await AuthApi.Authorize(
            usernameTextBox.Text,
            passwordTextBox.Text
        );
        
        switch (result)
        {
            case AuthApiResponse.Success:
                addToLog("Auth", "Authentication successful");
                OnLoginSuccess();
                break;
                
            case AuthApiResponse.InvalidCredentials:
                MessageBox.Show(
                    "Invalid username or password",
                    "Authentication Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                passwordTextBox.Clear();
                passwordTextBox.Focus();
                break;
                
            case AuthApiResponse.AccountDisabled:
                MessageBox.Show(
                    "Your account has been disabled.\nPlease contact support.",
                    "Account Disabled",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                break;
                
            case AuthApiResponse.ServerError:
                MessageBox.Show(
                    "Cannot reach authentication server.\nPlease check your internet connection.",
                    "Server Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                break;
        }
    }
    catch (Exception ex)
    {
        addToLog("Error", $"Authentication error: {ex.Message}");
        MessageBox.Show($"Authentication error: {ex.Message}");
    }
    finally
    {
        loginButton.Enabled = true;
        loginProgressBar.Visible = false;
    }
}

private void OnLoginSuccess()
{
    // Save credentials (encrypted)
    SaveCredentials(usernameTextBox.Text, passwordTextBox.Text);
    
    // Show main VPN panel
    loginPanel.Visible = false;
    vpnPanel.Visible = true;
}
```

## Step 8: Implement Connection

Add connect/disconnect functionality:

```csharp
private string currentProtocol;
private string currentEntryName;
private ServerInformation currentServer;
private string username;
private string password;

private void ConnectButton_Click(object sender, EventArgs e)
{
    try
    {
        // Get selected server and protocol
        currentServer = (ServerInformation)serverListBox.SelectedItem;
        currentProtocol = protocolComboBox.SelectedItem.ToString();
        currentEntryName = $"VPN_{currentProtocol}";
        
        // Validate selection
        if (currentServer == null)
        {
            MessageBox.Show("Please select a server");
            return;
        }
        
        // Get stored credentials
        username = usernameTextBox.Text;
        password = passwordTextBox.Text;
        
        // Get L2TP pre-shared key if needed
        string l2tpKey = null;
        if (currentProtocol == "L2TP")
        {
            l2tpKey = Settings.Default.L2TPPreSharedKey;
        }
        
        addToLog("Connection", $"Connecting to {currentServer.name} via {currentProtocol}");
        
        // Initiate connection
        connectionManager.connectToVPN(
            currentProtocol,
            currentServer,
            currentEntryName,
            username,
            password,
            l2tpKey
        );
    }
    catch (Exception ex)
    {
        addToLog("Error", $"Connection failed: {ex.Message}");
        MessageBox.Show($"Connection failed: {ex.Message}");
    }
}

private void DisconnectButton_Click(object sender, EventArgs e)
{
    try
    {
        addToLog("Connection", "Disconnecting...");
        
        connectionManager.VPNDisconnect(
            currentProtocol,
            currentEntryName
        );
    }
    catch (Exception ex)
    {
        addToLog("Error", $"Disconnect failed: {ex.Message}");
    }
}
```

## Step 9: Add Connection Monitoring

Implement auto-reconnect and statistics:

```csharp
private Timer reconnectTimer;
private Timer statsTimer;
private string initialIP;

private void EnableMonitoring()
{
    // Get initial IP
    initialIP = autoReconnect.getConnectedIPAddress();
    
    // Setup reconnect timer
    reconnectTimer = new Timer();
    reconnectTimer.Interval = 30000; // 30 seconds
    reconnectTimer.Tick += ReconnectTimer_Tick;
    reconnectTimer.Start();
    
    // Setup statistics timer
    statsTimer = new Timer();
    statsTimer.Interval = 1000; // 1 second
    statsTimer.Tick += StatsTimer_Tick;
    statsTimer.Start();
}

private void DisableMonitoring()
{
    reconnectTimer?.Stop();
    statsTimer?.Stop();
}

private void ReconnectTimer_Tick(object sender, EventArgs e)
{
    try
    {
        string currentIP = autoReconnect.getConnectedIPAddress();
        
        if (connectionManager.isConnectionBroken(initialIP, currentIP))
        {
            addToLog("Warning", "Connection lost - attempting reconnection");
            
            // Disconnect
            connectionManager.VPNDisconnect(currentProtocol, currentEntryName, false);
            
            // Wait and reconnect
            Task.Delay(2000).ContinueWith(_ =>
            {
                connectionManager.connectToVPN(
                    currentProtocol,
                    currentServer,
                    currentEntryName,
                    username,
                    password,
                    currentProtocol == "L2TP" ? Settings.Default.L2TPPreSharedKey : null
                );
            });
        }
    }
    catch (Exception ex)
    {
        addToLog("Error", $"Reconnect check failed: {ex.Message}");
    }
}

private void StatsTimer_Tick(object sender, EventArgs e)
{
    connectionManager.UpdateConnectionStatistics();
}
```

## Step 10: Handle Application Cleanup

Proper cleanup when closing:

```csharp
protected override void OnFormClosing(FormClosingEventArgs e)
{
    try
    {
        // Ask user if connected
        if (connectionManager.IsConnectionActive() || 
            connectionManager.isOpenVPNConnected())
        {
            var result = MessageBox.Show(
                "VPN is still connected. Disconnect and exit?",
                "Confirm Exit",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );
            
            if (result == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }
        }
        
        // Stop timers
        DisableMonitoring();
        
        // Disconnect VPN
        if (currentProtocol != null && currentEntryName != null)
        {
            connectionManager.VPNDisconnect(currentProtocol, currentEntryName, false);
        }
        
        // Restore DNS
        networkManagement.changeDNSOnExit(this, EventArgs.Empty);
        
        addToLog("App", "Application closing");
    }
    catch (Exception ex)
    {
        addToLog("Error", $"Cleanup error: {ex.Message}");
    }
    
    base.OnFormClosing(e);
}
```

## Step 11: Add Credential Storage

Securely store credentials:

```csharp
private void SaveCredentials(string username, string password)
{
    try
    {
        string masterKey = Environment.MachineName + Environment.UserName;
        string encryptedPassword = Encrypter.Encrypt(password, masterKey);
        
        Settings.Default.Username = username;
        Settings.Default.EncryptedPassword = encryptedPassword;
        Settings.Default.Save();
        
        addToLog("Settings", "Credentials saved");
    }
    catch (Exception ex)
    {
        addToLog("Error", $"Failed to save credentials: {ex.Message}");
    }
}

private void LoadCredentials()
{
    try
    {
        string username = Settings.Default.Username;
        string encrypted = Settings.Default.EncryptedPassword;
        
        if (!string.IsNullOrEmpty(encrypted))
        {
            string masterKey = Environment.MachineName + Environment.UserName;
            string password = Encrypter.Decrypt(encrypted, masterKey);
            
            usernameTextBox.Text = username;
            passwordTextBox.Text = password;
            
            addToLog("Settings", "Credentials loaded");
        }
    }
    catch (Exception ex)
    {
        addToLog("Error", $"Failed to load credentials: {ex.Message}");
    }
}
```

## Step 12: Test Your Application

### Testing Checklist

- [ ] Application builds without errors
- [ ] Login with valid credentials works
- [ ] Server list loads correctly
- [ ] Can connect via OpenVPN
- [ ] Can connect via PPTP/L2TP/SSTP
- [ ] Connection state updates display
- [ ] Statistics update (for RAS protocols)
- [ ] Disconnect works properly
- [ ] Auto-reconnect triggers on connection loss
- [ ] Application closes cleanly
- [ ] DNS restored on exit
- [ ] Credentials saved and loaded

### Common Issues

**Issue:** "Access Denied" errors  
**Solution:** Run as Administrator

**Issue:** OpenVPN won't connect  
**Solution:** Ensure OpenVPN files in `OpenVPN/` folder and TAP driver installed

**Issue:** RAS connections fail  
**Solution:** Check Windows RAS service is running

**Issue:** DNS not restoring  
**Solution:** Ensure `changeDNSOnExit()` called in form closing event

## Next Steps

Congratulations! You now have a working VPN client. Consider adding:

1. **UI Enhancements**
   - Connection status indicator
   - Server ping/latency display
   - Favorite servers
   - Connection history

2. **Features**
   - Kill switch toggle
   - Auto-start on Windows boot
   - Custom DNS servers
   - Split tunneling

3. **Advanced Features**
   - Multiple simultaneous connections
   - Traffic routing rules
   - Connection profiles
   - Bandwidth limiting

## Additional Resources

- [API Documentation Index](INDEX.md)
- [Complete Examples](Examples.md)
- [Best Practices](BestPractices.md)
- [Troubleshooting Guide](ErrorHandling.md)

## Getting Help

If you encounter issues:

1. Check the [Error Handling](ErrorHandling.md) guide
2. Review application logs
3. Enable verbose logging
4. Check Windows Event Viewer
5. Contact the development team

---

**Guide Version**: 1.0  
**Last Updated**: October 2025  
**Estimated Completion Time**: 30-45 minutes

