using UnityEngine;

public class GroundController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float cameraHeight = 1.8f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2f;
    public bool invertY = false;
    public float minPitch = -60f;
    public float maxPitch = 60f;

    private Camera playerCamera;
    private float rotationX = 0f;
    private float rotationY = 0f;
    private bool cursorLocked = true;
    private bool ignoreMouseInput = false;

    private void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();

        if (playerCamera == null)
            playerCamera = Camera.main;

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Initialize rotation
        Vector3 rot = transform.eulerAngles;
        rotationY = rot.y;
        if (playerCamera != null)
        {
            rotationX = playerCamera.transform.rotation.eulerAngles.x;
            if (rotationX > 180f) rotationX -= 360f;
        }
    }

    private void OnEnable()
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
        rotationY = rot.y;
        if (playerCamera != null)
        {
            rotationX = playerCamera.transform.rotation.eulerAngles.x;
            if (rotationX > 180f) rotationX -= 360f;

            // Force camera back to correct height immediately
            Vector3 camPos = playerCamera.transform.position;
            camPos.y = cameraHeight;
            playerCamera.transform.position = camPos;
        }

        // Ignore mouse input for one frame to prevent camera snap
        ignoreMouseInput = true;
    }

    private void Update()
    {
        // Toggle cursor lock with Escape (unless UI is open)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Check if UI is open
            ProjectDetailUI detailUI = FindObjectOfType<ProjectDetailUI>();
            if (detailUI != null && detailUI.GetComponent<Canvas>()?.enabled == true)
            {
                return; // Let UI handle escape
            }

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
                rotationX = Mathf.Clamp(rotationX, minPitch, maxPitch);
                rotationY += mouseX;

                transform.rotation = Quaternion.Euler(0, rotationY, 0);
                if (playerCamera != null)
                {
                    // Use world rotation for separate camera
                    playerCamera.transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
                }
            }
        }

        // Movement
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        float horizontal = Input.GetAxis("Horizontal"); // A/D
        float vertical = Input.GetAxis("Vertical");     // W/S

        Vector3 movement = transform.right * horizontal + transform.forward * vertical;
        movement.y = 0; // Keep on horizontal plane

        transform.position += movement.normalized * speed * Time.deltaTime;
    }

    private void LateUpdate()
    {
        // Maintain camera height
        if (playerCamera != null)
        {
            Vector3 camPos = playerCamera.transform.position;
            camPos.y = cameraHeight;
            playerCamera.transform.position = camPos;
        }
    }
}
