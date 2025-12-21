using UnityEngine;

public abstract class AMovementSubState : IState<MovementStateContext>
{
    protected MovementStateContext p_context;

    private readonly int m_priority;

    public abstract void StateUpdate();

    public abstract bool TryCheckForExits(out string state_name);

    public virtual void Enter() { }

    public virtual void Exit() { }

    public AMovementSubState(int priority) { m_priority = priority; }

    public int GetExitPriority() => m_priority;

    public void SetStateContext(MovementStateContext context) => p_context = context;
}
