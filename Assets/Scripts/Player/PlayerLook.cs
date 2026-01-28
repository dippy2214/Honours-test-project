using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerLook : NetworkBehaviour
{
    public Camera cam;

    public float xRotation = 0f;

    public float ySensitivity = 20f;
    public float xSensitivity = 20f;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            cam.enabled = false;
            cam.gameObject.SetActive(false);
        }
    }

    // Client-only pitch (up/down)
    public void ProcessLook(Vector2 input)
    {
        if (!IsOwner) return;

        float mouseY = input.y * ySensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}