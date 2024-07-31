using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoSingleton<InputManager>
{
    private MainInputActions playerControls;

    private void Awake()
    {
        playerControls = new MainInputActions();
        Cursor.visible = false;
        //playerControls.PlayerActions.MouseWheel.performed += x
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    #region Input_Move
    public Vector2 GetPlayerMovement()
    {
        return playerControls.PlayerActions.Movement.ReadValue<Vector2>();
    }
    #endregion

    #region Input_Look
    public Vector2 GetMouseDelta()
    {
        return playerControls.PlayerActions.Look.ReadValue<Vector2>();
    }
    #endregion

    #region Input_Jump
    public bool PlayerJumpedThisFrame()
    {
        return playerControls.PlayerActions.Jump.triggered;
    }
    #endregion

    #region Input_Run
    public bool PlayerRan()
    {
        return playerControls.PlayerActions.Run.IsPressed();
    }

    public bool PlayerRunOnce()
    {
        return playerControls.PlayerActions.Run.WasPressedThisFrame();
    }
    public bool PlayerRunReleasedOnce()
    {
        return playerControls.PlayerActions.Run.WasReleasedThisFrame();
    }
    #endregion

    #region Input_Crouch
    public bool PlayerCrouchinged()
    {
        return playerControls.PlayerActions.Crouching.triggered;
    }
    #endregion

    #region Input_Wheel
    public Vector2 InventorySwitching()
    {
        return playerControls.PlayerActions.MouseWheel.ReadValue<Vector2>();
    }

    public bool IsScrollingEnter()
    {
        return playerControls.PlayerActions.MouseWheel.WasPressedThisFrame();
    }

    public bool IsScrollingExit()
    {
        return playerControls.PlayerActions.MouseWheel.WasReleasedThisFrame();
    }
    #endregion
}
