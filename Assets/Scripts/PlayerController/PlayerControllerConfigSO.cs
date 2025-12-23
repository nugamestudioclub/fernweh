using UnityEngine;

[CreateAssetMenu(fileName = "ConfigData", menuName = "ScriptableObject/ConfigData", order = 0)]
public class PlayerControllerConfigSO : ScriptableObject
{
    [Space]
    [Header("Jumping and Coyote Time")]
    public float JumpBufferDuration; // 0.1f;
    public float CoyoteDuration; // 0.05f
    public float MaxFallSpeed;
    public AnimationCurve NormalizedJumpForceCurve;
    public float JumpForce;
    public float FinalKeyTimestamp { get => NormalizedJumpForceCurve.keys[^1].time; }

    [Space]
    [Header("Ground Spherecast Config")]
    public string SpherecastOriginTransformTag;
    public float GroundSpherecastDistance;
    public float GroundSpherecastRadius;
    public LayerMask GroundSpherecastMask;

    [Space]
    [Header("Sticky Raycast Config")]
    public string RaycastOriginTransformTag;
    public float StickyRaycastDistance;
    public LayerMask StickyRaycastMask;

    [Space]
    [Header("Movement")]
    public float MaxGroundVelocityMagnitude;
    public float Acceleration;
    [Tooltip("The force to be applied per unit distance when stickiness to ground surface take effect. A positive value.")] 
    public float StickyForceScalar;

    [Header("Delta Acceleration")]
    public bool UseDeltaAccelerationModifier = true;
    public AnimationCurve NormalizedDeltaAccelerationCurve;
    public float DeltaAccelerationMagnitude;

    [Space]
    [Header("Zipline")]
    public float MaxDriveVelocityMagnitude;
    public float DriveAcceleration;

    [Space]
    [Header("Camera and Drone")]
    public float OrbitRotationSensitivity;
    public float OrbitDistance;
    public LayerMask OrbitCollisionMask;
    public float MaxPitchAngle;
}
