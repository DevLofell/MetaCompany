using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSoundSystem : MonoBehaviour
{
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private AudioClip JumpSound;
    [SerializeField] private AudioClip LandingSound;
    private AudioSource audioSource;
    [SerializeField] private float walkInterval = 0.5f; // 발걸음 소리 간격
    [SerializeField] private float runInterval = 0.25f; // 발걸음 소리 간격
    private Coroutine soundCoroutine;
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
        if (inputManager.PlayerJumpedThisFrame())
        {
            StopFootsteps();
            StartJump();
        }
        else if (inputManager.GetPlayerMovement() != Vector2.zero && !inputManager.inputCrouch)
        {
            bool currentlyRunning = inputManager.PlayerRan();
            if (currentlyRunning != isRunning || soundCoroutine == null)
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

    #region FootStep
    private void RestartFootsteps()
    {
        StopFootsteps();
        soundCoroutine = StartCoroutine(PlayFootsteps());
    }

    private IEnumerator PlayFootsteps()
    {
        
        while (true)
        {
            PlayRandomSound("FootStep");
            yield return new WaitForSeconds(isRunning ? runInterval : walkInterval);
        }
    }
    private void StartFootsteps()
    {
        if (soundCoroutine == null)
        {
            soundCoroutine = StartCoroutine(PlayFootsteps());
        }
    }

    private void StopFootsteps()
    {
        if (soundCoroutine != null)
        {
            StopCoroutine(soundCoroutine);
            soundCoroutine = null;
        }
    }
    #endregion

    #region Jump
    private void StartJump()
    {
        PlayRandomSound("Jump");
    }

    public void StartLanding()
    {
        PlayRandomSound("Landing");
    }
    #endregion
    private void PlayRandomSound(string type)
    {
        switch (type)
        {
            case "FootStep":
                if (footstepSounds.Length > 0)
                {
                    int randomIndex = Random.Range(0, footstepSounds.Length);
                    audioSource.PlayOneShot(footstepSounds[randomIndex]);
                }
                break;
            case "Jump":
                audioSource.PlayOneShot(JumpSound);
                break;
            case "Landing":
                audioSource.PlayOneShot(LandingSound);
                break;
        }
    }
}
