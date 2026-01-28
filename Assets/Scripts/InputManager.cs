using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class InputManager : NetworkBehaviour
{
    private PlayerInput playerInput;
    public PlayerInput.OnFootActions onFoot;
    public PlayerInput.SpectateActions spectate;

    private PlayerMotor motor;
    private SpectatorMotor spectatorMotor;
    private PlayerLook look;
    private PlayerShoot shoot;

    private bool readyToSendInput = false;

    void Awake()
    {
        playerInput = new PlayerInput();
        onFoot = playerInput.OnFoot;
        spectate = playerInput.Spectate;

        motor = GetComponent<PlayerMotor>();
        spectatorMotor = GetComponent<SpectatorMotor>();
        look = GetComponent<PlayerLook>();
        shoot = GetComponent<PlayerShoot>();

        // Player action bindings
        onFoot.Jump.performed += ctx => { if (readyToSendInput) SendJumpServerRpc(); };
        onFoot.Crouch.performed += ctx => { if (readyToSendInput) SendCrouchServerRpc(); };
        onFoot.Sprint.performed += ctx => { if (readyToSendInput) SendSprintServerRpc(); };
        onFoot.Shoot.performed += ctx => { if (readyToSendInput) shoot.Shoot(); };
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            readyToSendInput = true;
            playerInput.Enable();
            EnablePlayerControls();
        }
        else
        {
            readyToSendInput = false;
            playerInput.Disable();
        }
    }

    void Update()
    {
        if (!readyToSendInput) return;

        if (onFoot.enabled)
        {
            // Player movement via server
            Vector2 move = onFoot.Movement.ReadValue<Vector2>();
            SendMoveServerRpc(move);
        }
        else if (spectatorMotor.isSpectating)
        {
            HandleSpectatorInput(); // fully client-side
        }
    }

    void LateUpdate()
    {
        if (!IsOwner) return;

        if (onFoot.enabled)
        {
            // Player look
            Vector2 lookInput = onFoot.Look.ReadValue<Vector2>();

            look.ProcessLook(new Vector2(0f, lookInput.y));

            float deltaYaw = lookInput.x * look.xSensitivity * Time.deltaTime;
            transform.Rotate(Vector3.up * deltaYaw);
            SendYawInputServerRpc(transform.eulerAngles.y);
        }
        // Spectator look handled inside HandleSpectatorInput()
    }

    // =================== Player vs Spectator ===================

    private void HandleSpectatorInput()
    {
        if (!spectatorMotor.isSpectating) return;

        // Look input
        Vector2 lookInput = spectate.Look.ReadValue<Vector2>();
        spectatorMotor.Look(lookInput);

        // Movement input
        Vector2 move2D = spectate.Fly.ReadValue<Vector2>();
        float upDown = (spectate.MoveUp.IsPressed() ? 1f : 0f) +
                       (spectate.MoveDown.IsPressed() ? -1f : 0f);
        bool boosting = spectate.Boost.IsPressed();

        Vector3 moveInput = new Vector3(move2D.x, upDown, move2D.y);
        spectatorMotor.Move(moveInput, boosting);
    }

    public void EnablePlayerControls()
    {
        spectate.Disable();
        onFoot.Enable();

        motor.enabled = true;
        look.enabled = true;
        shoot.enabled = true;

        spectatorMotor.SetSpectatorMode(false);
    }

    public void EnableSpectatorControls()
    {
        onFoot.Disable();
        spectate.Enable();

        motor.enabled = false;
        look.enabled = false;
        shoot.enabled = false;

        spectatorMotor.SetSpectatorMode(true);
    }

    private void OnDisable()
    {
        playerInput.Disable();
    }

    // =================== SERVER RPCS (Player Only) ===================

    [ServerRpc]
    private void SendMoveServerRpc(Vector2 input)
    {
        motor.SetMoveInput(input);
    }

    [ServerRpc]
    private void SendYawInputServerRpc(float yaw)
    {
        transform.rotation = Quaternion.Euler(0, yaw, 0);
    }

    [ServerRpc]
    private void SendJumpServerRpc()
    {
        motor.Jump();
    }

    [ServerRpc]
    private void SendCrouchServerRpc()
    {
        motor.Crouch();
    }

    [ServerRpc]
    private void SendSprintServerRpc()
    {
        motor.Sprint();
    }
}