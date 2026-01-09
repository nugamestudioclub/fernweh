using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementStateContext : MonoBehaviour, IStateContext
{
    // ---------------- public read-write exposed ----------------
    public Transform PointOfView; // camera transform, basically
    public PlayerControllerConfigSO ConfigData;

    // ---------------- public read-write ----------------
    // TODO: Separate this into public read, and public read-write? Not all of these need to be mutated outside this class.
    public AirState AirState;

    [HideInInspector] public Vector2 MovementInput;
    public bool IsOnSlipSlope; // if we're on a too-intense incline and need to slide against it
    [HideInInspector] public bool IsJumpDown;
    [HideInInspector] public bool WasJumpPressedThisFrame;
    [HideInInspector] public TemporaryBoolean HasQueuedJumpAction; // have we input a jump action recently?

    // Not really "lateral" velocity, but moreso the velocity on the plane defined by the surface we
    // are sticking on (see Surface Normal and Sticky). It's basically on the XZ plane of the surface's
    // local transform, but in world space (so we do sometimes have a Y component if the plane is tilted in
    // world space, for instance).
    //
    // Also used by the Player SuperStatemachine to store Line-travel velocity. Allows for line velocity to
    // be carried over into the submachine, which is nice.
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
        // caching transforms for cast origins
        var origin_go_s = GameObject.FindGameObjectWithTag(ConfigData.SpherecastOriginTransformTag);
        if (origin_go_s == null) throw new System.ArgumentException("Cannot find object with tag: " + ConfigData.SpherecastOriginTransformTag);

        m_spherecastOrigin = origin_go_s.transform;

        var origin_go_r = GameObject.FindGameObjectWithTag(ConfigData.RaycastOriginTransformTag);
        if (origin_go_r == null) throw new System.ArgumentException("Cannot find object with tag: " + ConfigData.RaycastOriginTransformTag);

        m_raycastOrigin = origin_go_r.transform;

        // input actions
        m_movementAction = InputSystem.actions.FindAction("Move");
        m_jumpAction = InputSystem.actions.FindAction("Jump");

        // default state is grounded
        AirState = AirState.Grounded;

        HasQueuedJumpAction = new TemporaryBoolean();
        IsCoyoteTimerActive = new TemporaryBoolean();
        IsJumpGroundcastLocked = new TemporaryBoolean();
    }

    public void UpdateInputs()
    {
        MovementInput = m_movementAction.ReadValue<Vector2>();
        IsJumpDown = m_jumpAction.IsPressed();
        WasJumpPressedThisFrame = m_jumpAction.WasPerformedThisFrame();
    }

    public void UpdateContext()
    {
        PerformStickyRaycast();

        // ground spherecast data handling needs updated sticky ray information
        PerformGroundSpherecast();

        TickTemporaryBooleans();

        UpdateInputs();
        
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

        // see below for the logical explanation of this
        bool is_already_grounded = AirState == AirState.Grounded;
        bool grounded_this_frame = is_already_grounded ? did_hit || HasStickySurface() : did_hit;

        IsOnSlipSlope = false;
        SurfaceNormal = Vector3.zero;

        // if we did hit terrain and the angle is too steep, we're not grounded
        if (did_hit && Vector3.Angle(Vector3.up, hit.normal) > ConfigData.MaxInclineAngle)
        {
            grounded_this_frame = false;
            IsOnSlipSlope = true;
            SurfaceNormal = hit.normal;
        }

        // are we okay to check if grounded and we ARE grounded?
        // grounded in this context has two cases depending on if we were grounded the previous frame (i.e.
        // we have a surface to stick to). 
        // 1. We are landing onto the ground. Dont use the extended sticky ray as a ground check.
        // 2. We are walking on ground. Use the sticky ray to say we're still grounded.
        if (!IsJumpGroundcastLocked.IsTrue && grounded_this_frame)
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

    private bool HasStickySurface() => DistanceToStickySurface > 0f;
}
