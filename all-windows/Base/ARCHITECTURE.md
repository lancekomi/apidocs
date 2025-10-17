# Base VPN Library - Architecture Overview

## System Architecture

The Base VPN library follows a modular, layered architecture designed for reusability across multiple VPN client applications.

```
┌─────────────────────────────────────────────────────────────────┐
│                     VPN Client Applications                      │
│  (Getflix VPN, Pizza VPN, Smart DNS Proxy VPN, TrickByte VPN)  │
└───────────────────────────────┬─────────────────────────────────┘
                                │ References Shared Project
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                        Base (VpnBase.shproj)                     │
│                    Shared VPN Functionality                      │
└─────────────────────────────────────────────────────────────────┘
```

## Component Diagram

```
┌──────────────────────────────────────────────────────────────────────┐
│                              UI Layer                                 │
│                         (Implements IVPNView)                         │
└───────────────────────────────┬──────────────────────────────────────┘
                                │
                                ▼
┌──────────────────────────────────────────────────────────────────────┐
│                     VPNConnectionManager                              │
│                    (Central Orchestrator)                             │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │ • Protocol Selection   • Connection Management               │   │
│  │ • DNS Configuration    • State Monitoring                    │   │
│  │ • Statistics Tracking  • Reconnection Logic                  │   │
│  └──────────────────────────────────────────────────────────────┘   │
└───┬───────────────────────────┬──────────────────────────┬───────────┘
    │                           │                          │
    ▼                           ▼                          ▼
┌─────────────┐      ┌──────────────────┐      ┌───────────────────┐
│   OpenVPN   │      │       Ras        │      │  NetworkManagment │
│   Driver    │      │     Driver       │      │                   │
└──────┬──────┘      └────────┬─────────┘      └──────────┬────────┘
       │                      │                           │
       │                      │                           │
   ┌───▼──────────────────────▼───────────────────────────▼────┐
   │            Windows Network Stack / VPN APIs                │
   │  • OpenVPN Process    • RAS API    • Firewall API         │
   │  • Socket Interface   • DotRas     • DNS Management        │
   └────────────────────────────────────────────────────────────┘
```

## Data Flow Diagrams

### 1. Connection Establishment Flow

```
User                UI              VPNConnectionManager    AuthApi    OpenVPN/Ras
 │                  │                        │                │              │
 │──Click Connect──▶│                        │                │              │
 │                  │                        │                │              │
 │                  │──connectToVPN()──────▶│                │              │
 │                  │                        │                │              │
 │                  │                        │──Authorize()──▶│              │
 │                  │                        │◀──Response─────│              │
 │                  │                        │                │              │
 │                  │                        │──connect()────────────────────▶│
 │                  │                        │                │              │
 │                  │◀──updateState()───────│                │              │
 │◀──UI Update─────│                        │                │              │
 │                  │                        │                │              │
 │                  │                        │◀──state updates───────────────│
 │                  │◀──updateState()───────│                │              │
 │◀──UI Update─────│                        │                │              │
```

### 2. Auto-Reconnect Flow

```
Timer                AutoReconnect        VPNConnectionManager    OpenVPN/Ras
 │                        │                        │                    │
 │──Tick─────────────────▶│                        │                    │
 │                        │                        │                    │
 │                        │──getConnectedIP()─────▶│                    │
 │                        │◀──Current IP───────────│                    │
 │                        │                        │                    │
 │                        │──isConnectionBroken()─▶│                    │
 │                        │◀──TRUE/FALSE───────────│                    │
 │                        │                        │                    │
 │                        │                        │──disconnect()─────▶│
 │                        │                        │                    │
 │                        │                        │──connect()────────▶│
 │                        │                        │◀──connected────────│
```

### 3. DNS Management Flow

```
VPNConnectionManager    NetworkManagment    Windows Registry    Network Adapters
       │                       │                    │                  │
       │──setDNS()────────────▶│                    │                  │
       │                       │                    │                  │
       │                       │──Read Current──────▶│                  │
       │                       │◀──DNS Settings──────│                  │
       │                       │                    │                  │
       │                       │──Store Original────▶│                  │
       │                       │                    │                  │
       │                       │──netsh commands────────────────────────▶│
       │                       │                    │                  │
       │                       │──ipconfig /flushdns────────────────────▶│
       │                       │                    │                  │
       │◀──DNS Updated─────────│                    │                  │
```

## Class Hierarchy

