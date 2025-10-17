# OpenVPN API

## Overview

`OpenVPN` class provides complete OpenVPN protocol implementation including process management, socket-based management interface, and connection state monitoring.

**Namespace**: `SmartDNSProxy_VPN_Client`  
**Assembly**: Shared Project

## Class Declaration

```csharp
class OpenVPN
```

## Constructor

### OpenVPN

```csharp
public OpenVPN()
```

Creates a new OpenVPN instance and initializes configuration.

**Example:**
```csharp
var openVPN = new OpenVPN();
```

## Public Properties

### openVPNPath

```csharp
public string openVPNPath
```

**Type**: `string`  
**Default**: `AppDomain.CurrentDomain.BaseDirectory + @"\OpenVPN"`  
**Description**: Directory where OpenVPN executable and files are located

---

### logFileName

```csharp
public string logFileName
```

**Type**: `string`  
**Default**: `"log.txt"`  
**Description**: Name of OpenVPN log file

---

### certificateFile

```csharp
public string certificateFile
```

**Type**: `string`  
**Description**: Path to temporary certificate file (set during connection)

---

### passwordFile

```csharp
public string passwordFile
```

**Type**: `string`  
**Description**: Path to temporary password file (set during connection)

---

### openVPNDelegate

```csharp
public IOpenVPN openVPNDelegate
```

**Type**: `IOpenVPN`  
**Description**: Callback interface for OpenVPN state updates

## Public Methods

### connect

Initiates an OpenVPN connection to the specified server.

```csharp
public void connect(
    OpenVPNConnectionInfo connectionInfo, 
    Action OnOpenVPnConnectedCallback
)
```

**Parameters:**
- `connectionInfo` (OpenVPNConnectionInfo): Connection configuration
  - `host` (string): Server hostname or IP
  - `username` (string): VPN username
  - `password` (string): VPN password
  - `protocol` (string): "tcp" or "udp"
  - `port` (int): Port number (typically 443 or 1194)
- `OnOpenVPnConnectedCallback` (Action): Callback invoked when connection is established

**Behavior:**
1. Generates temporary certificate file with embedded CA certificate
2. Creates temporary password file with credentials
3. Launches openvpn.exe with appropriate parameters
4. Connects to OpenVPN management interface (localhost:12343)
5. Monitors connection state via socket
6. Invokes callback when fully connected

**Example:**
```csharp
var connectionInfo = new OpenVPNConnectionInfo
{
    host = "vpn.example.com",
    username = "myuser",
    password = "mypassword",
    protocol = "tcp",
    port = 443
};

openVPN.connect(connectionInfo, () => 
{
    Console.WriteLine("OpenVPN connected successfully!");
    // Perform post-connection tasks
});
```

**OpenVPN Parameters Generated:**
```
--dev tun
--proto tcp/udp
--remote <host> <port>
--resolv-retry 15
--client
--auth-user-pass <passwordFile>
--nobind
--persist-key
--persist-tun
--ns-cert-type server
--comp-lzo
--reneg-sec 0
--verb 3
--ca <certificateFile>
--ip-win32 ipapi
--log log.txt
--management localhost 12343
```

---

### disconnect

Disconnects the OpenVPN connection and cleans up resources.

```csharp
public void disconnect()
```

**Behavior:**
1. Sets socketShouldBeConnected flag to false
2. Kills openvpn.exe process if running
3. Closes management socket
4. Cleans up temporary files (certificate, password)

**Example:**
```csharp
openVPN.disconnect();
```

**Note:** Temporary files (certificate, password) should be deleted by the calling code after disconnection.

---

### isConnected

Checks if OpenVPN is currently connected.

```csharp
public bool isConnected()
```

**Returns:** `true` if connected, `false` otherwise

**Example:**
```csharp
if (openVPN.isConnected())
{
    Console.WriteLine("OpenVPN is connected");
}
else
{
    Console.WriteLine("OpenVPN is not connected");
}
```

## Management Interface

### Socket Communication

OpenVPN uses a TCP socket connection to communicate with the management interface:

**Address:** `localhost` (from Configuration.socketAddress)  
**Port:** `12343` (from Configuration.socketPort)  
**Protocol:** Line-based text protocol

### Management Commands

The class automatically sends these commands:

```
state on          // Enable state updates
signal SIGINT     // Graceful shutdown on disconnect
```

### State Messages

OpenVPN sends state updates in this format:
```
>STATE:<timestamp>,<state>,<description>,<local_ip>,<remote_ip>
```

**Recognized States:**
- `CONNECTING` - Establishing connection
- `WAIT` - Waiting for server response
- `AUTH` - Authenticating
- `GET_CONFIG` - Retrieving configuration
- `ASSIGN_IP` - IP address assignment
- `ADD_ROUTES` - Adding routes
- `CONNECTED` - Fully connected
- `RECONNECTING` - Reconnecting after failure
- `EXITING` - Shutting down

### Example State Update:
```
>STATE:1234567890,CONNECTED,SUCCESS,10.8.0.2,vpn.example.com
```

## Certificate Management

The class embeds a hardcoded CA certificate (Global Stealth, Inc. CA):

**Certificate Details:**
- **Issuer:** Global Stealth, Inc. CA
- **Valid From:** 2015-02-25
- **Valid To:** 2025-02-22
- **Key Size:** 2048-bit RSA

**Certificate File:** Created temporarily in system temp directory with random GUID filename

**Security Note:** Certificate is public (CA cert only, not private key)

## Connection Flow

