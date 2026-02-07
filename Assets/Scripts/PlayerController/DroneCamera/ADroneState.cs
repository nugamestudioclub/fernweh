using UnityEngine;

public abstract class ADroneState : IState<DroneStateContext, DroneStateMachine.State>
{
    protected DroneStateContext p_context;

    protected float p_pitch; // needed bc the pass-by 0 makes eulers awkward
    protected readonly DroneStateMachine.State p_enum;

    public ADroneState(DroneStateMachine.State enum_state) => p_enum = enum_state;

    // Using the rotation input from the context, calculates the new yaw and pitch for the state.
    // Clamps the pitch between the max angle as specified in the Config Data as well.
    protected (float yaw, float pitch) CalculateUnscaledRotationFromInput()
    {
        float yaw =
            p_context.TargetTransform.eulerAngles.y
            + p_context.RotationInput.x
            * p_context.ConfigData.OrbitRotationSensitivity;

        p_pitch +=
            p_context.RotationInput.y
            * p_context.ConfigData.OrbitRotationSensitivity;

        p_pitch = Mathf.Clamp(p_pitch, -p_context.ConfigData.MaxPitchAngle, p_context.ConfigData.MaxPitchAngle);

        return (yaw, p_pitch);
    }

    public abstract void StateUpdate();

    public abstract bool TryCheckForExits(out DroneStateMachine.State state_enum);

    public virtual void Enter() { }

    public virtual void Exit() { }

    public DroneStateMachine.State GetStateEnum() => p_enum;

    public void SetStateContext(DroneStateContext context) => p_context = context;
}
