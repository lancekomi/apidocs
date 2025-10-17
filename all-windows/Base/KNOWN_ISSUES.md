# Known Issues and Future Improvements

## Current Known Issues

### High Priority

#### 1. TODO Comment in VPNConnectionManager
**Location**: `VPNConnectionManager.cs:68`
```csharp
VPNDisconnect(selectedProtocol, entryName); //TODO nie mam pojącia po co to
```
**Issue**: The purpose of this disconnect call in the default case is unclear (Polish comment: "I have no idea why this is here")  
**Impact**: Potential logic issue in connection flow  
**Recommendation**: Review and document or remove this code

#### 2. AutostartManager Disabled Save
**Location**: `AutostartManager.cs:47`
```csharp
// TODO: Remove whole class if unnecessary
//shortcut.Save();  // Save the shortcut
```
**Issue**: Shortcut creation is disabled but the code is still present  
**Impact**: Shortcut-based autostart doesn't work (registry method is used instead)  
**Recommendation**: Remove legacy shortcut code if registry approach is confirmed as the standard

#### 3. Hardcoded Certificate in OpenVPN
**Location**: `OpenVPN.clsss.cs:58-86`  
**Issue**: CA certificate is hardcoded in the source code  
**Impact**: 
- Certificate cannot be updated without recompiling
- Security risk if certificate is compromised
- Certificate expiration (expires 2025-02-22)  
**Recommendation**: 
- Load certificate from encrypted resource file
- Implement certificate rotation mechanism
- Add certificate expiration checking

### Medium Priority

#### 4. Deprecated RAS API Usage
**Location**: `Ras.class.cs:8`
```csharp
#pragma warning disable CS0618 // Type or member is obsolete
```
**Issue**: Using obsolete RAS API methods  
**Impact**: May not work on future Windows versions  
**Recommendation**: Update to newer Windows VPN APIs or document minimum/maximum supported Windows versions

#### 5. Security Exception Not Properly Logged
**Location**: Multiple locations  
**Issue**: Many catch blocks swallow exceptions without logging:
```csharp
catch (Exception ex)
{
    var error = ex;  // Not logged anywhere
}
```
**Impact**: Difficult to debug issues in production  
**Recommendation**: Implement proper logging throughout the application

#### 6. IP Detection Relies on Third-Party Service
**Location**: `AutoReconnect.cs:18`  
**Issue**: Depends on `https://api.ipify.org` for IP detection  
**Impact**: 
- Single point of failure
- Privacy concerns (third party sees connection attempts)  
**Recommendation**: 
- Add fallback IP detection services
- Implement custom IP detection endpoint

#### 7. Authentication Uses MD5 Hash
**Location**: `AuthApi.cs:60-69`  
**Issue**: Using MD5 for password hashing  
**Impact**: MD5 is cryptographically broken and should not be used for security  
**Recommendation**: 
- Upgrade to SHA-256 or better
- Implement proper password hashing (bcrypt, Argon2)
- Add salt to prevent rainbow table attacks

### Low Priority

#### 8. Namespace Mismatch
**Location**: All files  
**Issue**: All files use namespace `SmartDNSProxy_VPN_Client` even though this is a shared library  
**Impact**: Naming confusion, not generic  
**Recommendation**: Refactor to generic namespace like `VpnBase` or `GlobalStealthVPN.Core`

#### 9. CSV Parsing Could Be More Robust
**Location**: `ServerListClient.cs:55-152`  
**Issue**: CSV parsing is manual and fragile  
**Impact**: May break if CSV format changes  
**Recommendation**: 
- Use CSV parsing library
- Add format validation
- Better error messages when CSV is malformed

#### 10. No Logging Framework
**Issue**: Uses `Debug.WriteLine()` for logging  
**Impact**: 
- Logs only available during debugging
- No production logging
- No log levels  
**Recommendation**: Implement proper logging framework (NLog, Serilog, etc.)

## Code Quality Issues

