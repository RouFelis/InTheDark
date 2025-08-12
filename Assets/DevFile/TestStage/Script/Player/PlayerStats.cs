using UnityEngine;


[DisallowMultipleComponent]
public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;

    [HideInInspector]
    public float currentHealth;

    [Header("Fall / Collision")]
    public float fallThreshold = 5f;
    public float damageMultiplier = 10f;
    public float collisionSpeedThreshold = 8f;
    public float collisionDamageMultiplier = 5f;
    public float damageCooldown = 0.5f;

    private void Reset()
    {
        currentHealth = maxHealth;
    }

    private void Awake()
    {
        currentHealth = Mathf.Max(0f, currentHealth == 0f ? maxHealth : currentHealth);
    }
}

