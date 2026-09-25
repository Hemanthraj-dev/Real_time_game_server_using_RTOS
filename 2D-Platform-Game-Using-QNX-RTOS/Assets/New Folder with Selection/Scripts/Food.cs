using UnityEngine;

public class Food : MonoBehaviour
{
    public int healingAmount = 20;

    [Header("Floating Animation")]
    public float floatHeight = 0.15f;
    public float floatSpeed = 2f;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        float newY = startPosition.y +
                     Mathf.Sin(Time.time * floatSpeed) * floatHeight;

        transform.position = new Vector3(
            startPosition.x,
            newY,
            startPosition.z
        );
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Damageable damageable = collision.GetComponent<Damageable>();

        if (damageable != null)
        {
            bool healed = damageable.Heal(healingAmount);

            if (healed)
            {
                Destroy(gameObject);
            }
        }
    }
}