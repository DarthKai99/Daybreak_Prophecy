using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]

public class FireballProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 30f;
    [SerializeField] private float lifetime = 10f;

    private Rigidbody2D rb;
    private GameObject owner;
    private int damage;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Init(Vector2 dir, int dmg, GameObject ownerGO)
    {
        damage = dmg;
        owner = ownerGO;

        // physics setup (just in case not set on prefab)
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // launch
        rb.linearVelocity = dir.normalized * this.speed;

        // auto-despawn
        Destroy(gameObject, this.lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == owner) return;

        // Hit enemy?
        var enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Hit something else that's not pass-through? (e.g., walls with triggers)
        // If you use non-trigger walls, also add OnCollisionEnter2D below.
        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject == owner) return;

        var enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}
