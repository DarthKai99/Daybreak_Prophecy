using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Target")]
    public Transform target;          // your player

    [Header("Follow Settings")]
    public float smoothTime = 0.15f;  // lower = snappier, higher = smoother
    public Vector2 offset = Vector2.zero; // e.g. (0, 0)

    [Header("Bounds (optional)")]
    public bool confineToBounds = false;
    public Vector2 minBounds;         // world-space min (x,y)
    public Vector2 maxBounds;         // world-space max (x,y)

    private Vector3 _vel;             // for SmoothDamp
    private float _z;                 // keep original camera Z

    void Awake()
    {
        _z = transform.position.z;    // typically -10 for 2D
        if (Camera.main.orthographic == false)
            Camera.main.orthographic = true; // top-down should be orthographic
    }

    void LateUpdate()
    {
        if (!target) return;

        // desired position (match X/Y, keep camera Z)
        Vector3 targetPos = new Vector3(target.position.x + offset.x,
                                        target.position.y + offset.y,
                                        _z);

        // Smooth follow
        Vector3 next = Vector3.SmoothDamp(transform.position, targetPos, ref _vel, smoothTime);

        // Optional confine
        if (confineToBounds)
        {
            float halfH = Camera.main.orthographicSize;
            float halfW = halfH * Camera.main.aspect;

            next.x = Mathf.Clamp(next.x, minBounds.x + halfW,  maxBounds.x - halfW);
            next.y = Mathf.Clamp(next.y, minBounds.y + halfH,  maxBounds.y - halfH);
        }

        transform.position = next;
    }

}