```
┌───────────────────────────────────────────────────────────────┐
│                         Interfaces                             │
├───────────────────────────────────────────────────────────────┤
│                                                                │
│  IVPNView                    IOpenVPN                          │
│  ├─ updateState()            ├─ updateOpenVPNState()           │
│  ├─ addToLog()               │                                │
│  ├─ UpdateConnectionStatistics()                              │
│  ├─ isInvokeRequired()                                        │
│  └─ Invoke()                                                  │
│                                                                │
└───────────────────────────────────────────────────────────────┘

┌───────────────────────────────────────────────────────────────┐
│                      Core Classes                              │
├───────────────────────────────────────────────────────────────┤
│                                                                │
│  VPNConnectionManager                                          │
│  ├─ connectToVPN()                                            │
│  ├─ VPNDisconnect()                                           │
│  ├─ UpdateConnectionStatistics()                              │
│  ├─ isConnectionBroken()                                      │
│  └─ setDNSAddress()                                           │
│                                                                │
│  OpenVPN                                                       │
│  ├─ connect()                                                 │
│  ├─ disconnect()                                              │
│  ├─ isConnected()                                             │
│  └─ InitializeOpenVPNStatusThread()                           │
│                                                                │
│  Ras                                                           │
│  ├─ Connect()                                                 │
│  ├─ disconnect()                                              │
│  ├─ isConnected()                                             │
│  └─ createVpnEntry()                                          │
│                                                                │
└───────────────────────────────────────────────────────────────┘

┌───────────────────────────────────────────────────────────────┐
│                    Service Classes                             │
├───────────────────────────────────────────────────────────────┤
│                                                                │
│  AuthApi (static)                NetworkManagment              │
│  └─ Authorize()                  ├─ setDNS()                  │
│                                  ├─ disableInternetConnections()│
│  ServerListClient                ├─ enableInternetConnections()│
│  └─ GetServerList()              └─ allowSmartDNSProxyApp()   │
│                                                                │
│  AutoReconnect                   CheckUpdates                  │
│  └─ getConnectedIPAddress()      ├─ appname                   │
│                                  ├─ version                    │
│  AutostartManager (static)       └─ newdownloadlink           │
│  ├─ AddApplicationToStartup()                                 │
│  └─ RemoveApplicationFromStartup()                            │
│                                                                │
└───────────────────────────────────────────────────────────────┘

┌───────────────────────────────────────────────────────────────┐
│                     Utility Classes                            │
├───────────────────────────────────────────────────────────────┤
│                                                                │
│  Encrypter (static)              Configuration (static)        │
│  ├─ Encrypt()                    ├─ autoreconnectTimeInterval │
│  └─ Decrypt()                    ├─ connectionTimerInterval   │
│                                  ├─ vpnTimerInterval           │
│                                  ├─ socketAddress              │
│                                  └─ socketPort                 │
│                                                                │
└───────────────────────────────────────────────────────────────┘

┌───────────────────────────────────────────────────────────────┐
│                      Data Structures                           │
├───────────────────────────────────────────────────────────────┤
│                                                                │
│  ServerInformation               OpenVPNConnectionInfo         │
│  ├─ DnsAddress                   ├─ host                      │
│  ├─ country                      ├─ username                  │
│  ├─ city                         ├─ password                  │
│  ├─ protocols[]                  ├─ protocol                  │
│  ├─ name                         └─ port                      │
│  ├─ torrentP2P                                                │
│  ├─ smartVPN                     StandardVpnProtocol (enum)   │
│  └─ ip[]                         ├─ PPTP                      │
│                                  ├─ L2TP                       │
│  AuthApiResponse (enum)          └─ SSTP                      │
│  ├─ Success                                                   │
│  ├─ InvalidCredentials                                        │
│  ├─ AccountDisabled                                           │
│  └─ ServerError                                               │
│                                                                │
└───────────────────────────────────────────────────────────────┘
```

## State Machine Diagrams

### VPN Connection State Machine

