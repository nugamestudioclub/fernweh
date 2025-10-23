using TMPro;
using UnityEngine;

public class OrbitCameraBehavior : MonoBehaviour
{
    [SerializeField] private float m_radius;
    [SerializeField] private float m_lerpRate;
    [SerializeField] private Transform m_focusTarget;

    [Space(10)]

    [SerializeField] private float m_xRotationSpeed;
    [SerializeField] private float m_yRotationSpeed;

    [Space(10)]

    [SerializeField] private Vector2 m_verticalRotationBounds = new Vector2(-80, 75);

    private Transform m_cameraTransform;
    private Transform m_targetTransform;

    private Vector3 m_focusHeightOffset = Vector3.up * 2f;
    private LayerMask m_ignoreFocus;
    private float m_yRot; // not really y-rotation, but tracks the "vertical" rotation

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;

        m_cameraTransform = Camera.main.transform;

        m_cameraTransform.LookAt(m_focusTarget);

        m_targetTransform = new GameObject("Camera_TargetTransform").transform;
        m_targetTransform.SetParent(m_focusTarget); // so that the target moves with the focus, so that the CAMERA moves with the focus.
        m_targetTransform.SetPositionAndRotation(m_cameraTransform.position + m_focusHeightOffset, m_cameraTransform.rotation);

        m_ignoreFocus = ~(1 << m_focusTarget.gameObject.layer);
    }

    private void Update()
    {
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
        m_cameraTransform.LookAt(m_focusTarget);
    }

    private void HandleObstructions()
    {
        var direction_vector = (m_targetTransform.position - GetFocusPosition()).normalized;
        // Debug.DrawLine(GetFocusPosition(), GetFocusPosition()+direction_vector*m_radius, Color.yellow);
        
        // not quite as good as spherecast, but much less of a fuss when it comes to setting the new position.
        if (Physics.Raycast(GetFocusPosition(), direction_vector, out var hit, m_radius, m_ignoreFocus))
        {
            // Debug.Log(hit.collider.name);
            // Debug.DrawRay(hit.point, Vector3.up, Color.red);
            m_targetTransform.position = hit.point;
        }
        else
        {
            m_targetTransform.position = GetFocusPosition() + direction_vector * m_radius;
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

    private Vector3 GetFocusPosition() => m_focusHeightOffset + m_focusTarget.position; // Does this work? Test.
}
