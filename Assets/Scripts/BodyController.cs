using System.Collections;
using UnityEngine;

public class BodyController : MonoBehaviour
{
    private CharacterController m_controller;

    [SerializeField, Tooltip("The target lateral velocity to move towards.")]
    private float m_maxGroundVelocityMagnitude;

    [SerializeField, Tooltip("How fast we move towards our target velocity over the course of one second.")]
    private float m_acceleration;

    [SerializeField, Tooltip("The scale of additional acceleration to apply depending on the dot product of input direction and move direction.")]
    private AnimationCurve m_deltaAccelerationCurve;

    [SerializeField, Tooltip("The maximum magnitude of a delta force applied.")]
    private float m_deltaAccelerationMagnitude;

    [SerializeField, Tooltip("The coyote-time in seconds.")]
    private float m_coyoteTime;

    [SerializeField, Tooltip("The jump buffer duration in seconds.")]
    private float m_jumpBufferTime;

    [SerializeField] private Transform m_raycastOrigin;
    private Transform m_perspective;

    [SerializeField] private AnimationCurve m_jumpCurve;
    [SerializeField] private float m_jumpRiseEndTimestamp;

    private const int LAYER_MASK = ~(1 << 3); // ignores player


    private Vector3 m_groundVelocity;
    private float m_yVelocity;

    private bool m_wasPreviouslyGrounded;
    private bool m_applyGravity;

    private Coroutine m_jumpRiseRoutine;
    private TemporaryBoolean m_hasPendingJump;
    private TemporaryBoolean m_isCoyoteFloating;

    private void Awake()
    {
        m_controller = GetComponent<CharacterController>();
        m_perspective = Camera.main.transform;

        m_hasPendingJump = new TemporaryBoolean();
        m_isCoyoteFloating = new TemporaryBoolean();
    }


    private void Update()
    {
        TickTemporaryBools();

        // gather input
        var inputs = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        bool is_jump_down = Input.GetAxisRaw("Jump") > 0.05f;

        bool is_grounded = false;
        var surface_up = Vector3.up; // if miss, use world up
        // check for ground collision to get surface normal
        if (Physics.SphereCast(m_raycastOrigin.position, m_controller.radius, Vector3.down, out var hit, 0.1f, LAYER_MASK))
        {
            surface_up = hit.normal;
            is_grounded = true;
        }

        // 

        // update the player's nonvertical velocity
        // mutates m_groundVelocity.
        ComputeGroundVelocity(inputs, surface_up);

        // checks for initiating the Coyote Float status
        // mutates the m_isCoyoteFloating temporary boolean
        HandleCoyoteFloat(is_grounded);

        // handles performing jumps and computing gravity forces for the frame
        HandleJumpAndGravity(is_jump_down, is_grounded);

        // apply movement
        m_controller.Move(m_groundVelocity * Time.deltaTime + m_yVelocity * Time.deltaTime * Vector3.up);

        CheckResetYVelo(is_grounded);
    }

    private void CheckResetYVelo(bool is_grounded)
    {
        // if contact with the ground was made, reset yvelo to 0.
        if (is_grounded != m_wasPreviouslyGrounded && is_grounded)
        {
            m_yVelocity = 0f;
        }

        m_wasPreviouslyGrounded = is_grounded;
    }

    private void HandleCoyoteFloat(bool is_grounded)
    {
        // if conditions are met, enter coyote float.
        //
        // CONDITIONS:
        // if you aren't grounded, you start coyote floating
        // if you are already experiencing gravity, then it's too late to coyote float
        // if you are already floating, don't restart the timer
        // if you are jumping, you dont get to float
        // if we just ended coyote float, don't restart it again immediately. This prevents coyote float loops
        // due to order of execution. Tick -> Coyote Check -> Gravity would always leave you in Coyote Float if you didnt
        // make sure to not restart Coyote Float as soon as it expires.
        if (!is_grounded && !m_applyGravity 
            && !m_isCoyoteFloating.State 
            && !IsPerformingJumpRise() && !m_isCoyoteFloating.ExpiredThisTick())
        {
            m_isCoyoteFloating.SetActive(m_coyoteTime);
        }
        // if you are grounded but also in the floating state, exit the float bc it's not needed.
        else if (is_grounded && m_isCoyoteFloating.State)
        {
            m_isCoyoteFloating.Expire();
        }
    }

