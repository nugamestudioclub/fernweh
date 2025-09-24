using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public class DetachableCameraController : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private Vector3 move_input;

    
    [SerializeField] private float maxSpeed;
    [SerializeField] private float accelRate;

    [SerializeField] private float mouseSensitivity;

    [SerializeField] private Transform cam;


    InputAction camMoveAction;
    InputAction mouseAction;
    InputAction camSpinAction;

    void Start() {
        rb = gameObject.GetComponent<Rigidbody>();    

        camMoveAction = InputSystem.actions.FindAction("CamMove");
        mouseAction = InputSystem.actions.FindAction("Look");
        camSpinAction = InputSystem.actions.FindAction("CamSpin");

    }
    void Update() {
        
        CursorStuff();
    }
    void FixedUpdate() {
        Movement();
        Rotation();
    }

    private void Movement() {
        move_input = camMoveAction.ReadValue<Vector3>();
        move_input.Normalize();

        Vector3 targetSpeed = transform.TransformDirection(move_input) * maxSpeed;
        Vector3 speedDif = targetSpeed - rb.linearVelocity;
        Vector3 movement = speedDif * accelRate;
        rb.AddForce(movement);
    }

    private void Rotation() {
        Vector2 mouse = mouseAction.ReadValue<Vector2>() * mouseSensitivity * Time.deltaTime;
        float spin = camSpinAction.ReadValue<float>() * 30 * Time.deltaTime;

        rb.AddTorque(transform.right * mouse.y * -1);
        rb.AddTorque(transform.up * mouse.x * 1);
        rb.AddTorque(transform.forward * spin * -1);
    }

    private void CursorStuff() {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Input.GetMouseButtonDown(0)) // click to re-lock
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
