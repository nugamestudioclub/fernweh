using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerAnimationManager : MonoBehaviour
{
    [SerializeField] private Transform m_facingDirTransform;

    [Space]

    [SerializeField] private Animator m_animator;
    [SerializeField] private RigBuilder m_rigs;
    [SerializeField] private int m_footIKRigLayerIndex;
    [SerializeField] private int m_aimRigLayerIndex;

    [Space]

    [SerializeField] private float m_animSpeedPerVelo = 3f;

    [Space]

    [SerializeField] private Transform m_headBone;
    [SerializeField] private Transform m_lookTarget;
    [SerializeField] private Transform m_reticleTarget;
    [SerializeField] private float m_lookTargetDistance;

    [Space]

    // minor optimization, but disabling feet IK scripts when rig is disabled
    // prevents needless spherecasts
    [SerializeField] private MonoBehaviour[] m_feetIKsToToggle;
    [SerializeField] private MovementStateContext m_movementContext; // to source velocity

    private Vector3 m_previousTargetForward;
    private bool m_isLocked;

    private enum AnimatorState
    {
        MovementTree = 0,
        ZiplinePose = 1,
        JumpRise = 2,
        Airborne = 3
    }

    private enum OrientState
    {
        None,
        ReticleDirection,
        MoveDirection
    }

    private OrientState m_orientMode;
    private float m_xVel;
    private float m_yVel;

    // param hashes ----------
    private readonly int m_state = Animator.StringToHash("State");
    private readonly int m_moveXHash = Animator.StringToHash("Move_X");
    private readonly int m_moveYHash = Animator.StringToHash("Move_Y");
    private readonly int m_aimTrigger = Animator.StringToHash("Trigger");

    private void LateUpdate()
    {
        var velocity = m_movementContext.LateralVelocity;

        m_previousTargetForward = m_facingDirTransform.forward;

        // orient body direction
        var target_forward = m_facingDirTransform.forward;
        switch (m_orientMode)
        {
            case OrientState.ReticleDirection:
                target_forward = m_reticleTarget.position - m_headBone.position;
                break;

            case OrientState.MoveDirection:
                // if look vector is pretty close to 0 (or is 0), skip
                if (velocity.magnitude < 0.05f) break;
                target_forward = velocity.normalized;
                break;

            default: // None = pass orientation step
                break;
        }

        m_lookTarget.position =
            Vector3.Slerp(
                m_lookTarget.position, 
                m_headBone.position + m_lookTargetDistance * target_forward, 
                5f*Time.deltaTime);

        var flattened_forward = new Vector3(target_forward.x, 0f, target_forward.z);
        m_facingDirTransform.forward = Vector3.Lerp(m_facingDirTransform.forward, flattened_forward.normalized, 10f * Time.deltaTime);
        
        // transform velocity to pass it to animator correctly
        var norm_velo = m_facingDirTransform.InverseTransformVector(velocity).normalized;

        if (m_isLocked)
        {
            m_xVel = 0f;
        }

        m_xVel = Mathf.Lerp(m_xVel, norm_velo.x, 10f * Time.deltaTime);
        m_yVel = Mathf.Lerp(m_yVel, norm_velo.z, 10f * Time.deltaTime);
        m_animator.SetFloat(m_moveXHash, m_xVel);
        m_animator.SetFloat(m_moveYHash, m_yVel);

        m_animator.speed = Mathf.Max(1f, velocity.magnitude / m_animSpeedPerVelo);
    }

    public void OnPlayerStateChange(
        IState<PlayerStateContext, PlayerStateMachine.State> from,
        IState<PlayerStateContext, PlayerStateMachine.State> to)
    {
        Debug.Log(to.GetStateEnum());
        m_isLocked = false;

        switch (to.GetStateEnum())
        {
            case PlayerStateMachine.State.OnZipline:
                // disable foot IK Rig and Foot IK Scripts
                SetFootIKState(false);
                SetOrientMode(OrientState.MoveDirection);
                SetStateParameter(AnimatorState.ZiplinePose);
                break;

            case PlayerStateMachine.State.Locked:
                // enable foot IK Rig and Foot IK Scripts
                SetFootIKState(true);
                SetOrientMode(OrientState.None);
                SetStateParameter(AnimatorState.MovementTree);
                m_isLocked = true;
                break;

            case PlayerStateMachine.State.Movement:
                // if we're exiting from Zipline, only state for that are the airborne ones (since that's how you get off a zipline)
                var target_state =
                    from != null && from.GetStateEnum() == PlayerStateMachine.State.OnZipline ?
                        (m_yVel > 0 ? AnimatorState.JumpRise : AnimatorState.Airborne) : AnimatorState.MovementTree;

                // enable foot IK Rig and Foot IK Scripts
                SetFootIKState(true);
                SetOrientMode(OrientState.MoveDirection);
                SetStateParameter(target_state);
                break;

            default:
                throw new System.ArgumentException("State not supported.");
        }
    }

    public void OnMovementStateChange(AMovementSubState _, AMovementSubState to)
    {
        switch (to.GetStateEnum())
        {
            case MovementState.State.Grounded:
                SetFootIKState(true);
                SetStateParameter(AnimatorState.MovementTree);
                break;
            case MovementState.State.Airborne:
                SetFootIKState(false);
                SetStateParameter(AnimatorState.Airborne);
                break;
            case MovementState.State.JumpRise:
                SetFootIKState(false);
                SetStateParameter(AnimatorState.JumpRise);
                break;

            default:
                throw new System.ArgumentException("State not supported.");
        }
    }

    public void CheckAimState(ADroneState from, ADroneState to)
    {
        // if we're going into or leaving from Aim state, trigger us to change
        // our anim state on the Aim layer.
        if (to.GetStateEnum() == DroneStateMachine.State.Aim)
        {
            m_animator.SetTrigger(m_aimTrigger);
            m_rigs.layers[m_aimRigLayerIndex].active = true;
            SetOrientMode(OrientState.ReticleDirection);
        }
        else if (from?.GetStateEnum() == DroneStateMachine.State.Aim)
        {
            m_animator.SetTrigger(m_aimTrigger);
            m_rigs.layers[m_aimRigLayerIndex].active = false;
            SetOrientMode(OrientState.MoveDirection);
        }
    }

    private void SetFootIKState(bool state)
    {
        var rig = m_rigs.layers[m_footIKRigLayerIndex];

        if (rig.active == state) return;

        rig.active = state;
        foreach (var item in m_feetIKsToToggle) { item.enabled = state; }
    }

    private void SetStateParameter(AnimatorState state)
    {
        m_animator.SetInteger(m_state, (int)state);
    }

    private void SetOrientMode(OrientState state) => m_orientMode = state;
}
