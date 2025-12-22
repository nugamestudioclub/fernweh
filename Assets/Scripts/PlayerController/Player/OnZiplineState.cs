using UnityEngine;

public class OnZiplineState : IState<PlayerStateContext, PlayerStateMachine.State>
{
    private PlayerStateContext m_myContext;

    public bool TryCheckForExits(out PlayerStateMachine.State state_enum)
    {
        if (!m_myContext.IsOnZipline)
        {
            state_enum = PlayerStateMachine.State.Movement;
            return true;
        }

        state_enum = default;
        return false;
    }

    public void StateUpdate()
    {
        throw new System.NotImplementedException();
    }

    #region
    public void Enter() { }

    public void Exit() { }

    public int GetExitPriority() => 0;

    public void SetStateContext(PlayerStateContext context) => m_myContext = context;
    #endregion
}
