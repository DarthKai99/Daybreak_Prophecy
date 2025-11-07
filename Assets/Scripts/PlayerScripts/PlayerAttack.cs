using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private GameObject attackPrefab; // Prefab with AttackHitbox + trigger collider
    [SerializeField] private float distance = 0.6f;   // How far in front of the player
    [SerializeField] private float lifetime = 0.1f;   // How long the hitbox exists
    [SerializeField] private int damage = 1;

    private Player_movement mover;

    void Awake()
    {
        mover = GetComponent<Player_movement>();
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            DoAttack();
    }

    void DoAttack()
    {
        if (!attackPrefab) return;

        // Use last facing direction (defaults to right if you never moved)
        Vector2 dir = mover ? (mover.FacingDir == Vector2.zero ? Vector2.right : mover.FacingDir) : Vector2.right;
        dir = new Vector2(Mathf.Round(dir.x), Mathf.Round(dir.y)); // ensure 4-way placement
        if (dir == Vector2.zero) dir = Vector2.right;

        Vector3 spawnPos = transform.position + (Vector3)dir.normalized * distance;
        var go = Instantiate(attackPrefab, spawnPos, Quaternion.identity);
        var hb = go.GetComponent<AttackHitbox>();
        if (hb) hb.Init(damage, lifetime, gameObject);
    }
}
