using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Damageable : MonoBehaviour
{
    public UnityEvent<int, Vector2> damageableHit;

    [SerializeField]
    private GameObject damageTextPrefab;

    Animator animator;

    [SerializeField]
    private int _maxHealth = 100;

    public int MaxHealth
    {
        get
        {
            return _maxHealth;
        }
        set
        {
            _maxHealth = value;
        }
    }

    [SerializeField]
    private int _health = 100;

    public int Health
    {
        get
        {
            return _health;
        }
        set
        {
            _health = value;

            if (_health <= 0)
            {
                IsAlive = false;
            }
        }
    }

    [SerializeField]
    private bool _isAlive = true;

    [SerializeField]
    private bool isInvincible = false;

    private float timeSinceHit = 0;

    public float invincibilityTime = 0.25f;

    public bool IsAlive
    {
        get
        {
            return _isAlive;
        }
        set
        {
            _isAlive = value;

            animator.SetBool(
                AnimationStrings.isAlive,
                value
            );

            Debug.Log("IsAlive set " + value);
        }
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (isInvincible)
        {
            timeSinceHit += Time.deltaTime;

            if (timeSinceHit > invincibilityTime)
            {
                // Remove invincibility
                isInvincible = false;
                timeSinceHit = 0;
            }
        }
    }

    public bool Hit(int damage, Vector2 knockback)
    {
        if (IsAlive && !isInvincible)
        {
            // Reduce health
            Health -= damage;

            if (damageTextPrefab != null)
            {
                Debug.Log("Spawning damage text on " + gameObject.name);

                GameObject damageText = Instantiate(
                    damageTextPrefab,
                    transform.position + Vector3.up * 1f,
                    Quaternion.identity
                );

                DamageText text = damageText.GetComponent<DamageText>();

                if (text != null)
                {
                    Debug.Log("DamageText component found");
                    text.SetDamage(damage, false);
                }
                else
                {
                    Debug.LogError("DamageText component NOT found on prefab!");
                }
            }
            else
            {
                Debug.LogError("Damage Text Prefab is NOT assigned on " + gameObject.name);
            }

            // Enable invincibility
            isInvincible = true;

            // Play hit animation
            animator.SetTrigger(AnimationStrings.hitTrigger);

            // Lock player velocity so knockback is not overwritten
            animator.SetBool(
                AnimationStrings.lockVelocity,
                true
            );

            // Apply knockback through UnityEvent
            damageableHit?.Invoke(damage, knockback);

            // Unlock velocity after a short delay
            StartCoroutine(UnlockVelocity());

            return true;
        }

        return false;
    }

    public bool Heal(int amount)
    {
        if (!IsAlive || Health >= MaxHealth)
        {
            return false;
        }

        int oldHealth = Health;

        Health += amount;

        // Don't go above maximum health
        if (Health > MaxHealth)
        {
            Health = MaxHealth;
        }

        int actualHealing = Health - oldHealth;

        // Spawn floating healing text
        if (actualHealing > 0 && damageTextPrefab != null)
        {
            GameObject damageText = Instantiate(
                damageTextPrefab,
                transform.position + Vector3.up * 1f,
                Quaternion.identity
            );

            DamageText text = damageText.GetComponent<DamageText>();

            if (text != null)
            {
                text.SetDamage(actualHealing, true);
            }
        }

        return actualHealing > 0;
    }

    private IEnumerator UnlockVelocity()
    {
        yield return new WaitForSeconds(invincibilityTime);

        animator.SetBool(
            AnimationStrings.lockVelocity,
            false
        );
    }

    
}