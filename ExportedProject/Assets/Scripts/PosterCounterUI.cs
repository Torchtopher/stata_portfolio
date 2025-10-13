using UnityEngine;
using TMPro;

public class PosterCounterUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("TextMeshPro component for displaying the counter")]
    public TextMeshProUGUI counterText;

    [Header("Display Settings")]
    [Tooltip("Text format (use {0} for discovered count, {1} for total count)")]
    public string textFormat = "Posters Found: {0}/{1}";

    [Tooltip("Color when all posters are found")]
    public Color completionColor = new Color(1f, 0.84f, 0f); // Gold

    [Tooltip("Color during normal progress")]
    public Color normalColor = Color.white;

    [Header("Animation")]
    [Tooltip("Enable scale animation when discovering a new poster")]
    public bool enableDiscoveryAnimation = true;

    [Tooltip("Duration of discovery animation in seconds")]
    public float animationDuration = 0.3f;

    [Tooltip("Scale multiplier during animation")]
    public float animationScale = 1.2f;

    [Header("Debug")]
    [Tooltip("Enable debug logging to diagnose issues")]
    public bool enableDebugLogging = false;

    private Vector3 originalScale;
    private bool isAnimating = false;
    private float animationTimer = 0f;

    private void Start()
    {
        if (enableDebugLogging)
            Debug.Log($"PosterCounterUI: Start() called on GameObject '{gameObject.name}'");

        if (counterText == null)
        {
            Debug.LogError($"PosterCounterUI: counterText is not assigned on GameObject '{gameObject.name}'! Please assign it in the Inspector.");
            enabled = false;
            return;
        }

        if (enableDebugLogging)
        {
            Debug.Log($"PosterCounterUI: counterText found: '{counterText.gameObject.name}'");
            Debug.Log($"PosterCounterUI: Canvas enabled: {counterText.canvas?.enabled}");
            Debug.Log($"PosterCounterUI: Text initial value: '{counterText.text}'");
            Debug.Log($"PosterCounterUI: Text color: {counterText.color}");
            Debug.Log($"PosterCounterUI: Text active: {counterText.gameObject.activeInHierarchy}");
        }

        originalScale = counterText.transform.localScale;

        // Subscribe to discovery changes
        PosterDiscoveryTracker.Instance.OnDiscoveryChanged += OnDiscoveryChanged;

        if (enableDebugLogging)
            Debug.Log($"PosterCounterUI: Subscribed to PosterDiscoveryTracker events");

        // Initial update
        int discovered = PosterDiscoveryTracker.Instance.GetDiscoveredCount();
        int total = PosterDiscoveryTracker.Instance.GetTotalCount();

        if (enableDebugLogging)
            Debug.Log($"PosterCounterUI: Initial counts - Discovered: {discovered}, Total: {total}");

        UpdateDisplay(discovered, total);
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        // Use FindObjectOfType to avoid auto-creating during cleanup
        var tracker = FindObjectOfType<PosterDiscoveryTracker>();
        if (tracker != null)
        {
            tracker.OnDiscoveryChanged -= OnDiscoveryChanged;
        }
    }

    private void Update()
    {
        if (isAnimating)
        {
            UpdateAnimation();
        }
    }

    private void OnDiscoveryChanged(int discovered, int total)
    {
        UpdateDisplay(discovered, total);

        // Trigger animation if enabled and count increased
        if (enableDiscoveryAnimation && discovered > 0)
        {
            StartDiscoveryAnimation();
        }
    }

    private void UpdateDisplay(int discovered, int total)
    {
        if (counterText == null) return;

        // Update text
        string newText = string.Format(textFormat, discovered, total);
        counterText.text = newText;

        if (enableDebugLogging)
            Debug.Log($"PosterCounterUI: Updated text to '{newText}'");

        // Update color based on completion
        if (discovered >= total && total > 0)
        {
            counterText.color = completionColor;
            if (enableDebugLogging)
                Debug.Log($"PosterCounterUI: All posters found! Color changed to gold");
        }
        else
        {
            counterText.color = normalColor;
        }
    }

    private void StartDiscoveryAnimation()
    {
        isAnimating = true;
        animationTimer = 0f;
    }

    private void UpdateAnimation()
    {
        animationTimer += Time.deltaTime;
        float progress = animationTimer / animationDuration;

        if (progress >= 1f)
        {
            // Animation complete
            counterText.transform.localScale = originalScale;
            isAnimating = false;
            return;
        }

        // Bounce effect: scale up then down
        // Use a sine wave for smooth easing
        float scaleMultiplier = 1f + (Mathf.Sin(progress * Mathf.PI) * (animationScale - 1f));
        counterText.transform.localScale = originalScale * scaleMultiplier;
    }

    public void RefreshDisplay()
    {
        UpdateDisplay(PosterDiscoveryTracker.Instance.GetDiscoveredCount(),
                      PosterDiscoveryTracker.Instance.GetTotalCount());
    }
}
