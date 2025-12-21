using UnityEngine;
using UnityEngine.InputSystem;

public class MovementStateContext : MonoBehaviour, IStateContext
{
    // ---------------- public read-write exposed ----------------
    public Transform PointOfView; // camera transform, basically
    public PlayerControllerConfigSO ConfigData;

    // ---------------- public read-write ----------------
    [HideInInspector] public AirState AirState;

    [HideInInspector] public Vector2 MovementInput;
    [HideInInspector] public TemporaryBoolean HasQueuedJumpAction; // have we input a jump action recently?

    // can we perform the ground-checK raycast?
    // this is mutated when actions occur that should prevent a ground raycast check,
    // such as initiating a jump.
    [HideInInspector] public TemporaryBoolean IsJumpGroundcastLocked;
    [HideInInspector] public TemporaryBoolean IsCoyoteTimerActive;

    [HideInInspector] public bool CanEnterCoyoteState;

    [HideInInspector] public Vector3 SurfaceNormal;

    [HideInInspector] public CharacterController CharacterController;

    // ---------------- private exposed ----------------
    // none. private exposed things are usually config data, which go in the SO.

    // ---------------- private ----------------
    private InputAction m_movementAction;
    private InputAction m_jumpAction;

    private Transform m_originTransform;

    private void Awake()
    {
        var origin_go = GameObject.FindGameObjectWithTag(ConfigData.SpherecastOriginTransformTag);
        if (origin_go == null) throw new System.ArgumentException("Cannot find object with tag: " + ConfigData.SpherecastOriginTransformTag);

        m_originTransform = origin_go.transform;

        m_movementAction = InputSystem.actions.FindAction("Move");
        m_jumpAction = InputSystem.actions.FindAction("Jump");

        AirState = AirState.Grounded;

        HasQueuedJumpAction = new TemporaryBoolean();
        IsCoyoteTimerActive = new TemporaryBoolean();
    }

    public void UpdateContext()
    {
        bool did_hit = Physics.SphereCast(
            m_originTransform.position,
            ConfigData.GroundSpherecastRadius, Vector3.down, out var hit,
            ConfigData.GroundSpherecastDistance,
            ConfigData.GroundSpherecastMask);

        SurfaceNormal = Vector3.zero;
        // are we okay to check if grounded and we ARE grounded?
        if (!IsJumpGroundcastLocked.IsTrue && did_hit)
        {
            AirState = AirState.Grounded;
            SurfaceNormal = hit.normal;

            IsCoyoteTimerActive.Expire();
            CanEnterCoyoteState = true; // recharge coyote state
        }
        // are we okay to check, and we just missed?
        else if (!IsJumpGroundcastLocked.IsTrue) 
        {
            // if we're not already doing coyote time and can do it, set state and start the timer.
            // otherwise, dont restart it.
            //
            // jumping from solid ground requires the latter bool to be explicitly consumed, else the
            // player will be in coyote time for a single frame at immediately after Groundcast Lock goes away
            if (!IsCoyoteTimerActive.IsTrue && CanEnterCoyoteState)
            {
                AirState = AirState.CoyoteTime;

                IsCoyoteTimerActive.SetActive(ConfigData.CoyoteDuration);
                CanEnterCoyoteState = false; // consume coyote state charge
            }
            // if the coyote timer is not active, that means we either we in coyote time and ran out,
            // or we jumped from solid ground, skipping coyote time (see above).
            else if (!IsCoyoteTimerActive.IsTrue)
            {
                AirState = AirState.Airborne;
            }
        }

        HasQueuedJumpAction.Tick(Time.deltaTime);
        IsCoyoteTimerActive.Tick(Time.deltaTime);
        IsJumpGroundcastLocked.Tick(Time.deltaTime);

        MovementInput = m_movementAction.ReadValue<Vector2>();
        
        // if jump action down, start timer (even if it was already started)
        if (m_jumpAction.ReadValue<float>() > 0.5f)
        {
            HasQueuedJumpAction.SetActive(ConfigData.JumpBufferDuration);
        }
    }
}
