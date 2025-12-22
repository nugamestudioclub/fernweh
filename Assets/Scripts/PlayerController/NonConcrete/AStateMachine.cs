using System;

public abstract class AStateMachine<C, S, E> : IStateMachine<C, S, E> where C : IStateContext where S : IState<C, E> where E : Enum
{
    protected C p_contextForStates;

    private S m_currentState;

    public void ChangeState(S to_state)
    {
        m_currentState?.Exit();

        m_currentState = to_state;

        m_currentState.SetStateContext(p_contextForStates);

        m_currentState.Enter();
    }

    public void SetContext(C context) => p_contextForStates = context;

    public void MachineUpdate()
    {
        p_contextForStates.UpdateContext();
        
        if (m_currentState != null && m_currentState.TryCheckForExits(out E exit_to_state))
        {
            ChangeState(FactoryProduceState(exit_to_state));
        }
        
        m_currentState.StateUpdate();
    }

    public abstract S FactoryProduceState(E state_enum);

    protected S GetCurrentState() => m_currentState;
}
