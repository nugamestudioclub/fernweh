using UnityEngine;

public class PlayerStateContext : MonoBehaviour, IStateContext
{
    // fields and properties here

    // Semi-debug, semi-stub, semi-partial implementation
    // these should be private-write, public-read, rather than public-all
    public bool IsOnZipline;

    public CharacterController CharacterController;

    public void UpdateContext()
    {
        // stub
    }
}
