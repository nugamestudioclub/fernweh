using UnityEngine;

public class ConfigCastViewer : MonoBehaviour
{
    [SerializeField] private PlayerControllerConfigSO m_config;
    [SerializeField] private bool m_onlyOnSelected = true;
    [SerializeField] private int m_sphereSteps = 5;

    private Transform m_raycastTransform;
    private Transform m_spherecastTransform;

    private void OnDrawGizmos()
    {
        if (m_onlyOnSelected) return;

        DrawGizmos();
    }

    private void OnDrawGizmosSelected()
    {
        if (!m_onlyOnSelected) return;

        DrawGizmos();
    }

    private void DrawGizmos()
    {
        try
        {
            var dud = m_raycastTransform.position;
            var dud2 = m_spherecastTransform.position;
        }
        catch
        {
            return;
        }

        // sticky ray
        Gizmos.color = Color.green;
        Gizmos.DrawLine(m_raycastTransform.position, m_raycastTransform.position + m_config.StickyRaycastDistance * Vector3.down);

        // hit point
        if (Physics.Raycast(
            m_raycastTransform.position, Vector3.down, out var hit,
            m_config.StickyRaycastDistance,
            m_config.StickyRaycastMask))
        {
            // darker the further of a hit it is
            Gizmos.color = Color.Lerp(Color.green, Color.white, hit.distance / m_config.StickyRaycastDistance);
            Gizmos.DrawSphere(hit.point, 0.25f);
        }

        // sphere ray
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(m_spherecastTransform.position, m_spherecastTransform.position + m_config.GroundSpherecastDistance * Vector3.down);
        DrawSpheresAlongLine(m_spherecastTransform.position, m_spherecastTransform.position + m_config.GroundSpherecastDistance * Vector3.down);

        // hit point
        if (Physics.SphereCast(
            m_spherecastTransform.position, 
            m_config.GroundSpherecastRadius, Vector3.down, out var s_hit,
            m_config.GroundSpherecastDistance,
            m_config.GroundSpherecastMask))
        {
            // darker the further of a hit it is
            Gizmos.color = Color.Lerp(Color.blue, Color.black, s_hit.distance / m_config.GroundSpherecastDistance);
            Gizmos.DrawSphere(hit.point, 0.25f);
        }
    }

    private void DrawSpheresAlongLine(Vector3 start, Vector3 end)
    {
        for (int i = 0; i < m_sphereSteps; ++i)
        {
            var pos = Vector3.Lerp(start, end, (float)i / m_sphereSteps);
            Gizmos.DrawWireSphere(pos, m_config.GroundSpherecastRadius);
        }
    }

    [ContextMenu("Scan for Transforms")]
    private void GetTransforms()
    {
        var origin_go_s = GameObject.FindGameObjectWithTag(m_config.SpherecastOriginTransformTag);
        if (origin_go_s == null) throw new System.ArgumentException("Cannot find object with tag: " + m_config.SpherecastOriginTransformTag);

        m_spherecastTransform = origin_go_s.transform;

        var origin_go_r = GameObject.FindGameObjectWithTag(m_config.RaycastOriginTransformTag);
        if (origin_go_r == null) throw new System.ArgumentException("Cannot find object with tag: " + m_config.RaycastOriginTransformTag);

        m_raycastTransform = origin_go_r.transform;
    }
}
