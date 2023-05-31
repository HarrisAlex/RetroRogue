using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour, IDamagable
{
    public int Health { get; private set; }

    // Editor variables
    [SerializeField] private float moveSensitivity = 3;
    [SerializeField] private float lookSensitivity = 5;
    [SerializeField] private float lookAngleMax = 85;
    [SerializeField] private float lookAngleMin = -70;
    [SerializeField] private float jumpForce = 6;

    // Movement
    private Vector3 moveVelocity = Vector3.zero;
    private Vector2 lookVelocity = Vector2.zero;

    // Gravity
    private bool isGrounded;

    // Component References
    private CharacterController controller;
    private new Camera camera;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        camera = transform.GetChild(0).GetComponent<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Cache result of IsGrounded()
        isGrounded = IsGrounded();

        #region EditorOnly
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

        moveVelocity.x = Input.GetAxis("Horizontal") * moveSensitivity * Time.smoothDeltaTime;
        moveVelocity.z = Input.GetAxis("Vertical") * moveSensitivity * Time.smoothDeltaTime;

        if (isGrounded)
        {
            if (Input.GetButtonDown("Jump"))
            {
                moveVelocity.y = jumpForce * Time.smoothDeltaTime;
            }
        }
        else
        {
            moveVelocity.y -= Physics.gravity.y;
        }

        // Apply movement
        moveVelocity = transform.TransformDirection(moveVelocity);
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