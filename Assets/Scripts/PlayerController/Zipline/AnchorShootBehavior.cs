using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnchorShootBehavior : MonoBehaviour
{
    [SerializeField] private Transform m_referencePOV;
    // [SerializeField] private Transform m_lineOriginTransform;
    [SerializeField] private SpriteRenderer m_reticle;

    [Space]

    [SerializeField] private PlayerControllerConfigSO m_config;

    private IList<ZiplineAnchor> m_placedAnchors;
    private float m_targetDistance;

    private InputAction m_fireAnchor;
    // TODO input for destroying anchors

    private void Awake()
    {
        m_placedAnchors = new List<ZiplineAnchor>();
        m_fireAnchor = InputSystem.actions.FindAction("Attack");
    }

    // Entry and exit to this state is determined by Aim state and when it is entered.
    private void Update()
    {
        if (Physics.Raycast(m_referencePOV.position, m_referencePOV.forward, out var hit, m_config.MaxAnchorPlaceDistance, m_config.AnchorPlacementMask))
        {
            m_reticle.transform.position = hit.point + hit.normal * 0.05f;
            m_reticle.transform.forward = hit.normal;

            m_reticle.color = Color.green;

            m_targetDistance = hit.distance;

            // TODO Visual of placed-anchor radius and what it'd connect to

            if (m_fireAnchor.WasPressedThisFrame())
            {
                MakeAnchor(hit.normal, hit.point);
            }
        }
        else
        {
            m_reticle.transform.position = m_referencePOV.forward * m_config.MaxAnchorPlaceDistance + m_referencePOV.position;
            m_reticle.transform.forward = -m_referencePOV.forward;

            m_reticle.color = Color.red;

            m_targetDistance = -1f;
        }
    }

    private void MakeAnchor(Vector3 surface_normal, Vector3 world_pos)
    {
        if (m_targetDistance < 0f || m_placedAnchors.Count >= m_config.MaxPlayerAnchorCount) return;

        var instance = GameObject.Instantiate(m_config.AnchorPrefab);
        instance.transform.up = surface_normal;
        instance.transform.position = world_pos;

        // manually perform scan after we fix position; only player anchors dont scan on start
        instance.SetAttachmentRadius(m_config.PlayerAnchorAttachmentRadius);
        instance.FindAnchorsAndAttach(); 

        // instance.SetAnchors(m_lineOriginTransform.position + m_referencePOV.forward, m_referencePOV.forward * m_targetDistance + m_referencePOV.position);

        m_placedAnchors.Add(instance);
    }
}
