# Base VPN Library - API Documentation

## Overview

The **Base VPN Library** (`VpnBase.shproj`) is a shared C# library providing complete VPN client functionality for Windows applications. It supports multiple VPN protocols (OpenVPN, PPTP, L2TP, SSTP) with features including authentication, connection management, DNS configuration, and security controls.

**Namespace**: `SmartDNSProxy_VPN_Client`  
**Target Framework**: .NET Framework 4.5+  
**Platform**: Windows 7 or later

## Purpose

This library serves as the foundation for multiple white-label VPN client applications by providing:
- Multi-protocol VPN connection management
- Centralized authentication
- Network and DNS management
- Auto-reconnection and monitoring
- Security features (kill switch, DNS leak prevention)
- Server list management
- Configuration and credential storage

## API Documentation Structure

### Core APIs
1. **[VPNConnectionManager](VPNConnectionManager.md)** - Primary connection orchestrator
2. **[OpenVPN](OpenVPN.md)** - OpenVPN protocol implementation
3. **[Ras](Ras.md)** - Windows RAS (PPTP/L2TP/SSTP) driver

### Service APIs
4. **[AuthApi](AuthApi.md)** - User authentication service
5. **[ServerListClient](ServerListClient.md)** - VPN server discovery and management
6. **[NetworkManagment](NetworkManagment.md)** - DNS and firewall configuration
7. **[AutoReconnect](AutoReconnect.md)** - Connection monitoring and IP detection

### Utility APIs
8. **[AutostartManager](AutostartManager.md)** - Windows startup integration
9. **[CheckUpdates](CheckUpdates.md)** - Application update checking
10. **[Encrypter](Encrypter.md)** - Credential encryption utilities
11. **[Configuration](Configuration.md)** - Static configuration values

### Interfaces
12. **[IVPNView](IVPNView.md)** - UI integration interface
13. **[IOpenVPN](IOpenVPN.md)** - OpenVPN callback interface

### Data Models
14. **[Data Models](DataModels.md)** - ServerInformation, OpenVPNConnectionInfo, enumerations

## Quick Start

### Basic Implementation

```csharp
using SmartDNSProxy_VPN_Client;

// 1. Implement the IVPNView interface in your form
public class MainForm : Form, IVPNView
{
    private VPNConnectionManager connectionManager;
    
    public MainForm()
    {
        // 2. Initialize VPN components
        var openVPN = new OpenVPN();
        var networkMgmt = new NetworkManagment();
        connectionManager = new VPNConnectionManager(this, openVPN, networkMgmt);
    }
    
    // 3. Authenticate and connect
    private async void ConnectButton_Click(object sender, EventArgs e)
    {
        var authResult = await AuthApi.Authorize(username, password);
        if (authResult == AuthApiResponse.Success)
        {
            var server = new ServerInformation { 
                DnsAddress = "vpn.example.com" 
            };
            connectionManager.connectToVPN("OpenVPN", server, "MyVPN", 
                                          username, password, null);
        }
    }
    
    // 4. Implement IVPNView interface methods
    public void updateState(string state) 
        => statusLabel.Text = state;
    public void addToLog(string category, string message) 
        => logBox.AppendText($"[{category}] {message}\n");
    public void UpdateConnectionStatistics(long rx, long tx) { }
    public bool isInvokeRequired() => InvokeRequired;
    public object Invoke(Delegate method) => base.Invoke(method);
}
```

## API Categories

### Connection Management
- `VPNConnectionManager.connectToVPN()` - Initiate VPN connection
- `VPNConnectionManager.VPNDisconnect()` - Disconnect VPN
- `VPNConnectionManager.isOpenVPNConnected()` - Check OpenVPN status
- `VPNConnectionManager.IsConnectionActive()` - Check RAS status

### Authentication
- `AuthApi.Authorize()` - Authenticate user credentials

### Server Management
- `ServerListClient.GetServerList()` - Retrieve available VPN servers

### Network Configuration
- `NetworkManagment.setDNS()` - Configure DNS servers
- `NetworkManagment.disableInternetConnections()` - Enable kill switch
- `NetworkManagment.enableInternetConnections()` - Disable kill switch

### Monitoring
- `AutoReconnect.getConnectedIPAddress()` - Get current public IP
- `VPNConnectionManager.isConnectionBroken()` - Check connection health
- `VPNConnectionManager.UpdateConnectionStatistics()` - Update traffic stats

### Security
- `Encrypter.Encrypt()` - Encrypt sensitive data
- `Encrypter.Decrypt()` - Decrypt sensitive data
- `NetworkManagment.allowSmartDNSProxyApp()` - Configure firewall rules

## Common Use Cases

### Use Case 1: Basic VPN Connection
```csharp
var manager = new VPNConnectionManager(view, new OpenVPN(), new NetworkManagment());
var server = new ServerInformation { DnsAddress = "vpn.example.com" };
manager.connectToVPN("OpenVPN", server, "MyVPN", "user", "password", null);
```

### Use Case 2: Enable Kill Switch
```csharp
var networkMgmt = new NetworkManagment();
networkMgmt.disableInternetConnections();
networkMgmt.allowSmartDNSProxyApp();
```

### Use Case 3: Monitor Connection
```csharp
var autoRecon = new AutoReconnect();
string currentIp = autoRecon.getConnectedIPAddress();
if (manager.isConnectionBroken(startIp, currentIp))
{
    // Reconnect
}
```

### Use Case 4: Secure Credential Storage
```csharp
string key = Environment.MachineName;
string encrypted = Encrypter.Encrypt(password, key);
// Store encrypted
string decrypted = Encrypter.Decrypt(encrypted, key);
```

## Dependencies

### Required NuGet Packages
- **DotRas** - Windows RAS API wrapper
- **Newtonsoft.Json** (optional) - JSON serialization

### System Requirements
- Windows 7 or later
- .NET Framework 4.5 or later
- Administrator privileges (for VPN operations)
- OpenVPN executable (for OpenVPN protocol)

### Windows APIs
- `rasapi32.dll` - Remote Access Service
- `NetFwTypeLib` - Windows Firewall
- `System.Management` - Network interface management

## Thread Safety

All VPN operations that update the UI use the `IVPNView.Invoke()` pattern for thread-safe UI updates. Your implementation of `IVPNView` must handle cross-thread marshalling correctly.

## Error Handling

The library uses a combination of:
- **Exceptions** for critical failures
- **Callbacks** for state updates and errors
- **Return values** (bool, null) for status checks

Always wrap VPN operations in try-catch blocks and implement proper error logging.

## Additional Resources

- **[Getting Started Guide](GettingStarted.md)** - Step-by-step integration guide
- **[Examples](Examples.md)** - Complete code examples
- **[Error Handling](ErrorHandling.md)** - Error codes and handling strategies
- **[Best Practices](BestPractices.md)** - Recommended patterns and practices

## Version Information

**API Version**: 1.0  
**Last Updated**: October 2025  
**Compatibility**: Windows 7, 8, 8.1, 10, 11

---

**Next Steps:**
1. Review [VPNConnectionManager](VPNConnectionManager.md) for the primary API
2. Implement [IVPNView](IVPNView.md) in your application
3. Follow the [Getting Started Guide](GettingStarted.md)
4. Review [Examples](Examples.md) for complete implementations

