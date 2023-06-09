using UnityEngine;

public class LookController : MonoBehaviour
{
    // Editor variables
    [SerializeField] private float lookSensitivity = 5;
    [SerializeField] private float lookAngleMax = 85;
    [SerializeField] private float lookAngleMin = -70;

    private Vector2 lookVelocity = Vector2.zero;

    // Component References
    private new Transform camera;

    private void Awake()
    {
        camera = transform.GetChild(0);
    }

    private void LateUpdate()
    {
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
        camera.localEulerAngles = new Vector3(lookVelocity.y, 0, 0);
        transform.eulerAngles = new Vector3(0, lookVelocity.x, 0);
    }

    public void SetInput(float horizontal, float vertical)
    {
        lookVelocity.x += horizontal * lookSensitivity * Time.smoothDeltaTime * 100;
        lookVelocity.y -= vertical * lookSensitivity * Time.smoothDeltaTime * 100;
    }
}