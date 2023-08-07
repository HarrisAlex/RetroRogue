using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Ground
{
    Rock,
    Metal,
    Sand,
    Wood
}

[RequireComponent(typeof(AudioSource))]
public class Footstep : MonoBehaviour
{
    private MovementController movementController;
    private CharacterController controller;

    [SerializeField] private AnimationCurve stepsSpeed;

    private AudioSource source;

    [SerializeField] private AudioClip[] rockWalking;
    [SerializeField] private AudioClip[] metalWalking;
    [SerializeField] private AudioClip[] sandWalking;
    [SerializeField] private AudioClip[] woodWalking;
    [SerializeField] private AudioClip[] rockRunning;
    [SerializeField] private AudioClip[] metalRunning;
    [SerializeField] private AudioClip[] sandRunning;
    [SerializeField] private AudioClip[] woodRunning;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        movementController = GetComponent<MovementController>();
        controller = GetComponent<CharacterController>();

        if (source == null) Destroy(this);
        if (movementController == null) Destroy(this);
        if (controller == null) Destroy(this);

        StartCoroutine(PlayFootsteps());
    }

    private IEnumerator PlayFootsteps()
    {
        float elapsedTime = 0;
        while (true)
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime >= (60 / stepsSpeed.Evaluate(controller.velocity.sqrMagnitude)) && controller.velocity.sqrMagnitude > 0.01f && movementController.IsGrounded)
            {
                PlayFootstep();
                elapsedTime = 0;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    private void PlayFootstep()
    {
        if (source == null) return;

        AudioClip clip = null;

        if (controller.velocity.sqrMagnitude < 4)
            switch (movementController.CurrentGround)
            {
                case Ground.Rock:
                    if (rockWalking.Length < 1) return;
                    clip = rockWalking[Random.Range(0, rockWalking.Length - 1)];
                    break;
                case Ground.Metal:
                    if (metalWalking.Length < 1) return;
                    clip = metalWalking[Random.Range(0, metalWalking.Length - 1)];
                    break;
                case Ground.Sand:
                    if (sandWalking.Length < 1) return;
                    clip = sandWalking[Random.Range(0, sandWalking.Length - 1)];
                    break;
                case Ground.Wood:
                    if (woodWalking.Length < 1) return;
                    clip = woodWalking[Random.Range(0, woodWalking.Length - 1)];
                    break;
            }
        else
        {
            switch (movementController.CurrentGround)
            {
                case Ground.Rock:
                    if (rockRunning.Length < 1) return;
                    clip = rockRunning[Random.Range(0, rockRunning.Length - 1)];
                    break;
                case Ground.Metal:
                    if (metalRunning.Length < 1) return;
                    clip = metalRunning[Random.Range(0, metalRunning.Length - 1)];
                    break;
                case Ground.Sand:
                    if (sandRunning.Length < 1) return;
                    clip = sandRunning[Random.Range(0, sandRunning.Length - 1)];
                    break;
                case Ground.Wood:
                    if (woodRunning.Length < 1) return;
                    clip = woodRunning[Random.Range(0, woodRunning.Length - 1)];
                    break;
            }
        }

        if (clip == null) return;

        source.PlayOneShot(clip);
    }
}
