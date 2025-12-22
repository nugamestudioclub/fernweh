using UnityEngine;

public class PlayerMachineDriver : MonoBehaviour
{
    [SerializeField] private PlayerStateContext m_playerStateContext;
    [SerializeField] private MovementStateContext m_movementStateContext;

    private PlayerStateMachine m_playerStateMachine;

    private void Awake()
    {
        m_playerStateMachine = new PlayerStateMachine(m_movementStateContext);
        m_playerStateMachine.SetContext(m_playerStateContext);
        m_playerStateMachine.ChangeState(m_playerStateMachine.FactoryProduceState(PlayerStateMachine.State.Movement));
    }

    private void Update()
    {
        m_playerStateMachine.MachineUpdate();
    }
}