```
                     ┌──────────────┐
                     │ Disconnected │◀───┐
                     └───────┬──────┘    │
                             │           │
                     connectToVPN()      │
                             │           │
                             ▼           │
                     ┌──────────────┐    │
                     │  Connecting  │    │
                     └───────┬──────┘    │
                             │           │
                  ┌──────────┴──────────┐│
                  │                     ││
      ┌───────────▼──────────┐   ┌──────▼─────────┐
      │   Authenticating     │   │     Error      │
      └───────────┬──────────┘   └────────────────┘
                  │                      │
                  ▼                      │
      ┌───────────────────────┐         │
      │  Device Connected     │         │
      └───────────┬───────────┘         │
                  │                     │
                  ▼                     │
      ┌───────────────────────┐        │
      │      Connected        │        │
      └───────────┬───────────┘        │
                  │                    │
          ┌───────┴────────┐           │
          │                │           │
    VPNDisconnect()   Connection       │
          │           Broken            │
          │                │            │
          └────────────────┴────────────┘
```

### OpenVPN State Machine

```
          ┌────────────┐
          │   INIT     │
          └──────┬─────┘
                 │
                 ▼
          ┌────────────┐
          │ CONNECTING │
          └──────┬─────┘
                 │
                 ▼
          ┌────────────┐
          │    WAIT    │
          └──────┬─────┘
                 │
                 ▼
          ┌────────────┐
          │    AUTH    │
          └──────┬─────┘
                 │
                 ▼
          ┌────────────┐
          │ GET_CONFIG │
          └──────┬─────┘
                 │
                 ▼
          ┌────────────┐
          │ ASSIGN_IP  │
          └──────┬─────┘
                 │
                 ▼
          ┌────────────┐
          │ ADD_ROUTES │
          └──────┬─────┘
                 │
                 ▼
          ┌────────────┐      Error        ┌──────────────┐
          │  CONNECTED │◀─────────────────▶│ RECONNECTING │
          └──────┬─────┘                   └──────────────┘
                 │
           disconnect()
                 │
                 ▼
          ┌────────────┐
          │   EXITING  │
          └────────────┘
```

## Threading Model

```
┌─────────────────────────────────────────────────────────────────┐
│                          Main UI Thread                          │
│                                                                  │
│  • Form Events                                                   │
│  • UI Updates (via Invoke())                                     │
│  • User Interaction                                              │
│                                                                  │
└────────────────────────┬────────────────────────────────────────┘
                         │
        ┌────────────────┼────────────────┐
        │                │                │
        ▼                ▼                ▼
┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│ Task.Run()   │  │ Timer Threads│  │ OpenVPN      │
│ Threads      │  │              │  │ Socket Thread│
├──────────────┤  ├──────────────┤  ├──────────────┤
│ • Connect    │  │ • Reconnect  │  │ • Socket     │
│ • Disconnect │  │   Timer      │  │   Read       │
│ • Auth       │  │ • Stats      │  │ • State      │
│   Check      │  │   Timer      │  │   Monitor    │
│              │  │              │  │              │
└──────┬───────┘  └──────┬───────┘  └──────┬───────┘
       │                 │                 │
       └─────────────────┼─────────────────┘
                         │
                   view.Invoke()
                         │
                         ▼
                  ┌──────────────┐
                  │  UI Update   │
                  └──────────────┘
```

### Thread Safety Considerations

1. **UI Updates**: All UI updates go through `Invoke()` pattern
2. **Connection Operations**: Use `Task.Run()` to avoid blocking UI
3. **Socket Management**: Dedicated thread for OpenVPN management interface
4. **Timers**: Fire on separate threads, must invoke for UI updates
5. **Shared State**: `VPNConnectionManager` maintains connection state, accessed from multiple threads

## Protocol Implementation

### OpenVPN Protocol Stack

```
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                         │
│                  VPNConnectionManager                        │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                   OpenVPN Class                              │
│  ┌────────────────────────────────────────────────────────┐ │
│  │ • Process Management                                   │ │
│  │ • Certificate Handling                                 │ │
│  │ • Socket Communication                                 │ │
│  └────────────────────────────────────────────────────────┘ │
└───────────────────────────┬─────────────────────────────────┘
                            │
                ┌───────────┴───────────┐
                │                       │
                ▼                       ▼
┌──────────────────────┐    ┌──────────────────────┐
│  OpenVPN Process     │    │  Management Socket   │
│  (openvpn.exe)       │◀───│  (localhost:12343)   │
│                      │    │                      │
│  • TUN/TAP Driver    │    │  • State Updates     │
│  • Encryption        │    │  • Control Commands  │
│  • Routing           │    │  • Status Queries    │
└──────────────────────┘    └──────────────────────┘
```

### RAS Protocol Stack

