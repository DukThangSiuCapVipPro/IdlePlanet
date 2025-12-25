using UnityEngine;

/// <summary>
/// Adjusts a RectTransform to fit within the device's safe area.
/// Useful for handling notches, rounded corners, and other screen cutouts on modern devices.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class LeanSafeArea : MonoBehaviour
{
    [Header("Safe Area Settings")]
    [Tooltip("Apply safe area adjustments on awake")]
    [SerializeField] private bool applyOnAwake = true;

    [Tooltip("Continuously check for safe area changes (e.g., orientation changes)")]
    [SerializeField] private bool continuousUpdate = false;

    [Header("Padding Adjustments")]
    [Tooltip("Additional padding to add to the left side")]
    [SerializeField] private float leftPadding = 0f;

    [Tooltip("Additional padding to add to the right side")]
    [SerializeField] private float rightPadding = 0f;

    [Tooltip("Additional padding to add to the top side")]
    [SerializeField] private float topPadding = 0f;

    [Tooltip("Additional padding to add to the bottom side")]
    [SerializeField] private float bottomPadding = 0f;

    [Header("Debug")]
    [Tooltip("Log safe area information")]
    [SerializeField] private bool debugMode = false;

    private RectTransform rectTransform;
    private Rect lastSafeArea;
    private Vector2Int lastScreenSize;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (applyOnAwake)
        {
            ApplySafeArea();
        }
    }

    private void Update()
    {
        if (continuousUpdate)
        {
            CheckForChanges();
        }
    }

    /// <summary>
    /// Checks if the safe area or screen size has changed and reapplies if necessary
    /// </summary>
    private void CheckForChanges()
    {
        if (Screen.safeArea != lastSafeArea ||
            Screen.width != lastScreenSize.x ||
            Screen.height != lastScreenSize.y)
        {
            ApplySafeArea();
        }
    }

    /// <summary>
    /// Applies the safe area to the RectTransform
    /// </summary>
    public void ApplySafeArea()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        Rect safeArea = Screen.safeArea;
        lastSafeArea = safeArea;
        lastScreenSize = new Vector2Int(Screen.width, Screen.height);

        // Convert safe area rectangle from absolute pixels to normalized coordinates
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        // Apply padding adjustments (convert from pixels to normalized)
        float leftPaddingNormalized = leftPadding / Screen.width;
        float rightPaddingNormalized = rightPadding / Screen.width;
        float topPaddingNormalized = topPadding / Screen.height;
        float bottomPaddingNormalized = bottomPadding / Screen.height;

        anchorMin.x += leftPaddingNormalized;
        anchorMin.y += bottomPaddingNormalized;
        anchorMax.x -= rightPaddingNormalized;
        anchorMax.y -= topPaddingNormalized;

        // Clamp to valid range
        anchorMin.x = Mathf.Clamp01(anchorMin.x);
        anchorMin.y = Mathf.Clamp01(anchorMin.y);
        anchorMax.x = Mathf.Clamp01(anchorMax.x);
        anchorMax.y = Mathf.Clamp01(anchorMax.y);

        // Apply to RectTransform
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;

        // Reset offsets since we're using anchors
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        if (debugMode)
        {
            Debug.Log($"[LeanSafeArea] Applied safe area: {safeArea} | Screen: {Screen.width}x{Screen.height} | Anchors: Min={anchorMin}, Max={anchorMax}");
        }
    }

    /// <summary>
    /// Resets the RectTransform to fill the entire screen
    /// </summary>
    public void ResetToFullScreen()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        if (debugMode)
        {
            Debug.Log("[LeanSafeArea] Reset to full screen");
        }
    }

    /// <summary>
    /// Updates the padding values and reapplies the safe area
    /// </summary>
    public void SetPadding(float left, float right, float top, float bottom)
    {
        leftPadding = left;
        rightPadding = right;
        topPadding = top;
        bottomPadding = bottom;
        ApplySafeArea();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Reapply safe area when values change in the inspector
        if (Application.isPlaying && rectTransform != null)
        {
            ApplySafeArea();
        }
    }
#endif
}
