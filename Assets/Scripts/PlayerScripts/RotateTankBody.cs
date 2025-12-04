using UnityEngine;

public class RotateTankBody : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform body;
    [SerializeField] private Rigidbody2D rb;

    [Header("Tuning")]
    [SerializeField] private float minSpeedToRotate = 0.05f;
    [SerializeField] private float turnLerp = 20f;
    [SerializeField] private float angleOffset = 0f;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!body) Debug.LogWarning("RotateTankBody: 'body' is not assigned. Drag TankBody here in the Inspector.");
    }

    void LateUpdate()
    {
        if (!body || !rb) return;

        Vector2 v = rb.linearVelocity;
        if (v.sqrMagnitude < minSpeedToRotate * minSpeedToRotate) return;

        float ang = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg + angleOffset;
        Quaternion target = Quaternion.AngleAxis(ang, Vector3.forward);
        body.rotation = Quaternion.Lerp(body.rotation, target, Time.deltaTime * turnLerp);
    }
}
