using UnityEngine;

public class FreeCamController : MonoBehaviour
{
    [Header("Movement")]
    public float normalSpeed = 5f;
    public float fastSpeed = 15f;
    public float slowSpeed = 1f;
    
    [Header("Mouse")]
    public float mouseSensitivity = 2f;
    public bool invertY = false;
    
    private float rotationX = 0f;
    private float rotationY = 0f;
    private bool cursorLocked = true;
    private bool ignoreMouseInput = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // get rotation from current transform
        Vector3 rot = transform.eulerAngles;
        rotationX = rot.x;
        rotationY = rot.y;
    }

    void OnEnable()
    {
        // When re-enabled after UI closes, restore cursor lock and re-sync rotation
        if (cursorLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Re-initialize rotation from current transform to prevent snapping
        // This prevents accumulated mouse input while disabled from corrupting rotation
        Vector3 rot = transform.eulerAngles;
        rotationX = rot.x;
        rotationY = rot.y;

        // Ignore mouse input for one frame to prevent camera snap
        ignoreMouseInput = true;
    }
    
    void Update()
    {
        // Toggle cursor lock with left click
        if (Input.GetMouseButtonDown(0) && !Screen.fullScreen)
        {
            cursorLocked = !cursorLocked;
            Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !cursorLocked;
        }
        
        if (cursorLocked)
        {
            // Mouse look - skip first frame after re-enable to prevent snap
            if (ignoreMouseInput)
            {
                // Consume mouse input without applying it
                Input.GetAxis("Mouse X");
                Input.GetAxis("Mouse Y");
                ignoreMouseInput = false;
            }
            else
            {
                float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
                float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

                if (invertY) mouseY = -mouseY;

                rotationX -= mouseY;
                rotationX = Mathf.Clamp(rotationX, -90f, 90f);
                rotationY += mouseX;

                transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
            }
        }
        
        // Movement
        float speed = normalSpeed;
        if (Input.GetKey(KeyCode.LeftShift)) speed = fastSpeed;
        if (Input.GetKey(KeyCode.LeftControl)) speed = slowSpeed;
        
        float horizontal = Input.GetAxis("Horizontal"); // A/D
        float vertical = Input.GetAxis("Vertical");     // W/S
        float upDown = 0f;
        
        if (Input.GetKey(KeyCode.Q)) upDown = -1f; // Down
        if (Input.GetKey(KeyCode.E)) upDown = 1f;  // Up
        
        Vector3 direction = transform.right * horizontal + 
                           transform.forward * vertical + 
                           Vector3.up * upDown;
        
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }
}