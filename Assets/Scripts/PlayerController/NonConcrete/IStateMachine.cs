using System;

public interface IStateMachine<C, S, E> where C : IStateContext where S : IState<C, E> where E : Enum
{
    void ChangeState(S to_state);
    void SetContext(C context);
    void MachineUpdate();
    S FactoryProduceState(E state_enum);
}
