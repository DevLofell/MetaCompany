using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootStepSystem : MonoBehaviour
{
    public AudioClip[] footstepSounds;
    private AudioSource audioSource;
    [SerializeField] private float walkInterval = 0.5f; // 발걸음 소리 간격
    [SerializeField] private float runInterval = 0.25f; // 발걸음 소리 간격
    private Coroutine footstepCoroutine;
    private InputManager inputManager;
    private float interval = 0f;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        inputManager = InputManager.instance;
    }

    private bool isRunning = false;

    private void Update()
    {
        if (inputManager.GetPlayerMovement() != Vector2.zero && !inputManager.inputCrouch)
        {
            bool currentlyRunning = inputManager.PlayerRan();
            if (currentlyRunning != isRunning || footstepCoroutine == null)
            {
                isRunning = currentlyRunning;
                RestartFootsteps();
            }
        }
        else
        {
            StopFootsteps();
        }
    }

    private void RestartFootsteps()
    {
        StopFootsteps();
        footstepCoroutine = StartCoroutine(PlayFootsteps());
    }

    private IEnumerator PlayFootsteps()
    {
        while (true)
        {
            PlayRandomFootstepSound();
            yield return new WaitForSeconds(isRunning ? runInterval : walkInterval);
        }
    }
    public void StartFootsteps()
    {
        if (footstepCoroutine == null)
        {
            print("@");
            footstepCoroutine = StartCoroutine(PlayFootsteps());
        }
    }

    public void StopFootsteps()
    {
        if (footstepCoroutine != null)
        {
            StopCoroutine(footstepCoroutine);
            footstepCoroutine = null;
        }
    }

    private void PlayRandomFootstepSound()
    {
        if (footstepSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, footstepSounds.Length);
            audioSource.PlayOneShot(footstepSounds[randomIndex]);
        }
    }
}
