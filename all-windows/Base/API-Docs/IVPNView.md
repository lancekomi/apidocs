# IVPNView Interface API

## Overview

`IVPNView` is the primary interface that your UI must implement to receive callbacks from the VPN connection manager. It provides methods for state updates, logging, statistics, and thread marshalling.

**Namespace**: `SmartDNSProxy_VPN_Client`  
**Assembly**: Shared Project

## Interface Declaration

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

## Methods

### isInvokeRequired

Determines if thread marshalling is required for UI updates.

```csharp
bool isInvokeRequired()
```

**Returns:** `true` if calling thread is not the UI thread, `false` otherwise

**Implementation:**
```csharp
public bool isInvokeRequired()
{
    return InvokeRequired; // For Windows Forms
}
```

**Purpose:** Used internally by VPNConnectionManager to determine if `Invoke()` is needed.

---

### Invoke

Marshals a delegate execution to the UI thread.

```csharp
object Invoke(Delegate method)
```

**Parameters:**
- `method` (Delegate): Delegate to execute on UI thread

**Returns:** Result from the invoked delegate

**Implementation:**
```csharp
public object Invoke(Delegate method)
{
    return base.Invoke(method); // For Windows Forms
}
```

**Purpose:** Ensures thread-safe UI updates from background threads.

---

### updateState

Receives VPN connection state updates.

```csharp
void updateState(string state)
```

**Parameters:**
- `state` (string): Current connection state

**Common State Values:**

| State | Description |
|-------|-------------|
| `"connecting"` | Connection attempt in progress |
| `"connected"` | Successfully connected |
| `"disconnected"` | Not connected |
| `"OpenPort"` | (RAS) Opening network port |
| `"DeviceConnected"` | (RAS) Device connected |
| `"Authenticate"` | (RAS) Authenticating |
| `"Authenticated"` | (RAS) Authentication successful |
| `"CONNECTING"` | (OpenVPN) Establishing connection |
| `"WAIT"` | (OpenVPN) Waiting for server |
| `"AUTH"` | (OpenVPN) Authenticating |
| `"CONNECTED"` | (OpenVPN) Fully connected |

**Implementation Example:**
```csharp
public void updateState(string state)
{
    // Update UI to show current state
    statusLabel.Text = $"Status: {state}";
    
    // Handle specific states
    switch (state.ToLower())
    {
        case "connected":
            connectButton.Enabled = false;
            disconnectButton.Enabled = true;
            connectionPanel.BackColor = Color.Green;
            ShowNotification("VPN Connected", "You are now protected");
            break;
            
        case "disconnected":
            connectButton.Enabled = true;
            disconnectButton.Enabled = false;
            connectionPanel.BackColor = Color.Red;
            break;
            
        case "connecting":
            connectButton.Enabled = false;
            disconnectButton.Enabled = false;
            connectionPanel.BackColor = Color.Yellow;
            progressBar.Style = ProgressBarStyle.Marquee;
            break;
    }
    
    // Log state change
    addToLog("State", $"Connection state changed to: {state}");
}
```

---

### addToLog

Receives log messages for display or persistence.

```csharp
void addToLog(string log, string message)
```

**Parameters:**
- `log` (string): Log category/type
- `message` (string): Log message

**Common Log Categories:**

| Category | Description |
|----------|-------------|
| `"OpenVPN"` | OpenVPN-related messages |
| `"PPTPL2TPSSTP"` | RAS protocol messages |
| `"Info"` | Informational messages |
| `"Error"` | Error messages |
| `"Warning"` | Warning messages |
| `"DNS"` | DNS configuration messages |
| `"Auth"` | Authentication messages |

**Implementation Example:**
```csharp
public void addToLog(string category, string message)
{
    // Add timestamp
    string timestamp = DateTime.Now.ToString("HH:mm:ss");
    string logEntry = $"[{timestamp}] [{category}] {message}";
    
    // Display in UI
    if (logTextBox.InvokeRequired)
    {
        logTextBox.Invoke(new Action(() => 
        {
            logTextBox.AppendText(logEntry + Environment.NewLine);
            logTextBox.ScrollToCaret();
        }));
    }
    else
    {
        logTextBox.AppendText(logEntry + Environment.NewLine);
        logTextBox.ScrollToCaret();
    }
    
    // Also write to file
    File.AppendAllText("vpn-log.txt", logEntry + Environment.NewLine);
    
    // Limit log size in UI
    if (logTextBox.Lines.Length > 1000)
    {
        var lines = logTextBox.Lines.Skip(500).ToArray();
        logTextBox.Lines = lines;
    }
}
```

