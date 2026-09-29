
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;

    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0, 3, -5);
    public float followSpeed = 10f;
    public float rotationSpeed = 10f;

    void LateUpdate()
    {
        if (player == null) return;

        // Camera follows behind the player's rotation
        Vector3 targetPosition =
            player.position + player.rotation * offset;

        // Smooth camera movement
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );

        // Look at the player
        Vector3 lookPosition =
            player.position + Vector3.up * 1.5f;

        Quaternion targetRotation =
            Quaternion.LookRotation(
                lookPosition - transform.position
            );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}