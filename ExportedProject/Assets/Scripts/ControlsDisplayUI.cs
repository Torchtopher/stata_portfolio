using UnityEngine;
using TMPro;

public class ControlsDisplayUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI controlsText;

    [Header("Canvas Control")]
    [Tooltip("Hides the entire canvas selected, rather than just control text")]
    public Canvas canvasToToggle;

    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.F1;
    public bool showOnStart = true;

    private bool isVisible;

    private void Start()
    {
        if (controlsText == null)
        {
            Debug.LogError("ControlsDisplayUI: controlsText is not assigned");
            return;
        }

        // check parent canvas if not assigned
        if (canvasToToggle == null)
        {
            canvasToToggle = GetComponentInParent<Canvas>();
            if (canvasToToggle == null)
            {
                canvasToToggle = controlsText.canvas;
            }
        }

        SetVisible(showOnStart);
        UpdateControlsText();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            SetVisible(!isVisible);
        }

        // don't show controls if project UI is open
        ProjectDetailUI detailUI = FindObjectOfType<ProjectDetailUI>();
        if (detailUI != null && detailUI.GetComponent<Canvas>()?.enabled == true)
        {
            if (isVisible)
                SetVisible(false);
        }
    }

    private void UpdateControlsText()
    {
        if (controlsText == null) return;

        // ground vs freecam, see whats enabled
        FreeCamController freeCam = FindObjectOfType<FreeCamController>();

        if (freeCam != null && freeCam.enabled)
        {
            controlsText.text =
                "<b>FREECAM MODE</b>\n" +
                "WASD - Move\n" +
                "Q/E - Down/Up\n" +
                "Shift - Fast\n" +
                "Ctrl - Slow\n" +
                "Mouse - Look\n" +
                "F - Toggle Ground Mode\n" +
                "F1 - Hide UI";
        }
        else
        {
            controlsText.text =
                "<b>GROUND MODE</b>\n" +
                "WASD - Move\n" +
                "Mouse - Look\n" +
                "Shift - Run\n" +
                "F - Toggle Freecam\n" +
                "E - Interact\n" +
                "F1 - Hide UI";
        }
    }

    private void SetVisible(bool visible)
    {
        isVisible = visible;
        if (canvasToToggle != null)
        {
            canvasToToggle.enabled = visible;
        }
    }

    public void RefreshDisplay()
    {
        UpdateControlsText();
    }

    public void Show()
    {
        SetVisible(true);
    }
}
