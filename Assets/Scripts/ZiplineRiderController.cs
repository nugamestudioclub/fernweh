using System.Collections;
using UnityEngine;

// not made with BodyController.cs to prevent the clutter, altho they should be made together into a state machine later.
public class ZiplineRiderController : MonoBehaviour
{
    [SerializeField] private float m_maxDriveVelocity;
    [SerializeField] private float m_driveAcceleration;

    private CharacterController m_characterController;
    private BodyController m_bodyController;

    private ZiplineObject m_currentLine;
    private Transform m_perspective;

    private Vector3 m_currentLineVelocity;


    private void Awake()
    {
        m_characterController = GetComponent<CharacterController>();
        m_bodyController = GetComponent<BodyController>();

        m_perspective = Camera.main.transform;
    }

    public void RideLine(ZiplineObject obj, float distance_from_start)
    {
        m_currentLine = obj;
        m_bodyController.ResetAttributes();
        m_bodyController.enabled = false; // stop processing normal stuff

        var target_pos = m_currentLine.GetDirection() * distance_from_start
            + m_currentLine.GetStart()
            - Vector3.up * m_characterController.height / 2f;

        // m_characterController.Move(target_pos - m_characterController.transform.position);
        StartCoroutine(IE_LerpMount(target_pos - m_characterController.transform.position));
    }

    private IEnumerator IE_LerpMount(Vector3 delta)
    {
        int steps = 10;
        var delta_increment = delta / steps;

        for (int i = 0; i < steps; i++)
        {
            m_characterController.Move(delta_increment);

            yield return new WaitForEndOfFrame();
        }
    }

    private void Update()
    {
        // if we jump off, exit state
        if (Input.GetButtonDown("Jump"))
        {
            DismountLine();

            return;
        }

        var dir_inputs = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;
        var perspective_dir = m_perspective.TransformDirection(dir_inputs);

        float dot = Vector3.Dot(perspective_dir, m_currentLine.GetDirection());

        if (dot < 0.2f && dot > 0f) dot = 0f; // if just above 0, make it 0
        if (dot > -0.2f && dot < 0f) dot = 0f; // if just below 0, make it 0
        if (dot > 0.7f) dot = 1f; // if just under 1, make it 1
        if (dot < -0.7f) dot = -1f; // if just above 1, make it -1

        var target_line_velo = dot * m_maxDriveVelocity * m_currentLine.GetDirection();

        m_currentLineVelocity = Vector3.MoveTowards(
                m_currentLineVelocity,
                target_line_velo,
                m_driveAcceleration * Time.deltaTime);

        var backup_pos = m_characterController.transform.position;

        CollisionFlags move_result = m_characterController.Move(m_currentLineVelocity * Time.deltaTime);

        if (move_result != CollisionFlags.None || !m_currentLine.IsPositionOnLine(m_characterController.transform.position))
        {
            m_characterController.transform.position = backup_pos;
            m_currentLineVelocity = Vector3.zero;
        }
    }

    private void DismountLine()
    {
        m_currentLine.Dismount();

        // cleanup
        this.enabled = false;
        m_currentLine = null;
        m_bodyController.enabled = true;

        m_bodyController.ForceJumpBuffer();
    }
}
