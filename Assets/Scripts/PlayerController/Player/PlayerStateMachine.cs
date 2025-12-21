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
        // TODO:
        // it could be resolved if movement context was a part of the player context
        // the MovementState could instead derive the movement context from the player context it owns as a part of being a State.
        if (in_state is MovementState state)
        {
            state.SetContext(m_movementSubmachineContext);
        }
    }

    protected override IState<PlayerStateContext> FactoryProduceState(string state_name)
    {
        return default; // STUB
    }
}