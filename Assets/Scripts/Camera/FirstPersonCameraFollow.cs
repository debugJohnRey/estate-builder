using UnityEngine;

public class FirstPersonCameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform playerBody;
    public Vector3 headOffset = new Vector3(0f, 1.7f, 0.1f);

    [Header("Mouse Look")]
    public float mouseSensitivityX = 3f;
    public float mouseSensitivityY = 2f;
    public float minVerticalAngle = -60f;
    public float maxVerticalAngle = 60f;

    [Header("Smoothing")]
    public float smoothing = 15f;

    private float rotX = 0f;
    private float rotY = 0f;
    private float currentRotX = 0f;

    public float RotationY => rotY;

    void OnEnable()
    {
        if (playerBody != null)
            rotY = playerBody.eulerAngles.y;

        if (!PauseMenuController.IsMenuOpen)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus || PauseMenuController.IsMenuOpen) return;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void LateUpdate()
    {
        if (playerBody == null) return;

        // release cursor and pause camera when menu is open
        if (PauseMenuController.IsMenuOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        HandleMouseLook();
        UpdatePosition();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivityX;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivityY;

        rotX -= mouseY;
        rotX = Mathf.Clamp(rotX, minVerticalAngle, maxVerticalAngle);
        rotY += mouseX;

        currentRotX = Mathf.Lerp(currentRotX, rotX, smoothing * Time.deltaTime);
    }

    void UpdatePosition()
    {
        // follow player root position + fixed head offset (ignores bone animation)
        Vector3 headPos = new Vector3(
            playerBody.position.x,
            playerBody.position.y + headOffset.y,
            playerBody.position.z
        );

        transform.position = headPos + playerBody.forward * headOffset.z;
        transform.rotation = Quaternion.Euler(currentRotX, rotY, 0f);
        playerBody.rotation = Quaternion.Euler(0f, rotY, 0f);
    }
}