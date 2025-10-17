# AuthApi API

## Overview

`AuthApi` is a static class providing centralized user authentication via RESTful API. It validates user credentials against a remote authentication server and returns the authentication status.

**Namespace**: `SmartDNSProxy_VPN_Client`  
**Assembly**: Shared Project  
**Class Type**: Static/Internal

## Class Declaration

```csharp
internal static class AuthApi
```

## Public Methods

### Authorize

Authenticates a user with the given credentials against the authentication API.

```csharp
public static async Task<AuthApiResponse> Authorize(
    string username, 
    string password
)
```

**Parameters:**
- `username` (string): User's username or email
- `password` (string): User's password in plain text

**Returns:** `Task<AuthApiResponse>` - Authentication result

**Return Values:**
- `AuthApiResponse.Success` - Authentication successful, user can proceed
- `AuthApiResponse.InvalidCredentials` - Wrong username or password
- `AuthApiResponse.AccountDisabled` - Account exists but is disabled
- `AuthApiResponse.ServerError` - Cannot reach authentication server or server error

**Example:**
```csharp
var result = await AuthApi.Authorize("myusername", "mypassword");

switch (result)
{
    case AuthApiResponse.Success:
        Console.WriteLine("Login successful!");
        // Proceed with VPN connection
        break;
        
    case AuthApiResponse.InvalidCredentials:
        MessageBox.Show("Invalid username or password");
        break;
        
    case AuthApiResponse.AccountDisabled:
        MessageBox.Show("Your account has been disabled. Please contact support.");
        break;
        
    case AuthApiResponse.ServerError:
        MessageBox.Show("Cannot reach authentication server. Please check your internet connection.");
        break;
}
```

## AuthApiResponse Enumeration

```csharp
public enum AuthApiResponse
{
    Success,              // HTTP 200 - Authentication successful
    InvalidCredentials,   // HTTP 404/405 - Wrong username/password
    AccountDisabled,      // HTTP 401 - Account is disabled
    ServerError           // HTTP 5xx or network error
}
```

## API Endpoints

### Production

```
https://auth-api.glbls.net:5000/auth/{username}
```

**Used when:** Application built in Release mode

### Debug/Development

```
http://199.241.146.241:5000/auth/{username}
```

**Used when:** Application built in Debug mode

## Authentication Flow

```
1. User provides username and password
   ↓
2. Generate MD5 hash of (username + password)
   ↓
3. Create HTTP POST request
   URL: {AuthApiUrl}/{username}
   Body: {"hash": "<md5_hash>"}
   Content-Type: application/json
   ↓
4. Send request to authentication server
   ↓
5. Parse HTTP status code:
   - 200 OK → Success
   - 401 Unauthorized → AccountDisabled
   - 404 Not Found → InvalidCredentials
   - 405 Method Not Allowed → InvalidCredentials
   - Other/Exception → ServerError
   ↓
6. Return AuthApiResponse
```

## Request Format

**Method:** POST  
**URL:** `{AuthApiUrl}/{username}`  
**Headers:**
```
Accept: application/json
Content-Type: application/json
```

**Body:**
```json
{
  "hash": "5f4dcc3b5aa765d61d8327deb882cf99"
}
```

## Response Codes

| HTTP Code | Meaning | AuthApiResponse |
|-----------|---------|-----------------|
| 200 OK | Valid credentials | `Success` |
| 401 Unauthorized | Account disabled | `AccountDisabled` |
| 404 Not Found | User not found | `InvalidCredentials` |
| 405 Method Not Allowed | Wrong credentials | `InvalidCredentials` |
| 500-599 Server Error | Server issue | `ServerError` |
| Network Exception | Cannot reach server | `ServerError` |

## Hash Generation

The authentication uses MD5 hashing:

```csharp
string input = username + password;  // Concatenate
string hash = MD5(input).ToLowerCase();  // Generate MD5, lowercase
```

**Example:**
```
Username: "user123"
Password: "pass456"
Input: "user123pass456"
MD5 Hash: "a1b2c3d4e5f6..." (32 hex characters, lowercase)
```

