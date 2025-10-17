# VPNConnectionManager API

## Overview

`VPNConnectionManager` is the central orchestrator for all VPN operations. It manages protocol selection, connection lifecycle, DNS configuration, and integrates with both OpenVPN and RAS drivers.

**Namespace**: `SmartDNSProxy_VPN_Client`  
**Assembly**: Shared Project (compiled into consuming application)

## Class Declaration

```csharp
class VPNConnectionManager
```

## Constructor

### VPNConnectionManager

```csharp
public VPNConnectionManager(
    IVPNView view, 
    OpenVPN openVPNDriver, 
    NetworkManagment networkManagment
)
```

**Parameters:**
- `view` (IVPNView): Implementation of IVPNView interface for UI callbacks
- `openVPNDriver` (OpenVPN): Initialized OpenVPN driver instance
- `networkManagment` (NetworkManagment): Initialized network management instance

**Example:**
```csharp
var view = this; // assuming form implements IVPNView
var openVPN = new OpenVPN();
var networkMgmt = new NetworkManagment();
var manager = new VPNConnectionManager(view, openVPN, networkMgmt);
```

## Public Methods

### connectToVPN

Initiates a VPN connection with the specified protocol and server.

```csharp
public void connectToVPN(
    string selectedProtocol, 
    ServerInformation selectedServer, 
    string entryName,
    string login, 
    string password, 
    string L2TPpassword
)
```

**Parameters:**
- `selectedProtocol` (string): Protocol to use - "OpenVPN", "PPTP", "L2TP", or "SSTP"
- `selectedServer` (ServerInformation): Server information including hostname and DNS
- `entryName` (string): Unique name for this VPN connection entry
- `login` (string): Username for VPN authentication
- `password` (string): Password for VPN authentication
- `L2TPpassword` (string): Pre-shared key for L2TP (null for other protocols)

**Behavior:**
- Executes asynchronously using `Task.Run()`
- Updates state to "connecting" immediately
- For RAS protocols: Creates phonebook entry and dials
- For OpenVPN: Launches OpenVPN process and manages socket connection
- Automatically configures DNS settings
- Callbacks are made to IVPNView for state updates

**Example:**
```csharp
var server = new ServerInformation 
{
    DnsAddress = "us-ny.vpn.example.com",
    ip = new[] { "8.8.8.8", "8.8.4.4" },
    smartVPN = "No"
};

manager.connectToVPN(
    selectedProtocol: "OpenVPN",
    selectedServer: server,
    entryName: "MyVPN_OpenVPN",
    login: "myusername",
    password: "mypassword",
    L2TPpassword: null // Not needed for OpenVPN
);
```

**Thread Safety:** Method is thread-safe. UI callbacks are marshalled via `Invoke()`.

---

### VPNDisconnect

Disconnects the active VPN connection.

```csharp
public void VPNDisconnect(
    string protocol, 
    string entryName, 
    bool notifyUi = true
)
```

**Parameters:**
- `protocol` (string): Protocol of current connection - "OpenVPN", "PPTP", "L2TP", or "SSTP"
- `entryName` (string): Entry name used when connecting
- `notifyUi` (bool): Whether to notify UI of disconnection (default: true)

**Behavior:**
- Executes asynchronously using `Task.Run()`
- For RAS protocols: Hangs up connection and deletes phonebook entry
- For OpenVPN: Kills OpenVPN process and closes socket
- Restores original DNS settings
- Updates state to "disconnected" if notifyUi is true

**Example:**
```csharp
// Disconnect and notify UI
manager.VPNDisconnect("OpenVPN", "MyVPN_OpenVPN");

// Disconnect without UI notification (e.g., during reconnection)
manager.VPNDisconnect("OpenVPN", "MyVPN_OpenVPN", notifyUi: false);
```

---

### isOpenVPNConnected

Checks if OpenVPN is currently connected.

```csharp
public bool isOpenVPNConnected()
```

**Returns:** `true` if OpenVPN is connected, `false` otherwise

