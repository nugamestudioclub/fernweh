public class DroneStateMachine : 
    AStateMachine<
        DroneStateContext, 
        ADroneState, 
        DroneStateMachine.State>
{
    public enum State
    {
        Orbit,
        Aim,
        Drone
    }

    public override ADroneState FactoryProduceState(State state_enum)
    {
        return state_enum switch
        {
            State.Orbit => new OrbitState(),
            State.Drone => new DroneFlightState(),
            State.Aim => new AimState(),
            _ => throw new System.ArgumentException("Invalid state enum: " + state_enum),
        };
    }
}