### Architecture Concerns

#### 1. Tight Coupling to Settings
Many classes directly access `Properties.Settings.Default`  
**Recommendation**: Use dependency injection for settings

#### 2. Missing Unit Tests
No unit tests are present in the project  
**Recommendation**: Add comprehensive test coverage

#### 3. Thread Safety
Some shared state may not be properly thread-safe  
**Recommendation**: Review and add locks where needed

#### 4. Resource Disposal
Not all resources are properly disposed (sockets, processes, streams)  
**Recommendation**: Use `using` statements consistently

### Code Cleanup Needed

#### 1. Unused Code
```csharp
// OpenVPN.clsss.cs - Unused Config class
public class Config
{
    public string name { get; set; }
    public string[] parameters { get; set; }
}

// Configuration is read but never used
config = new List<Config>();
```

#### 2. Magic Numbers
Many hardcoded values should be constants:
```csharp
// OpenVPN.clsss.cs:152
Thread.Sleep(500);  // Why 500ms?

// AutoReconnect.cs:14
for (var i = 0; i < 10; i++)  // Why 10 retries?

// Encrypter.cs:18
private const int DerivationIterations = 1000;  // Too low for modern standards
```

#### 3. Inconsistent Error Handling
Some methods throw exceptions, others return false/null, others use callbacks

#### 4. Missing XML Documentation
No XML documentation comments for public APIs

## Security Issues

### Critical

1. **Plain Text Password in OpenVPN Credentials File**
   - Location: `OpenVPN.clsss.cs:88-90`
   - Passwords written to temp files in plain text
   - Files not always cleaned up on crash

2. **Weak Encryption Parameters**
   - Location: `Encrypter.cs:18`
   - Only 1000 iterations for PBKDF2 (should be 100,000+)

3. **No Certificate Pinning**
   - OpenVPN and HTTPS connections don't validate certificates
   - Vulnerable to MITM attacks

### High

1. **DNS Leak Risk**
   - Race condition between VPN connection and DNS configuration
   - Original DNS temporarily exposed

2. **Kill Switch Race Condition**
   - Brief moment between connection drop and kill switch activation

3. **Credentials in Memory**
   - Passwords kept in memory as strings (not SecureString)
   - May be visible in crash dumps

## Performance Issues

### 1. Synchronous File Operations
Many file operations block the thread  
**Recommendation**: Use async file I/O

### 2. Inefficient IP Checking
AutoReconnect makes up to 10 sequential HTTP requests  
**Recommendation**: Exponential backoff, or fail faster

### 3. No Connection Pooling
New HttpClient instances created for each request  
**Recommendation**: Reuse HttpClient instances

### 4. DNS Changes Flush Entire Cache
`ipconfig /flushdns` clears all DNS cache  
**Recommendation**: Only flush specific entries if possible

## Compatibility Issues

### Windows Version Support

| Windows Version | Status | Notes |
|----------------|---------|-------|
| Windows 7 | ✓ Supported | Deprecated by Microsoft |
| Windows 8/8.1 | ✓ Supported | Deprecated by Microsoft |
| Windows 10 | ✓ Supported | Primary target |
| Windows 11 | ⚠️ Untested | Should work but needs testing |
| Windows Server | ⚠️ Untested | May have different RAS behavior |

### Known Limitations

1. **Requires Administrator Privileges**
   - Cannot change DNS without admin rights
   - Cannot modify firewall without admin rights
   - Cannot create RAS entries without admin rights

2. **TAP Driver Dependency**
   - OpenVPN requires TAP-Windows driver
   - Must be installed separately

3. **Single Instance**
   - No protection against multiple instances running
   - Could cause conflicts

4. **IPv6 Not Handled**
   - Only manages IPv4 DNS
   - Potential IPv6 leak

## Future Improvements

### Short Term (1-3 months)

