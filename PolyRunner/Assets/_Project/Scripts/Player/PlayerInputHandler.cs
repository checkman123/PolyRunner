using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool DashPressed { get; private set; }
    public bool SlideHeld { get; private set; }

    private PlayerInputActions _actions;

    private void Awake()
    {
        _actions = new PlayerInputActions();
    }

    private void OnEnable() => _actions.Enable();
    private void OnDisable() => _actions.Disable();

    private void Update()
    {
        MoveInput = _actions.Player.Move.ReadValue<Vector2>();
        JumpHeld = _actions.Player.Jump.IsPressed();
        SprintHeld = _actions.Player.Sprint.IsPressed();
        SlideHeld = _actions.Player.Slide.IsPressed();

        JumpPressed = _actions.Player.Jump.WasPressedThisFrame();
        DashPressed = _actions.Player.Dash.WasPressedThisFrame();
    }
}