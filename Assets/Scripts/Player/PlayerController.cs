using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour, IDamagable
{
    public int Health { get; private set; }

    // Editor variables
    [SerializeField] private float walkSpeed = 2;
    [SerializeField] private float springSpeed = 4.5f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float sprintTransition = 1;

    [SerializeField] private float lookSensitivity = 5;
    [SerializeField] private float lookAngleMax = 85;
    [SerializeField] private float lookAngleMin = -70;

    [SerializeField] private float jumpForce = 6;

    // Movement
    private Vector3 moveVelocity = Vector3.zero;
    private Vector2 lookVelocity = Vector2.zero;
    private float speed;

    // States
    public enum MovementState
    {
        Walking,
        Sprinting,
        Crouching
    }
    public MovementState state { get; set; }

    // Gravity
    private bool isGrounded;

    // Component References
    private CharacterController controller;
    private new Camera camera;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        camera = transform.GetChild(0).GetComponent<Camera>();

        // Initialize cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Initialize vars
        state = MovementState.Walking;
        speed = walkSpeed;
    }

    private void Update()
    {
        #region Editor - Pausing
#if UNITY_EDITOR
        // Enables pausing the game and unlocking mouse for dev purposes
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = !Cursor.visible;
            Time.timeScale = Time.timeScale == 0 ? 1 : 0;
        }
#endif
        #endregion

        // Cache result of IsGrounded()
        isGrounded = IsGrounded();

        // Update current movement state
        state = GetState();

        // Set player speed based on movement state
        SetSpeed();

        moveVelocity.x = Input.GetAxis("Horizontal") * speed * Time.smoothDeltaTime;
        moveVelocity.z = Input.GetAxis("Vertical") * speed * Time.smoothDeltaTime;

        if (isGrounded)
        {
            moveVelocity.y = 0;

            if (Input.GetButtonDown("Jump"))
            {
                moveVelocity.y = jumpForce * Time.smoothDeltaTime;
            }
        }
        else
        {
            moveVelocity.y -= 9.8f * Time.smoothDeltaTime;
        }

        // Remove vertical velocity from transformation
        float tempY = moveVelocity.y;
        moveVelocity.y = 0;
        moveVelocity = transform.TransformDirection(moveVelocity);
        moveVelocity.y = tempY;

        // Apply movement
        controller.Move(moveVelocity);
    }

    private void LateUpdate()
    {
        lookVelocity.x += Input.GetAxis("Mouse X") * lookSensitivity * Time.smoothDeltaTime * 100;
        lookVelocity.y -= Input.GetAxis("Mouse Y") * lookSensitivity * Time.smoothDeltaTime * 100;

        // Wrap horizontal angle
        if (lookVelocity.x > 360)
        {
            lookVelocity.x -= 360;
        }
        else if (lookVelocity.x < -360)
        {
            lookVelocity.x += 360;
        }

        // Clamp vertial angle
        lookVelocity.y = Mathf.Clamp(lookVelocity.y, lookAngleMin, lookAngleMax);

        camera.transform.localEulerAngles = new Vector3(lookVelocity.y, 0, 0);
        transform.eulerAngles = new Vector3(0, lookVelocity.x, 0);
    }

    private MovementState GetState()
    {
        if (Input.GetButtonDown("Sprint"))
        {
            return MovementState.Sprinting;
        }
        else if (Input.GetButtonUp("Sprint"))
        {
            return MovementState.Walking;
        }

        if (Input.GetButtonDown("Crouch"))
        {
            if (state == MovementState.Crouching)
                return MovementState.Walking;
            else
                return MovementState.Crouching;
        }

        // Default state
        return state;
    }

    private void SetSpeed()
    {
        switch (state)
        {
            case MovementState.Walking:
                speed = Mathf.Lerp(speed, walkSpeed, Time.smoothDeltaTime * sprintTransition);
                break;
            case MovementState.Sprinting:
                speed = Mathf.Lerp(speed, springSpeed, Time.smoothDeltaTime * sprintTransition);
                break;
            case MovementState.Crouching:
                speed = Mathf.Lerp(speed, crouchSpeed, Time.smoothDeltaTime * sprintTransition);
                break;
        }
    }

    private bool IsGrounded()
    {
        return Physics.CheckSphere(transform.position + new Vector3(0, controller.radius - 0.05f, 0), controller.radius, ~LayerMask.GetMask("Player"));
    }

    public void Damage(int amount)
    {
        Health -= amount;
    }

    public void Heal(int amount)
    {
        Health += amount;
    }
}