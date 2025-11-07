using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int hp = 2;
    [SerializeField] private int touchDamage = 1;  // optional if you want contact damage

    [Header("Movement")]
    [SerializeField] private float speed = 2.5f;
    [SerializeField] private Vector2 changeDirEvery = new Vector2(0.8f, 1.6f);

    [Header("Drops")]
    [SerializeField] private GameObject healthPickupPrefab; // small red square
    [SerializeField] private GameObject mpPickupPrefab;     // small blue square
    [SerializeField] private int healthAmount = 3;
    [SerializeField] private int mpAmount = 3;

    private Rigidbody2D rb;
    private Vector2 dir = Vector2.zero;
    private float timer = 0f;
    private float nextChange = 1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        PickNewDir();
    }

    void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;
        if (timer >= nextChange)
        {
            PickNewDir(); 
        }

        rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);
    }

    void PickNewDir()
    {
        timer = 0f;
        nextChange = Random.Range(changeDirEvery.x, changeDirEvery.y);
        // Choose 4-way to match the player style
        Vector2[] four = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        dir = four[Random.Range(0, four.Length)];
    }

    public void TakeDamage(int amount)
    {
        hp -= amount;
        if (hp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Award XP
        var playerStats = FindFirstObjectByType<PlayerStats>();
        if (playerStats)
        {
            playerStats.AddXP(5); 
        }

        // Drop table: 0 = nothing, 1 = health, 2 = mp
        int roll = Random.Range(0, 3);
        if (roll == 1 && healthPickupPrefab)
        {
            Instantiate(healthPickupPrefab, transform.position, Quaternion.identity);
        }
        else if (roll == 2 && mpPickupPrefab)
        {
            Instantiate(mpPickupPrefab, transform.position, Quaternion.identity);
        }
        
        // >>> Tell the TimingSystem a kill happened <<<
        var ts = FindFirstObjectByType<TimingSystem>();
        if (ts)
        {
            ts.ReportEnemyKilled(); 
        }

        Destroy(gameObject);
    }

    // Optional: hurt player on contact (enable if you want this behavior)
    void OnCollisionEnter2D(Collision2D col) { TryDamagePlayer(col.gameObject); }
    void OnTriggerEnter2D(Collider2D col)    { TryDamagePlayer(col.gameObject); }

    void TryDamagePlayer(GameObject other)
    {
        var ps = other.GetComponent<PlayerStats>();
        if (ps != null)
        {
            ps.TakeDamage(touchDamage);
        }
    }
}
