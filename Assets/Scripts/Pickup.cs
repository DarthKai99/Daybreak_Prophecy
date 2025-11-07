using UnityEngine;

public class Pickup : MonoBehaviour
{
    public enum Kind { Health, MP ,EXP }
    public Kind type = Kind.Health;
    public int amount = 3;

    void OnTriggerEnter2D(Collider2D other)
    {
        var ps = other.GetComponent<PlayerStats>();
        if (!ps) return;

        if (type == Kind.Health) ps.Heal(amount);
        else                     ps.RestoreMP(amount);

        Destroy(gameObject);
    }
}
