using UnityEngine;

public class AimState : ADroneState
{
    public AimState() : base(DroneStateMachine.State.Aim) { }

    public override void StateUpdate()
    {
        var target_transform = p_context.TargetTransform;

        var start_pos = p_context.FocusTransform.position + p_context.ConfigData.AimPerchOffset;
        target_transform.position = start_pos;

        (float yaw, float pitch) = CalculateRotationFromInput();

        target_transform.rotation = Quaternion.Euler(-pitch, 0f, 0f);
        target_transform.RotateAround(p_context.FocusTransform.position, Vector3.up, yaw);


        /* TODO: Fix the positional interpolation issues.
         * Has jitters when translating
         Vector3.Lerp(
                p_context.CameraTransform.position,
                p_context.TargetTransform.position,
                10f * Time.deltaTime)

        has jitters when rotating fast (bc of interpolating through the player and stuff)
        Vector3.MoveTowards(
                p_context.CameraTransform.position,
                p_context.TargetTransform.position,
                20f * Time.deltaTime)
        */

        p_context.CameraTransform.SetPositionAndRotation(
            Vector3.MoveTowards(
                p_context.CameraTransform.position,
                p_context.TargetTransform.position,
                15f * Time.deltaTime),
            Quaternion.Lerp(
                p_context.CameraTransform.rotation,
                target_transform.rotation,
                10f * Time.deltaTime));
    }

    public override void Enter()
    {
        // ensure we're facing toward the focus before we begin
        var flattened = p_context.FocusTransform.position - p_context.TargetTransform.position;
        flattened.y = 0; // our pitch will match the initial value of 0 it gets
        p_context.TargetTransform.forward = flattened; // gets normalized automatically
    }

    public override bool TryCheckForExits(out DroneStateMachine.State state_enum)
    {
        if (p_context.ToggleDroneState)
        {
            state_enum = DroneStateMachine.State.Drone;
            return true;
        }

        if (p_context.ToggleAimState)
        {
            state_enum = DroneStateMachine.State.Orbit;
            return true;
        }

        state_enum = default;
        return false;
    }
}
