using UnityEngine;

public class PlayerDriver : MonoBehaviour
{
    [SerializeField] private PlayerStateContext m_playerStateContext;
    [SerializeField] private MovementStateContext m_movementStateContext;
    [SerializeField] private DroneStateContext m_droneStateContext;

    [Space]

    [SerializeField] private AnchorShootBehavior m_shootBehavior;
    [SerializeField] private PlayerAnimationManager m_animationManager;

    private PlayerStateMachine m_playerStateMachine;
    private DroneStateMachine m_droneStateMachine;

    private void Awake()
    {
        // setup machines ----

        // create player machine with animation manager's MovementStateChange listener passed as a func
        // to subscribe whenever MovementState comes around
        m_playerStateMachine = 
            new PlayerStateMachine(
                m_movementStateContext,
                new MovementState.StateChanged[] { m_animationManager.OnMovementStateChange });

        m_playerStateMachine.SetContext(m_playerStateContext);

        // subscriptions for animation
        m_playerStateMachine.OnStateChanged += m_animationManager.OnPlayerStateChange;

        // creating drone state machine and locking cursor
        m_droneStateMachine = new DroneStateMachine();
        Cursor.lockState = CursorLockMode.Locked;
        m_droneStateMachine.SetContext(m_droneStateContext);

        // subscriptions for state changes for camera; the main way to tell when entering Aim state
        m_droneStateMachine.OnStateChanged += CheckIfShouldIdle;
        m_droneStateMachine.OnStateChanged += CheckIfShootState;
        m_droneStateMachine.OnStateChanged += m_animationManager.CheckAimState;

        // ensure we dont start in shooting state
        m_shootBehavior.gameObject.SetActive(false);

        // start machines
        m_playerStateMachine.ChangeState(m_playerStateMachine.FactoryProduceState(PlayerStateMachine.State.Movement));
        m_droneStateMachine.ChangeState(m_droneStateMachine.FactoryProduceState(DroneStateMachine.State.Orbit));
    }

    // Desub when destroyed. Unlikely to happen, but just in case.
    private void OnDestroy()
    {
        m_playerStateMachine.OnStateChanged -= m_animationManager.OnPlayerStateChange;

        m_droneStateMachine.OnStateChanged -= CheckIfShouldIdle;
        m_droneStateMachine.OnStateChanged -= CheckIfShootState;
        m_droneStateMachine.OnStateChanged -= m_animationManager.CheckAimState;
    }

    // on update cycle, update the machines
    private void Update()
    {
        m_playerStateMachine.MachineUpdate();
        m_droneStateMachine.MachineUpdate();
    }

    // a bit hacky for now, but this is generally how this sort of thing should go.
    // this might be better placed in the PlayerStateMachine (how to hook that up, ill think on it)
    private void CheckIfShouldIdle(ADroneState _, ADroneState to)
    {
        if (to.GetStateEnum() == DroneStateMachine.State.Drone)
        {
            m_playerStateContext.IsPlayerLocked = true;
        }
        else
        {
            m_playerStateContext.IsPlayerLocked = false;
        }
    }

    // if we're in the aim state, activate our shooting behavior script to go along with it.
    // disable it if we're not, tho.
    private void CheckIfShootState(ADroneState _, ADroneState to)
    {
        if (to.GetStateEnum() == DroneStateMachine.State.Aim)
        {
            m_shootBehavior.gameObject.SetActive(true);
        }
        else
        {
            m_shootBehavior.gameObject.SetActive(false);
        }
    }
}
