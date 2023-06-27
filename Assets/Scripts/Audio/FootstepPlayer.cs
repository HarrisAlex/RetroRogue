using System.IO;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FootstepPlayer : MonoBehaviour
{
    [System.Serializable]
    public struct FootstepTier
    {
        public float distance;
        public float volume;
    }

    // Editor variables
    [SerializeField] private FootstepTier walkTier;
    [SerializeField] private FootstepTier sprintTier;
    [SerializeField] private FootstepTier crouchTier;

    // AssetBundle
    private static AssetBundle footstepBundle;
    private static AudioClip[] footstepSounds;

    // Component references
    private MovementController controller;
    private AudioSource source;

    private Vector3 lastFootstepPosition;
    private FootstepTier footstepTier;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        controller = GetComponent<MovementController>();

        // Load Footstep AssetBundle
        footstepBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "footsteps"));
        footstepSounds = footstepBundle.LoadAllAssets<AudioClip>();
    }

    void Update()
    {
        switch (controller.State)
        {
            case MovementController.MovementState.Walking:
                footstepTier = walkTier;
                break;
            case MovementController.MovementState.Sprinting:
                footstepTier = sprintTier;
                break;
            case MovementController.MovementState.Crouching:
                footstepTier = crouchTier;
                break;
        }

        if (GetXZDistance(transform.position, lastFootstepPosition) > footstepTier.distance)
            PlayFootstep();
    }

    private float GetXZDistance(Vector3 a, Vector3 b)
    {
        a.y = 0;
        b.y = 0;

        return (a - b).sqrMagnitude;
    }

    void PlayFootstep()
    {
        if (!source) return;
        if (footstepSounds.Length < 1) return;

        // Play random footstep sound at appropriate volume
        source.PlayOneShot(footstepSounds[Random.Range(0, footstepSounds.Length)], footstepTier.volume);

        // Set last footstep position
        lastFootstepPosition = transform.position;
    }
}
