using UnityEngine;

public class LookController : MonoBehaviour
{
    // Editor variables
    [SerializeField] private float lookSensitivity = 5;
    [SerializeField] private float lookAngleMax = 85;
    [SerializeField] private float lookAngleMin = -70;

    private Vector2 lookVelocity = Vector2.zero;
    private bool applyInput = false;

    // Component References
    private new Transform camera;

    private void Awake()
    {
        if (transform.childCount > 0)
            camera = transform.GetChild(0);
    }

    private void LateUpdate()
    {
        if (!applyInput)
            return;

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

        // Apply rotation
        transform.eulerAngles = new Vector3(0, lookVelocity.x, 0);
        if (camera != null)
            camera.localEulerAngles = new Vector3(lookVelocity.y, 0, 0);

        applyInput = false;
    }

    public void SetInput(float horizontal, float vertical)
    {
        lookVelocity.x += horizontal * lookSensitivity * Time.smoothDeltaTime * 100;
        lookVelocity.y -= vertical * lookSensitivity * Time.smoothDeltaTime * 100;

        applyInput = true;
    }

    public void SetInput(Vector3 target)
    {
        Vector3 toVector = new Vector3(target.x - transform.position.x, 0, target.z - transform.position.z);

        lookVelocity.x = Vector3.SignedAngle(Vector3.forward, toVector, Vector3.up);
        lookVelocity.y = 0;

        applyInput = true;
    }
}