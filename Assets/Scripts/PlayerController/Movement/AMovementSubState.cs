using UnityEngine;

public abstract class AMovementSubState : IState<MovementStateContext>
{
    protected MovementStateContext p_context;

    private readonly int m_priority;

    public abstract void StateUpdate();

    // public abstract bool TryCheckForExits<T_Wrapper>(out T_Wrapper highest_prio_exit) where T_Wrapper : ISelfState<MovementStateContext>;

    public virtual void Enter() { }

    public virtual void Exit() { }

    public AMovementSubState(int priority) { m_priority = priority; }

    public int GetExitPriority() => m_priority;

    public void SetStateContext(MovementStateContext context) => p_context = context;
}
