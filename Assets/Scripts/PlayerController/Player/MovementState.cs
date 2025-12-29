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

    private const PlayerStateMachine.State STATE_ENUM = PlayerStateMachine.State.Movement;

    private PlayerStateContext m_myContext;

    public MovementState(MovementStateContext context_for_states)
    {
        p_contextForStates = context_for_states;

        // "smartly" decide what state to enter depending on the context.
        // This works because the player state context cant keep its grubby little
        // mitts off the movement state context (intentional choice), indicating through
        // the AirState what state the movement subsystem should start in.
        AMovementSubState target_state;
        if (p_contextForStates.AirState == AirState.Airborne)
        {
            if (p_contextForStates.IsJumpDown)
            {
                target_state = new JumpRiseState();
            }
            else
            {
                target_state = new AirborneState();
            }
        }
        else 
        {
            target_state = new GroundedState();
        }

        ChangeState(target_state);
    }
    
    public bool TryCheckForExits(out PlayerStateMachine.State state_enum)
    {
        if (m_myContext.IsPlayerLocked)
        {
            state_enum = PlayerStateMachine.State.Locked;
            return true;
        }

        if (m_myContext.IsOnZipline)
        {
            state_enum = PlayerStateMachine.State.OnZipline;
            return true;
        }

        state_enum = default;
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
    public PlayerStateMachine.State GetStateEnum() => STATE_ENUM;
    public void Enter() { }
    public void Exit() { }

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
