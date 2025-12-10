using UnityEngine;

public class EnemyNormal : EnemyBase
{
    [Header("Melee Settings")]
    [SerializeField] private int touchDamage = 1;
    [SerializeField] private float touchDamageCooldown = 0.5f;

    private float lastTouchDamageTime = -999f;

    protected override void Move(Vector2 dir, float dist)
    {
        rb.linearVelocity = dir * moveSpeed; // always chase
    }

    void OnCollisionStay2D(Collision2D col)
    {
        TryDamagePlayer(col.gameObject);
    }

    void OnTriggerStay2D(Collider2D col)
    {
        TryDamagePlayer(col.gameObject);
    }

    void TryDamagePlayer(GameObject other)
    {
        if (isDead) return;
        if (Time.time - lastTouchDamageTime < touchDamageCooldown) return;

        var ps = other.GetComponent<PlayerStats>();
        if (ps == null) return;

        ps.TakeDamage(touchDamage);
        lastTouchDamageTime = Time.time;
    }

}
