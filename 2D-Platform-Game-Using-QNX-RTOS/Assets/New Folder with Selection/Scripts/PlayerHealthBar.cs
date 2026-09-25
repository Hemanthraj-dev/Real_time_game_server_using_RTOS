using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] private Damageable player;
    [SerializeField] private Slider healthSlider;

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("PlayerHealthBar: Player reference is missing!");
            return;
        }

        if (healthSlider == null)
        {
            Debug.LogError("PlayerHealthBar: Health Slider reference is missing!");
            return;
        }

        healthSlider.minValue = 0;
        healthSlider.maxValue = player.MaxHealth;
        healthSlider.value = player.Health;
    }

    private void Update()
    {
        if (player == null || healthSlider == null)
            return;

        healthSlider.value = player.Health;
    }
}