# Unity Integration Guide 🎮

A comprehensive guide for using **ResilientHttpClient** in Unity projects while avoiding socket exhaustion and other common pitfalls.

---

## Table of Contents

- [Why Socket Exhaustion Matters in Unity](#why-socket-exhaustion-matters-in-unity)
- [The Golden Rule](#the-golden-rule)
- [Recommended Patterns](#recommended-patterns)
  - [Pattern 1: Singleton Manager (Recommended)](#pattern-1-singleton-manager-recommended)
  - [Pattern 2: Static Service Locator](#pattern-2-static-service-locator)
  - [Pattern 3: ScriptableObject Configuration](#pattern-3-scriptableobject-configuration)
- [Common Scenarios](#common-scenarios)
- [Anti-Patterns to Avoid](#anti-patterns-to-avoid)
- [Testing in Unity Editor](#testing-in-unity-editor)
- [Platform-Specific Considerations](#platform-specific-considerations)
- [Troubleshooting](#troubleshooting)

---

## Why Socket Exhaustion Matters in Unity

### The Problem

Creating a new `HttpClient` (or `ResilientHttpClient`) for every network request is a **critical anti-pattern** that can crash your game, especially on mobile devices.

**What happens when you create too many instances:**

1. **Socket Exhaustion** 💥
   - Each HttpClient instance creates its own connection pool
   - Operating systems limit the number of open sockets (typically 1000-5000)
   - Mobile devices have even stricter limits (as low as 256 on some platforms)
   - When you run out: `SocketException: Too many open files`

2. **Memory Leaks** 🧠
   - Each instance holds onto TCP connections even after disposal
   - Connections remain in TIME_WAIT state for 30-120 seconds
   - Your game's memory footprint grows silently

3. **DNS Issues** 🌐
   - HttpClient caches DNS results for the lifetime of the instance
   - Creating many short-lived instances ignores DNS changes
   - Can cause connection failures if server IPs change

4. **Performance Degradation** 🐌
   - Creating new connections is expensive (TCP handshake, TLS negotiation)
   - Connection pooling only works when you reuse the same instance
   - Your game becomes slower over time

### Real-World Impact

```csharp
// ❌ THIS WILL CRASH YOUR GAME AFTER ~50-200 REQUESTS
public async Task<PlayerData> GetPlayerData(string playerId)
{
    var client = ResilientHttpClientFactory.CreateClient(); // NEW INSTANCE EVERY TIME!
    var response = await client.GetAsync($"https://api.mygame.com/players/{playerId}");
    // ... process response
}

// After your players use this 100 times:
// System.Net.Sockets.SocketException: Too many open files
// Game crashes or freezes
```

---

## The Golden Rule

> **Create ONE ResilientHttpClient instance for your entire game and reuse it for ALL requests.**

`ResilientHttpClient` is **thread-safe** and **designed to be long-lived**. There is no benefit to creating multiple instances unless you're calling completely different APIs with different resilience requirements.

---

## Recommended Patterns

### Pattern 1: Singleton Manager (Recommended)

This is the **safest and most Unity-friendly** approach. Perfect for most games.

```csharp
using UnityEngine;
using ResilientHttpClient.Core;
using System;
using System.Threading.Tasks;

/// <summary>
/// Singleton manager for all API calls in the game.
/// Ensures only ONE ResilientHttpClient instance exists.
/// </summary>
public class ApiManager : MonoBehaviour
{
    private static ApiManager _instance;
    private static readonly object _lock = new object();
    
    // The ONE AND ONLY client instance for the entire game
    private IResilientHttpClient _client;
    
    /// <summary>
    /// Singleton instance. Automatically creates a persistent GameObject if needed.
    /// </summary>
    public static ApiManager Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        // Create a persistent GameObject for the manager
                        GameObject go = new GameObject("ApiManager");
                        _instance = go.AddComponent<ApiManager>();
                        DontDestroyOnLoad(go);
                    }
                }
            }
            return _instance;
        }
    }
    
    /// <summary>
    /// Gets the shared ResilientHttpClient instance.
    /// </summary>
    public IResilientHttpClient Client
    {
        get
        {
            if (_client == null)
            {
                InitializeClient();
            }
            return _client;
        }
    }
    
    private void Awake()
    {
        // Ensure only one instance exists
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeClient();
    }
    
    private void InitializeClient()
    {
        var options = new ResilientHttpClientOptions
        {
            MaxRetries = 3,
            RetryDelay = TimeSpan.FromSeconds(1),
            MaxFailures = 5,
            CircuitResetTime = TimeSpan.FromSeconds(30)
        };
        
        // Create the ONE instance that will be used for the entire game session
        _client = ResilientHttpClientFactory.CreateClient("https://api.yourgame.com", options);
        
        Debug.Log("[ApiManager] ResilientHttpClient initialized - this should only happen ONCE per game session");
    }
    
    private void OnDestroy()
    {
        // Clean up when the game exits
        if (_instance == this)
        {
            _instance = null;
        }
    }
    
    // --- Example API Methods ---
    
    /// <summary>
    /// Fetches player data from the server.
    /// </summary>
    public async Task<PlayerData> GetPlayerDataAsync(string playerId)
    {
        try
        {
            var response = await Client.GetAsync($"/players/{playerId}");
            response.EnsureSuccessStatusCode();
            
            string json = await response.Content.ReadAsStringAsync();
            return JsonUtility.FromJson<PlayerData>(json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ApiManager] Failed to get player data: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Submits a player's score to the leaderboard.
    /// </summary>
    public async Task<bool> SubmitScoreAsync(int score)
    {
        try
        {
            var content = new System.Net.Http.StringContent(
                $"{{\"score\":{score}}}",
                System.Text.Encoding.UTF8,
                "application/json");
            
            var response = await Client.PostAsync("/leaderboard", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ApiManager] Failed to submit score: {ex.Message}");
            return false;
        }
    }
}

// --- Usage in your game code ---

public class PlayerController : MonoBehaviour
{
    private async void Start()
    {
        // Access the singleton manager - no need to create anything
        var playerData = await ApiManager.Instance.GetPlayerDataAsync("player123");
        Debug.Log($"Player Level: {playerData.Level}");
    }
    
    private async void OnLevelComplete()
    {
        int score = CalculateScore();
        bool success = await ApiManager.Instance.SubmitScoreAsync(score);
        
        if (success)
        {
            Debug.Log("Score submitted successfully!");
        }
    }
}

[Serializable]
public class PlayerData
{
    public string PlayerId;
    public int Level;
    public int Experience;
}
```

**Advantages:**
- ✅ Only ONE HttpClient instance for the entire game
- ✅ Survives scene changes with `DontDestroyOnLoad`
- ✅ Thread-safe singleton pattern
- ✅ Easy to access from anywhere: `ApiManager.Instance.Client`
- ✅ Centralized API logic
- ✅ Proper cleanup on game exit

---

### Pattern 2: Static Service Locator

For games that prefer a static service locator pattern without MonoBehaviour.

```csharp
using ResilientHttpClient.Core;
using System;

/// <summary>
/// Static service locator for the ResilientHttpClient.
/// No MonoBehaviour required - perfect for non-MonoBehaviour code.
/// </summary>
public static class HttpClientService
{
    private static IResilientHttpClient _instance;
    private static readonly object _lock = new object();
    
    /// <summary>
    /// Gets the singleton ResilientHttpClient instance.
    /// Lazy-initialized on first access.
    /// </summary>
    public static IResilientHttpClient Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        Initialize();
                    }
                }
            }
            return _instance;
        }
    }
    
    private static void Initialize()
    {
        var options = new ResilientHttpClientOptions
        {
            MaxRetries = 3,
            RetryDelay = TimeSpan.FromSeconds(1),
            MaxFailures = 5,
            CircuitResetTime = TimeSpan.FromSeconds(30)
        };
        
        _instance = ResilientHttpClientFactory.CreateClient("https://api.yourgame.com", options);
        
        UnityEngine.Debug.Log("[HttpClientService] Initialized - ONE instance for entire game");
    }
    
    /// <summary>
    /// Optional: Manually reset the client (e.g., for testing or if you need to change the base URL).
    /// ⚠️ WARNING: Only call this if you really need to. Normal games should never call this.
    /// </summary>
    public static void Reset()
    {
        lock (_lock)
        {
            _instance = null;
        }
    }
}

// --- Usage Example ---

public class GameServices
{
    public async Task<string> FetchConfigAsync()
    {
        // Access the static instance - no GameObject needed
        var client = HttpClientService.Instance;
        var response = await client.GetAsync("/config");
        return await response.Content.ReadAsStringAsync();
    }
}
```

**Advantages:**
- ✅ No MonoBehaviour overhead
- ✅ Works in any C# class, including static classes
- ✅ Perfect for utility classes and services
- ✅ Thread-safe lazy initialization

**Disadvantages:**
- ⚠️ Survives domain reloads in Unity Editor (can cause issues during development)
- ⚠️ No automatic cleanup (but HttpClient doesn't strictly need disposal in most cases)

---

### Pattern 3: ScriptableObject Configuration

For games that use ScriptableObjects for configuration.

```csharp
using UnityEngine;
using ResilientHttpClient.Core;
using System;

/// <summary>
/// ScriptableObject configuration for API settings.
/// Create via: Assets > Create > Game Config > API Settings
/// </summary>
[CreateAssetMenu(fileName = "ApiSettings", menuName = "Game Config/API Settings")]
public class ApiSettings : ScriptableObject
{
    [Header("API Configuration")]
    [Tooltip("Base URL for your game's API")]
    public string BaseUrl = "https://api.yourgame.com";
    
    [Header("Resilience Settings")]
    [Range(1, 10)]
    [Tooltip("Maximum number of retry attempts")]
    public int MaxRetries = 3;
    
    [Range(0.1f, 10f)]
    [Tooltip("Delay between retries in seconds")]
    public float RetryDelaySeconds = 1f;
    
    [Range(1, 20)]
    [Tooltip("Maximum failures before circuit opens")]
    public int MaxFailures = 5;
    
    [Range(5, 300)]
    [Tooltip("Time to wait before testing if circuit can close (seconds)")]
    public float CircuitResetTimeSeconds = 30f;
    
    // Cached client instance - ONE per ScriptableObject instance
    private IResilientHttpClient _client;
    
    /// <summary>
    /// Gets or creates the ResilientHttpClient instance.
    /// </summary>
    public IResilientHttpClient GetClient()
    {
        if (_client == null)
        {
            var options = new ResilientHttpClientOptions
            {
                MaxRetries = MaxRetries,
                RetryDelay = TimeSpan.FromSeconds(RetryDelaySeconds),
                MaxFailures = MaxFailures,
                CircuitResetTime = TimeSpan.FromSeconds(CircuitResetTimeSeconds)
            };
            
            _client = ResilientHttpClientFactory.CreateClient(BaseUrl, options);
            Debug.Log($"[ApiSettings] Created ResilientHttpClient for {BaseUrl}");
        }
        
        return _client;
    }
    
    private void OnDisable()
    {
        // Note: ScriptableObjects persist in Editor, so this may not be called when you expect
        _client = null;
    }
}

// --- Usage Example ---

public class ApiService : MonoBehaviour
{
    [SerializeField] private ApiSettings apiSettings;
    
    private async void Start()
    {
        // Get the client from the ScriptableObject
        var client = apiSettings.GetClient();
        
        var response = await client.GetAsync("/status");
        Debug.Log($"API Status: {response.StatusCode}");
    }
}
```

**Advantages:**
- ✅ Designer-friendly configuration in Inspector
- ✅ Easy to change settings without code changes
- ✅ Can have different configs for dev/staging/production
- ✅ Serialized settings persist between sessions

**Disadvantages:**
- ⚠️ Requires creating ScriptableObject asset in project
- ⚠️ ScriptableObject instances persist in Unity Editor (can be confusing during development)

---

## Common Scenarios

### Scenario 1: Multiple APIs in One Game

If your game calls **different APIs** (e.g., game backend, analytics, ads), create **one client per API** but still reuse each client.

```csharp
public static class GameHttpClients
{
    // One client per API - all created ONCE and reused
    private static IResilientHttpClient _gameApiClient;
    private static IResilientHttpClient _analyticsClient;
    
    public static IResilientHttpClient GameApi
    {
        get
        {
            if (_gameApiClient == null)
            {
                _gameApiClient = ResilientHttpClientFactory.CreateClient("https://api.mygame.com");
            }
            return _gameApiClient;
        }
    }
    
    public static IResilientHttpClient Analytics
    {
        get
        {
            if (_analyticsClient == null)
            {
                var options = new ResilientHttpClientOptions
                {
                    MaxRetries = 5, // Analytics can retry more
                    RetryDelay = TimeSpan.FromSeconds(2)
                };
                _analyticsClient = ResilientHttpClientFactory.CreateClient("https://analytics.mygame.com", options);
            }
            return _analyticsClient;
        }
    }
}

// Usage
var playerData = await GameHttpClients.GameApi.GetAsync("/player/123");
var analyticsResponse = await GameHttpClients.Analytics.PostAsync("/events", content);
```

### Scenario 2: Parallel Requests (Safe!)

**Good news:** You can safely make multiple concurrent requests with the same client!

```csharp
public async Task LoadGameDataAsync()
{
    var client = ApiManager.Instance.Client;
    
    // All three requests run in parallel - SAFE because we're reusing the same client
    var playerTask = client.GetAsync("/player");
    var inventoryTask = client.GetAsync("/inventory");
    var achievementsTask = client.GetAsync("/achievements");
    
    // Wait for all to complete
    await Task.WhenAll(playerTask, inventoryTask, achievementsTask);
    
    var playerData = await playerTask.Result.Content.ReadAsStringAsync();
    var inventoryData = await inventoryTask.Result.Content.ReadAsStringAsync();
    var achievementsData = await achievementsTask.Result.Content.ReadAsStringAsync();
    
    Debug.Log("All data loaded in parallel!");
}
```

### Scenario 3: WebGL Builds

WebGL has special considerations due to browser limitations.

```csharp
#if UNITY_WEBGL && !UNITY_EDITOR
    // WebGL-specific initialization
    var options = new ResilientHttpClientOptions
    {
        MaxRetries = 2, // Lower retries due to browser timeouts
        RetryDelay = TimeSpan.FromMilliseconds(500),
        // Note: Some browser features may limit connection pooling
    };
#else
    // Native platform settings
    var options = new ResilientHttpClientOptions
    {
        MaxRetries = 3,
        RetryDelay = TimeSpan.FromSeconds(1)
    };
#endif

var client = ResilientHttpClientFactory.CreateClient("https://api.mygame.com", options);
```

---

## Anti-Patterns to Avoid

### ❌ Anti-Pattern 1: Creating Client in Coroutines

```csharp
// ❌ WRONG - Creates a new client on every frame or event!
IEnumerator FetchDataCoroutine()
{
    var client = ResilientHttpClientFactory.CreateClient(); // BAD!
    // ...
}
```

**Fix:** Create the client once in a manager or static class.

### ❌ Anti-Pattern 2: Creating Client in Update/FixedUpdate

```csharp
// ❌ DISASTER - Creates DOZENS of clients per second!
private void Update()
{
    if (needsDataUpdate)
    {
        var client = ResilientHttpClientFactory.CreateClient(); // TERRIBLE!
        // ...
    }
}
```

**Fix:** Store the client as a field or use a singleton manager.

### ❌ Anti-Pattern 3: Creating Client Per Request Method

```csharp
// ❌ WRONG - Each method creates its own client!
public class BadApiService
{
    public async Task<string> GetPlayer()
    {
        var client = ResilientHttpClientFactory.CreateClient(); // BAD!
        // ...
    }
    
    public async Task<string> GetInventory()
    {
        var client = ResilientHttpClientFactory.CreateClient(); // BAD!
        // ...
    }
}
```

**Fix:** Create one client in the class constructor or use a shared instance.

```csharp
// ✅ CORRECT
public class GoodApiService
{
    private readonly IResilientHttpClient _client;
    
    public GoodApiService()
    {
        // Create ONCE in constructor
        _client = ResilientHttpClientFactory.CreateClient("https://api.mygame.com");
    }
    
    public async Task<string> GetPlayer()
    {
        // Reuse the same instance
        var response = await _client.GetAsync("/player");
        return await response.Content.ReadAsStringAsync();
    }
    
    public async Task<string> GetInventory()
    {
        // Reuse the same instance
        var response = await _client.GetAsync("/inventory");
        return await response.Content.ReadAsStringAsync();
    }
}
```

### ❌ Anti-Pattern 4: Disposing the Client Too Early

```csharp
// ❌ WRONG - Disposes a shared instance!
using (var client = ApiManager.Instance.Client) // BAD!
{
    await client.GetAsync("/data");
} // Disposes the shared client - now it's broken for everyone!
```

**Fix:** Never dispose a shared client instance. Let it live for the entire game session.

---

## Testing in Unity Editor

### Play Mode Testing

When testing in Play Mode, be aware that **domain reloads reset static fields but not always in the way you expect**.

```csharp
// Add this to your singleton manager for easier testing
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
private static void ResetStaticFields()
{
    // Unity calls this before entering Play Mode
    _instance = null;
    // This ensures a clean state for testing
}
```

### Detecting Socket Exhaustion During Development

Add this diagnostic helper to catch issues early:

```csharp
public class SocketDiagnostics : MonoBehaviour
{
    private int _requestCount = 0;
    
    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 20), $"API Requests: {_requestCount}");
    }
    
    public void IncrementRequestCount()
    {
        _requestCount++;
        
        // Warn if you're making suspiciously many requests
        if (_requestCount > 100)
        {
            Debug.LogWarning($"[SocketDiagnostics] {_requestCount} requests made - check for client instance leaks!");
        }
    }
}
```

---

## Platform-Specific Considerations

### iOS

- **TLS 1.2+ Required:** Ensure your API supports TLS 1.2 or higher
- **App Transport Security:** May need to configure Info.plist for non-HTTPS endpoints (not recommended)
- **Background Requests:** iOS suspends tasks when the app backgrounds - handle gracefully

```csharp
#if UNITY_IOS
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // iOS is backgrounding the app - cancel pending requests
            Debug.Log("[ApiManager] App paused - pending requests may be suspended");
        }
    }
#endif
```

### Android

- **Network Security Config:** Android 9+ blocks cleartext HTTP by default
- **Permissions:** Ensure `INTERNET` permission is set in AndroidManifest.xml

### WebGL

- **CORS Required:** Your API must have proper CORS headers
- **Browser Limitations:** Some advanced HttpClient features may not work
- **No Async/Await in WebGL:** Be cautious with async patterns on older Unity versions

---

## Troubleshooting

### Issue: "Too many open files" or SocketException

**Cause:** Creating too many HttpClient instances.

**Solution:**
1. Review your code for any place creating `ResilientHttpClient`
2. Ensure you're using a singleton pattern
3. Add logging to count how many times the client is created
4. Should see "ResilientHttpClient initialized" **only ONCE** per game session

### Issue: Requests Fail After Scene Changes

**Cause:** Your client instance is being destroyed during scene transitions.

**Solution:**
- Use `DontDestroyOnLoad` on your manager GameObject
- OR use a static service locator pattern that doesn't depend on MonoBehaviour

### Issue: Client Behavior Changes in Editor vs Build

**Cause:** Unity Editor domain reloads can reset static fields unexpectedly.

**Solution:**
- Add `[RuntimeInitializeOnLoadMethod]` to reset static fields on Play Mode
- Test in actual builds, not just Editor

### Issue: Slow First Request

**Cause:** HttpClient connection pooling happens on first request (TCP handshake, TLS negotiation).

**Solution:**
- This is normal and expected
- Consider a "warmup" request during game loading
- Subsequent requests will be much faster

```csharp
private async void Start()
{
    // Warmup request during loading screen
    try
    {
        await ApiManager.Instance.Client.GetAsync("/health");
        Debug.Log("API connection warmed up");
    }
    catch (Exception ex)
    {
        Debug.LogWarning($"API warmup failed (not critical): {ex.Message}");
    }
}
```

---

## Summary

### The Three Golden Rules for Unity

1. **Create ONCE** - One client instance per API for the entire game session
2. **Reuse FOREVER** - Never create a new client for each request
3. **Never Dispose Shared Instances** - Let the client live until game exit

### Recommended Checklist

- ✅ Use singleton pattern (MonoBehaviour or static)
- ✅ Add `DontDestroyOnLoad` if using MonoBehaviour
- ✅ Log when client is created (should only see it ONCE)
- ✅ Test in actual builds, not just Editor
- ✅ Monitor for SocketExceptions during testing
- ✅ Use the same client for parallel requests
- ✅ Consider platform-specific configurations

### Need Help?

- Check the main [README.md](README.md) for general usage
- Review [Best Practices](README.md#%EF%B8%8F-best-practices) section
- File an issue on GitHub if you encounter problems

Happy coding! 🎮✨