```
1. connect() called
   ↓
2. Create certificate file in temp directory
   ↓
3. Create password file in temp directory
   ↓
4. Launch openvpn.exe process
   ↓
5. Start socket management thread
   ↓
6. Connect to management socket (localhost:12343)
   ↓
7. Send "state on" command
   ↓
8. Read state updates from socket
   ↓
9. When state = "CONNECTED":
   - Set isOpenVPNConnected = true
   - Invoke OnOpenVPnConnectedCallback
   ↓
10. Continue monitoring until disconnect
```

## Thread Management

### Socket Management Thread

A dedicated thread handles socket communication:

**Thread Name:** "Socket watch"  
**Lifecycle:** Created in `InitializeOpenVPNStatusThread()`, runs until disconnect

**Thread Activities:**
- Connects to management socket
- Reads messages byte-by-byte
- Parses complete lines (terminated by '\n')
- Handles state messages
- Monitors OpenVPN process existence

**Thread Safety:** Uses socket timeout and periodic process checks

## Error Handling

### Connection Failures

If OpenVPN process fails to start or crashes:
- `isConnected()` returns false
- Socket read loop detects missing process
- Callback not invoked if never reached CONNECTED state

### Socket Errors

- `SocketException`: Logged to Debug output, connection continues
- `ThreadAbortException`: Re-thrown (expected during disconnect)
- Other exceptions: Logged, socket reconnection attempted

## Best Practices

### 1. Proper Initialization
```csharp
var openVPN = new OpenVPN();
openVPN.openVPNDelegate = this; // if implementing IOpenVPN
```

### 2. Handle Connection Callback
```csharp
openVPN.connect(connectionInfo, () => 
{
    // Ensure UI updates are marshalled to UI thread
    Invoke(new Action(() => 
    {
        statusLabel.Text = "Connected";
        EnableDisconnectButton();
    }));
});
```

### 3. Monitor Connection State
```csharp
var checkTimer = new Timer { Interval = 10000 }; // 10 seconds
checkTimer.Tick += (s, e) =>
{
    if (!openVPN.isConnected() && shouldBeConnected)
    {
        // Connection lost - attempt reconnection
        openVPN.connect(connectionInfo, callback);
    }
};
```

### 4. Cleanup Temporary Files
```csharp
openVPN.disconnect();

// Clean up temporary files
try
{
    if (File.Exists(openVPN.certificateFile))
        File.Delete(openVPN.certificateFile);
    if (File.Exists(openVPN.passwordFile))
        File.Delete(openVPN.passwordFile);
}
catch (Exception ex)
{
    Debug.WriteLine($"Cleanup failed: {ex.Message}");
}
```

### 5. Implement IOpenVPN for State Updates
```csharp
public class MyForm : Form, IOpenVPN
{
    public void updateOpenVPNState(string state)
    {
        Invoke(new Action(() => 
        {
            statusLabel.Text = $"OpenVPN: {state}";
            
            if (state == "CONNECTED")
                OnConnectionEstablished();
            else if (state == "RECONNECTING")
                OnConnectionLost();
        }));
    }
}
```

## Performance Considerations

- OpenVPN process startup: 3-5 seconds typical
- Socket connection: Usually immediate (localhost)
- State updates: Real-time (< 100ms latency)
- Memory usage: ~10-15 MB for OpenVPN process

## Requirements

### OpenVPN Directory Structure
```
ApplicationDirectory\
  └─ OpenVPN\
      ├─ openvpn.exe       (required)
      ├─ libeay32.dll      (required)
      ├─ ssleay32.dll      (required)
      └─ log.txt           (created by OpenVPN)
```

### TAP Driver
OpenVPN requires TAP-Windows adapter driver to be installed.

## Limitations

- Only one OpenVPN connection per instance
- Certificate cannot be changed without recompiling
- No support for custom OpenVPN configuration files
- Management interface port is fixed (12343)
- No IPv6 configuration options exposed

## Security Considerations

### Credential Files
- Username and password written to temp file in plain text
- Files use random GUID filenames
- Files should be deleted after connection
- Files may remain if application crashes

**Mitigation:** Ensure cleanup in Application.OnExit event

### Certificate
- CA certificate is hardcoded and public
- Expires February 22, 2025
- No certificate pinning implemented

## Troubleshooting

### OpenVPN Won't Connect

**Check:**
1. OpenVPN.exe exists in OpenVPN directory
2. TAP driver is installed (`ipconfig` should show TAP adapter)
3. Port 12343 is not in use
4. Administrator privileges granted
5. Firewall allows OpenVPN.exe

### Connection Drops Immediately

**Check:**
1. Server address is correct
2. Port is open (not blocked by firewall)
3. Credentials are correct
4. Server supports the selected protocol (TCP/UDP)

### Socket Errors

If socket connection fails:
```csharp
// Change the management port if 12343 is in use
Configuration.socketPort = 12344;
```

## Related APIs

- [VPNConnectionManager](VPNConnectionManager.md) - Uses OpenVPN for OpenVPN protocol
- [IOpenVPN](IOpenVPN.md) - State callback interface
- [OpenVPNConnectionInfo](DataModels.md#openvpnconnectioninfo) - Connection parameters
- [Configuration](Configuration.md) - Socket configuration

## See Also

- [OpenVPN Documentation](https://openvpn.net/community-resources/reference-manual-for-openvpn-2-4/)
- [Examples](Examples.md)
- [Error Handling](ErrorHandling.md)

---

**API Version**: 1.0  
**Last Updated**: October 2025

