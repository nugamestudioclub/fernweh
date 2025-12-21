using UnityEngine;

[CreateAssetMenu(fileName = "ConfigData", menuName = "ScriptableObject", order = 0)]
public class PlayerControllerConfigSO : ScriptableObject
{
    [Space]
    [Header("Jumping and Coyote Time")]
    public float JumpBufferDuration; // 0.1f;
    public float CoyoteDuration; // 0.05f

    [Space]
    [Header("Ground Spherecast Config")]
    public string SpherecastOriginTransformTag;
    public float GroundSpherecastDistance;
    public float GroundSpherecastRadius;
    public LayerMask GroundSpherecastMask;

    [Space]
    [Header("Movement")]
    public float MaxGroundVelocityMagnitude;
    public float Acceleration;

    [Header("Delta Acceleration")]
    public bool UseDeltaAccelerationModifier = true;
    public AnimationCurve NormalizedDeltaAccelerationCurve;
    public float DeltaAccelerationMagnitude;
}
