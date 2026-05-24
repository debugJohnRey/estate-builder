using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;            // Drag your Girl character here
    public Vector3 offset = new Vector3(0f, 2.5f, -4f); // Height and distance behind character
    public float smoothSpeed = 10f;     // Camera catch-up speed

    void LateUpdate()
    {
        if (target == null) return;

        // Calculate the desired position behind the character
        Vector3 desiredPosition = target.TransformPoint(offset);

        // Smoothly slide the camera to that position
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Keep the camera looking at the character's upper body
        Vector3 lookAtTarget = target.position + Vector3.up * 1.5f;
        transform.LookAt(lookAtTarget);
    }
}