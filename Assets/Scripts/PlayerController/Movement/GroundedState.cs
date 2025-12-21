using UnityEngine;

public class GroundedState : AMovementSubState
{
    public const string Name = "GroundedState";

    private const int STATE_PRIORITY = 0;

    private MovementStateContext m_context;

    private Vector3 m_velocity;

    public GroundedState() : base(STATE_PRIORITY) { }

    
    public override bool TryCheckForExits(out string state_name)
    {
        bool is_airstate_eligible_for_jump = 
            m_context.AirState == AirState.Grounded 
            || m_context.AirState == AirState.CoyoteTime;

        // grounded => jump (rise)
        if (is_airstate_eligible_for_jump && m_context.HasQueuedJumpAction.IsTrue)
        {
            // mutate the context reference to show that the change has been acknowledged
            m_context.HasQueuedJumpAction.Expire(); // jump consumed
            m_context.IsCoyoteTimerActive.Expire(); // any active coyote timer auto-expired

            // consume coyote state charge to prevent the possible single-frame of entering Coyote Time
            // when jumping from solid ground.
            m_context.CanEnterCoyoteState = false;

            // lock groundcasting so that we dont count as "grounded" for the few frames after jumping begins
            m_context.IsJumpGroundcastLocked.SetActive(0.5f);

            // slightly redundant as the Context class would establish this on the next ContextUpdate anyways
            // m_context.AirState = AirState.Airborne;

            state_name = JumpRiseState.Name;
            return true;
        }

        // grounded => airborne
        if (m_context.AirState == AirState.Airborne) // Context handles this enum's calculation
        {
            // no need to expire timers or set values
            state_name = AirborneState.Name;
            return true;
        }

        state_name = default;
        return false;
    }

    public override void StateUpdate()
    {
        // TODO may need to make an abstract MovementSubstate class or something, bc i might have a lot of dupe physics code
        // TODO handling gravity? (prolly in Airborne, with JumpRise being similar) Staircases? (in grounded, need to add more to Context when impling, imo)
        // TODO renaming contexts? rn they're just data containers for references and values, and they can be mutated, which feels weird...

        ComputeVelocity();
    }

    private void ComputeVelocity()
    {
        var inputs = m_context.MovementInput;
        var surface_normal = m_context.SurfaceNormal;
        var perspective = m_context.PointOfView;
        var config = m_context.ConfigData;

        // if we have no surface normal, default to using the world up
        if (surface_normal == Vector3.zero)
        {
            surface_normal = Vector3.up;
        }

        // obtain the surface-oriented forward direction from the POV and the normal of the surface we're on
        var forward = Vector3.Cross(perspective.right, surface_normal);

        // get the rotation from up to the surface so we can transform our lateral inputs
        var slope_quat = Quaternion.FromToRotation(Vector3.up, surface_normal);

        var right_vec = inputs.x * config.MaxGroundVelocityMagnitude * (slope_quat * perspective.right);
        var forward_vec = inputs.y * config.MaxGroundVelocityMagnitude * forward;

        // not just "xzvelo" or something like that, since this is also what keeps helps
        // us on the ground when we are on slopes or something.
        var target_ground_velocity = forward_vec + right_vec;

        // if we're using delta acceleration modifiers, compute the additional bonus acceleration now
        float accel_bonus = config.UseDeltaAccelerationModifier ? ComputeDeltaAcceleration(inputs, m_velocity) : 0;

        // change our velocity towards our target velo up to a magnitude determined by our acceleration
        m_velocity = Vector3.MoveTowards(
                m_velocity,
                target_ground_velocity,
                (accel_bonus + config.Acceleration) * Time.deltaTime);
    }

    private float ComputeDeltaAcceleration(Vector2 normalized_dir, Vector2 non_normalized_velo)
    {
        if (non_normalized_velo.magnitude < 0.5f) return 0f;

        return m_context.ConfigData.NormalizedDeltaAccelerationCurve.Evaluate(Vector2.Dot(non_normalized_velo.normalized, normalized_dir))
                * m_context.ConfigData.DeltaAccelerationMagnitude;
    }
}
