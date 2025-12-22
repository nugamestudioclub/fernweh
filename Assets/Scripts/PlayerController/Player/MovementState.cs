using UnityEngine;

public class MovementState : 
    AStateMachine<MovementStateContext, AMovementSubState, MovementState.State>, 
    IState<PlayerStateContext, PlayerStateMachine.State>
{
    public enum State
    {
        Grounded,
        Airborne,
        JumpRise
    }

    private PlayerStateContext m_myContext;

    public MovementState()
    {
        // DEBUG
        ChangeState(new GroundedState());
    }
    
    public bool TryCheckForExits(out PlayerStateMachine.State state_name)
    {
        // stub
        state_name = default;
        return false;
    }

    public void StateUpdate()
    {
        MachineUpdate();

        m_myContext.CharacterController.Move(
            p_contextForStates.LateralVelocity * Time.deltaTime 
            + p_contextForStates.AdditiveYVelocity * Time.deltaTime * Vector3.up);
    }

    public void SetStateContext(PlayerStateContext context) => m_myContext = context;
    public int GetExitPriority() => 0;
    public void Enter() { Debug.Log("Entered Movement."); }
    public void Exit() { Debug.Log("Exited Movement."); }

    public override AMovementSubState FactoryProduceState(State state_enum)
    {
        return state_enum switch
        {
            State.Grounded => new GroundedState(),
            State.Airborne => new AirborneState(),
            State.JumpRise => new JumpRiseState(),
            _ => throw new System.ArgumentException("Invalid state enum: " + state_enum),
        };
    }
}
