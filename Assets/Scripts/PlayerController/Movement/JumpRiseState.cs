using UnityEngine;

public class JumpRiseState : AMovementSubState
{
    public const string Name = "JumpRiseState";

    private const int STATE_PRIORITY = 0;

    public JumpRiseState() : base(STATE_PRIORITY) { }

    public override void StateUpdate()
    {
        throw new System.NotImplementedException();
    }

    
    public override bool TryCheckForExits(out MovementState.State state_name)
    {
        throw new System.NotImplementedException();
    }
    
}
