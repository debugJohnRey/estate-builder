using UnityEngine;

public class FirstPersonCameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform playerBody;
    public Vector3 headOffset = new Vector3(0f, 2.05f, 0.1f);

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

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void LateUpdate()
    {
        if (playerBody == null) return;
        if (PauseMenuController.IsMenuOpen) return;

        // hold right mouse button to rotate the first-person camera
        if (Input.GetMouseButton(1))
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
