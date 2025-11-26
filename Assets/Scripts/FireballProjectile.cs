using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]

public class FireballProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 16f;
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private LayerMask enemyMask; // optional, if you want layer filtering

    private Rigidbody2D rb;
    private Collider2D col;
    private GameObject owner;
    private int damage;

    private bool hasHit = false;   // prevent multiple hits

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        col.isTrigger = true; // projectiles as triggers = easiest
    }

    public void Init(Vector2 dir, int dmg, GameObject ownerGO)
    {
        owner = ownerGO;
        damage = dmg;

        // Don't hit the shooter
        foreach (var oc in owner.GetComponentsInChildren<Collider2D>())
            Physics2D.IgnoreCollision(col, oc, true);

        rb.linearVelocity = dir.normalized * speed;
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;
        if (!other || other.gameObject == owner) return;

        GameObject go = other.gameObject;

        // OPTIONAL: if you want to use the enemyMask, check layer here:
        // bool isEnemyLayer = (enemyMask.value & (1 << go.layer)) != 0;

        // 1) Enemy? -> damage + destroy
        var enemy = go.GetComponentInParent<EnemyBase>();
        if (enemy != null)
        {
            hasHit = true;
            enemy.TakeDamage(damage);   // <-- THIS is what was missing
            Destroy(gameObject);
            return;
        }

        // 2) Wall / any solid collider? -> destroy projectile
        if (!other.isTrigger)
        {
            hasHit = true;
            Destroy(gameObject);
        }
    }

    
}
