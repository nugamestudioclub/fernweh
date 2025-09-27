using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public enum MoveType {
    FREE_LOOK,
    LOCKED_LOOK
}
public class DetachableCameraController : MonoBehaviour
{
    [SerializeField] private MoveType moveType; 
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

    [Header("Rotation 2")]
    private float yaw = 0;
    private float pitch = 0;
    private float yawVelocity = 0f;
    private float pitchVelocity = 0f;
    [SerializeField] private float maxPitch = 90;
    [SerializeField] private float rotationSmoothTime = 0.12f;
    [SerializeField] private float mouseSensitivity2 = 2;

    void Start() {
        rb = gameObject.GetComponent<Rigidbody>();   

        rb.linearDamping = airResistance;
        rb.angularDamping = 2f;
        rb.useGravity = false;
        
        Cursor.lockState = CursorLockMode.Locked;

    }
    void Update() {

        if (moveType == MoveType.LOCKED_LOOK) {
            Rotation2();
        }
        
    }
    void FixedUpdate() {
        if (moveType == MoveType.FREE_LOOK) {
            Movement();
            Rotation();
        } else if (moveType == MoveType.LOCKED_LOOK) {
            Movement2();
        }
       
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

    private void Rotation2()
    {
        yaw += lookInput.x * mouseSensitivity2;
        pitch -= lookInput.y * mouseSensitivity2;

        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);
        
        float currentYaw = transform.eulerAngles.y;
        float currentPitch = transform.eulerAngles.x;

        if (currentPitch > 180f) currentPitch -= 360f;
        
        float smoothYaw = Mathf.SmoothDampAngle(currentYaw, yaw, ref yawVelocity, rotationSmoothTime);
        float smoothPitch = Mathf.SmoothDampAngle(currentPitch, pitch, ref pitchVelocity, rotationSmoothTime);
        
        transform.rotation = Quaternion.Euler(smoothPitch, smoothYaw, 0f);
    }

    private void Movement2() {
        Vector3 directionalMovement = new Vector3(movementInput.x, 0f, movementInput.z);
        directionalMovement = transform.TransformDirection(directionalMovement);
        directionalMovement.y = movementInput.y;

        Vector3 thrust = directionalMovement * speed;

        rb.AddForce(thrust, ForceMode.Acceleration);
        VelocityLimit();
    }
}
