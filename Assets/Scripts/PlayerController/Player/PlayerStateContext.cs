using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateContext : MonoBehaviour, IStateContext
{
    // fields and properties here
    public MovementStateContext SubmachineStateContext;
    public Transform LineRiderAnchorTransform;

    // ---------------- public read-write ----------------
    [HideInInspector] public bool IsOnZipline;
    [HideInInspector] public Vector3 HitPoint; // INVARIANT: if IsOnZipline, HitPoint != Vec.Zero
    [HideInInspector] public ZiplineObject MountedLine; // INVARIANT: if IsOnZipline, MountedLine != null

    [HideInInspector] public CharacterController CharacterController;

    [HideInInspector] public bool IsPlayerLocked;

    private void Awake()
    {
        CharacterController = GetComponent<CharacterController>();
    }

    public void UpdateContext()
    {
        // don't fully update the substate context because we don't care about most of it.
        SubmachineStateContext.UpdateInputs();

        if (SubmachineStateContext.WasJumpPressedThisFrame && IsOnZipline)
        {
            MountedLine.Dismount();

            IsOnZipline = false;
            MountedLine = null;
        }
    }

    // pull into interface method?
    public void MountLine(ZiplineObject line, Vector3 hit_point)
    {
        // allows for zipline-zipline transfer
        if (MountedLine != null) MountedLine.Dismount();

        IsOnZipline = true;
        MountedLine = line;

        HitPoint = hit_point;
    }
}
