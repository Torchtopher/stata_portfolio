using UnityEngine;

public class ControllerManager : MonoBehaviour
{
    [Header("Controller References")]
    public GroundController groundController;
    public FreeCamController freeCamController;

    [Header("Settings")]
    public KeyCode toggleModeKey = KeyCode.F;
    public bool startInGroundMode = true;

    [Header("UI")]
    public ControlsDisplayUI controlsDisplay;

    private bool isGroundMode;

    private void Start()
    {
        // if not assigned try to find them
        if (groundController == null)
            groundController = GetComponent<GroundController>();

        if (freeCamController == null)
            freeCamController = GetComponent<FreeCamController>();

        if (controlsDisplay == null)
            controlsDisplay = FindObjectOfType<ControlsDisplayUI>();

        // default is ground mode
        SetMode(startInGroundMode);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleModeKey))
        {
            ToggleMode();
        }
    }

    private void ToggleMode()
    {
        SetMode(!isGroundMode);
    }

    private void SetMode(bool groundMode)
    {
        isGroundMode = groundMode;

        if (groundMode)
        {
            // enable ground mode
            if (groundController != null) groundController.enabled = true;
            if (freeCamController != null) freeCamController.enabled = false;

            Debug.Log("Switched to Ground Mode");
        }
        else
        {
            // enable freecam mode
            if (groundController != null) groundController.enabled = false;
            if (freeCamController != null) freeCamController.enabled = true;

            Debug.Log("Switched to Freecam Mode");
        }

        if (controlsDisplay != null)
        {
            controlsDisplay.RefreshDisplay();
        }

        // cursor should not be visible 
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public bool IsGroundMode()
    {
        return isGroundMode;
    }
}
