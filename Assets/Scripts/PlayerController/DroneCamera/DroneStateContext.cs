using UnityEngine;
using UnityEngine.InputSystem;

public class DroneStateContext : MonoBehaviour, IStateContext
{
    public PlayerControllerConfigSO ConfigData;
    public Transform FocusTransform;

    [HideInInspector] public Transform CameraTransform;
    [HideInInspector] public Transform TargetCameraTransform;
    [HideInInspector] public Vector2 RotationInput;

    private InputAction m_cameraRotateAction;

    private void Awake()
    {
        CameraTransform = Camera.main.transform;

        TargetCameraTransform = new GameObject("Anchor").transform;

        m_cameraRotateAction = InputSystem.actions.FindAction("Look");
    }

    public void UpdateContext()
    {
        RotationInput = m_cameraRotateAction.ReadValue<Vector2>();
    }
}