**Example:**
```csharp
if (manager.isOpenVPNConnected())
{
    Console.WriteLine("OpenVPN is connected");
}
```

---

### IsConnectionActive

Checks if a RAS connection (PPTP/L2TP/SSTP) is currently active.

```csharp
public bool IsConnectionActive()
```

**Returns:** `true` if RAS connection is active and in Connected state, `false` otherwise

**Example:**
```csharp
if (manager.IsConnectionActive())
{
    Console.WriteLine("RAS connection is active");
}
```

---

### UpdateConnectionStatistics

Retrieves and updates connection statistics for RAS connections.

```csharp
public void UpdateConnectionStatistics()
```

**Behavior:**
- Only works for RAS connections (PPTP, L2TP, SSTP)
- Does nothing for OpenVPN connections
- Calls `IVPNView.UpdateConnectionStatistics()` with current values
- Executes asynchronously

**Example:**
```csharp
// Call periodically (e.g., every second) in a timer
private void statsTimer_Tick(object sender, EventArgs e)
{
    manager.UpdateConnectionStatistics();
}
```

---

### GetConnectionByEntryName

Checks if a specific RAS connection entry exists and is active.

```csharp
public bool GetConnectionByEntryName(string entryName)
```

**Parameters:**
- `entryName` (string): Name of the VPN entry to check

**Returns:** `true` if connection exists and is active, `false` otherwise

**Example:**
```csharp
if (manager.GetConnectionByEntryName("MyVPN_PPTP"))
{
    Console.WriteLine("PPTP connection found");
}
```

---

### isConnectionBroken

Determines if the VPN connection has been broken or lost.

```csharp
public bool isConnectionBroken(string startIp, string currentIp)
```

**Parameters:**
- `startIp` (string): IP address before connecting to VPN
- `currentIp` (string): Current IP address

**Returns:** `true` if connection is broken, `false` if still active

**Logic:**
- Returns `true` if currentIp is "broken" (IP detection failed)
- Returns `true` if not connected via OpenVPN or RAS
- Returns `true` if IP address has reverted to original (startIp == currentIp)
- Returns `false` if connected and IP is different from start

**Example:**
```csharp
string startIp = autoRecon.getConnectedIPAddress();
manager.connectToVPN(...);

// Later, check if connection dropped
string currentIp = autoRecon.getConnectedIPAddress();
if (manager.isConnectionBroken(startIp, currentIp))
{
    Console.WriteLine("Connection lost - reconnecting");
    manager.connectToVPN(...);
}
```

## State Updates

The VPNConnectionManager provides state updates through the `IVPNView` interface:

### Connection States

**For All Protocols:**
- `"connecting"` - Connection attempt in progress
- `"connected"` - Successfully connected
- `"disconnected"` - Not connected

**For RAS Protocols (PPTP/L2TP/SSTP):**
- `"OpenPort"` - Opening network port
- `"ConnectDevice"` - Connecting device
- `"DeviceConnected"` - Device connected
- `"Authenticate"` - Authenticating credentials
- `"Authenticated"` - Authentication successful
- Various error states

**For OpenVPN:**
- `"CONNECTING"` - Establishing connection
- `"WAIT"` - Waiting for server
- `"AUTH"` - Authenticating
- `"GET_CONFIG"` - Retrieving configuration
- `"ASSIGN_IP"` - Assigning IP address
- `"ADD_ROUTES"` - Adding routes
- `"CONNECTED"` - Fully connected
- `"RECONNECTING"` - Attempting reconnection

## DNS Management

The VPNConnectionManager automatically manages DNS settings during VPN connections:

1. **Before Connection:** Stores current DNS settings for all network adapters
2. **During Connection:** Sets VPN-specific DNS servers based on server configuration
3. **After Disconnection:** Restores original DNS settings

DNS is set based on ServerInformation:
- If `smartVPN == "no"`: Uses OpenDNS (208.67.222.222, 208.67.220.220)
- If server provides DNS IPs: Uses those
- Otherwise: Uses Google DNS (8.8.8.8, 8.8.4.4)