**Security Note:** MD5 is cryptographically broken. This implementation is maintained for backward compatibility with existing authentication servers.

## Implementation Details

### HttpClient Configuration

```csharp
private static readonly HttpClient RestClient;

static AuthApi()
{
    RestClient = new HttpClient();
    // Clear default headers
    foreach (var header in RestClient.DefaultRequestHeaders)
    {
        RestClient.DefaultRequestHeaders.Remove(header.Key);
    }
    // Set Accept header
    RestClient.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json")
    );
}
```

**Note:** HttpClient is reused (static) for optimal performance.

## Usage Patterns

### Pattern 1: Simple Authentication

```csharp
private async void LoginButton_Click(object sender, EventArgs e)
{
    var result = await AuthApi.Authorize(
        usernameTextBox.Text, 
        passwordTextBox.Text
    );
    
    if (result == AuthApiResponse.Success)
    {
        // Proceed to main application
        var mainForm = new MainForm();
        mainForm.Show();
        this.Hide();
    }
    else
    {
        MessageBox.Show($"Login failed: {result}");
    }
}
```

### Pattern 2: Authentication with Loading Indicator

```csharp
private async void LoginButton_Click(object sender, EventArgs e)
{
    loginButton.Enabled = false;
    loadingSpinner.Visible = true;
    errorLabel.Text = "";
    
    try
    {
        var result = await AuthApi.Authorize(
            usernameTextBox.Text, 
            passwordTextBox.Text
        );
        
        switch (result)
        {
            case AuthApiResponse.Success:
                OpenMainApplication();
                break;
            case AuthApiResponse.InvalidCredentials:
                errorLabel.Text = "Invalid username or password";
                passwordTextBox.Clear();
                passwordTextBox.Focus();
                break;
            case AuthApiResponse.AccountDisabled:
                errorLabel.Text = "Account disabled. Contact support.";
                break;
            case AuthApiResponse.ServerError:
                errorLabel.Text = "Cannot reach server. Try again later.";
                break;
        }
    }
    finally
    {
        loginButton.Enabled = true;
        loadingSpinner.Visible = false;
    }
}
```

### Pattern 3: Authentication with Retry Logic

```csharp
private async Task<AuthApiResponse> AuthenticateWithRetry(
    string username, 
    string password, 
    int maxRetries = 3
)
{
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        var result = await AuthApi.Authorize(username, password);
        
        if (result != AuthApiResponse.ServerError)
        {
            return result; // Success or credential issue
        }
        
        if (attempt < maxRetries)
        {
            await Task.Delay(2000 * attempt); // Exponential backoff
        }
    }
    
    return AuthApiResponse.ServerError;
}
```

### Pattern 4: Offline Mode Fallback

```csharp
private async Task<bool> AuthenticateUser(string username, string password)
{
    var result = await AuthApi.Authorize(username, password);
    
    if (result == AuthApiResponse.Success)
    {
        // Save successful authentication
        SaveLastSuccessfulAuth(username);
        return true;
    }
    else if (result == AuthApiResponse.ServerError)
    {
        // Server unreachable - check if user authenticated recently
        if (IsRecentlyAuthenticated(username))
        {
            MessageBox.Show(
                "Server unavailable. Using cached authentication.",
                "Offline Mode",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
            return true;
        }
    }
    
    return false;
}
```

## Error Handling

### Network Errors

```csharp
try
{
    var result = await AuthApi.Authorize(username, password);
}
catch (TaskCanceledException)
{
    // Request timeout
    MessageBox.Show("Request timed out. Please check your internet connection.");
}
catch (HttpRequestException ex)
{
    // Network error - already handled by returning ServerError
    // But you might want additional logging
    Log.Error("Authentication network error", ex);
}
```

**Note:** `Authorize()` catches `HttpRequestException` internally and returns `AuthApiResponse.ServerError`.

### Invalid Input

```csharp
if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
{
    MessageBox.Show("Please enter username and password");
    return;
}

var result = await AuthApi.Authorize(username.Trim(), password);
```

## Performance Considerations

