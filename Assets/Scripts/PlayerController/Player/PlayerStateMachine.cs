using System;
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
        OnZipline,
        Locked
    }

    private readonly MovementStateContext m_movementSubmachineContext;
    private readonly MovementState.StateChanged[] m_movementSubstateSubscribers;

    public PlayerStateMachine(MovementStateContext submachine_context, MovementState.StateChanged[] substate_subs)
    {
        m_movementSubmachineContext = submachine_context;
        m_movementSubstateSubscribers = substate_subs;
    }

    public override IState<PlayerStateContext, State> FactoryProduceState(State state_enum)
    {
        switch (state_enum)
        {
            case State.Movement:
                var state = new MovementState(m_movementSubmachineContext);
                
                foreach (var action in m_movementSubstateSubscribers)
                {
                    state.OnStateChanged += action;
                }

                return state;

            case State.OnZipline: 
                return new OnZiplineState();

            case State.Locked:
                return new IdleState();

            default:
                throw new System.ArgumentException("Invalid state enum: " + state_enum);
        }
    }
}