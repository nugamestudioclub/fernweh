using UnityEngine;
using UnityEngine.InputSystem;

public class DroneStateContext : MonoBehaviour, IStateContext
{
    public PlayerControllerConfigSO ConfigData;
    public Transform FocusTransform;

    [HideInInspector] public Transform CameraTransform;
    [HideInInspector] public Transform TargetCameraTransform;
    [HideInInspector] public Vector2 RotationInput;
    [HideInInspector] public Vector2 MovementInput;
    [HideInInspector] public bool ToggleDroneState;

    private InputAction m_cameraRotateAction;
    private InputAction m_moveAction; // similar to MovementStateContext. Maybe there's an abstraction here?
    private InputAction m_droneToggleAction;

    private void Awake()
    {
        CameraTransform = Camera.main.transform;

        TargetCameraTransform = new GameObject("Anchor").transform;

        m_cameraRotateAction = InputSystem.actions.FindAction("Look");
        m_moveAction = InputSystem.actions.FindAction("Move");
        m_droneToggleAction = InputSystem.actions.FindAction("Crouch");
    }

    public void UpdateContext()
    {
        RotationInput = m_cameraRotateAction.ReadValue<Vector2>();
        MovementInput = m_moveAction.ReadValue<Vector2>();
        ToggleDroneState = m_droneToggleAction.WasPerformedThisFrame();
    }
}