- **HTTP Client Reuse:** Static HttpClient instance for connection pooling
- **Async/Await:** Non-blocking authentication
- **Request Timeout:** Default timeout is typically 100 seconds (HttpClient default)
- **Network Latency:** Typically 100-500ms for authentication request

### Recommended Timeout Configuration

```csharp
// In static constructor, add:
RestClient.Timeout = TimeSpan.FromSeconds(10);
```

## Security Considerations

### Critical Issues

1. **MD5 Hash is Weak**
   - MD5 is cryptographically broken
   - Vulnerable to collision attacks
   - Should be upgraded to SHA-256 or better
   - **Mitigation:** Server-side validation required

2. **Password Sent Over Network (as hash)**
   - Hash is generated client-side
   - Same hash always generated for same credentials
   - Vulnerable to replay attacks if intercepted
   - **Mitigation:** Use HTTPS (enforced in production)

3. **No Salt**
   - Hash has no salt value
   - Identical credentials produce identical hashes
   - **Mitigation:** Server should use salted hashing

### Best Practices

```csharp
// 1. Always use HTTPS in production (already enforced)
// 2. Don't log passwords or hashes
// 3. Clear password from memory after authentication
private async void LoginButton_Click(object sender, EventArgs e)
{
    string password = passwordTextBox.Text;
    
    try
    {
        var result = await AuthApi.Authorize(username, password);
        // Handle result
    }
    finally
    {
        // Clear password from memory
        password = null;
        passwordTextBox.Clear();
    }
}
```

## Testing

### Unit Testing

```csharp
[TestMethod]
public async Task TestValidCredentials()
{
    var result = await AuthApi.Authorize("testuser", "testpass");
    Assert.AreEqual(AuthApiResponse.Success, result);
}

[TestMethod]
public async Task TestInvalidCredentials()
{
    var result = await AuthApi.Authorize("wronguser", "wrongpass");
    Assert.AreEqual(AuthApiResponse.InvalidCredentials, result);
}

[TestMethod]
public async Task TestDisabledAccount()
{
    var result = await AuthApi.Authorize("disableduser", "password");
    Assert.AreEqual(AuthApiResponse.AccountDisabled, result);
}
```

### Integration Testing

Test against actual authentication server:
- Valid credentials → Success
- Invalid username → InvalidCredentials
- Invalid password → InvalidCredentials
- Disabled account → AccountDisabled
- No internet connection → ServerError
- Server down → ServerError

## Troubleshooting

### Issue: Always Returns ServerError

**Causes:**
1. No internet connection
2. Firewall blocking outbound connections
3. DNS cannot resolve auth-api.glbls.net
4. Server is down

**Solution:**
```csharp
// Test connectivity
try
{
    using (var client = new HttpClient())
    {
        var response = await client.GetAsync("https://www.google.com");
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Internet works, auth server might be down");
        }
    }
}
catch
{
    Console.WriteLine("No internet connection");
}
```

### Issue: Always Returns InvalidCredentials

**Causes:**
1. Wrong credentials
2. Username/password contains special characters not handled correctly
3. Whitespace in credentials

**Solution:**
```csharp
var result = await AuthApi.Authorize(
    username.Trim(), 
    password // Don't trim password - might be intentional
);
```

## Migration Guide

### Upgrading from MD5 to SHA-256

When server supports SHA-256:

```csharp
// New implementation
private static string GenerateSha256Hash(string input)
{
    using (var sha256 = SHA256.Create())
    {
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = sha256.ComputeHash(inputBytes);
        
        var sb = new StringBuilder();
        foreach (byte b in hashBytes)
            sb.Append(b.ToString("x2"));
        return sb.ToString().ToLower();
    }
}
```

## Related APIs

- [VPNConnectionManager](VPNConnectionManager.md) - Uses authentication before connecting
- [Encrypter](Encrypter.md) - For storing credentials locally

## See Also

- [Getting Started Guide](GettingStarted.md)
- [Security Best Practices](BestPractices.md)
- [Error Handling](ErrorHandling.md)

---

**API Version**: 1.0  
**Last Updated**: October 2025  
**Security Notice**: MD5 hashing used for backward compatibility. Upgrade recommended.

