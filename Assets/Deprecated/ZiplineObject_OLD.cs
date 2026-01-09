using UnityEngine;

public class ZiplineObject_OLD : MonoBehaviour
{
    [SerializeField] private float m_onLineLeeway = 1f;
    [SerializeField] private float m_lockoutDuration = 2f;

    [SerializeField] private Vector3 m_startPosition;
    [SerializeField] private Vector3 m_endPosition;

    [SerializeField] private LayerMask m_riderLayer = 1 << 3;

    private LineRenderer m_lineRenderer;

    private bool m_doScanForRider;
    private bool m_hasRider;
    private TemporaryBoolean m_isScanLockedOut;

    private Vector3 m_lineDirection;

    private void Awake()
    {
        m_lineRenderer = GetComponent<LineRenderer>();
        m_isScanLockedOut = new TemporaryBoolean();
    }
    private void Start()
    {
        if (m_startPosition != Vector3.zero || m_endPosition != Vector3.zero) UpdateAnchors();
    }

    public void SetAnchors(Vector3 start, Vector3 end)
    {
        m_startPosition = start;
        m_endPosition = end;

        UpdateAnchors();
    }

    private void UpdateAnchors()
    {
        m_lineRenderer.positionCount = 2;
        m_lineRenderer.SetPositions(new Vector3[] { m_startPosition, m_endPosition });

        m_lineDirection = (m_endPosition - m_startPosition).normalized;

        DEBUG_VisualizeAnchors();

        m_doScanForRider = true;

        GizmoDebug.Instance.DrawSphereRay(m_startPosition, m_endPosition, 0.1f, Color.red);
    }

    private void Update()
    {
        m_isScanLockedOut.Tick(Time.deltaTime);

        if (m_doScanForRider && !m_hasRider && !m_isScanLockedOut.IsTrue
            && Physics.SphereCast(m_startPosition, 0.1f, m_endPosition - m_startPosition, out var hit, float.MaxValue, m_riderLayer)
            && hit.transform.parent.TryGetComponent<PlayerStateContext>(out var player_context))
        {
            m_hasRider = true;

            //player_context.MountLine(this, hit.point);

            //component.enabled = true;
            //component.RideLine(this, hit.distance);
        }
    }

    public void Dismount()
    {
        m_hasRider = false;
        m_isScanLockedOut.SetActive(m_lockoutDuration);
    }

    public Vector3 GetDirection() => m_lineDirection;
    public Vector3 GetStart() => m_startPosition;
    public Vector3 GetEnd() => m_endPosition;

    public bool IsPositionOnLine(Vector3 position)
    {
        // https://stackoverflow.com/questions/17692922/check-whether-a-point-x-y-is-on-the-line-between-two-other-points

        float sum = Vector3.Distance(m_startPosition, position) + Vector3.Distance(m_endPosition, position);
        return sum - Vector3.Distance(m_startPosition, m_endPosition) < m_onLineLeeway;
    }

    private void DEBUG_VisualizeAnchors()
    {
        var start_orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var end_orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        Destroy(start_orb.GetComponent<Collider>());
        Destroy(end_orb.GetComponent<Collider>());

        start_orb.GetComponent<Renderer>().material.color = Color.green;
        end_orb.GetComponent<Renderer>().material.color = Color.blue;

        start_orb.transform.position = m_startPosition;
        end_orb.transform.position = m_endPosition;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(m_startPosition, 0.25f);

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(m_startPosition, m_endPosition);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(m_endPosition, 0.25f);
    }
}
