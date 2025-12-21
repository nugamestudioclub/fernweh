using UnityEngine;

public abstract class AStateMachine<C, S> : IStateMachine<C, S> where C : IStateContext where S : IState<C>
{
    protected C p_context;

    private S m_currentState;

    public void ChangeState(S to_state)
    {
        m_currentState?.Exit();

        m_currentState = to_state;
        CheckForSubmachineContext(m_currentState);

        m_currentState.Enter();
    }

    public void SetContext(C context) => p_context = context;

    public void MachineUpdate()
    {
        p_context.UpdateContext();

        /*
        if (m_currentState != null && m_currentState.TryCheckForExits(out var exit_to_state))
        {
            ChangeState(exit_to_state);
        }
        */

        m_currentState.StateUpdate();
    }

    protected S GetCurrentState() => m_currentState;

    // if the next state is a submachine that needs a context reference, pass it the one it needs.
    // hacky?
    protected virtual void CheckForSubmachineContext(S in_state) { }
}
