using UnityEngine;

public class ProjectileFaceVelocityDirection : MonoBehaviour
{
    [SerializeField] float angleOffset = 0f;

    Rigidbody2D rb;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    void LateUpdate()
    {
        if (!rb) return;
        Vector2 v = rb.linearVelocity;
        if (v.sqrMagnitude < 0.0001f) return;

        float ang = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg + angleOffset;
        transform.rotation = Quaternion.AngleAxis(ang, Vector3.forward);
    }
}
