using UnityEngine;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Damageable enemy;
    [SerializeField] private RectTransform fill;
    [SerializeField] private float maxWidth = 227f;

    private void Update()
    {
        if (enemy == null)
        {
            Debug.LogWarning("EnemyHealthBar: Enemy is not assigned!");
            return;
        }

        if (fill == null)
        {
            Debug.LogWarning("EnemyHealthBar: Fill is not assigned!");
            return;
        }

        float healthPercent = (float)enemy.Health / enemy.MaxHealth;

        float newWidth = maxWidth * healthPercent;

        fill.sizeDelta = new Vector2(
            newWidth,
            fill.sizeDelta.y
        );
    }
}