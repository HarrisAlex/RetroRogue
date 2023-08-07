using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MovementController : MonoBehaviour, IController
{
    // Editor variables
    public float WalkSpeed => walkSpeed;
    [SerializeField] private float walkSpeed = 2;

    public float SprintSpeed => sprintSpeed;
    [SerializeField] private float sprintSpeed = 4.5f;

    public float CrouchSpeed => crouchSpeed;
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
                    return sprintSpeed;
                case MovementState.Crouching:
                    return crouchSpeed;
                default:
                    return walkSpeed;
            }
        }
    }
    private const float sprintTransition = 4;
    private Vector3 velocity = Vector3.zero;

    public bool IsGrounded => isGrounded;
    private bool isGrounded;

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

    public Ground CurrentGround => currentGround;
    private Ground currentGround;

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        State = MovementState.Walking;
        currentGround = Ground.Rock;
    }

    public void Run()
    {
        // Cache variables
        speed = Mathf.Lerp(TargetSpeed, speed, Mathf.Pow(0.5f, Time.deltaTime * sprintTransition));
        isGrounded = Physics.CheckSphere(transform.position + new Vector3(0, controller.radius - 0.05f, 0),
            controller.radius, ~LayerMask.GetMask("Character"));

        // Add gravity
        if (isGrounded)
            velocity.y = 0;
        else
            velocity.y -= 9.8f * Time.deltaTime * Time.deltaTime;

        // Localize direction of velocity
        float tempY = velocity.y;
        velocity.y = 0;
        velocity = transform.TransformDirection(velocity);
        velocity.y = tempY;

        // Clamp velocity
        velocity = Vector3.ClampMagnitude(velocity, speed * Time.deltaTime);

        controller.Move(velocity);
        Debug.Log(controller.velocity.magnitude);
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
        velocity = new Vector3(horizontal * speed * Time.deltaTime, velocity.y, vertical * speed * Time.deltaTime);
    }

    public void Jump()
    {
        velocity.y = jumpForce * Time.deltaTime;
    }
}
