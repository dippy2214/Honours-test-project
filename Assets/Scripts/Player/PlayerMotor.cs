using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerMotor : NetworkBehaviour
{
    private CharacterController controller;

    private Vector3 velocity;
    private Vector2 serverMoveInput;

    private float speed = 5f;
    private bool isGrounded;

    public float gravity = -9.8f;
    public float jumpHeight = 3;

    private bool lerpCrouch;
    private bool crouching;
    private float crouchTimer;
    private bool sprinting;

    public override void OnNetworkSpawn()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (!IsServer) return;

        isGrounded = controller.isGrounded;

        // --- Handle crouch lerping ---
        if (lerpCrouch)
        {
            crouchTimer += Time.deltaTime;
            float p = crouchTimer / 1f;
            p *= p;

            controller.height = crouching ?
                Mathf.Lerp(controller.height, 1f, p) :
                Mathf.Lerp(controller.height, 2f, p);

            if (p >= 1f)
            {
                lerpCrouch = false;
                crouchTimer = 0f;
            }
        }
    }

    void FixedUpdate()
    {
        if (!IsServer) return;

        ProcessMove(serverMoveInput);
    }

    // --- Called once per server physics tick ---
    private void ProcessMove(Vector2 input)
    {
        Vector3 moveDirection = new Vector3(input.x, 0, input.y); 
        controller.Move(transform.TransformDirection(moveDirection) * speed * Time.deltaTime); 
        velocity.y += gravity * Time.deltaTime; 

        if (isGrounded && velocity.y < 0) 
            velocity.y = -2f; 

        controller.Move(velocity * Time.deltaTime);
    }

    // --- Input assignment (from client RPC) ---
    public void SetMoveInput(Vector2 input)
    {
        serverMoveInput = input;
    }

    public void Jump()
    {
        if (!IsServer) return;

        if (isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -3f * gravity);
    }

    public void Crouch()
    {
        if (!IsServer) return;

        crouching = !crouching;
        crouchTimer = 0;
        lerpCrouch = true;
    }

    public void Sprint()
    {
        if (!IsServer) return;

        sprinting = !sprinting;
        speed = sprinting ? 8f : 5f;
    }
}