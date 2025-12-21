using UnityEngine;

public class JumpRiseState : AMovementSubState
{
    private const int STATE_PRIORITY = 0;

    public JumpRiseState() : base(STATE_PRIORITY) { }

    public override void StateUpdate()
    {
        throw new System.NotImplementedException();
    }

    /*
    public override bool TryCheckForExits<T_Wrapper>(out T_Wrapper highest_prio_exit)
    {
        throw new System.NotImplementedException();
    }
    */
}
