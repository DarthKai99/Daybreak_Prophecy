using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AttackHitbox : MonoBehaviour
{
   private int damage = 1;
    private GameObject owner;

    public void Init(int dmg, float life, GameObject ownerGO)
    {
        damage = dmg;
        owner = ownerGO;
        Destroy(gameObject, life);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == owner) return;

        var e = other.GetComponent<Enemy>();
        if (e != null)
        {
            e.TakeDamage(damage);
            Destroy(gameObject); // one hit then disappear
        }
    }
}
