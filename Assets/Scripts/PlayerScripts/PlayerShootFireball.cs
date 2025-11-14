using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerShootFireball : MonoBehaviour
{
   [Header("Fireball")]
    [SerializeField] private GameObject fireballPrefab; // assign in Inspector
    [SerializeField] private float spawnOffset = 0.6f;  // how far in front of player
    [SerializeField] private float cooldown = 0.25f;    // seconds between shots
    [SerializeField] private int   damage = 2;          // damage per fireball
    [SerializeField] private int   mpCost = 2;          // MP per shot

    private Player_movement mover;
    private PlayerStats stats;
    private float nextShootTime = 0f;

    void Awake()
    {
        mover = GetComponent<Player_movement>();
        stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.fKey.wasPressedThisFrame)
            TryShoot();
    }

    void TryShoot()
    {
        if (Time.time < nextShootTime) return;

        // MP check (optional)
        if (stats != null && !stats.UseMP(mpCost))
        {
            // TODO: show "Not enough MP"
            return;
        }

        Vector2 dir = mover ? mover.FacingDir : Vector2.right;
        if (dir == Vector2.zero) dir = Vector2.right;          // fallback
        dir = new Vector2(Mathf.Round(dir.x), Mathf.Round(dir.y)); // ensure 4-way

        Vector3 pos = transform.position + (Vector3)(dir.normalized * spawnOffset);

        var go = Instantiate(fireballPrefab, pos, Quaternion.identity);
        var proj = go.GetComponent<FireballProjectile>();
        if (proj != null) proj.Init(dir,
                                    damage,
                                    gameObject);

        nextShootTime = Time.time + cooldown;
    }
}
