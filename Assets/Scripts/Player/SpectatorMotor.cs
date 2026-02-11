using UnityEngine;

public class SpectatorMotor : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float boostMultiplier = 3f;
    public float verticalSpeed = 8f;
    float lookSensitivity = 20f;

    [Header("References")]
    public Transform cameraHolder; // assign the pivot of the camera

    [Header("State")]
    public float yaw;
    public float pitch;
    public bool isSpectating = false;

    // --- Enter / Exit Spectator Mode ---
    public void SetSpectatorMode(bool active)
    {
        isSpectating = active;

        if (active)
        {
            // Detach camera from player hierarchy
            cameraHolder.SetParent(null, true);

            // Initialize yaw/pitch from current camera rotation
            yaw = cameraHolder.eulerAngles.y;
            pitch = cameraHolder.eulerAngles.x;
        }
        else
        {
            // Reattach camera to player
            cameraHolder.SetParent(transform, false);
            cameraHolder.localPosition = new Vector3(0f, 0.4f, 0.3f);
            cameraHolder.localRotation = Quaternion.identity;

            // Reset yaw/pitch to match player rotation
            yaw = transform.eulerAngles.y;
            pitch = 0f;
        }
    }

    // --- Call in Update() for movement ---
    public void Move(Vector3 input, bool boosting)
    {
        if (!isSpectating) return;

        float speed = moveSpeed * (boosting ? boostMultiplier : 1f);

        // Move relative to camera
        Vector3 direction = cameraHolder.forward * input.z +
                            cameraHolder.right * input.x +
                            Vector3.up * input.y;

        cameraHolder.position += direction * speed * Time.deltaTime;
    }

    // --- Call in LateUpdate() for look ---
    public void Look(Vector2 delta)
    {
        if (!isSpectating) return;

        yaw += delta.x * lookSensitivity * Time.deltaTime;
        pitch -= delta.y * lookSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -89f, 89f);

        //Debug.Log("pitch: " + pitch + ", yaw: " + yaw + ", look sensitivity: " + lookSensitivity);

        cameraHolder.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}