using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public class DetachableCameraController : MonoBehaviour
{
    [Header("Flight Settings")]
    [SerializeField] private float speed = 15;
    [SerializeField] private float maxSpeed = 50;
    [SerializeField] private float spinSpeed = 1;
    [SerializeField] private float mouseSensitivity = 2f;


    [Header("Physics")]
    [SerializeField] private float airResistance = 0.02f;
    [SerializeField] private float dampingFactor = 0.95f;
    
    private Rigidbody rb;
    
    // Input values
    private Vector3 movementInput;
    private Vector2 lookInput;
    private float spinInput;

    void Start() {
        rb = gameObject.GetComponent<Rigidbody>();   

        rb.linearDamping = airResistance;
        rb.angularDamping = 2f;
        rb.useGravity = false;
        
        Cursor.lockState = CursorLockMode.Locked;

    }
    void FixedUpdate() {
        Movement();
        Rotation();
        Damping();
    }

    private void Movement() {
        Vector3 directionalMovement = transform.TransformDirection(movementInput);
        Vector3 thrust = directionalMovement * speed;

        rb.AddForce(thrust, ForceMode.Acceleration);
        VelocityLimit();
    }

    private void Rotation() {

        Vector3 torque = 
        transform.right * -lookInput.y * mouseSensitivity+   // pitch
        transform.up * lookInput.x * mouseSensitivity+       // yaw
        transform.forward * -spinInput * spinSpeed;     // roll

        rb.AddTorque(torque);   
    }

    private void VelocityLimit()
    {
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }
    
    private void Damping()
    {
        if (movementInput.magnitude < .1)
        {
            rb.linearVelocity *= dampingFactor;
        }
        
        rb.angularVelocity *= dampingFactor;
    }

    public void OnCamMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector3>();
    }
    
    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }
    
    public void OnCamSpin(InputAction.CallbackContext context)
    {
        spinInput = context.ReadValue<float>();
    }
}
