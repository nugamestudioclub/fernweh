using UnityEngine;

public abstract class AMovementSubState : IState<MovementStateContext, MovementState.State>
{
    protected MovementStateContext p_context;

    private readonly int m_priority;

    public abstract void StateUpdate();

    public abstract bool TryCheckForExits(out MovementState.State state_name);

    public virtual void Enter() { }

    public virtual void Exit() { }

    public AMovementSubState(int priority) { m_priority = priority; }

    public int GetExitPriority() => m_priority;

    public void SetStateContext(MovementStateContext context) => p_context = context;

    protected Vector3 ComputeBaseVelocity(Vector3 velocity)
    {
        var inputs = p_context.MovementInput.normalized;
        var surface_normal = p_context.SurfaceNormal;
        var perspective = p_context.PointOfView;
        var config = p_context.ConfigData;

        // if we have no surface normal, default to using the world up
        // some states, like Grounded, will always have a surface normal. This check is to abstract this method
        // so that it can be used by other states without overrides
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
        float accel_bonus = config.UseDeltaAccelerationModifier ? ComputeDeltaAcceleration(inputs, velocity) : 0;

        // change our velocity towards our target velo up to a magnitude determined by our acceleration
        return Vector3.MoveTowards(
                velocity,
                target_ground_velocity,
                (accel_bonus + config.Acceleration) * Time.deltaTime);
    }

    protected float ComputeDeltaAcceleration(Vector2 n_input_dir, Vector2 non_normalized_velo)
    {
        if (non_normalized_velo.magnitude < 0.5f) return 0f;

        var flattened = non_normalized_velo;
        flattened.y = 0f;

        return p_context.ConfigData.NormalizedDeltaAccelerationCurve.Evaluate(Vector2.Dot(flattened.normalized, n_input_dir))
                * p_context.ConfigData.DeltaAccelerationMagnitude;
    }
}
