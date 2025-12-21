using UnityEngine;

public class MovementState : AStateMachine<MovementStateContext, AMovementSubState>, IState<PlayerStateContext>
{
    private PlayerStateContext m_superstateContext;
    
    public bool TryCheckForExits(out string state_name)
    {
        // stub
        state_name = default;
        return false;
    }
    

    public void StateUpdate()
    {
        MachineUpdate();

        // TODO: applying movement and stuff
    }

    public void SetStateContext(PlayerStateContext context) => m_superstateContext = context;
    public int GetExitPriority() => 0;
    public void Enter() { Debug.Log("Entered Movement."); }
    public void Exit() { Debug.Log("Exited Movement."); }

    protected override AMovementSubState FactoryProduceState(string state_name)
    {
        switch (state_name)
        {
            case GroundedState.Name:
                return new GroundedState();

            case AirborneState.Name:
                return new AirborneState();

            case JumpRiseState.Name:
                return new JumpRiseState();

            default:
                throw new System.ArgumentException("Invalid state name: " + state_name);
        }
    }
}
