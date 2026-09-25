using System.Collections;
using UnityEngine;

public class PauseMusicDelay : MonoBehaviour
{
    [SerializeField] private float delay = 3f;

    private AudioSource audioSource;
    private Coroutine playCoroutine;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        // Make sure the music doesn't start immediately
        audioSource.Stop();

        playCoroutine = StartCoroutine(PlayAfterDelay());
    }

    private void OnDisable()
    {
        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
            playCoroutine = null;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    private IEnumerator PlayAfterDelay()
    {
        // Uses real time because the game is paused
        yield return new WaitForSecondsRealtime(delay);

        if (audioSource != null && gameObject.activeInHierarchy)
        {
            audioSource.Play();
        }
    }
}