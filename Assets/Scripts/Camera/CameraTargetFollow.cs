using UnityEngine;

public class CameraTargetFollow : MonoBehaviour
{
    public Transform player;        // drag Girl here
    public Vector3 offset = new Vector3(0f, 1.5f, 0f);

    void LateUpdate()
    {
        if (player == null) return;

        // only follow XZ position, fixed Y offset
        // this ignores all bone animation movement
        transform.position = new Vector3(
            player.position.x + offset.x,
            player.position.y + offset.y,
            player.position.z + offset.z
        );
    }
}