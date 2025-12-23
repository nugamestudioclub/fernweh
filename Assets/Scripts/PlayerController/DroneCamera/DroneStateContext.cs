using UnityEngine;
using UnityEngine.InputSystem;

public class DroneStateContext : MonoBehaviour, IStateContext
{
    public PlayerControllerConfigSO ConfigData;
    public Transform FocusTransform;

    [HideInInspector] public Transform CameraTransform;
    [HideInInspector] public Transform TargetTransform;
    [HideInInspector] public Vector2 RotationInput;
    [HideInInspector] public Vector2 MovementInput;
    [HideInInspector] public bool ToggleDroneState;
    [HideInInspector] public bool ToggleAimState;

    private InputAction m_cameraRotateAction;
    private InputAction m_moveAction; // similar to MovementStateContext. Maybe there's an abstraction here?
    private InputAction m_droneToggleAction;
    private InputAction m_aimToggleAction;

    private void Awake()
    {
        CameraTransform = Camera.main.transform;

        TargetTransform = new GameObject("Target").transform;

        m_cameraRotateAction = InputSystem.actions.FindAction("Look");
        m_moveAction = InputSystem.actions.FindAction("Move");
        m_droneToggleAction = InputSystem.actions.FindAction("Crouch");
        m_aimToggleAction = InputSystem.actions.FindAction("Interact");
    }

    public void UpdateContext()
    {
        RotationInput = m_cameraRotateAction.ReadValue<Vector2>();
        MovementInput = m_moveAction.ReadValue<Vector2>();
        ToggleDroneState = m_droneToggleAction.WasPerformedThisFrame();
        ToggleAimState = m_aimToggleAction.WasPerformedThisFrame();
    }
}
