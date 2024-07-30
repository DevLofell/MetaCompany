using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private static InputManager _instance;

    public static InputManager Instance
    {
        get
        {
            return _instance;
        }
    }
    private MainInputActions playerControls;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
        playerControls = new MainInputActions();
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    public Vector2 GetPlayerMovement()
    {
        return playerControls.Player.Movement.ReadValue<Vector2>();
    }

    public Vector2 GetMouseDelta()
    {
        return playerControls.Player.Look.ReadValue<Vector2>();
    }

    public bool PlayerJumpedThisFrame()
    {
        return playerControls.Player.Jump.triggered;
    }

    public bool PlayerRan()
    {
        return playerControls.Player.Run.IsPressed();
    }

    public bool PlayerRunOnce()
    {
        return playerControls.Player.Run.WasPressedThisFrame();
    }
    public bool PlayerRunReleasedOnce()
    {
        return playerControls.Player.Run.WasReleasedThisFrame();
    }
    public bool PlayerCrouchinged()
    {
        return playerControls.Player.Crouching.triggered;
    }
}
