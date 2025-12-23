public class DroneStateMachine : 
    AStateMachine<
        DroneStateContext, 
        IState<DroneStateContext, DroneStateMachine.State>, 
        DroneStateMachine.State>
{
    public enum State
    {
        Orbit,
        Aim,
        Drone
    }

    public override IState<DroneStateContext, State> FactoryProduceState(State state_enum)
    {
        return state_enum switch
        {
            State.Orbit => new OrbitState(),
            _ => throw new System.ArgumentException("Invalid state enum: " + state_enum),
        };
    }
}