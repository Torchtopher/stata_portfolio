using UnityEngine;

public class InteractionMarker : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Speed of rotation around Y axis (degrees per second)")]
    public float rotationSpeed = 60f;

    [Header("Bobbing Settings")]
    [Tooltip("Enable vertical bobbing animation")]
    public bool enableBobbing = true;

    [Tooltip("How far to bob up and down")]
    public float bobbingAmount = 0.2f;

    [Tooltip("Speed of bobbing animation")]
    public float bobbingSpeed = 2f;

    [Header("Fade Settings")]
    [Tooltip("How long to fade out when hiding (seconds)")]
    public float fadeOutDuration = 0.5f;

    private Vector3 startPosition;
    private float bobbingTimer = 0f;
    private bool isHiding = false;
    private float hideTimer = 0f;
    private Renderer[] renderers;
    private Color[] originalColors;
    private bool hasInteracted = false;
    private bool startPositionCaptured = false;

    private void Start()
    {
        // Get all renderers for fading
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material != null)
            {
                originalColors[i] = renderers[i].material.color;
            }
        }
    }

    private void CaptureStartPosition()
    {
        if (!startPositionCaptured)
        {
            startPosition = transform.localPosition;
            startPositionCaptured = true;
        }
    }

    public void UpdateStartPosition(Vector3 newPosition)
    {
        startPosition = newPosition;
        startPositionCaptured = true;
    }

    private void Update()
    {
        // Capture start position on first frame (after other Start() methods have run)
        CaptureStartPosition();

        if (isHiding)
        {
            UpdateHideAnimation();
            return;
        }

        // Rotate around Y axis
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        // Bob up and down
        if (enableBobbing)
        {
            bobbingTimer += Time.deltaTime * bobbingSpeed;
            float offset = Mathf.Sin(bobbingTimer) * bobbingAmount;
            transform.localPosition = startPosition + new Vector3(0, offset, 0);
        }
    }

    public void Hide()
    {
        if (hasInteracted) return; // Already hidden

        hasInteracted = true;
        isHiding = true;
        hideTimer = 0f;
    }

    public void Show()
    {
        hasInteracted = false;
        isHiding = false;
        hideTimer = 0f;
        gameObject.SetActive(true);

        // Restore original colors
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material != null)
            {
                Color color = originalColors[i];
                color.a = 1f;
                renderers[i].material.color = color;
            }
        }
    }

    public bool HasInteracted()
    {
        return hasInteracted;
    }

    private void UpdateHideAnimation()
    {
        hideTimer += Time.deltaTime;
        float progress = hideTimer / fadeOutDuration;

        if (progress >= 1f)
        {
            // Fade complete, disable the object
            gameObject.SetActive(false);
            return;
        }

        // Fade out all materials
        float alpha = 1f - progress;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material != null)
            {
                Color color = originalColors[i];
                color.a = alpha;
                renderers[i].material.color = color;
            }
        }

        // Optional: Scale down while fading
        float scale = 1f - (progress * 0.3f); // Shrink to 70% of original
        transform.localScale = Vector3.one * scale;
    }

    private void OnDisable()
    {
        // Reset bobbing position when disabled
        if (startPosition != Vector3.zero)
        {
            transform.localPosition = startPosition;
        }
    }
}
