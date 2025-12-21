using System.Collections.Generic;
using UnityEngine;
public interface IState<T> where T : IStateContext
{
    int GetExitPriority();
    void SetStateContext(T context);

    bool TryCheckForExits(out string state_name);

    void Enter();
    void StateUpdate();
    void Exit();
}