using UnityEngine;
using UnityEngine.UI;

public class KnightHealthBar : MonoBehaviour
{
    [SerializeField] private Damageable knight;
    [SerializeField] private Slider healthSlider;

    [SerializeField] private float hideDelay = 3f;
    [SerializeField] private float fadeDuration = 0.5f;

    private float hideTimer;
    private int previousHealth;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = healthSlider.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = healthSlider.gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void Start()
    {
        healthSlider.minValue = 0;
        healthSlider.maxValue = knight.MaxHealth;
        healthSlider.value = knight.Health;

        previousHealth = knight.Health;

        canvasGroup.alpha = 0f;
        healthSlider.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (knight == null || healthSlider == null)
            return;

        // Update health bar
        healthSlider.value = knight.Health;

        // Knight is dead
        if (!knight.IsAlive)
        {
            canvasGroup.alpha = 0f;
            healthSlider.gameObject.SetActive(false);
            return;
        }

        // Knight was hit
        if (knight.Health != previousHealth)
        {
            previousHealth = knight.Health;

            healthSlider.gameObject.SetActive(true);

            // Show immediately
            canvasGroup.alpha = 1f;

            // Reset timer
            hideTimer = hideDelay;
        }

        // If health bar is visible
        if (healthSlider.gameObject.activeSelf)
        {
            hideTimer -= Time.deltaTime;

            // Start fading during the last fadeDuration
            if (hideTimer <= fadeDuration)
            {
                float fadeAmount = hideTimer / fadeDuration;

                canvasGroup.alpha = Mathf.Clamp01(fadeAmount);
            }

            // Completely hide after timer finishes
            if (hideTimer <= 0f)
            {
                canvasGroup.alpha = 0f;
                healthSlider.gameObject.SetActive(false);
            }
        }
    }
}