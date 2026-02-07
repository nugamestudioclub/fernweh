using UnityEngine;
using UnityEngine.Timeline;

public class AirborneState : AMovementSubState
{
    public AirborneState() : base(MovementState.State.Airborne) { }

    public override bool TryCheckForExits(out MovementState.State state_enum)
    {
        // if grounded, go to grounded state.
        // the coyote time is just a catch case. I dont think it can come up, but you never know...
        if (p_context.AirState == AirState.Grounded || p_context.AirState == AirState.CoyoteTime)
        {
            state_enum = MovementState.State.Grounded;
            return true;
        }

        state_enum = default;
        return false;
    }

    public override void Exit()
    {
        // when leaving this state, reset the yvelo to prevent it from stacking or affecting
        // states that dont constantly interact with it (e.g. Grounded)
        p_context.AdditiveYVelocity = 0f;
    }

    public override void StateUpdate()
    {
        // compute the "lateral" velocity for this frame
        p_context.LateralVelocity = ComputeBaseVelocity(p_context.LateralVelocity);

        p_context.AdditiveYVelocity = Mathf.Max(ComputeYVelo(p_context.AdditiveYVelocity), -p_context.ConfigData.MaxFallSpeed);

        HandleSlipSlopeLateralVelocity();
    }

    private float ComputeYVelo(float current_y_velo)
    {
        return current_y_velo + p_context.ConfigData.Gravity * Time.deltaTime;
    }

    // if we're on a slippery slope, we wont see the vertical velocity since we're just stuck on it.
    // as such, this method adds a lateral displacement velocity so we "slide" against the slope
    private void HandleSlipSlopeLateralVelocity()
    {
        if (!p_context.IsOnSlipSlope) return; // INVARIANT: if on slip slope, we have surface normal

        var normal = p_context.SurfaceNormal;
        var velo = p_context.LateralVelocity;

        var surf_right = Vector3.Cross(Vector3.up, normal);
        var surf_forward = Vector3.Cross(normal, surf_right);
        var surf_downslope = -surf_forward;

        // TEST THIS: lateral velo is supposed to have no y component, but if it works we'll figure it out
        p_context.LateralVelocity += 
            Time.deltaTime * 
            p_context.ConfigData.SlopeSlideForceScalar *
            Mathf.Abs(p_context.AdditiveYVelocity) * 
            Mathf.Cos(Vector3.Angle(surf_downslope, Vector3.down)) * 
            surf_downslope;
    }
}
