using UnityEngine;
using UnityEngine.Animations.Rigging;

public class SimpleFootIKRigBehavior : MonoBehaviour
{
    [SerializeField] private TwoBoneIKConstraint m_constraint;
    [SerializeField] private Transform m_footRefConstraint;
    [SerializeField] private Transform m_footTarget;

    [Space]

    [SerializeField] private float m_rayYOffset;
    [SerializeField] private float m_footRadius;
    [SerializeField] private float m_rayDistance;
    [SerializeField] private LayerMask m_rayMask;

    private Vector3 DEBUG_pos;
    private bool DEBUG_tooLow;

    private void Awake()
    {
        m_constraint.weight = 0f;
    }

    private void LateUpdate()
    {
        // still unsure about what the ref constraint is supposed to do, but it seems to work fine anyways
        var pos = m_footRefConstraint.position;
        m_constraint.weight = 0f;

        DEBUG_pos = Vector3.zero;
        DEBUG_tooLow = false;

        if (Physics.SphereCast(
            pos + Vector3.up * m_rayYOffset,
            m_footRadius,
            Vector3.down,
            out var hit,
            m_rayDistance,
            m_rayMask))
        {
            DEBUG_pos = pos;

            // if our hit is below the foot, don't snap to that point since we might have some more animating to do.
            // TODO, is this right? maybe not for stairs...
            if (hit.point.y < pos.y)
            {
                DEBUG_tooLow = true;
                // Debug.Log("Too low, ignoring.");
                return;
            }

            // Debug.Log($"Snapping!");

            // enable constraint and reposition target
            m_constraint.weight = 1f;

            var hit_point = hit.point;
            hit_point.x = pos.x;
            hit_point.z = pos.z;

            m_footTarget.position = hit_point;

            // no need for rotation of foot since temp_bot doesn't have those
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (m_footRefConstraint == null) return;

        var pos = m_footRefConstraint.position;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(pos + Vector3.up * m_rayYOffset, Vector3.down * m_rayDistance);

        if (DEBUG_pos == Vector3.zero) return;

        Gizmos.color = DEBUG_tooLow ? Color.red : Color.green;
        Gizmos.DrawWireSphere(DEBUG_pos, 0.25f);
    }
}
