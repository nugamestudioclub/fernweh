using UnityEngine;

public class GroundedState : AMovementSubState
{
    public GroundedState() : base(MovementState.State.Grounded) { }

    public override bool TryCheckForExits(out MovementState.State state_name)
    {
        bool is_airstate_eligible_for_jump = 
            p_context.AirState == AirState.Grounded 
            || p_context.AirState == AirState.CoyoteTime;

        // grounded => jump (rise)
        if (is_airstate_eligible_for_jump && p_context.HasQueuedJumpAction.IsTrue)
        {
            // mutate the context reference to show that the change has been acknowledged
            p_context.HasQueuedJumpAction.Expire(); // jump consumed
            p_context.IsCoyoteTimerActive.Expire(); // any active coyote timer auto-expired

            // consume coyote state charge to prevent the possible single-frame of entering Coyote Time
            // when jumping from solid ground.
            p_context.CanEnterCoyoteState = false;

            // lock groundcasting so that we dont count as "grounded" for the few frames after jumping begins
            p_context.IsJumpGroundcastLocked.SetActive(0.5f);

            // NOTE:
            // this is slightly bad design because it forces what would be a read-only data attribute to have a certain
            // value. The reason why this is necessary is because of the Sticky-Surface "is_grounded" stuff in the context class.
            // If we start jumping, that section of code will keep us technically in AirState Grounded because our sticky
            // surface check is hitting something and we were grounded in previous frames. This line of code explicitly forces
            // us out of that state, something that needs to be done in one way or another (i.e. we need to let the code know to
            // break stickiness if we voluntarily jump).
            p_context.AirState = AirState.Airborne;

            state_name = MovementState.State.JumpRise;
            return true;
        }

        // grounded => airborne
        if (p_context.AirState == AirState.Airborne) // Context handles this enum's calculation
        {
            // no need to expire timers or set values
            state_name = MovementState.State.Airborne;
            return true;
        }

        state_name = default;
        return false;
    }

    public override void StateUpdate()
    {
        // TODO handling gravity? (prolly in Airborne, with JumpRise being similar)
        // TODO renaming contexts? rn they're just data containers for references and values, and they can be mutated, which feels weird...

        // compute the "lateral" velocity for this frame
        p_context.LateralVelocity = ComputeBaseVelocity(p_context.LateralVelocity);

        // if we need to stick to a surface...
        float sticky_velocity = 0f;
        if (p_context.DistanceToStickySurface > 0)
        {
            //...calculate it...
            sticky_velocity = AddStickyForceToVelocity(p_context.DistanceToStickySurface);
        }

        // ...note the scaled sticky velocity in our context
        p_context.AdditiveYVelocity = sticky_velocity;
    }

    private float AddStickyForceToVelocity(float sticky_distance)
    {
        // no acceleration, applies instantly
        // only scaled when applied to the CC
        return -sticky_distance * p_context.ConfigData.StickyForceScalar;
    }
}
