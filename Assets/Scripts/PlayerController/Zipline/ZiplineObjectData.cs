using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

// currently pretty stub, but when weight-points or more complicated Point-making behavior is
// added, this will get filled out more
public class ZiplineObjectData
{
    private const float ON_ZIPLINE_LEEWAY = 0.5f;
    private readonly int RIDER_LAYER = (1 << 6);

    private Vector3 m_anchorOne;
    private Vector3 m_anchorTwo;

    private Vector3 m_directionVector;
    private float m_lineDistance;

    private IList<Vector3> m_points;

    public ZiplineObjectData(Vector3 anchor_one, Vector3 anchor_two)
    {
        m_anchorOne = anchor_one;
        m_anchorTwo = anchor_two;

        var difference = m_anchorTwo - m_anchorOne;
        m_lineDistance = difference.magnitude;
        m_directionVector = difference / m_lineDistance;
    }

    // exposed so that it can be manually fired when more is added
    public void MakePoints()
    {
        // more complicated logic here eventually
        m_points = new List<Vector3> { m_anchorOne, m_anchorTwo };
    }

    public void SetRendererPoints(LineRenderer renderer)
    {
        renderer.positionCount = m_points.Count;
        renderer.SetPositions(m_points.ToArray());
    }

    public bool IsPositionOnLine(Vector3 position)
    {
        // https://stackoverflow.com/questions/17692922/check-whether-a-point-x-y-is-on-the-line-between-two-other-points

        float sum = Vector3.Distance(m_anchorOne, position) + Vector3.Distance(m_anchorTwo, position);
        return sum - Vector3.Distance(m_anchorOne, m_anchorTwo) < ON_ZIPLINE_LEEWAY;
    }

    public Vector3 GetDirection() => m_directionVector;

    public bool SpherecastDownLine(out RaycastHit hit)
    {
        return Physics.SphereCast(m_anchorOne, 0.1f, m_directionVector, out hit, m_lineDistance, RIDER_LAYER);
    }
}
