using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform cameraTarget;

    [Header("Distance")]
    public float distance = 4f;
    public float minDistance = 1.5f;
    public float maxDistance = 8f;

    [Header("Rotation Sensitivity")]
    public float mouseSensitivityX = 3f;
    public float mouseSensitivityY = 2f;
    public float minVerticalAngle = -10f;
    public float maxVerticalAngle = 60f;

    [Header("Zoom")]
    public float scrollSpeed = 3f;

    [Header("Collision")]
    public LayerMask collisionMask;
    public float collisionRadius = 0.2f;

    private float rotX = 20f;
    private float rotY = 0f;
    private float currentDistance;
    private bool isDragging = false;

    public float RotationY => rotY;

    void Start()
    {
        currentDistance = distance;

        // initialize from camera's actual scene rotation to avoid direction mismatch
        rotY = transform.eulerAngles.y;
        rotX = transform.eulerAngles.x;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void LateUpdate()
    {
        if (cameraTarget == null) return;

        // release cursor and pause camera when menu is open
        if (PauseMenuController.IsMenuOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        HandleMouseDrag();
        HandleScrollZoom();
        UpdatePosition();
    }

    void HandleMouseDrag()
    {
        // right mouse button to rotate camera
        if (Input.GetMouseButtonDown(1)) isDragging = true;
        if (Input.GetMouseButtonUp(1)) isDragging = false;

        if (isDragging)
        {
            rotY += Input.GetAxis("Mouse X") * mouseSensitivityX;
            rotX -= Input.GetAxis("Mouse Y") * mouseSensitivityY;
            rotX = Mathf.Clamp(rotX, minVerticalAngle, maxVerticalAngle);
        }
    }

    void HandleScrollZoom()
    {
        currentDistance -= Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
    }

    void UpdatePosition()
    {
        Quaternion rotation = Quaternion.Euler(rotX, rotY, 0f);
        Vector3 offset = rotation * new Vector3(0f, 0f, -currentDistance);
        Vector3 desiredPos = cameraTarget.position + offset;
        Vector3 finalPos = CheckCollision(cameraTarget.position, desiredPos);

        transform.position = finalPos;
        transform.rotation = rotation;
    }

    Vector3 CheckCollision(Vector3 from, Vector3 to)
    {
        Vector3 dir = to - from;
        RaycastHit hit;

        if (dir.magnitude > 0 && Physics.SphereCast(
            from, collisionRadius, dir.normalized,
            out hit, dir.magnitude, collisionMask))
        {
            return hit.point + hit.normal * collisionRadius;
        }

        return to;
    }

    public void EnableCamera()
    {
        gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void DisableCamera()
    {
        gameObject.SetActive(false);
    }
}