using UnityEngine;

public class ZiplineObject : MonoBehaviour
{
    [SerializeField] private LineRenderer m_renderer;
    [SerializeField] private float m_lineRemountTimeout = 1f;

    private ZiplineObjectData m_data;

    // there's an argument for putting these in the data, but honestly
    // it'd just clutter it up more.
    private bool m_doScanForRider;
    private bool m_hasRider;
    private TemporaryBoolean m_isScanLockedOut;

    public void MakeData(Vector3 anchor_one_headpos, Vector3 anchor_two_headpos)
    {
        m_data = new ZiplineObjectData(anchor_one_headpos, anchor_two_headpos);

        m_doScanForRider = true;
        m_hasRider = false;
        m_isScanLockedOut = new TemporaryBoolean();
    }

    public void UpdateVisual()
    {
        m_data.MakePoints();

        m_data.SetRendererPoints(m_renderer);

        // m_lineEmissionSystem.Play();
    }

    public ZiplineObjectData GetData() => m_data;

    private void Update()
    {
        if (m_data == null) return;

        m_isScanLockedOut.Tick(Time.deltaTime);

        if (m_doScanForRider && !m_hasRider && !m_isScanLockedOut.IsTrue
            && m_data.SpherecastDownLine(out var hit)
            && hit.transform.parent.TryGetComponent<PlayerStateContext>(out var player_context))
        {
            m_hasRider = true;

            player_context.MountLine(this, hit.point);
        }
    }

    public void Dismount()
    {
        m_hasRider = false;
        m_isScanLockedOut.SetActive(m_lineRemountTimeout);
    }
}
