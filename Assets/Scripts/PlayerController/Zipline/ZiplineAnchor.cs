using System.Collections.Generic;
using UnityEngine;

public class ZiplineAnchor : MonoBehaviour
{
    [SerializeField] private string m_anchorLayerName;

    [Space]

    [SerializeField] private bool m_scanOnStart = false; // defaults for player Anchors
    [SerializeField] private bool m_canBeRemoved = true; // FOR FUTURE IMPL
    [SerializeField] private float m_attachmentRadiusOverride = -1; // -1 = no override

    [Space]

    [SerializeField] private ParticleSystem m_anchorPulse;

    [Space]

    [SerializeField] private Transform m_headPosition;
    [SerializeField] private string m_ziplineInstanceParentTag;
    [SerializeField] private ZiplineObject m_ziplinePrefab;

    private IDictionary<ZiplineAnchor, ZiplineObject> m_anchorLinePairs;
    private Transform m_ziplineInstanceParent;
    private float m_attachmentRadius;

    private int m_anchorLayerIndex;

    private void Awake()
    {
        m_anchorLinePairs = new Dictionary<ZiplineAnchor, ZiplineObject>();

        m_anchorLayerIndex = LayerMask.NameToLayer(m_anchorLayerName);

        var parent = GameObject.FindGameObjectWithTag(m_ziplineInstanceParentTag);
        if (parent != null)
        {
            m_ziplineInstanceParent = parent.transform;
        }
        else
        {
            throw new System.Exception($"No parent object found with tag \"{m_ziplineInstanceParentTag}\".");
        }
    }

    private void Start()
    {
        if (m_scanOnStart) FindAnchorsAndAttach();
    }

    public void SetAttachmentRadius(float radius) => m_attachmentRadius = radius;

    public void FindAnchorsAndAttach()
    {
        // assign override if necessary
        if (m_attachmentRadiusOverride != -1) m_attachmentRadius = m_attachmentRadiusOverride;

        // collect all valid anchors
        var all_anchors = FindObjectsByType<ZiplineAnchor>(FindObjectsSortMode.None);
        foreach (var anchor in all_anchors)
        {
            if (m_anchorLinePairs.ContainsKey(anchor) || anchor == this) continue;

            if (Vector3.Distance(m_headPosition.position, anchor.m_headPosition.position) <= m_attachmentRadius // if within range
                && !Physics.Linecast(GetHeadPosition(), anchor.GetHeadPosition(), ~(1 << m_anchorLayerIndex))) // if no obstructions
            {
                MakeAttachment(anchor);
            }
        }
    }

    // on destroy, remove yourself from all anchors you are attached to
    private void OnDestroy()
    {
        var copy = new List<ZiplineAnchor>(m_anchorLinePairs.Keys);
        foreach (var anchor in copy)
        {
            anchor.BreakAttachment(this);
        }
    }

    private void MakeAttachment(ZiplineAnchor other)
    {
        var zipline = MakeZipline(this, other);

        m_anchorLinePairs.Add(other, zipline);
        other.m_anchorLinePairs.Add(this, zipline);
    }

    private void BreakAttachment(ZiplineAnchor anchor)
    {
        var zipline = m_anchorLinePairs[anchor];

        m_anchorLinePairs.Remove(anchor);
        anchor.m_anchorLinePairs.Remove(this);

        if (zipline != null) Destroy(zipline.gameObject);
    }

    private ZiplineObject MakeZipline(ZiplineAnchor anchor_1, ZiplineAnchor anchor_2)
    {
        var fab = Instantiate(m_ziplinePrefab, m_ziplineInstanceParent);
        fab.MakeData(anchor_1.GetHeadPosition(), anchor_2.GetHeadPosition());

        fab.UpdateVisual();

        anchor_1.m_anchorPulse.Play();
        anchor_2.m_anchorPulse.Play();

        return fab;
    }

    private Vector3 GetHeadPosition() => m_headPosition.position;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Gizmos.DrawSphere(m_headPosition.position, m_attachmentRadiusOverride);
    }
}
