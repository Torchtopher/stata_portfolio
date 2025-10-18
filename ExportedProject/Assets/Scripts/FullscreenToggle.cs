using UnityEngine;

public class FullscreenToggle : MonoBehaviour
{
    [Header("Fullscreen Settings")]
    [Tooltip("Key to toggle fullscreen mode")]
    public KeyCode fullscreenKey = KeyCode.F11;

    private void Update()
    {
        if (Input.GetKeyDown(fullscreenKey))
        {
            Screen.fullScreen = !Screen.fullScreen;
        }
    }
}
