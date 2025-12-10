using UnityEngine;

public class EnemyExplosive : EnemyBase
{
    [Header("Explosion Settings")]
    [SerializeField] private float explodeRadius = 2.5f;
    [SerializeField] private int explodeDamage = 4;
    [SerializeField] private LayerMask explodeMask;

    [Header("Explode When Shot")]
    [SerializeField] private bool explodeOnShot = true;
    [SerializeField] private int hitsToExplodeOnShot = 2;
    private int shotHitsAccum = 0;

    private bool hasExploded = false;  // <<< IMPORTANT FIX

    protected override void Move(Vector2 dir, float dist)
    {
        rb.linearVelocity = dir * moveSpeed;
    }

    public override void TakeDamage(int amount)
    {
        if (isDead) return;

        hp -= amount;

        // If still alive, check "explode-on-shot"
        if (explodeOnShot && hp > 0)
        {
            shotHitsAccum++;
            if (shotHitsAccum >= hitsToExplodeOnShot)
            {
                SafeExplodeAndDie();
                return;
            }
        }

        // Explode on normal death
        if (hp <= 0)
        {
            SafeExplodeAndDie();
        }
    }

    // PLAYER contact = explode
    void OnCollisionEnter2D(Collision2D col)  { TryExplode(col.gameObject); }
    void OnTriggerEnter2D(Collider2D col)     { TryExplode(col.gameObject); }

    void TryExplode(GameObject other)
    {
        if (isDead) return;
        if (!other.GetComponent<PlayerStats>()) return;

        SafeExplodeAndDie();
    }

    /// SAFE explosion wrapper to prevent recursion
    private void SafeExplodeAndDie()
    {
        if (isDead) return;
        if (hasExploded) return;  // <<< prevents multiple explosions

        hasExploded = true;       // <<< important
        Explode();                // AoE damage

        base.Die();               // award XP, report kill, destroy object
    }

    void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explodeRadius, explodeMask);

        foreach (var h in hits)
        {
            if (!h) continue;

            // damage player
            var ps = h.GetComponent<PlayerStats>();
            if (ps)
            {
                ps.TakeDamage(explodeDamage);
            }

            // damage other enemies
            var otherEnemy = h.GetComponent<EnemyBase>();
            if (otherEnemy && otherEnemy != this)
            {
                otherEnemy.TakeDamage(explodeDamage);
            }
        }

        // optional VFX/SFX
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explodeRadius);
    }
}
