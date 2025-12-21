using UnityEngine;

public class MovementState : AStateMachine<MovementStateContext, AMovementSubState>, IState<PlayerStateContext>
{
    private PlayerStateContext m_superstateContext;

    /*
    public bool TryCheckForExits(out ISelfState<PlayerStateContext> highest_prio_exit)
    {
        // stub
        highest_prio_exit = default;
        return false;
    }
    */

    public void StateUpdate()
    {
        MachineUpdate();

        
    }

    public void SetStateContext(PlayerStateContext context) => m_superstateContext = context;
    public int GetExitPriority() => 0;
    public void Enter() { Debug.Log("Entered Movement."); }
    public void Exit() { Debug.Log("Exited Movement."); }
}
