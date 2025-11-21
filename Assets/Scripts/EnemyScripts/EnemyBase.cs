using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected int hp = 3;
    [SerializeField] protected float moveSpeed = 2.5f;
    [SerializeField] protected float chaseRange = 20f;

    // Optional: only allow damage via ApplyProjectileDamage()
    [SerializeField] public bool onlyProjectileDamage = false;

    // for our drops pick up for MP and HP

    // EnemyBase fields (put near your other [SerializeField]s)
    [Header("Drops")]
    [SerializeField] private bool useDrops = true;         // turn drops on/off per enemy prefab
    [Range(0f, 1f)] [SerializeField] private float dropChance = 0.25f; // chance *on death*
    [Range(0f, 1f)] [SerializeField] private float hpShare = 0.5f;     // of drops, % that are HP
    [SerializeField] private GameObject hpPickupPrefab;    // assign in Inspector
    [SerializeField] private GameObject mpPickupPrefab;    // assign in Inspector

    // Optional: only SOME enemies are eligible from the moment they spawn
    [SerializeField] private bool chooseEligibleAtSpawn = false;
    [Range(0f, 1f)] [SerializeField] private float spawnEligibleChance = 0.5f;

    private bool eligibleThisSpawn = true;  // decided in Awake if chooseEligibleAtSpawn = true


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
        if (stats) {
            player = stats.transform;
        }

        if (chooseEligibleAtSpawn){
            eligibleThisSpawn = (Random.value < spawnEligibleChance);
        }
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

            TryDrop();                // <-- add this line


        Destroy(gameObject);
    }

    void TryDrop()
    {
        if (!useDrops) return;
        if (!eligibleThisSpawn) return;                     // if using the per-spawn eligibility
        if (!hpPickupPrefab && !mpPickupPrefab) return;

        // Per-death roll: only sometimes drop
        if (Random.value > dropChance) return;              // Random.value is 0..1 float

        bool dropHP = (Random.value < hpShare);
        GameObject prefab = dropHP ? hpPickupPrefab : mpPickupPrefab;

        // fallback if one prefab not assigned
        if (!prefab) prefab = dropHP ? mpPickupPrefab : hpPickupPrefab;
        if (!prefab) return;

        Instantiate(prefab, transform.position, Quaternion.identity);
    }
}
