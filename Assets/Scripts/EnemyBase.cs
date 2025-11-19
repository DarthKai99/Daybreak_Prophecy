using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected int hp = 3;
    [SerializeField] protected float moveSpeed = 2.5f;
    [SerializeField] protected float chaseRange = 20f;

    // Optional: only allow damage via ApplyProjectileDamage()
    [SerializeField] public bool onlyProjectileDamage = false;

    protected Rigidbody2D rb;
    protected Transform player;
    protected bool isDead = false;

    // gate to allow damage only during projectile calls
    bool _allowDamageThisFrame = false;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        var stats = FindFirstObjectByType<PlayerStats>();
        if (stats) player = stats.transform;
    }

    protected virtual void FixedUpdate()
    {
        if (isDead || !player)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > chaseRange)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 dirToPlayer = ((Vector2)player.position - (Vector2)transform.position).normalized;
        Move(dirToPlayer, dist);
    }

    // Default "chase" move; children override if needed
    protected virtual void Move(Vector2 dirToPlayer, float dist)
    {
        rb.linearVelocity = dirToPlayer * moveSpeed;
    }

    // Call this from projectiles to apply damage even when onlyProjectileDamage = true
    public void ApplyProjectileDamage(int amount)
    {
        _allowDamageThisFrame = true;
        TakeDamage(amount);
        _allowDamageThisFrame = false;
    }

    // NOTE: virtual (not override) — this is the base definition
    public virtual void TakeDamage(int amount)
    {
        if (isDead) return;
        Debug.Log($"[{name}] TOOK {amount} (proj={_allowDamageThisFrame}) at t={Time.time}\n{System.Environment.StackTrace}");
        hp -= amount;
        if (hp <= 0) Die();

    }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        var ps = FindFirstObjectByType<PlayerStats>();
        if (ps) ps.AddXP(5);

        var ts = FindFirstObjectByType<TimingSystem>();
        if (ts) ts.ReportEnemyKilled();

        Destroy(gameObject);
    }
}
