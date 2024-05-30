using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    public string currentControlScheme { get; private set; }

    public PlayerInput playerInput;

    #region In Game Input References

    public Vector2 Move { get; private set; }
    public Vector2 Look { get; private set; }

    public bool InteractPressed { get; private set; }
    public bool InteractHeld { get; private set; }
    public bool InteractReleased { get; private set; }

    public bool ReloadPressed { get; private set; }
    public bool ReloadHeld { get; private set; }
    public bool ReloadReleased { get; private set; }

    public bool JumpPressed { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool JumpReleased { get; private set; }

    public bool DashPressed { get; private set; }
    public bool DashHeld { get; private set; }
    public bool DashReleased { get; private set; }

    public bool SprintPressed { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool SprintReleased { get; private set; }

    public bool WeaponPrimaryPressed { get; private set; }
    public bool WeaponPrimaryHeld { get; private set; }
    public bool WeaponPrimaryReleased { get; private set; }

    public bool WeaponSecondaryPressed { get; private set; }
    public bool WeaponSecondaryHeld { get; private set; }
    public bool WeaponSecondaryReleased { get; private set; }

    public Vector2 SwapWeapons { get; private set; }

    public bool PauseGamePressed { get; private set; }

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction interactAction;
    private InputAction reloadAction;
    private InputAction jumpAction;
    private InputAction dashAction;
    private InputAction sprintAction;
    private InputAction weaponPrimaryAction;
    private InputAction weaponSecondaryAction;
    private InputAction swapWeaponsAction;
    private InputAction pauseGameAction;
    #endregion

    #region UI Navigation Input References

    public Vector2 Navigate { get; private set; }
    public Vector2 Point { get; private set; }
    public bool SubmitPressed { get; private set; }
    public bool CancelPressed { get; private set; }
    public bool ClickPressed { get; private set; }

    private InputAction navigateAction;
    private InputAction pointAction;
    private InputAction submitAction;
    private InputAction cancelAction;
    private InputAction clickAction;

    #endregion

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        playerInput = GetComponent<PlayerInput>();

        SetupInputActions();
    }

    private void Update()
    {
        UpdateInputs();
    }

    private void SetupInputActions()
    {   
        // In Game Inputs
        moveAction = playerInput.actions[InputActions.Move];
        lookAction = playerInput.actions[InputActions.Look];
        interactAction = playerInput.actions[InputActions.Interact];
        reloadAction = playerInput.actions[InputActions.Reload];
        jumpAction = playerInput.actions[InputActions.Jump];
        dashAction = playerInput.actions[InputActions.Dash];
        sprintAction = playerInput.actions[InputActions.Sprint];
        weaponPrimaryAction = playerInput.actions[InputActions.WeaponPrimary];
        weaponSecondaryAction = playerInput.actions[InputActions.WeaponSecondary];
        swapWeaponsAction = playerInput.actions[InputActions.SwapWeapons];
        pauseGameAction = playerInput.actions[InputActions.PauseGame];

        // UI Navigation Inputs
        navigateAction = playerInput.actions[InputActions.Navigate];
        pointAction = playerInput.actions[InputActions.Point];
        submitAction = playerInput.actions[InputActions.Submit];
        cancelAction = playerInput.actions[InputActions.Cancel];
        clickAction = playerInput.actions[InputActions.Click];
    }

    private void UpdateInputs()
    {   
        // In Game Inputs
        Move = moveAction.ReadValue<Vector2>();
        Look = lookAction.ReadValue<Vector2>();

        InteractPressed = interactAction.WasPressedThisFrame();
        InteractHeld = interactAction.IsPressed();
        InteractReleased = interactAction.WasReleasedThisFrame();

        ReloadPressed = reloadAction.WasPressedThisFrame();
        ReloadHeld = reloadAction.IsPressed();
        ReloadReleased = reloadAction.WasReleasedThisFrame();

        JumpPressed = jumpAction.WasPressedThisFrame();
        JumpHeld = jumpAction.IsPressed();
        JumpReleased = jumpAction.WasReleasedThisFrame();

        DashPressed = dashAction.WasPressedThisFrame();
        DashHeld = dashAction.IsPressed();
        DashReleased = dashAction.WasReleasedThisFrame();

        SprintPressed = sprintAction.WasPressedThisFrame();
        SprintHeld = sprintAction.IsPressed();
        SprintReleased = sprintAction.WasReleasedThisFrame();

        WeaponPrimaryPressed = weaponPrimaryAction.WasPressedThisFrame();
        WeaponPrimaryHeld = weaponPrimaryAction.IsPressed();
        WeaponPrimaryReleased = weaponPrimaryAction.WasReleasedThisFrame();

        WeaponSecondaryPressed = weaponSecondaryAction.WasPressedThisFrame();
        WeaponSecondaryHeld = weaponSecondaryAction.IsPressed();
        WeaponSecondaryReleased = weaponSecondaryAction.WasReleasedThisFrame();

        SwapWeapons = swapWeaponsAction.ReadValue<Vector2>();

        PauseGamePressed = pauseGameAction.WasPressedThisFrame();


        // UI Navigation Inputs
        Navigate = navigateAction.ReadValue<Vector2>();
        Point = pointAction.ReadValue<Vector2>();

        SubmitPressed = submitAction.WasPressedThisFrame();
        CancelPressed = cancelAction.WasPressedThisFrame();
        ClickPressed = clickAction.WasPressedThisFrame();

        currentControlScheme = playerInput.currentControlScheme;
    }
}

public static class InputActions
{
    public const string Move = "Move";
    public const string Look = "Look";
    public const string Interact = "Interact";
    public const string Reload = "Reload";
    public const string Jump = "Jump";
    public const string Dash = "Dash";
    public const string Sprint = "Sprint";
    public const string WeaponPrimary = "WeaponPrimary";
    public const string WeaponSecondary = "WeaponSecondary";
    public const string SwapWeapons = "SwapWeapons";
    public const string PauseGame = "PauseGame";

    public const string Navigate = "Navigate";
    public const string Point = "Point";
    public const string Submit = "Submit";
    public const string Cancel = "Cancel";
    public const string Click = "Click";
}