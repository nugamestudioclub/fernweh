using UnityEngine;

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
        throw new System.NotImplementedException();
    }
}