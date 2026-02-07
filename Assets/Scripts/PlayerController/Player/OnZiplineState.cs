using UnityEditor.Analytics;
using UnityEngine;

public class OnZiplineState : IState<PlayerStateContext, PlayerStateMachine.State>
{
    private const int STEP_COUNT = 10;
    private const PlayerStateMachine.State STATE_ENUM = PlayerStateMachine.State.OnZipline;

    private PlayerStateContext m_myContext;
    private MovementStateContext m_mContextCache; // cached to code golf and to read it easier

    private Vector3 m_cumulativeFrameMovement;

    private Vector3 m_deltaPerStep;
    private int m_step;

    public bool TryCheckForExits(out PlayerStateMachine.State state_enum)
    {
        if (m_myContext.IsPlayerLocked)
        {
            state_enum = PlayerStateMachine.State.Locked;
            return true;
        }

        if (!m_myContext.IsOnZipline)
        {
            state_enum = PlayerStateMachine.State.Movement;
            return true;
        }

        state_enum = default;
        return false;
    }

    public void StateUpdate()
    {
        var current_line = m_myContext.MountedLine;
        var cc = m_myContext.CharacterController;

        m_mContextCache.LateralVelocity = ComputeLineVelocity();

        var backup_pos = cc.transform.position;

        // if we have any delta remaining to fix our attachment to the rider anchor, get it
        m_cumulativeFrameMovement = GetCorrectionDelta(); // UNSCALED BY TIME

        // add onto any movement that already exists for the frame, like snapping offsets
        m_cumulativeFrameMovement += m_mContextCache.LateralVelocity * Time.deltaTime;

        // calling Move here is fine because we do the same thing in the MovementState. Semantics.
        CollisionFlags move_result = cc.Move(m_cumulativeFrameMovement);

        // once we've moved for the frame, clear the cumulative movement
        m_cumulativeFrameMovement = Vector3.zero;

        if (move_result != CollisionFlags.None || !current_line.GetData().IsPositionOnLine(cc.transform.position))
        {
            cc.transform.position = backup_pos;
            m_mContextCache.LateralVelocity = Vector3.zero;
        }
    }

    private Vector3 GetCorrectionDelta()
    {
        if (m_step < STEP_COUNT)
        {
            m_step++;
            return m_deltaPerStep;
        }

        return Vector3.zero;
    }

    private Vector3 MatchVelocityToLine(Vector3 perspective_dir, Vector3 current_line_dir)
    {
        float dot = Vector3.Dot(perspective_dir, current_line_dir);

        if (dot < 0.1f && dot > 0f) dot = 0f; // if just above 0, make it 0
        if (dot > -0.1f && dot < 0f) dot = 0f; // if just below 0, make it 0
        if (dot > 0.9f) dot = 1f; // if just under 1, make it 1
        if (dot < -0.9f) dot = -1f; // if just above 1, make it -1

        return dot * current_line_dir;
    }

    private Vector3 ComputeLineVelocity()
    {
        var inputs = m_mContextCache.MovementInput.normalized;
        var current_line = m_myContext.MountedLine;

        var perspective_dir = m_mContextCache.PointOfView.TransformDirection(new Vector3(inputs.x, 0f, inputs.y));

        var target_line_velo = MatchVelocityToLine(perspective_dir, current_line.GetData().GetDirection()) * m_mContextCache.ConfigData.MaxDriveVelocityMagnitude;

        return Vector3.MoveTowards(
                m_mContextCache.LateralVelocity,
                target_line_velo,
                m_mContextCache.ConfigData.DriveAcceleration * Time.deltaTime);
    }

    #region
    public void Enter()
    {
        m_mContextCache = m_myContext.SubmachineStateContext;

        // if our hit point was consumed without reapplication, that means we entered this state
        // without hitting a ZiplineObject_OLD, meaning we don't need to do this snapping behavior.
        // E.G. Locked -> Zipline.
        if (m_myContext.HitPoint == Vector3.zero)
        {
            m_step = STEP_COUNT;
            return;
        }

        // begin delta snaapping for player so that the anchor is on the line
        var delta = m_myContext.HitPoint - m_myContext.LineRiderAnchorTransform.position;

        // mark the hit point as consumed so the above logic holds
        m_myContext.HitPoint = Vector3.zero;

        m_step = 0;
        m_deltaPerStep = delta / STEP_COUNT;

        // a small bit of code dupe, i suppose.
        var inputs = m_mContextCache.MovementInput.normalized;
        var current_line = m_myContext.MountedLine;

        // reorient the player's velocity to along the line
        m_mContextCache.LateralVelocity =
            MatchVelocityToLine(
                m_mContextCache.PointOfView.TransformDirection(new Vector3(inputs.x, 0f, inputs.y)),
                current_line.GetData().GetDirection()) // get the velo dir...
            * m_mContextCache.LateralVelocity.magnitude; // ... and rescale
    }

    public void Exit() 
    {
        // since we're exiting, we wanna make sure that the movement subsystem starts off on the
        // right foot. Because of this, we'll say that we're airborne after dismounting (true).
        //
        // Since our JumpDown is gonna be true for this to take effect, we know that in MovementState,
        // the player will automatically enter JumpRise due to the logic in the state constructor.
        // As per design choice, the player jumps off of lines, so this coding choice works!
        if (m_mContextCache.IsJumpDown)
        {
            m_mContextCache.AirState = AirState.Airborne;
        }
    }

    public PlayerStateMachine.State GetStateEnum() => STATE_ENUM;
    public void SetStateContext(PlayerStateContext context) => m_myContext = context;
    #endregion
}
