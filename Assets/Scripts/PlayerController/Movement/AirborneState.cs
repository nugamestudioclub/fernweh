using UnityEngine;

public class AirborneState : AMovementSubState
{
    private const int STATE_PRIORITY = 0;

    public AirborneState() : base(STATE_PRIORITY) { }

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
