using System;
using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float lifetime = 0.8f;

    private TMP_Text damageText;
    private float timer;

    private void Awake()
    {
        damageText = GetComponent<TMP_Text>();
    }

    public void SetDamage(int amount, bool isHealing)
    {
        if (isHealing)
        {
            damageText.text = "+" + amount.ToString();

            // Healing = Green
            damageText.color = new Color32(0, 195, 0, 255);
        }
        else
        {
            damageText.text = "-" + amount.ToString();

            // Damage = Red
            damageText.color = new Color32(255, 0, 0, 255);
        }
    }

    private void Update()
    {
        // Move upward
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // Increase timer
        timer += Time.deltaTime;

        // Fade out
        float alpha = Mathf.Lerp(1f, 0f, timer / lifetime);

        Color currentColor = damageText.color;
        currentColor.a = alpha;
        damageText.color = currentColor;

        // Destroy after lifetime
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    internal void SetDamage(int actualHeal)
    {
        throw new NotImplementedException();
    }
}