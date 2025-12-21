using Unity.VisualScripting;
using UnityEngine;

public class ZiplineObject : MonoBehaviour
{
    [SerializeField] private float m_onLineLeeway = 1f;
    [SerializeField] private float m_lockoutDuration = 2f;

    private Vector3 m_startPosition;
    private Vector3 m_endPosition;

    private LineRenderer m_lineRenderer;

    private bool m_doScan;
    private bool m_isMounted;
    private TemporaryBoolean m_isLockedOut;

    private Vector3 m_lineDirection;

    private void Awake()
    {
        m_lineRenderer = GetComponent<LineRenderer>();
        m_isLockedOut = new TemporaryBoolean();
    }

    public void SetAnchors(Vector3 start, Vector3 end)
    {
        m_startPosition = start;
        m_endPosition = end;

        m_lineRenderer.positionCount = 2;
        m_lineRenderer.SetPositions(new Vector3[] { m_startPosition, m_endPosition } );

        MakeAnchors();

        m_doScan = true;

        GizmoDebug.Instance.DrawSphereRay(start, end, 0.5f, Color.red);
    }

    private void Update()
    {
        m_isLockedOut.Tick(Time.deltaTime);

        if (m_doScan && !m_isMounted && !m_isLockedOut.IsTrue
            && Physics.SphereCast(m_startPosition, 0.5f, m_endPosition - m_startPosition, out var hit, float.MaxValue, 1 << 3)
            && hit.collider.TryGetComponent<ZiplineRiderController>(out var component))
        {
            m_isMounted = true;

            component.enabled = true;
            component.RideLine(this, hit.distance);
        }
    }

    private void MakeAnchors()
    {
        var start_orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var end_orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        Destroy(start_orb.GetComponent<Collider>());
        Destroy(end_orb.GetComponent<Collider>());

        start_orb.GetComponent<Renderer>().material.color = Color.green;
        end_orb.GetComponent<Renderer>().material.color = Color.blue;

        start_orb.transform.position = m_startPosition;
        end_orb.transform.position = m_endPosition;

        m_lineDirection = (m_endPosition - m_startPosition).normalized;
    }

    public Vector3 GetDirection() => m_lineDirection;
    public void Dismount()
    {
        m_isMounted = false;
        m_isLockedOut.SetActive(m_lockoutDuration);
    }

    public Vector3 GetStart() => m_startPosition;
    public Vector3 GetEnd() => m_endPosition;

    public bool IsPositionOnLine(Vector3 position)
    {
        // https://stackoverflow.com/questions/17692922/check-whether-a-point-x-y-is-on-the-line-between-two-other-points

        float sum = Vector3.Distance(m_startPosition, position) + Vector3.Distance(m_endPosition, position);
        return sum - Vector3.Distance(m_startPosition, m_endPosition) < m_onLineLeeway;
    }

}
