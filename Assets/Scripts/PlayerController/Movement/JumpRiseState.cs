using UnityEngine;

public class JumpRiseState : AMovementSubState
{
    private const int STATE_PRIORITY = 0;

    private float m_timeElapsed;

    public JumpRiseState() : base(STATE_PRIORITY) { }

    public override bool TryCheckForExits(out MovementState.State state_enum)
    {
        // if jump is no longer held OR we've played out the full jump rise, exit this state
        if (!p_context.IsJumpDown || m_timeElapsed > p_context.ConfigData.FinalKeyTimestamp)
        {
            state_enum = MovementState.State.Airborne;
            return true;
        }

        state_enum = default;
        return false;
    }

    public override void Exit()
    {
        // the earlier you leave this state, the more it cuts your vertical velocity.
        p_context.AdditiveYVelocity /= 2f;
    }

    public override void StateUpdate()
    {
        // compute the "lateral" velocity for this frame
        p_context.LateralVelocity = ComputeBaseVelocity(p_context.LateralVelocity);

        // very similar to Airborne. This method could be put into the abstract state,
        // but they're arent entirely the same, and I would need to empty-override it
        // in Grounded (or just not use it at all, which is kinda wonky).
        p_context.AdditiveYVelocity = ComputeYVelo();

        m_timeElapsed += Time.deltaTime;
    }

    private float ComputeYVelo()
    {
        return p_context.ConfigData.NormalizedJumpForceCurve.Evaluate(m_timeElapsed) * p_context.ConfigData.JumpForce;
    }
}
