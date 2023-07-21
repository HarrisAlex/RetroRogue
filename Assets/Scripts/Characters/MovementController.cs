using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MovementController : MonoBehaviour
{
    // Editor variables
    [SerializeField] private float walkSpeed = 2;
    [SerializeField] private float springSpeed = 4.5f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float jumpForce = 6;

    // Movement
    private float speed = 1;
    private float TargetSpeed
    {
        get
        {
            switch (State)
            {
                case MovementState.Sprinting:
                    return springSpeed;
                case MovementState.Crouching:
                    return crouchSpeed;
                default:
                    return walkSpeed;
            }
        }
    }
    private const float sprintTransition = 4;
    private Vector3 velocity = Vector3.zero;
    private bool isGrounded;
    private bool applyInput = false;

    // Component references
    private CharacterController controller;

    // States
    public enum MovementState
    {
        Walking,
        Sprinting,
        Crouching
    }
    public MovementState State { get; private set; }


    private void Start()
    {
        controller = GetComponent<CharacterController>();

        State = MovementState.Walking;
    }

    private void Update()
    {

        // Cache variables
        speed = Mathf.Lerp(speed, TargetSpeed, Time.smoothDeltaTime * sprintTransition);
        isGrounded = Physics.CheckSphere(transform.position + new Vector3(0, controller.radius - 0.05f, 0),
            controller.radius, ~LayerMask.GetMask("Character"));

        // Add gravity
        if (isGrounded)
            velocity.y = 0;
        else
            velocity.y -= 9.8f * Time.smoothDeltaTime;

        // Localize direction of velocity
        float tempY = velocity.y;
        velocity.y = 0;
        velocity = transform.TransformDirection(velocity);
        velocity.y = tempY;

        // Clamp velocity
        velocity = Vector3.ClampMagnitude(velocity, speed);

        if (!applyInput)
        {
            velocity.x = 0;
            velocity.z = 0;
        }

        controller.Move(velocity);

        applyInput = false;
    }

    public void SetSprint(bool active)
    {
        if (active)
            State = MovementState.Sprinting;
        else
            State = MovementState.Walking;
    }

    public void ToggleCrouch()
    {
        if (State == MovementState.Crouching)
            State = MovementState.Walking;
        else
            State = MovementState.Crouching;
    }

    public void SetInput(float horizontal, float vertical)
    {
        velocity = new Vector3(horizontal * speed * Time.smoothDeltaTime, velocity.y, vertical * speed * Time.smoothDeltaTime);

        applyInput = true;
    }

    public void Jump()
    {
        velocity.y = jumpForce * Time.smoothDeltaTime;
    }
}
