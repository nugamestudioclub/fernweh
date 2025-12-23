using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class DroneFlightState : ADroneState
{
    public DroneFlightState() : base(DroneStateMachine.State.Drone) { }

    public override void StateUpdate()
    {
        var inputs = p_context.MovementInput;
        (float yaw, float pitch) = CalculateRotationFromInput();

        var direction = p_context.CameraTransform.TransformDirection(new Vector3(inputs.x, 0f, inputs.y));

        var target_transform = p_context.TargetTransform;
        var target_pos = target_transform.position + Time.deltaTime * p_context.ConfigData.DroneFlySpeed * direction;

        // kinda goofy addition, but collisions are just you bonking off of things
        // this'll need to be improved later, definitely. TODO
        if (Physics.SphereCast(
            target_transform.position,
            p_context.ConfigData.CameraRadius, 
            direction, out var hit, 
            Time.deltaTime * p_context.ConfigData.DroneFlySpeed))
        {
            target_pos += Vector3.Reflect(direction, hit.normal) * 3f;
        }

        target_transform.SetPositionAndRotation(
            target_pos,
            Quaternion.Euler(-pitch, yaw, 0f));

        // this seems to pop up in two states... 
        // maybe if the exact same appears in the Aim state, we can pull this into the StateMachine?
        p_context.CameraTransform.SetPositionAndRotation(
            Vector3.Lerp(
                p_context.CameraTransform.position,
                target_transform.position,
                5f * Time.deltaTime),
            Quaternion.Lerp(
                p_context.CameraTransform.rotation,
                target_transform.rotation,
                5f * Time.deltaTime));
    }

    public override bool TryCheckForExits(out DroneStateMachine.State state_enum)
    {
        if (p_context.ToggleDroneState) // TODO add distance constraint? Or does the drone always blink back to the player?
        {
            state_enum = DroneStateMachine.State.Orbit;
            return true;
        }

        if (p_context.ToggleAimState)
        {
            state_enum = DroneStateMachine.State.Aim;
            return true;
        }

        state_enum = default;
        return false;
    }
}
