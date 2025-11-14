using UnityEngine;

public class EnemyExplosive : EnemyBase
{
    [Header("Explosion Settings")]
    [SerializeField] private float explodeRadius = 2.5f;
    [SerializeField] private int explodeDamage = 4;
    [SerializeField] private LayerMask explodeMask;

    [Header("Explode When Shot")]
    [SerializeField] private bool explodeOnShot = true;   // explode before death after N hits
    [SerializeField] private int hitsToExplodeOnShot = 2; // "a couple" hits
    private int shotHitsAccum = 0;

    protected override void Move(Vector2 dir, float dist)
    {
        rb.linearVelocity = dir * moveSpeed; // use velocity
    }

    public override void TakeDamage(int amount)
    {
        if (isDead) return;

        hp -= amount;

        // explode while still alive after N hits
        if (explodeOnShot && hp > 0)
        {
            shotHitsAccum++;
            if (shotHitsAccum >= hitsToExplodeOnShot)
            {
                Explode();       // AoE only
                base.Die();      // count kill + destroy
                return;
            }
        }

        // died from damage -> explode on death
        if (hp <= 0)
        {
            Explode();           // AoE only
            base.Die();          // count kill + destroy
        }
    }

    void OnCollisionEnter2D(Collision2D col)  { TryExplode(col.gameObject); }
    void OnTriggerEnter2D(Collider2D col)     { TryExplode(col.gameObject); }

    // Contact with PLAYER explodes immediately
    void TryExplode(GameObject other)
    {
        if (isDead) return;
        if (!other.GetComponent<PlayerStats>()) return;

        Explode();               // AoE only
        base.Die();              // count kill + destroy
    }

    // Do NOT set isDead or Destroy here!
    void Explode()
    {
        // AoE damage to player + enemies
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explodeRadius, explodeMask);
        foreach (var h in hits)
        {
            if (!h) continue;

            var ps = h.GetComponent<PlayerStats>();
            if (ps) ps.TakeDamage(explodeDamage);

            var otherEnemy = h.GetComponent<EnemyBase>();
            if (otherEnemy && otherEnemy != this)
                otherEnemy.TakeDamage(explodeDamage);
        }

        // Optional: VFX/SFX here
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explodeRadius);
    }
}
