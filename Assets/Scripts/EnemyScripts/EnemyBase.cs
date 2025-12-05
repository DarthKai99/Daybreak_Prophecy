using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected int hp = 3;
    [SerializeField] protected float moveSpeed = 2.5f;
    [SerializeField] protected float chaseRange = 20f;

 

    // Drops
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

    [Header("Wander Settings")]
    [SerializeField] private bool enableWander = true;
    [SerializeField] private float wanderRadius = 5f;
    [Range(0f, 1f)] [SerializeField] private float wanderSpeedMultiplier = 0.6f;
    [SerializeField] private float minWanderInterval = 1.5f;
    [SerializeField] private float maxWanderInterval = 3.5f;


    [Header("Obstacle Avoidance")]
    [SerializeField] private bool enableAvoidance = true;
    [SerializeField] private LayerMask obstacleMask;  // set to Obstacle layer in Inspector
    [SerializeField] private float avoidDistance = 1.0f;   // how far ahead to check
    [SerializeField] private float sideCheckDistance = 0.75f; // side rays length

    protected Rigidbody2D rb;
    protected Transform player;
    protected bool isDead = false;



    // wander state
    private Vector2 spawnPosition;
    private Vector2 wanderTarget;
    private float nextWanderPickTime = 0f;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        var stats = FindFirstObjectByType<PlayerStats>();
        if (stats)
        {
            player = stats.transform;
        }

        if (chooseEligibleAtSpawn)
        {
            eligibleThisSpawn = (Random.value < spawnEligibleChance);
        }

        // record where this enemy spawned – wander centers around this
        spawnPosition = transform.position;
        wanderTarget = spawnPosition;
        nextWanderPickTime = Time.time;
    }

    protected virtual void FixedUpdate()
    {
        if (isDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float distToPlayer = Mathf.Infinity;
        bool hasPlayer = player != null;

        if (hasPlayer)
        {
            distToPlayer = Vector2.Distance(transform.position, player.position);
        }

        // If we have a player in range → chase (child overrides Move)
        if (hasPlayer && distToPlayer <= chaseRange)
        {
            Vector2 dirToPlayer = ((Vector2)player.position - (Vector2)transform.position).normalized;
            Move(dirToPlayer, distToPlayer);
        }
        else
        {
            // Outside chase range or no player: wander or idle
            if (enableWander)
            {
                Wander();
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    // Default "chase" move; children override if needed
    protected virtual void Move(Vector2 dirToPlayer, float dist)
    {
        Vector2 desiredDir = dirToPlayer.normalized;

        // Apply obstacle avoidance
        desiredDir = ApplyAvoidance(desiredDir);

        // Use the avoided direction, NOT dirToPlayer
        rb.linearVelocity = desiredDir * moveSpeed;
    }

    // --- Wander logic ---
    private void Wander()
    {
        // How close is current position to wander target?
        Vector2 pos = transform.position;
        float sqrDist = (wanderTarget - pos).sqrMagnitude;
        bool reachedTarget = sqrDist < 0.1f * 0.1f;

        // Time to pick a new random point?
        if (Time.time >= nextWanderPickTime || reachedTarget)
        {
            Vector2 offset = Random.insideUnitCircle * wanderRadius;
            wanderTarget = spawnPosition + offset;

            float interval = Random.Range(minWanderInterval, maxWanderInterval);
            nextWanderPickTime = Time.time + interval;
        }

        Vector2 dir = (wanderTarget - pos).normalized;
         dir = ApplyAvoidance(dir); // <--- add this
        rb.linearVelocity = dir * moveSpeed * wanderSpeedMultiplier;
    }

    // Call this from projectiles to apply damage even when onlyProjectileDamage = true
 
    // NOTE: virtual (not override) — this is the base definition
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

        // PLAY ENEMY DEATH SFX (ONCE)
        if (AudioManager.Instance)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.enemyDeathClip);
        }

        var ps = FindFirstObjectByType<PlayerStats>();
        if (ps) ps.AddXP(2);

        var ts = FindFirstObjectByType<TimingSystem>();
        if (ts) ts.ReportEnemyKilled();

        TryDrop();
        Destroy(gameObject);
    }

    void TryDrop()
    {
        if (!useDrops) return;
        if (!eligibleThisSpawn) return;
        if (!hpPickupPrefab && !mpPickupPrefab) return;

        // Per-death roll: only sometimes drop
        if (Random.value > dropChance) return;

        bool dropHP = (Random.value < hpShare);
        GameObject prefab = dropHP ? hpPickupPrefab : mpPickupPrefab;

        // fallback if one prefab not assigned
        if (!prefab) prefab = dropHP ? mpPickupPrefab : hpPickupPrefab;
        if (!prefab) return;

        Instantiate(prefab, transform.position, Quaternion.identity);
    }

    // Adjust desiredDir to steer around obstacles
    protected Vector2 ApplyAvoidance(Vector2 desiredDir)
    {
        if (!enableAvoidance) return desiredDir;
        if (desiredDir.sqrMagnitude < 0.0001f) return desiredDir;

        Vector2 pos = transform.position;
        desiredDir = desiredDir.normalized;

        // Ray straight ahead
        RaycastHit2D hitFront = Physics2D.Raycast(pos, desiredDir, avoidDistance, obstacleMask);
        if (!hitFront) return desiredDir; // nothing in front

        // Directions to left and right (perpendicular)
        Vector2 leftDir  = new Vector2(-desiredDir.y,  desiredDir.x);
        Vector2 rightDir = new Vector2( desiredDir.y, -desiredDir.x);

        bool leftBlocked  = Physics2D.Raycast(pos, leftDir,  sideCheckDistance, obstacleMask);
        bool rightBlocked = Physics2D.Raycast(pos, rightDir, sideCheckDistance, obstacleMask);

        // CASE 1: front blocked, but only one side blocked -> go to the free side
        if (!leftBlocked && rightBlocked)
        {
            return leftDir;   // left is open
        }
        if (!rightBlocked && leftBlocked)
        {
            return rightDir;  // right is open
        }

        // CASE 2: front blocked, both sides free -> slide along obstacle
        if (!leftBlocked && !rightBlocked)
        {
            // slide along the obstacle surface using its normal
            Vector2 normal = hitFront.normal;                // points away from wall
            Vector2 slideDir = Vector2.Perpendicular(normal); // perpendicular to wall

            // choose the slide direction that's closer to where we wanted to go
            if (Vector2.Dot(slideDir, desiredDir) < 0)
                slideDir = -slideDir;

            return slideDir.normalized;
        }

        // CASE 3: front + both sides blocked -> BACK OUT
        // e.g. wedged between two blocks, dead-end corner, etc.
        return -desiredDir;

    }



    // --- Gizmos: visualize chase + wander radius ---
    private void OnDrawGizmosSelected()
    {
        // chase range (red)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        // wander radius (cyan) around spawn position if possible
        Gizmos.color = Color.cyan;

        Vector3 center;
        if (Application.isPlaying)
        {
            center = spawnPosition;
        }
        else
        {
            // in edit mode, use current position as "spawn"
            center = transform.position;
        }

        if (enableWander)
        {
            Gizmos.DrawWireSphere(center, wanderRadius);
        }
    }
}
