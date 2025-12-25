using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Manages internet connection state and triggers SDK reloads when reconnect happens.
/// Attach this to a persistent GameObject (e.g., in your bootstrap scene).
/// </summary>
public class CheckInternetManager : Singleton<CheckInternetManager>
{
    [Header("Settings")]
    [SerializeField] private float checkInterval = 2f;   // how often to check (seconds)
    [SerializeField] private bool useWebPing = false;    // true = verify real internet, false = only network reachability

    private Coroutine checkRoutine;
    private bool wasDisconnected = false;

    #region Unity Methods
    private void Start()
    {
        checkRoutine = StartCoroutine(CheckConnectionLoop());
    }

    private void OnDestroy()
    {
        if (checkRoutine != null)
            StopCoroutine(checkRoutine);
    }
    #endregion

    #region Connection Checking
    private IEnumerator CheckConnectionLoop()
    {
        while (true)
        {
            bool isConnected = false;

            if (useWebPing)
            {
                // Optional: real internet check (slower, more battery)
                yield return StartCoroutine(PingGoogle(success => isConnected = success));
            }
            else
            {
                // Fast check: only looks at network reachability
                isConnected = Application.internetReachability != NetworkReachability.NotReachable;
            }

            if (isConnected)
            {
                if (wasDisconnected)
                {
                    wasDisconnected = false;
                    HandleInternetAvailable();
                }
            }
            else
            {
                if (!wasDisconnected)
                {
                    wasDisconnected = true;
                    HandleInternetNotAvailable();
                }
            }

            yield return new WaitForSecondsRealtime(checkInterval);
        }
    }

    /// <summary>
    /// Pings Google to verify actual internet connection.
    /// </summary>
    private IEnumerator PingGoogle(Action<bool> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Head("https://www.google.com"))
        {
            request.timeout = 5;
            yield return request.SendWebRequest();
            bool success = !(request.result != UnityWebRequest.Result.Success);
            callback?.Invoke(success);
        }
    }
    #endregion

    #region Handlers
    private void HandleInternetNotAvailable()
    {
        Debug.Log("[CheckInternetManager] Internet lost.");
        // TODO: show popup if needed
    }

    private void HandleInternetAvailable()
    {
        Debug.Log("[CheckInternetManager] Internet reconnected!");

        try
        {
            // ✅ Safe: Fetch config again
            // FirebaseManager.Instance.FetchConfig();

            // ✅ Instead of re-initializing SDKs, reload ads
            // AdsManager.Instance.ReloadAllAds();

            // TODO: hide popup if showing
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CheckInternetManager] Exception during reconnect: {ex.Message}");
        }
    }
    #endregion
}
