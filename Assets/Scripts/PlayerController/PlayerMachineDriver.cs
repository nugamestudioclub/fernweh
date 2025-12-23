using UnityEngine;

public class PlayerMachineDriver : MonoBehaviour
{
    [SerializeField] private PlayerStateContext m_playerStateContext;
    [SerializeField] private MovementStateContext m_movementStateContext;
    [SerializeField] private DroneStateContext m_droneStateContext;

    private PlayerStateMachine m_playerStateMachine;
    private DroneStateMachine m_droneStateMachine;

    private void Awake()
    {
        m_playerStateMachine = new PlayerStateMachine(m_movementStateContext);
        m_playerStateMachine.SetContext(m_playerStateContext);
        m_playerStateMachine.ChangeState(m_playerStateMachine.FactoryProduceState(PlayerStateMachine.State.Movement));

        Cursor.lockState = CursorLockMode.Locked;

        m_droneStateMachine = new DroneStateMachine();
        m_droneStateMachine.SetContext(m_droneStateContext);
        m_droneStateMachine.ChangeState(m_droneStateMachine.FactoryProduceState(DroneStateMachine.State.Orbit));
    }

    private void Update()
    {
        m_playerStateMachine.MachineUpdate();
        m_droneStateMachine.MachineUpdate();
    }
}
