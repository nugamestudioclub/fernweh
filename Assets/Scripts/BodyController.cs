using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(CharacterController))]
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

    [SerializeField, Tooltip("The jump impulse force to instantly apply.")]
    private float m_jumpImpulse;

    [SerializeField] private Transform m_raycastOrigin;
    private Transform m_perspective;

    [SerializeField] private AnimationCurve m_jumpCurve;
    [SerializeField] private float m_jumpRiseEndTimestamp;
    private Coroutine m_jumpRoutine;

    private Vector3 m_groundVelocity;
    private float m_yVelocity;

    private bool m_applyGravity;
    private bool m_coyoteHanging;
    private Coroutine m_coyoteTimer;

    private bool m_isJumpBuffered;
    private Coroutine m_jumpBufferTimer;
    // TODO:
    // gravity DONE -> 
    // coyote time DONE ->
    // jumping DONE ->
    // Hollow Knight jump height control DONE ->
    // custom gravity scalars ->
    // cleanup

    private void Awake()
    {
        m_controller = GetComponent<CharacterController>();
        m_perspective = Camera.main.transform;
    }

    private void Update()
    {
        var inputs = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        bool was_jump_pressed = Input.GetAxisRaw("Jump") > 0.05f;

        if (was_jump_pressed)
        {
            if (m_jumpBufferTimer != null)
            {
                StopCoroutine(m_jumpBufferTimer);
            }

            StartCoroutine(IEBufferThenClear());
        }

        bool is_grounded = false;
        Vector3 surface_up = Vector3.up; // if miss, use world up
        if (Physics.SphereCast(
                m_raycastOrigin.position,
                m_controller.radius,
                Vector3.down, 
                out var hit, 
                0.1f,
                ~(1 << 3))) // ignoring the player
        {
            surface_up = hit.normal;

            is_grounded = true;
        }

        var forward = Vector3.Cross(m_perspective.right, surface_up);

        var slope_quat = Quaternion.FromToRotation(Vector3.up, surface_up);

        var right_vec = inputs.x * m_maxGroundVelocityMagnitude * (slope_quat * m_perspective.right);
        var forward_vec = inputs.y * m_maxGroundVelocityMagnitude * forward;

        // not just "xzvelo" or something like that, since this is also what keeps helps
        // us on the ground when we are on slopes or something.
        var target_ground_velocity = forward_vec + right_vec;

        m_groundVelocity = 
            Vector3.MoveTowards(
                m_groundVelocity, 
                target_ground_velocity, 
                (ComputeDeltaAcceleration(inputs, m_groundVelocity) + m_acceleration) * Time.deltaTime);


        // COYOTE TIME HANDLING
        // only trigger coyote time if we're not grounded, not already adding gravity (i.e. if we jumped)
        // and if we're already not running a coyote timer, and not already jumping
        if (!is_grounded && !m_applyGravity && m_coyoteTimer == null && m_jumpRoutine == null)
        {
            m_coyoteTimer = StartCoroutine(IEWaitForCoyoteTime());
        }
        else if (is_grounded && m_coyoteTimer != null)
        {
            StopCoroutine(m_coyoteTimer);
            m_coyoteTimer = null;
        }
        
        if (is_grounded || m_coyoteHanging)
        {
            if (m_isJumpBuffered)
            {
                // stop jump routine if going (precaution)
                if (m_jumpRoutine != null)
                {
                    StopCoroutine(m_jumpRoutine);
                    m_jumpRoutine = null;
                }

                // stop coyote timer if ongoing (we dont want gravity to go off yet)
                if (m_coyoteTimer != null)
                {
                    StopCoroutine(m_coyoteTimer);
                    m_coyoteTimer = null;
                }

                // start jump routine
                m_jumpRoutine = StartCoroutine(IEPerformJumpRoutine());

                // clear coyote time (precaution)
                m_coyoteHanging = false; // TODO make a "temporary bool" variable that has timing built into it.
            }
            else
            {
                m_applyGravity = false;
            }
        }

        if (m_applyGravity)
        {
            m_yVelocity += Physics.gravity.y * Time.deltaTime;
        }
        // END

        m_controller.Move(m_groundVelocity * Time.deltaTime + m_yVelocity * Time.deltaTime * Vector3.up);
    }

    private IEnumerator IEPerformJumpRoutine()
    {
        float time_elapsed = 0f;
        // while jump down, perform routine as usual
        while (Input.GetAxisRaw("Jump") > 0.05f && time_elapsed < m_jumpRiseEndTimestamp)
        {
            yield return new WaitForEndOfFrame();
            time_elapsed += Time.deltaTime;

            m_yVelocity = m_jumpCurve.Evaluate(time_elapsed);
        }

        m_yVelocity = m_jumpCurve.Evaluate(Mathf.Max(time_elapsed, m_jumpRiseEndTimestamp));

        // TODO impl other variant?
        // instead of cutting to the end of the curve and apply a bigger stopper-force,
        // why not just accelerate the evaluation of the graph? (exiting when done, ofc)
        // that should avoid the abrupt skip that jumping the rest of the curve has, since it's being evaluated now (but faster)
        while (m_yVelocity > 0.05f)
        {
            yield return new WaitForEndOfFrame();

            m_yVelocity += Physics.gravity.y * 2 * Time.deltaTime;
        }

        m_applyGravity = true;
    }

    // abstractable with below, obviously. later, later
    private IEnumerator IEWaitForCoyoteTime()
    {
        m_coyoteHanging = true;

        yield return new WaitForSeconds(m_coyoteTime);

        m_applyGravity = true;
        m_coyoteHanging = false;
    }

    private IEnumerator IEBufferThenClear()
    {
        m_isJumpBuffered = true;

        yield return new WaitForSeconds(m_jumpBufferTime);

        m_isJumpBuffered = false;
    }

    private float ComputeDeltaAcceleration(Vector2 normalized_dir, Vector2 non_normalized_velo)
    {
        if (non_normalized_velo.magnitude < 0.5f) return 0f;

        return m_deltaAccelerationCurve.Evaluate(Vector2.Dot(non_normalized_velo.normalized, normalized_dir)) * m_deltaAccelerationMagnitude;
    }
}