---

### UpdateConnectionStatistics

Receives connection statistics for display.

```csharp
void UpdateConnectionStatistics(long bytesReceived, long bytesTransmitted)
```

**Parameters:**
- `bytesReceived` (long): Total bytes received (downloaded)
- `bytesTransmitted` (long): Total bytes transmitted (uploaded)

**Note:** Only works for RAS connections (PPTP, L2TP, SSTP). Not available for OpenVPN.

**Implementation Example:**
```csharp
public void UpdateConnectionStatistics(long bytesReceived, long bytesTransmitted)
{
    // Format bytes for display
    downloadLabel.Text = $"↓ {FormatBytes(bytesReceived)}";
    uploadLabel.Text = $"↑ {FormatBytes(bytesTransmitted)}";
    
    // Calculate speed (if tracking over time)
    if (lastStatsUpdate != DateTime.MinValue)
    {
        var elapsed = (DateTime.Now - lastStatsUpdate).TotalSeconds;
        var downloadSpeed = (bytesReceived - lastBytesReceived) / elapsed;
        var uploadSpeed = (bytesTransmitted - lastBytesTransmitted) / elapsed;
        
        downloadSpeedLabel.Text = $"{FormatBytes((long)downloadSpeed)}/s";
        uploadSpeedLabel.Text = $"{FormatBytes((long)uploadSpeed)}/s";
    }
    
    lastBytesReceived = bytesReceived;
    lastBytesTransmitted = bytesTransmitted;
    lastStatsUpdate = DateTime.Now;
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

## Complete Implementation Example

```csharp
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SmartDNSProxy_VPN_Client;

public partial class MainForm : Form, IVPNView
{
    private VPNConnectionManager connectionManager;
    private DateTime lastStatsUpdate = DateTime.MinValue;
    private long lastBytesReceived = 0;
    private long lastBytesTransmitted = 0;
    
    public MainForm()
    {
        InitializeComponent();
        
        // Initialize VPN manager
        var openVPN = new OpenVPN();
        var networkMgmt = new NetworkManagment();
        connectionManager = new VPNConnectionManager(this, openVPN, networkMgmt);
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
        statusLabel.Text = $"Status: {state}";
        statusLabel.ForeColor = GetStateColor(state);
        
        switch (state.ToLower())
        {
            case "connected":
                connectButton.Text = "Connected";
                connectButton.Enabled = false;
                disconnectButton.Enabled = true;
                serverPanel.Enabled = false;
                StartStatisticsTimer();
                break;
                
            case "disconnected":
                connectButton.Text = "Connect";
                connectButton.Enabled = true;
                disconnectButton.Enabled = false;
                serverPanel.Enabled = true;
                StopStatisticsTimer();
                break;
                
            case "connecting":
                connectButton.Text = "Connecting...";
                connectButton.Enabled = false;
                disconnectButton.Enabled = false;
                serverPanel.Enabled = false;
                break;
        }
        
        addToLog("State", state);
    }
    
    public void addToLog(string category, string message)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string logEntry = $"[{timestamp}] [{category}] {message}";
        
        logTextBox.AppendText(logEntry + Environment.NewLine);
        logTextBox.SelectionStart = logTextBox.Text.Length;
        logTextBox.ScrollToCaret();
        
        // Write to log file
        try
        {
            File.AppendAllText("vpn.log", logEntry + Environment.NewLine);
        }
        catch { }
        
