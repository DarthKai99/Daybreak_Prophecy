using UnityEngine;

public class EnemyRanged : EnemyBase
{
    [Header("Ranged Settings")]
    [SerializeField] private GameObject projectilePrefab; // prefab with EnemyProjectile
    [SerializeField] private float desiredRange = 6f;
    [SerializeField] private float rangeSlack = 1.5f;
    [SerializeField] private float backAwayBoost = 1.2f;
    [SerializeField] private float fireCooldown = 1f;
    [SerializeField] private int projectileDamage = 1;
    [SerializeField] private float projectileSpeed = 10f;

    private float nextShootTime = 0f;

    protected override void Move(Vector2 dirToPlayer, float dist)
    {
        Vector2 vel = Vector2.zero;
        float minR = desiredRange - rangeSlack;
        float maxR = desiredRange + rangeSlack;

        if (dist > maxR)          vel = dirToPlayer * moveSpeed;
        else if (dist < minR)     vel = -dirToPlayer * (moveSpeed * backAwayBoost);
        else                      vel = Vector2.zero;

        rb.linearVelocity = vel;

        if (Time.time >= nextShootTime)
        {
            Shoot(dirToPlayer);
            nextShootTime = Time.time + fireCooldown;
        }
    }

    void Shoot(Vector2 dirToPlayer)
    {
        if (!projectilePrefab) return;

        float spawnOffset = 0.8f;
        Vector2 start = transform.position;
        Vector2 dir = dirToPlayer.normalized;

        // optional safe spawn
        float radius = 0.1f;
        var cc = projectilePrefab.GetComponent<CircleCollider2D>();
        if (cc) radius = Mathf.Max(0.05f, cc.radius * Mathf.Max(projectilePrefab.transform.localScale.x, projectilePrefab.transform.localScale.y));

        RaycastHit2D hit = Physics2D.CircleCast(start, radius, dir, spawnOffset);
        Vector3 spawnPos = start + dir * spawnOffset;
        if (hit.collider && hit.collider.gameObject != gameObject)
            spawnPos = (Vector2)hit.point - dir * (radius * 0.5f);

        var go = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        var ep = go.GetComponent<EnemyProjectile>();
        if (!ep) ep = go.AddComponent<EnemyProjectile>();
        ep.Init(dir, projectileSpeed, projectileDamage, gameObject);
    }
}
