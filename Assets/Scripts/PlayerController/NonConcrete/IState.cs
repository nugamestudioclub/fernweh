using System;
public interface IState<T, E> where T : IStateContext where E : Enum
{
    E GetStateEnum();
    void SetStateContext(T context);

    bool TryCheckForExits(out E state_enum);

    void Enter();
    void StateUpdate();
    void Exit();
}