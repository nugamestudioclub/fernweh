using UnityEngine;

public interface IStateMachine<C, S> where C : IStateContext where S : IState<C>
{
    void ChangeState(S to_state);
    void SetContext(C context);
    void MachineUpdate();
}
