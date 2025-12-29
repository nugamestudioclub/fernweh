using UnityEngine;

public class IdleState : IState<PlayerStateContext, PlayerStateMachine.State>
{
    private const PlayerStateMachine.State STATE_ENUM = PlayerStateMachine.State.Locked;

    private PlayerStateContext m_context;

    private PlayerStateMachine.State m_exitToState;

    public void Enter() 
    { 
        // cache the state we broke from so we can return to it when done
        if (m_context.IsOnZipline)
        {
            m_exitToState = PlayerStateMachine.State.OnZipline;
        }
        else // only one other state, since idle cannot interrupt itself.
        {
            m_exitToState = PlayerStateMachine.State.Movement;
        }
    }

    public void StateUpdate()
    {
        // pass, ignore all
        // ...EXCEPT for gravity. If needed.

        if (m_context.SubmachineStateContext.AirState == AirState.Grounded) return;

        m_context.SubmachineStateContext.AdditiveYVelocity = 
            Mathf.Max(
                m_context.SubmachineStateContext.AdditiveYVelocity + Physics.gravity.y * Time.deltaTime, 
                -m_context.SubmachineStateContext.ConfigData.MaxFallSpeed);
    }

    public bool TryCheckForExits(out PlayerStateMachine.State state_enum)
    {
        if (!m_context.IsPlayerLocked)
        {
            state_enum = m_exitToState;
            return true;
        }

        state_enum = default;
        return false;
    }

    #region
    public void Exit() { }
    public PlayerStateMachine.State GetStateEnum() => STATE_ENUM;
    public void SetStateContext(PlayerStateContext context) => m_context = context;
    #endregion
}
