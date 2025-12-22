using UnityEngine;

public class PlayerStateMachine : 
    AStateMachine<
        PlayerStateContext, 
        IState<PlayerStateContext, PlayerStateMachine.State>, 
        PlayerStateMachine.State> // currently does not need an abstract move_state.
{
    public enum State
    {
        Movement,
        OnZipline
    }

    private readonly MovementStateContext m_movementSubmachineContext;

    public PlayerStateMachine(MovementStateContext submachine_context)
    {
        m_movementSubmachineContext = submachine_context;
    }

    public override IState<PlayerStateContext, State> FactoryProduceState(State state_enum)
    {
        return state_enum switch
        {
            State.Movement => new MovementState(m_movementSubmachineContext),
            State.OnZipline => default, // TEMP
            _ => throw new System.ArgumentException("Invalid state enum: " + state_enum),
        };
    }
}