    private void HandleJumpAndGravity(bool jump_pressed, bool is_grounded)
    {
        // if we are to jump, enable the buffer
        if (jump_pressed) m_hasPendingJump.SetActive(m_jumpBufferTime);

        // semantically grounded if you are actually grounded or in coyote float
        bool grounded = is_grounded || m_isCoyoteFloating.State;

        if (grounded)
        {
            m_applyGravity = false;

            // if you are semantically grounded, check to see if you have a buffered jump to consume
            // only jump if you aren't jumping already. We can technically be semantically grounded and
            // in the beginning phase of a jump due to spherecast length error bounds.
            if (m_hasPendingJump.State && !IsPerformingJumpRise())
            {
                // consume jump buffer
                m_hasPendingJump.Expire();

                // automatically end coyote time
                m_isCoyoteFloating.Expire();

                BeginJump();
            }
        }
        else
        {
            // if you aren't semantically grounded, apply gravity if we are no longer in the rising-jump phase
            // of our airborne-ness.
            m_applyGravity = !IsPerformingJumpRise();
        }

        // if gravity is to be applied, compute it
        if (m_applyGravity)
        {
            m_yVelocity += Physics.gravity.y * Time.deltaTime;
        }
    }

    private void BeginJump()
    {
        if (IsPerformingJumpRise())
        {
            StopCoroutine(m_jumpRiseRoutine);
            m_jumpRiseRoutine = null;
        }

        m_jumpRiseRoutine = StartCoroutine(IEPerformJumpRise());
    }

    private bool IsPerformingJumpRise() => m_jumpRiseRoutine != null;

    private void ComputeGroundVelocity(Vector2 inputs, Vector3 surface_up)
    {
        var forward = Vector3.Cross(m_perspective.right, surface_up);

        var slope_quat = Quaternion.FromToRotation(Vector3.up, surface_up);

        var right_vec = inputs.x * m_maxGroundVelocityMagnitude * (slope_quat * m_perspective.right);
        var forward_vec = inputs.y * m_maxGroundVelocityMagnitude * forward;

        // not just "xzvelo" or something like that, since this is also what keeps helps
        // us on the ground when we are on slopes or something.
        var target_ground_velocity = forward_vec + right_vec;

        m_groundVelocity = Vector3.MoveTowards(
                m_groundVelocity,
                target_ground_velocity,
                (ComputeDeltaAcceleration(inputs, m_groundVelocity) + m_acceleration) * Time.deltaTime);
    }

    private float ComputeDeltaAcceleration(Vector2 normalized_dir, Vector2 non_normalized_velo)
    {
        if (non_normalized_velo.magnitude < 0.5f) return 0f;

        return m_deltaAccelerationCurve.Evaluate(Vector2.Dot(non_normalized_velo.normalized, normalized_dir)) * m_deltaAccelerationMagnitude;
    }

    private IEnumerator IEPerformJumpRise()
    {
        float time_elapsed = 0f;
        float last_key_timestamp = m_jumpCurve.keys[^1].time;

        // while jump down, perform routine as usual
        while (time_elapsed < m_jumpRiseEndTimestamp)
        {
            if (!(Input.GetAxisRaw("Jump") > 0.05f)) break;

            yield return new WaitForEndOfFrame();
            time_elapsed += Time.deltaTime;

            m_yVelocity = m_jumpCurve.Evaluate(time_elapsed);
        }

        while (time_elapsed < last_key_timestamp)
        {
            yield return new WaitForEndOfFrame();
            time_elapsed += Time.deltaTime * 3;

            m_yVelocity = m_jumpCurve.Evaluate(time_elapsed);
        }

        m_jumpRiseRoutine = null;
        m_applyGravity = true; // slightly hacky, but needed to prevent Coyote Float
                               // from activating immediately after ending jump rise.
    }

    private void TickTemporaryBools()
    {
        m_hasPendingJump.Tick(Time.deltaTime);
        m_isCoyoteFloating.Tick(Time.deltaTime);
    }
}
