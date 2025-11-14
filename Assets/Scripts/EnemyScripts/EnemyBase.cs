using UnityEngine;

public class EnemyBase : MonoBehaviour
{
   [Header("Stats")]
    [SerializeField] protected int hp = 3;
    [SerializeField] protected float moveSpeed = 2.5f;
    [SerializeField] protected float chaseRange = 20f;

    protected Rigidbody2D rb;
    protected Transform player;
    protected bool isDead = false;

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

    // Default "chase" move; children override
    protected virtual void Move(Vector2 dirToPlayer, float dist)
    {
        rb.linearVelocity = dirToPlayer * moveSpeed;
    }

    public virtual void TakeDamage(int amount)
    {
        if (isDead) return;
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