- [ ] Add proper logging framework
- [ ] Fix hardcoded certificate issue
- [ ] Implement fallback IP detection services
- [ ] Add unit tests for core functionality
- [ ] Update deprecated RAS API usage
- [ ] Improve error handling and reporting

### Medium Term (3-6 months)

- [ ] Refactor to dependency injection
- [ ] Implement certificate pinning
- [ ] Add IPv6 support
- [ ] Upgrade authentication to use modern hashing
- [ ] Add configuration validation
- [ ] Implement connection profiles

### Long Term (6-12 months)

- [ ] Support WireGuard protocol
- [ ] Add split tunneling support
- [ ] Implement traffic obfuscation
- [ ] Add multi-hop VPN support
- [ ] Create plugin system for extensibility
- [ ] Support macOS and Linux (via Mono/.NET Core)

## Breaking Changes to Consider

### Version 2.0 Considerations

1. **Namespace Change**
   - Change from `SmartDNSProxy_VPN_Client` to `VpnBase` or similar
   - Breaking change for all consumers

2. **Interface Changes**
   - Make IVPNView async-aware
   - Add cancellation token support

3. **Settings Architecture**
   - Move from static Settings to dependency injection
   - Use configuration providers

4. **Authentication API**
   - Break compatibility to upgrade from MD5 to modern hashing
   - Coordinate with server team

## Testing Checklist

Items that need regular testing:

### Functional Testing
- [ ] All four protocols connect successfully
- [ ] Auto-reconnect works when connection drops
- [ ] Kill switch blocks traffic when VPN disconnects
- [ ] DNS settings properly restored on exit
- [ ] Statistics update correctly
- [ ] Server list downloads and caches properly
- [ ] Authentication handles all response types
- [ ] Autostart adds/removes correctly

### Security Testing
- [ ] No DNS leaks during connection
- [ ] No DNS leaks when disconnecting
- [ ] Kill switch prevents IP leaks
- [ ] Credentials encrypted properly in storage
- [ ] No credentials in log files
- [ ] Certificate validation working
- [ ] IPv6 disabled or tunneled

### Performance Testing
- [ ] Memory usage stable over 24+ hours
- [ ] No memory leaks
- [ ] CPU usage acceptable during connection
- [ ] Reconnection time < 5 seconds
- [ ] UI remains responsive during operations

### Compatibility Testing
- [ ] Windows 10 (multiple versions)
- [ ] Windows 11
- [ ] Multiple network adapters
- [ ] WiFi and Ethernet
- [ ] With and without IPv6
- [ ] With different firewall software
- [ ] With antivirus software

## Reporting Issues

When reporting issues, please include:

1. **Environment Information**
   - Windows version and build number
   - Network adapter type (WiFi/Ethernet)
   - Antivirus/Firewall software
   - VPN client version

2. **Steps to Reproduce**
   - Detailed step-by-step instructions
   - Protocol being used
   - Server being connected to

3. **Logs**
   - Application logs
   - OpenVPN logs (if applicable)
   - Windows Event Viewer errors
   - Network configuration (`ipconfig /all`)

4. **Expected vs Actual Behavior**
   - What you expected to happen
   - What actually happened
   - Screenshots if applicable

## Contributing Guidelines

### Code Standards

1. **Use XML documentation** for all public APIs
2. **Follow C# naming conventions**
3. **Add unit tests** for new functionality
4. **Update documentation** when changing APIs
5. **Handle errors properly** - don't swallow exceptions
6. **Use async/await** for I/O operations
7. **Dispose resources** properly

### Pull Request Checklist

- [ ] Code compiles without warnings
- [ ] Unit tests pass
- [ ] Integration tests pass
- [ ] Documentation updated
- [ ] No security issues introduced
- [ ] Backward compatibility maintained (or breaking changes documented)
- [ ] Performance impact considered

---

**Last Updated**: October 2025  
**Maintained by**: Global Stealth VPNs Development Team

**Note**: This is a living document. Please keep it updated as issues are discovered and resolved.

