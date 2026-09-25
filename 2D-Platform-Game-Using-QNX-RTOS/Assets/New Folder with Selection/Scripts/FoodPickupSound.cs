using UnityEngine;

public class FoodPickupSound : MonoBehaviour
{
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private float volume = 1f;
    [SerializeField] private float pitch = 1f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Damageable player = collision.GetComponent<Damageable>();

        if (player != null && player.IsAlive)
        {
            if (pickupSound != null)
            {
                GameObject soundObject = new GameObject("FoodPickupSound");

                AudioSource audioSource = soundObject.AddComponent<AudioSource>();

                audioSource.clip = pickupSound;
                audioSource.volume = volume;
                audioSource.pitch = pitch;
                audioSource.Play();

                Destroy(
                    soundObject,
                    pickupSound.length / Mathf.Abs(pitch)
                );
            }
        }
    }
}