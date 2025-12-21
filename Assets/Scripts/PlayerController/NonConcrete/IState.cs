using System.Collections.Generic;
using UnityEngine;
public interface IState<T> where T : IStateContext
{
    int GetExitPriority();
    void SetStateContext(T context);

    // bool TryCheckForExits<T_Wrapper>(out T_Wrapper highest_prio_exit) where T_Wrapper : Self;

    void Enter();
    void StateUpdate();
    void Exit();
}