```
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                         │
│                  VPNConnectionManager                        │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                      Ras Class                               │
│  ┌────────────────────────────────────────────────────────┐ │
│  │ • Phonebook Management                                 │ │
│  │ • Credential Handling                                  │ │
│  │ • State Callbacks                                      │ │
│  └────────────────────────────────────────────────────────┘ │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                     DotRas Library                           │
│  • RasDialer                                                 │
│  • RasPhoneBook                                              │
│  • RasConnection                                             │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│               Windows RAS API (rasapi32.dll)                 │
│  • PPTP: GRE Protocol                                        │
│  • L2TP: IPsec (UDP 500, 4500)                              │
│  • SSTP: HTTPS (TCP 443)                                     │
└─────────────────────────────────────────────────────────────┘
```

## Security Architecture

### Kill Switch Implementation

```
Normal Operation:
┌──────────────┐        ┌──────────────┐
│ Application  │───────▶│   Internet   │
└──────────────┘        └──────────────┘
        │
        └──────────▶VPN──────────────────────────────▶ Internet


Kill Switch Enabled:
┌──────────────┐        ┌──────────────┐
│ Application  │   X    │   Internet   │ (BLOCKED)
└──────────────┘        └──────────────┘

┌──────────────┐
│  VPN App     │────────▶VPN Server────────────▶ Internet (ALLOWED)
│  OpenVPN.exe │
└──────────────┘

Firewall Rules:
- Default Outbound: BLOCK
- Allow: VPN Client.exe
- Allow: openvpn.exe
```

### DNS Leak Prevention

```
Before VPN:
┌──────────────┐  DNS Query   ┌──────────────┐
│ Application  │─────────────▶│  ISP DNS     │
└──────────────┘              └──────────────┘

After VPN Connection:
┌──────────────┐              ┌──────────────┐
│ Application  │              │  ISP DNS     │ (Bypassed)
└──────┬───────┘              └──────────────┘
       │
       │ DNS Query
       │
       ▼
┌──────────────┐   VPN Tunnel   ┌──────────────┐
│ VPN Adapter  │───────────────▶│  VPN DNS     │
└──────────────┘                └──────────────┘
```

### Credential Storage

```
┌─────────────────────────────────────────────────────────────┐
│                   User Credentials                           │
│                  (username, password)                        │
└───────────────────────────┬─────────────────────────────────┘
                            │
                  Encrypter.Encrypt()
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                 AES-256 Encryption                           │
│  • Random Salt                                               │
│  • Random IV                                                 │
│  • PBKDF2 Key Derivation (1000 iterations)                  │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│              Encrypted Credential Storage                    │
│              (Settings / Configuration File)                 │
└─────────────────────────────────────────────────────────────┘
```

## Network Flow

### Complete VPN Connection Flow

```
1. User Click Connect
   │
   ▼
2. Authenticate with AuthApi
   │
   ├─ Generate MD5(username + password)
   │
   ├─ POST https://auth-api.glbls.net/auth/{username}
   │
   └─ Verify response (200 = Success)
   │
   ▼
3. Get Current IP Address
   │
   └─ GET https://api.ipify.org → startIP
   │
   ▼
4. Initiate VPN Connection
   │
   ├─ OpenVPN
   │  ├─ Create certificate file
   │  ├─ Create credentials file
   │  ├─ Launch openvpn.exe
   │  ├─ Connect to management socket
   │  └─ Monitor state changes
   │
   └─ PPTP/L2TP/SSTP
      ├─ Create phonebook entry
      ├─ Set credentials
      └─ Dial connection
   │
   ▼
5. Configure DNS
   │
   ├─ Save current DNS settings
   │
   ├─ Set VPN DNS servers
   │
   └─ Flush DNS cache
   │
   ▼
6. Update UI State: "Connected"
   │
   ▼
7. Start Monitoring
   │
   ├─ Auto-reconnect timer
   │  └─ Check IP every 30 seconds
   │
   └─ Statistics timer
      └─ Update RX/TX every 1 second
```

## Dependency Graph

```
VpnBase.shproj (Base)
│
├─ External Dependencies
│  ├─ DotRas (RAS API wrapper)
│  ├─ NetFwTypeLib (Windows Firewall)
│  ├─ IWshRuntimeLibrary (Shortcuts)
│  └─ System.Management
│
├─ Windows APIs
│  ├─ rasapi32.dll (Remote Access Service)
│  ├─ netsh.exe (Network configuration)
│  ├─ ipconfig.exe (DNS flush)
│  └─ Windows Registry
│
└─ Third-Party Services
   ├─ auth-api.glbls.net (Authentication)
   ├─ network.glbls.net (Server list)
   └─ api.ipify.org (IP detection)
```