        // Trim log if too large
        if (logTextBox.Lines.Length > 1000)
        {
            var lines = logTextBox.Lines.Skip(500).ToArray();
            logTextBox.Lines = lines;
        }
    }
    
    public void UpdateConnectionStatistics(long bytesReceived, long bytesTransmitted)
    {
        downloadLabel.Text = $"Downloaded: {FormatBytes(bytesReceived)}";
        uploadLabel.Text = $"Uploaded: {FormatBytes(bytesTransmitted)}";
        
        // Calculate speeds
        if (lastStatsUpdate != DateTime.MinValue)
        {
            var seconds = (DateTime.Now - lastStatsUpdate).TotalSeconds;
            if (seconds > 0)
            {
                var dlSpeed = (bytesReceived - lastBytesReceived) / seconds;
                var ulSpeed = (bytesTransmitted - lastBytesTransmitted) / seconds;
                
                downloadSpeedLabel.Text = $"↓ {FormatBytes((long)dlSpeed)}/s";
                uploadSpeedLabel.Text = $"↑ {FormatBytes((long)ulSpeed)}/s";
            }
        }
        
        lastBytesReceived = bytesReceived;
        lastBytesTransmitted = bytesTransmitted;
        lastStatsUpdate = DateTime.Now;
    }
    
    // Helper Methods
    
    private Color GetStateColor(string state)
    {
        switch (state.ToLower())
        {
            case "connected":
                return Color.Green;
            case "disconnected":
                return Color.Red;
            case "connecting":
                return Color.Orange;
            default:
                return Color.Black;
        }
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
    
    private Timer statsTimer;
    
    private void StartStatisticsTimer()
    {
        if (statsTimer == null)
        {
            statsTimer = new Timer { Interval = 1000 };
            statsTimer.Tick += (s, e) => connectionManager.UpdateConnectionStatistics();
        }
        statsTimer.Start();
    }
    
    private void StopStatisticsTimer()
    {
        statsTimer?.Stop();
        lastStatsUpdate = DateTime.MinValue;
        downloadSpeedLabel.Text = "";
        uploadSpeedLabel.Text = "";
    }
}
```

## Best Practices

### 1. Thread-Safe UI Updates

Always check if invoke is required:
```csharp
public void updateState(string state)
{
    if (InvokeRequired)
    {
        Invoke(new Action(() => updateState(state)));
        return;
    }
    
    // Safe to update UI now
    statusLabel.Text = state;
}
```

### 2. Error Handling

```csharp
public void addToLog(string category, string message)
{
    try
    {
        // UI updates
        logTextBox.AppendText($"[{category}] {message}\n");
    }
    catch (ObjectDisposedException)
    {
        // Form is closing
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"Error in addToLog: {ex.Message}");
    }
}
```

### 3. Performance Optimization

```csharp
public void UpdateConnectionStatistics(long bytesReceived, long bytesTransmitted)
{
    // Throttle updates to avoid UI flicker
    if ((DateTime.Now - lastUpdate).TotalMilliseconds < 500)
        return;
        
    lastUpdate = DateTime.Now;
    
    // Update UI
    downloadLabel.Text = FormatBytes(bytesReceived);
    uploadLabel.Text = FormatBytes(bytesTransmitted);
}
```

### 4. Resource Cleanup

```csharp
protected override void Dispose(bool disposing)
{
    if (disposing)
    {
        statsTimer?.Stop();
        statsTimer?.Dispose();
        components?.Dispose();
    }
    base.Dispose(disposing);
}
```

## Testing

### Mock Implementation for Testing

```csharp
public class MockVPNView : IVPNView
{
    public List<string> States = new List<string>();
    public List<string> Logs = new List<string>();
    
    public bool isInvokeRequired() => false;
    public object Invoke(Delegate method) => method.DynamicInvoke();
    
    public void updateState(string state)
    {
        States.Add(state);
        Console.WriteLine($"State: {state}");
    }
    
    public void addToLog(string category, string message)
    {
        Logs.Add($"[{category}] {message}");
        Console.WriteLine($"[{category}] {message}");
    }
    
    public void UpdateConnectionStatistics(long rx, long tx)
    {
        Console.WriteLine($"Stats: RX={rx}, TX={tx}");
    }
}
```

## Related APIs

- [VPNConnectionManager](VPNConnectionManager.md) - Uses IVPNView for callbacks
- [IOpenVPN](IOpenVPN.md) - Similar interface for OpenVPN-specific callbacks

## See Also

- [Getting Started Guide](GettingStarted.md)
- [Examples](Examples.md)
- [Best Practices](BestPractices.md)

---

**API Version**: 1.0  
**Last Updated**: October 2025