## Protocol-Specific Behavior

### OpenVPN
- Launches `openvpn.exe` process with parameters
- Connects to management interface (localhost:12343)
- Monitors connection state via socket
- Automatically generates temporary certificate and credential files
- Cleans up temporary files on disconnect

### PPTP/L2TP/SSTP
- Uses Windows RAS (Remote Access Service)
- Creates phonebook entry in system
- Uses DotRas library for connection management
- Provides real-time statistics (bytes sent/received)
- Requires pre-shared key for L2TP

## Error Handling

The class uses callbacks for error reporting. Errors are communicated through:

1. **State Updates:** Error states passed to `IVPNView.updateState()`
2. **Log Messages:** Detailed errors passed to `IVPNView.addToLog()`
3. **Exceptions:** Critical failures may throw exceptions

**Recommended Practice:**
```csharp
try
{
    manager.connectToVPN(...);
}
catch (Exception ex)
{
    addToLog("Error", $"Connection failed: {ex.Message}");
    // Handle error appropriately
}
```

## Thread Safety

- All public methods are thread-safe
- Connection operations execute on background threads
- UI updates are marshalled to UI thread via `IVPNView.Invoke()`
- Internal state is protected from race conditions

## Best Practices

### 1. Proper Initialization
```csharp
var openVPN = new OpenVPN();
var networkMgmt = new NetworkManagment();
var manager = new VPNConnectionManager(this, openVPN, networkMgmt);
```

### 2. Handle State Updates
```csharp
public void updateState(string state)
{
    switch (state.ToLower())
    {
        case "connected":
            EnableDisconnectButton();
            break;
        case "disconnected":
            EnableConnectButton();
            break;
        case "connecting":
            ShowConnectingSpinner();
            break;
    }
}
```

### 3. Implement Auto-Reconnect
```csharp
private Timer reconnectTimer;
private string initialIp;

private void EnableAutoReconnect()
{
    initialIp = new AutoReconnect().getConnectedIPAddress();
    reconnectTimer = new Timer { Interval = 30000 };
    reconnectTimer.Tick += (s, e) =>
    {
        string currentIp = new AutoReconnect().getConnectedIPAddress();
        if (manager.isConnectionBroken(initialIp, currentIp))
        {
            manager.VPNDisconnect(protocol, entry, false);
            Task.Delay(2000).ContinueWith(_ => 
                manager.connectToVPN(protocol, server, entry, user, pass, key)
            );
        }
    };
    reconnectTimer.Start();
}
```

### 4. Cleanup on Exit
```csharp
protected override void OnFormClosing(FormClosingEventArgs e)
{
    if (manager.IsConnectionActive() || manager.isOpenVPNConnected())
    {
        manager.VPNDisconnect(currentProtocol, currentEntry);
    }
    base.OnFormClosing(e);
}
```

## Performance Considerations

- Connection operations are asynchronous and won't block the UI
- Statistics updates should be throttled (recommend max 1 per second)
- DNS operations involve system calls and may take 1-2 seconds
- OpenVPN process startup takes 3-5 seconds typically

## Limitations

- Only one active connection at a time per instance
- RAS connections limited by Windows (typically 1 active per protocol)
- OpenVPN statistics not available (only for RAS protocols)
- Requires administrator privileges for all operations

## Related APIs

- [IVPNView](IVPNView.md) - Required interface implementation
- [OpenVPN](OpenVPN.md) - OpenVPN driver
- [Ras](Ras.md) - RAS driver
- [NetworkManagment](NetworkManagment.md) - Network and DNS management
- [ServerInformation](DataModels.md#serverinformation) - Server data structure

## See Also

- [Getting Started Guide](GettingStarted.md)
- [Examples](Examples.md)
- [Error Handling](ErrorHandling.md)

---

**API Version**: 1.0  
**Last Updated**: October 2025

