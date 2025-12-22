using UnityEngine;
using UnityEngine.InputSystem;

public class MovementStateContext : MonoBehaviour, IStateContext
{
    // ---------------- public read-write exposed ----------------
    public Transform PointOfView; // camera transform, basically
    public PlayerControllerConfigSO ConfigData;

    // ---------------- public read-write ----------------
    // TODO: Separate this into public read, and public read-write? Not all of these need to be mutated outside this class.
    [HideInInspector] public AirState AirState;

    [HideInInspector] public Vector2 MovementInput;
    [HideInInspector] public TemporaryBoolean HasQueuedJumpAction; // have we input a jump action recently?

    // Not really "lateral" velocity, but moreso the velocity on the plane defined by the surface we
    // are sticking on (see Surface Normal and Sticky). It's basically on the XZ plane of the surface's
    // local transform, but in world space (so we do sometimes have a Y component if the plane is tilted in
    // world space, for instance).
    [HideInInspector] public Vector3 LateralVelocity;
    [HideInInspector] public float AdditiveYVelocity;

    // can we perform the ground-checK raycast?
    // this is mutated when actions occur that should prevent a ground raycast check,
    // such as initiating a jump.
    [HideInInspector] public TemporaryBoolean IsJumpGroundcastLocked;
    [HideInInspector] public TemporaryBoolean IsCoyoteTimerActive;

    [HideInInspector] public bool CanEnterCoyoteState;

    // how far we are from a surface we need to stick to
    // the further we are, the stronger our attraction force to the ground should be
    // so we don't detach from the surface (like on descending stairs)
    //
    // If negative, we don't have a sticky surface in reach.
    [HideInInspector] public float DistanceToStickySurface;

    [HideInInspector] public Vector3 SurfaceNormal;

    // ---------------- private exposed ----------------
    // none. private exposed things are usually config data, which go in the SO.

    // ---------------- private ----------------
    private InputAction m_movementAction;
    private InputAction m_jumpAction;

    private Transform m_spherecastOrigin;
    private Transform m_raycastOrigin;

    private void Awake()
    {
        var origin_go_s = GameObject.FindGameObjectWithTag(ConfigData.SpherecastOriginTransformTag);
        if (origin_go_s == null) throw new System.ArgumentException("Cannot find object with tag: " + ConfigData.SpherecastOriginTransformTag);

        m_spherecastOrigin = origin_go_s.transform;

        var origin_go_r = GameObject.FindGameObjectWithTag(ConfigData.RaycastOriginTransformTag);
        if (origin_go_r == null) throw new System.ArgumentException("Cannot find object with tag: " + ConfigData.RaycastOriginTransformTag);

        m_raycastOrigin = origin_go_r.transform;


        m_movementAction = InputSystem.actions.FindAction("Move");
        m_jumpAction = InputSystem.actions.FindAction("Jump");

        AirState = AirState.Grounded;

        HasQueuedJumpAction = new TemporaryBoolean();
        IsCoyoteTimerActive = new TemporaryBoolean();
    }

    public void UpdateContext()
    {
        PerformGroundSpherecast();

        PerformStickyRaycast();

        TickTemporaryBooleans();

        MovementInput = m_movementAction.ReadValue<Vector2>();
        
        // if jump action down, start timer (even if it was already started)
        if (m_jumpAction.ReadValue<float>() > 0.5f)
        {
            HasQueuedJumpAction.SetActive(ConfigData.JumpBufferDuration);
        }
    }

    private void PerformGroundSpherecast()
    {
        bool did_hit = Physics.SphereCast(
            m_spherecastOrigin.position,
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
    }

    private void PerformStickyRaycast()
    {
        bool did_hit = Physics.Raycast(
            m_raycastOrigin.position, Vector3.down, out var hit,
            ConfigData.StickyRaycastDistance,
            ConfigData.StickyRaycastMask);

        if (did_hit)
        {
            DistanceToStickySurface = hit.distance;
        }
        else
        {
            DistanceToStickySurface = -1f;
        }
    }

    private void TickTemporaryBooleans()
    {
        HasQueuedJumpAction.Tick(Time.deltaTime);
        IsCoyoteTimerActive.Tick(Time.deltaTime);
        IsJumpGroundcastLocked.Tick(Time.deltaTime);
    }
}
