using UnityEngine;

public class AirborneState : AMovementSubState
{
    public const string Name = "AirborneState";

    private const int STATE_PRIORITY = 0;

    public AirborneState() : base(STATE_PRIORITY) { }

    public override void StateUpdate()
    {
        throw new System.NotImplementedException();
    }
    
    public override bool TryCheckForExits(out string state_name)
    {
        throw new System.NotImplementedException();
    }
    
}
