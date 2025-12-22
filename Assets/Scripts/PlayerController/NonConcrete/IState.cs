using System;
public interface IState<T, E> where T : IStateContext where E : Enum
{
    int GetExitPriority();
    void SetStateContext(T context);

    bool TryCheckForExits(out E state_enum);

    void Enter();
    void StateUpdate();
    void Exit();
}