## Deployment Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                  VPN Client Application                          │
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │                    Application.exe                         │ │
│  │  (Compiled with Base shared project code)                  │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  Dependencies                                              │ │
│  │  • DotRas.dll                                              │ │
│  │  • MaterialSkin.dll                                        │ │
│  │  • MetroFramework.dll                                      │ │
│  │  • jSkin.dll                                               │ │
│  │  • Microsoft.Win32.TaskScheduler.dll                       │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  OpenVPN Directory                                         │ │
│  │  • openvpn.exe                                             │ │
│  │  • log.txt (generated)                                     │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  Resources                                                 │ │
│  │  • VPNServerList.csv (cached)                              │ │
│  │  • Network-Vpn-icon.ico                                    │ │
│  │  • app.config                                              │ │
│  └────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

## Error Handling Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                     Exception Handling Layers                    │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Layer 1: UI Layer                                               │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │ • Catch all exceptions                                     │ │
│  │ • Display user-friendly messages                           │ │
│  │ • Log to UI log control                                    │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              │                                   │
│                              ▼                                   │
│  Layer 2: VPNConnectionManager                                   │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │ • Try-catch around critical operations                     │ │
│  │ • Use callbacks to report errors to UI                     │ │
│  │ • Graceful degradation                                     │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              │                                   │
│                              ▼                                   │
│  Layer 3: Protocol Drivers (OpenVPN, Ras)                        │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │ • Handle protocol-specific errors                          │ │
│  │ • Cleanup resources on failure                             │ │
│  │ • Return error states                                      │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              │                                   │
│                              ▼                                   │
│  Layer 4: System APIs                                            │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │ • Windows RAS errors                                       │ │
│  │ • Socket exceptions                                        │ │
│  │ • Process exceptions                                       │ │
│  │ • File I/O errors                                          │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

## Performance Considerations

### Resource Usage

```
Component              CPU Usage    Memory     Network
────────────────────────────────────────────────────────
VPNConnectionManager   Low          ~5 MB      Minimal
OpenVPN Process        Medium       ~10-15 MB  High (encrypted traffic)
RAS Connection         Low          ~2 MB      High (encrypted traffic)
UI Layer               Low          ~20-30 MB  Minimal
Socket Management      Low          <1 MB      Minimal
Timers (all)           Minimal      <1 MB      Periodic HTTP requests

Total Application      ~50-70 MB    Variable depending on traffic
```

### Optimization Strategies

1. **Connection Pooling**: Reuse HTTP client for API calls
2. **Caching**: Server list cached locally, only download when updated
3. **Async Operations**: All network operations use Task.Run()
4. **Timer Throttling**: Configurable intervals for different checks
5. **Resource Cleanup**: Proper disposal of sockets, processes, timers

## Extensibility Points

### Adding New VPN Protocols

```csharp
// 1. Add to StandardVpnProtocol enum
public enum StandardVpnProtocol
{
    PPTP,
    L2TP,
    SSTP,
    IKEv2  // New protocol
}

// 2. Implement in Ras.createVpnEntry()
case StandardVpnProtocol.IKEv2:
    entry = RasEntry.CreateVpnEntry(entryName, host, 
        RasVpnStrategy.IkeV2Only,
        RasDevice.GetDeviceByName("(IKEv2)", RasDeviceType.Vpn));
    break;

// 3. Add to VPNConnectionManager.connectToVPN() switch
case "IKEv2":
    // Implementation
    break;
```

### Custom Authentication Providers

```csharp
// Create interface
public interface IAuthProvider
{
    Task<AuthApiResponse> Authenticate(string username, string password);
}

// Implement custom provider
public class CustomAuthProvider : IAuthProvider
{
    public async Task<AuthApiResponse> Authenticate(string username, string password)
    {
        // Custom authentication logic
    }
}

// Use in VPNConnectionManager constructor
public VPNConnectionManager(IVPNView view, OpenVPN openVPN, 
                            NetworkManagment networkManagment,
                            IAuthProvider authProvider = null)
{
    this.authProvider = authProvider ?? new DefaultAuthProvider();
}
```

---

**Architecture Document Version**: 1.0  
**Last Updated**: October 2025  
**Target Audience**: Developers integrating or maintaining the Base library

