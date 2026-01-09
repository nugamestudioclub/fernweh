using System;

public abstract class AStateMachine<C, S, E> : IStateMachine<C, S, E> where C : IStateContext where S : IState<C, E> where E : Enum
{
    public delegate void StateChanged(S from_state, S to_state);
    public event StateChanged OnStateChanged;

    protected C p_contextForStates;

    private S m_currentState;

    public void ChangeState(S to_state)
    {
        m_currentState?.Exit();

        var old_cache = m_currentState;

        to_state.SetStateContext(p_contextForStates);
        m_currentState = to_state;

        // UnityEngine.Debug.Log(GetType().Name + " is changing to state: " + m_currentState.GetType().Name);

        m_currentState.Enter();

        OnStateChanged?.Invoke(old_cache, to_state);
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
