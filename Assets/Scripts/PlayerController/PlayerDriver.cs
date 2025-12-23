using UnityEngine;

public class PlayerDriver : MonoBehaviour
{
    [SerializeField] private PlayerStateContext m_playerStateContext;
    [SerializeField] private MovementStateContext m_movementStateContext;
    [SerializeField] private DroneStateContext m_droneStateContext;
    [SerializeField] private ZiplineShootBehavior m_shootBehavior;

    private PlayerStateMachine m_playerStateMachine;
    private DroneStateMachine m_droneStateMachine;

    private void Awake()
    {
        // setup machines
        m_playerStateMachine = new PlayerStateMachine(m_movementStateContext);
        m_playerStateMachine.SetContext(m_playerStateContext);

        m_droneStateMachine = new DroneStateMachine();
        m_droneStateMachine.SetContext(m_droneStateContext);

        Cursor.lockState = CursorLockMode.Locked;
        m_droneStateMachine.OnStateChanged += CheckIfShouldIdle;
        m_droneStateMachine.OnStateChanged += CheckIfShootState;

        // ensure we dont start in shooting state
        m_shootBehavior.gameObject.SetActive(false);

        // start machines
        m_playerStateMachine.ChangeState(m_playerStateMachine.FactoryProduceState(PlayerStateMachine.State.Movement));
        m_droneStateMachine.ChangeState(m_droneStateMachine.FactoryProduceState(DroneStateMachine.State.Drone));
    }

    private void Update()
    {
        m_playerStateMachine.MachineUpdate();
        m_droneStateMachine.MachineUpdate();
    }

    // a bit hacky for now, but this is generally how this sort of thing should go.
    // this might be better placed in the PlayerStateMachine (how to hook that up, ill think on it)
    private void CheckIfShouldIdle(
        IState<DroneStateContext, DroneStateMachine.State> _, 
        IState<DroneStateContext, DroneStateMachine.State> to)
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
    private void CheckIfShootState(
        IState<DroneStateContext, DroneStateMachine.State> _,
        IState<DroneStateContext, DroneStateMachine.State> to)
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
