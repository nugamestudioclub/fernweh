using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// currently shoots straight shots, not subject to gravity
public class ZiplineShootBehavior : MonoBehaviour
{
    [SerializeField] private Transform m_referencePOV;
    [SerializeField] private Transform m_lineOriginTransform;
    [SerializeField] private SpriteRenderer m_reticle;

    [Space]

    [SerializeField] private float m_maxDistance;
    [SerializeField] private LayerMask m_hitMask;

    [Space]

    [SerializeField] private int m_maxZiplineCount = 5;
    [SerializeField] private ZiplineObject_OLD m_ziplineObjectPrefab;

    private IList<ZiplineObject_OLD> m_activeLines;
    private float m_targetDistance;

    private InputAction m_fireZipline;

    private void Awake()
    {
        m_activeLines = new List<ZiplineObject_OLD>();
        m_fireZipline = InputSystem.actions.FindAction("Attack");
    }

    // Entry and exit to this state is determined by Aim state and when it is entered.
    private void Update()
    {
        if (Physics.Raycast(m_referencePOV.position, m_referencePOV.forward, out var hit, m_maxDistance, m_hitMask))
        {
            m_reticle.transform.position = hit.point + hit.normal * 0.05f;
            m_reticle.transform.forward = hit.normal;

            m_reticle.color = Color.green;

            m_targetDistance = hit.distance;
        }
        else
        {
            m_reticle.transform.position = m_referencePOV.forward * m_maxDistance + m_referencePOV.position;
            m_reticle.transform.forward = -m_referencePOV.forward;

            m_reticle.color = Color.red;

            m_targetDistance = -1f;
        }

        if (m_fireZipline.WasPressedThisFrame())
        {
            MakeZipline();
        }
    }

    private void MakeZipline()
    {
        if (m_targetDistance < 0f || m_activeLines.Count >= m_maxZiplineCount) return;

        var instance = GameObject.Instantiate(m_ziplineObjectPrefab);

        instance.SetAnchors(m_lineOriginTransform.position + m_referencePOV.forward, m_referencePOV.forward * m_targetDistance + m_referencePOV.position);

        m_activeLines.Add(instance);
    }
}
