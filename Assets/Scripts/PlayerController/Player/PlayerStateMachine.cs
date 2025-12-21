using UnityEngine;

public class PlayerStateMachine : AStateMachine<PlayerStateContext, IState<PlayerStateContext>> // currently does not need an abstract state.
{
    private readonly MovementStateContext m_movementSubmachineContext;

    public PlayerStateMachine(MovementStateContext submachine_context)
    {
        m_movementSubmachineContext = submachine_context;
    }

    protected override void CheckForSubmachineContext(IState<PlayerStateContext> in_state)
    {
        // this feels weird...
        if (in_state is MovementState state)
        {
            state.SetContext(m_movementSubmachineContext);
        }
    }
}