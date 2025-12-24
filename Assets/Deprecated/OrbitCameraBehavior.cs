using TMPro;
using UnityEngine;

public class OrbitCameraBehavior : MonoBehaviour
{
    [SerializeField] private float m_orbitRadius;
    [SerializeField] private float m_lerpRate;
    [SerializeField] private Transform m_orbitTarget;
    [SerializeField] private Vector3 m_orbitHeightOffset = Vector3.up * 0.5f;

    [Space(10)]

    [SerializeField] private float m_xRotationSpeed;
    [SerializeField] private float m_yRotationSpeed;

    [Space(10)]

    [SerializeField] private Vector2 m_verticalRotationBounds = new Vector2(-80, 75);

    [Space(10)]

    [SerializeField] private float m_shootViewRadius;

    private Transform m_cameraTransform;
    private Transform m_targetTransform;

    private float m_currentRadius;
    private LayerMask m_ignoreFocus;
    private float m_yRot; // not really y-rotation, but tracks the "vertical" rotation

    private bool m_inShootView;

    private GameObject m_shootObject;

    public void CheckToggleShootViewState(bool do_toggle)
    {
        if (!do_toggle) return;

        m_inShootView = !m_inShootView;

        // enter state
        if (m_inShootView)
        {
            var offset_dir = -(m_cameraTransform.right + m_cameraTransform.forward).normalized;

            // set initial position offset so RotateAround behaves properly
            m_targetTransform.position = GetFocusPosition() + offset_dir * m_shootViewRadius;

            m_currentRadius = m_shootViewRadius;

            m_shootObject.gameObject.SetActive(true);
        }
        else
        {
            m_targetTransform.position = GetFocusPosition() - m_cameraTransform.forward * m_orbitRadius;

            m_currentRadius = m_orbitRadius;

            m_shootObject.gameObject.SetActive(false);
        }
    }

    private void Awake()
    {
        // prototype code structural setup, so just do whatever comes easiest.
        m_shootObject = FindFirstObjectByType<ZiplineShootBehavior_OLD>(FindObjectsInactive.Include).gameObject;

        Cursor.lockState = CursorLockMode.Locked;

        m_inShootView = false;
        m_currentRadius = m_orbitRadius;
        m_cameraTransform = Camera.main.transform;

        // m_cameraTransform.LookAt(GetFocusPosition());

        m_targetTransform = new GameObject("Camera_TargetTransform").transform;
        m_targetTransform.SetParent(m_orbitTarget); // so that the target moves with the focus, so that the CAMERA moves with the focus.
        m_targetTransform.SetPositionAndRotation(m_cameraTransform.position + m_orbitHeightOffset, m_cameraTransform.rotation);

        m_ignoreFocus = ~(1 << m_orbitTarget.gameObject.layer);
    }

    private void Update()
    {
        CheckToggleShootViewState(Input.GetButtonDown("Fire2"));

        var mouse_delta = Input.mousePositionDelta;

        m_targetTransform.RotateAround(GetFocusPosition(), Vector3.up, mouse_delta.x * Time.deltaTime * m_xRotationSpeed);

        var world_pos = m_targetTransform.position;
        float y_delta = mouse_delta.y * Time.deltaTime * m_xRotationSpeed;
        m_yRot += y_delta;
        m_targetTransform.RotateAround(GetFocusPosition(), m_targetTransform.right, y_delta);

        ClampToExtremities(world_pos); // clamps the previous rotation and reverts to a stable view if needed
        HandleObstructions(); // moves target to nonobstructed view if need be
    }

    private void LateUpdate()
    {
        // lerp camera transform position and rotation to that of the target
        m_cameraTransform.position = Vector3.Slerp(m_cameraTransform.position, m_targetTransform.position, Time.deltaTime * m_lerpRate);

        if (!m_inShootView)
        {
            m_cameraTransform.LookAt(GetFocusPosition());
        }
        else
        {
            m_cameraTransform.forward = Vector3.Lerp(m_cameraTransform.forward, m_targetTransform.forward, Time.deltaTime * m_lerpRate);
        }
    }

    private void HandleObstructions()
    {
        var direction_vector = (m_targetTransform.position - GetFocusPosition()).normalized;
        // Debug.DrawLine(GetFocusPosition(), GetFocusPosition()+direction_vector*m_radius, Color.yellow);
        
        // not quite as good as spherecast, but much less of a fuss when it comes to setting the new position.
        if (Physics.Raycast(GetFocusPosition(), direction_vector, out var hit, m_currentRadius, m_ignoreFocus))
        {
            // Debug.Log(hit.collider.name);
            // Debug.DrawRay(hit.point, Vector3.up, Color.red);
            m_targetTransform.position = hit.point;
        }
        else
        {
            m_targetTransform.position = GetFocusPosition() + direction_vector * m_currentRadius;
        }
    }

    private void ClampToExtremities(Vector3 fallback_position)
    {
        if (m_yRot > m_verticalRotationBounds.y)
        {
            m_yRot = m_verticalRotationBounds.y;

            var eulers = m_targetTransform.eulerAngles;
            eulers.x = m_yRot;

            m_targetTransform.eulerAngles = eulers;
            m_targetTransform.position = fallback_position;
        }
        else if (m_yRot < m_verticalRotationBounds.x)
        {
            m_yRot = m_verticalRotationBounds.x;

            var eulers = m_targetTransform.eulerAngles;
            eulers.x = m_yRot;

            m_targetTransform.eulerAngles = eulers;
            m_targetTransform.position = fallback_position;
        }
    }

    private Vector3 GetFocusPosition() => m_orbitHeightOffset + m_orbitTarget.position; // Does this work? Test.
}
