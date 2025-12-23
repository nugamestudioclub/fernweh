using UnityEngine;

public class OrbitState : IState<DroneStateContext, DroneStateMachine.State>
{
    private DroneStateContext m_context;

    private float m_pitch; // needed bc the pass-by 0 makes eulers awkward

    public void StateUpdate()
    {
        float yaw =
            m_context.TargetCameraTransform.eulerAngles.y
            + m_context.RotationInput.x 
            * m_context.ConfigData.OrbitRotationSensitivity 
            * Time.deltaTime;

        m_pitch = 
            m_pitch 
            + m_context.RotationInput.y 
            * m_context.ConfigData.OrbitRotationSensitivity 
            * Time.deltaTime;

        m_pitch = Mathf.Clamp(m_pitch, -m_context.ConfigData.MaxPitchAngle, m_context.ConfigData.MaxPitchAngle);

        // 
        var collision_dir = Quaternion.Euler(m_pitch, yaw, 0f) * Vector3.forward;

        float distance_to_focus;
        float ray_max_distance = m_context.ConfigData.OrbitDistance;
        var ray_origin = m_context.FocusTransform.position;
        if (Physics.Raycast(
            ray_origin, 
            collision_dir, 
            out var hit,
            ray_max_distance,
            m_context.ConfigData.OrbitCollisionMask))
        {
            distance_to_focus = hit.distance;
        }
        else
        {
            distance_to_focus = ray_max_distance;
        }

        Debug.DrawRay(ray_origin, collision_dir *  distance_to_focus, Color.red);

        m_context.TargetCameraTransform.SetPositionAndRotation(
            ray_origin + collision_dir * distance_to_focus,
            Quaternion.Euler(m_pitch, yaw, 0f));
        
        m_context.CameraTransform.SetPositionAndRotation(
            Vector3.Lerp(
                m_context.CameraTransform.position,
                m_context.TargetCameraTransform.position,
                10f * Time.deltaTime), 
            Quaternion.Lerp(
                m_context.CameraTransform.rotation,
                Quaternion.Euler(-m_pitch, yaw + 180, 0f),
                10f * Time.deltaTime));
        //m_context.CameraTransform.forward *= -1;
    }

    public bool TryCheckForExits(out DroneStateMachine.State state_enum)
    {
        state_enum = default;
        return false;
    }

    #region Boilerplate
    public void Enter() { }
    public void Exit() { }
    public int GetExitPriority() => 0;
    public void SetStateContext(DroneStateContext context) => m_context = context;
    #endregion
}
