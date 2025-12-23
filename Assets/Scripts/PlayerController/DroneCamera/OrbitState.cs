using UnityEngine;

public class OrbitState : ADroneState
{
    public OrbitState() : base(DroneStateMachine.State.Orbit) { }

    public override void StateUpdate()
    {
        (float yaw, float pitch) = CalculateRotationFromInput();

        // 
        var collision_dir = Quaternion.Euler(pitch, yaw, 0f) * Vector3.forward;

        // shoot a ray from the focus target to where the camera should be. If we hit something,
        // move our target position to just in front of it. This prevents camera clipping.
        float distance_to_focus;
        float ray_max_distance = p_context.ConfigData.OrbitDistance;
        var ray_origin = p_context.FocusTransform.position;
        if (Physics.SphereCast(
            ray_origin,
            p_context.ConfigData.CameraRadius,
            collision_dir, 
            out var hit,
            ray_max_distance,
            p_context.ConfigData.OrbitCollisionMask))
        {
            // reduce the distance by the camera radius so we dont clip into it much
            distance_to_focus = hit.distance - p_context.ConfigData.CameraRadius;
        }
        else
        {
            distance_to_focus = ray_max_distance;
        }

        Debug.DrawRay(ray_origin, collision_dir *  distance_to_focus, Color.red);

        // update target's pos and rot so we can lerp to it
        p_context.TargetCameraTransform.SetPositionAndRotation(
            ray_origin + collision_dir * distance_to_focus,
            Quaternion.Euler(pitch, yaw, 0f));
        
        // perform the lerps to the targeted transform
        p_context.CameraTransform.SetPositionAndRotation(
            Vector3.Lerp(
                p_context.CameraTransform.position,
                p_context.TargetCameraTransform.position,
                10f * Time.deltaTime), 
            Quaternion.Lerp(
                p_context.CameraTransform.rotation,
                Quaternion.Euler(-pitch, yaw + 180, 0f),
                10f * Time.deltaTime));
    }

    public override void Enter()
    {
        // when entering back into this state, since pitch is inverted and yaw is offset by 180
        // when used to move the camera to its target rotation, flip the incoming values so that
        // we get a smooth transition into this state without snapping the camera to a different rot
        FlipCorrect();
    }

    public override void Exit()
    {
        // when exiting out of this state, since pitch is inverted and yaw is offset by 180 when
        // used to move the camera to its target rotation, flip the outgoing values so that
        // we get a smooth transition out of this state without snapping the camera to a different rot
        FlipCorrect();
    }

    // symmetrical modifications made to the eulers, so we dont need two different modification methods
    // for Enter and Exit
    private void FlipCorrect()
    {
        var eulers = p_context.TargetCameraTransform.eulerAngles;
        eulers.y -= 180;
        eulers.x *= -1;

        p_context.TargetCameraTransform.eulerAngles = eulers;
    }

    public override bool TryCheckForExits(out DroneStateMachine.State state_enum)
    {
        if (p_context.ToggleDroneState)
        {
            state_enum = DroneStateMachine.State.Drone;
            return true;
        }

        state_enum = default;
        return false;
    }
}
