using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]

public class FireballProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 16f;
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private LayerMask enemyMask; // set to Enemy layer(s) in Inspector

    private Rigidbody2D rb;
    private Collider2D col;
    private GameObject owner;
    private int damage;

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
        if (!other || other.gameObject == owner) return;

        // 1) Enemy? -> damage + destroy
        var enemy = other.GetComponentInParent<EnemyBase>();
        if (enemy != null)
        {
            // Use the projectile gate so it works when onlyProjectileDamage = true
            enemy.ApplyProjectileDamage(damage);
            Destroy(gameObject);
            return;
        }

        // 2) Wall / any solid collider? -> destroy projectile
        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }

    
}
