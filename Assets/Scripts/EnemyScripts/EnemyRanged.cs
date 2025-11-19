using UnityEngine;

public class EnemyRanged : EnemyBase
{
    [Header("Ranged Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float desiredRange = 6f;   // target distance from player
    [SerializeField] private float rangeSlack = 1.5f;   // tolerance around desiredRange
    [SerializeField] private float backAwayBoost = 1.2f;// move a bit faster when backing up
    [SerializeField] private float fireCooldown = 1f;
    [SerializeField] private int projectileDamage = 1;
    [SerializeField] private float projectileSpeed = 10f;

    private float nextShootTime = 0f;

    protected override void Move(Vector2 dirToPlayer, float dist)
    {
        // dirToPlayer is FROM enemy TO player (normalized)
        Vector2 vel = Vector2.zero;

        float minR = desiredRange - rangeSlack;
        float maxR = desiredRange + rangeSlack;

        if (dist > maxR)
        {
            // Too far: move closer
            vel = dirToPlayer * moveSpeed;
        }
        else if (dist < minR)
        {
            // Too close: back up a bit faster
            vel = -dirToPlayer * (moveSpeed * backAwayBoost);
        }
        else
        {
            // In the comfort band: stop (or strafe if you like)
            vel = Vector2.zero;
        }

        rb.linearVelocity = vel;

        // Shoot on cooldown, always towards player
        if (Time.time >= nextShootTime)
        {
            Shoot(dirToPlayer);
            nextShootTime = Time.time + fireCooldown;
        }
    }

    void Shoot(Vector2 dirToPlayer)
    {
        if (!projectilePrefab) return;

        float spawnOffset = 0.6f;
        Vector2 start = transform.position;
        Vector2 dir = dirToPlayer.normalized;

        // Estimate projectile radius if CircleCollider2D, else small fallback
        float radius = 0.1f;
        var cc = projectilePrefab.GetComponent<CircleCollider2D>();
        if (cc) radius = Mathf.Max(0.05f, cc.radius * Mathf.Max(projectilePrefab.transform.localScale.x, projectilePrefab.transform.localScale.y));

        // Check the path to the intended spawn point
        RaycastHit2D hit = Physics2D.CircleCast(start, radius, dir, spawnOffset);
        Vector3 spawnPos = start + dir * spawnOffset;

        if (hit.collider != null && hit.collider.gameObject != gameObject)
        {
            // If blocked, place just before the wall
            spawnPos = (Vector2)hit.point - dir * (radius * 0.5f);
        }

        var go = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        var ep = go.GetComponent<EnemyProjectile>();
        if (!ep) ep = go.AddComponent<EnemyProjectile>();
        ep.Init(dir, projectileSpeed, projectileDamage, gameObject);

    }
}
