using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProximityPopup : MonoBehaviour
{
    [Header("UI References")]
    public Canvas canvas;
    public TextMeshProUGUI promptText;
    public Image backgroundImage;
    
    [Header("Animation Settings")]
    public float fadeSpeed = 5.0f;
    public float pulseSpeed = 2.0f;
    public float pulseIntensity = 0.2f;
    
    private CanvasGroup canvasGroup;
    private float baseAlpha = 0.8f;
    private bool isVisible = false;
    
    private void Awake()
    {
        if (canvas == null)
            canvas = GetComponent<Canvas>();
            
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            
        canvasGroup.alpha = 0f;
        SetVisible(false);
    }
    
    private void Update()
    {
        if (isVisible && canvasGroup != null)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, baseAlpha + pulse, Time.deltaTime * fadeSpeed);
        }
    }
    
    public void SetVisible(bool visible)
    {
        isVisible = visible;
        if (canvas != null)
            canvas.enabled = visible;
            
        if (!visible && canvasGroup != null)
            canvasGroup.alpha = 0f;
    }
    
    public void SetPromptText(string text)
    {
        if (promptText != null)
            promptText.text = text;
    }
    
    public void SetInteractionKey(KeyCode key)
    {
        string keyText = key.ToString();
        if (key == KeyCode.E)
            keyText = "E";
        else if (key == KeyCode.F)
            keyText = "F";
        else if (key == KeyCode.Space)
            keyText = "Space";
            
        SetPromptText($"Press [{keyText}] to view project details");
    }
}