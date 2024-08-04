using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoSingleton<InputManager>
{
    private MainInputActions playerControls;
    public bool inputCrouch = false;
    private bool inputEnabled = true;
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
        if (!inputEnabled) return Vector2.zero;
        
        return playerControls.PlayerActions.Look.ReadValue<Vector2>();
    }

    public void EnableInput(bool enable)
    {
        inputEnabled = enable;
    }

    public bool IsInputEnabled()
    {
        return inputEnabled;
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
        if (playerControls.PlayerActions.Run.IsPressed())
        {
            inputCrouch = false;
            return true;
        }
        
        return false;
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
        //ToggleCrouchinged();
        return playerControls.PlayerActions.Crouching.triggered;
    }

    public bool ToggleCrouchinged()
    {
        if (inputCrouch == false && playerControls.PlayerActions.Crouching.triggered)
        {
            inputCrouch = true;
        }
        else if (inputCrouch == true && playerControls.PlayerActions.Crouching.triggered)
        {
            inputCrouch = false;
        }
        return inputCrouch;
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

    #region Input_Interactive
    public bool PlayerInteractionThisFrame()
    {
        return playerControls.PlayerActions.Interaction.triggered;
    }
    #endregion
}
