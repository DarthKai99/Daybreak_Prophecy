using UnityEngine;
using UnityEngine.InputSystem;


public class RotateTankTurret : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform turret;
    [SerializeField] private Camera cam;

    [Header("Tuning")]
    [SerializeField] private float turnLerp = 25f;
    [SerializeField] private float angleOffset = 0f;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!turret) Debug.LogWarning("RotateTankTurret: 'turret' not assigned. Drag TankTurret here.");
    }

    void LateUpdate()
    {
        if (!turret || !cam) return;
        if (Mouse.current == null) return;

        Vector3 mouse = Mouse.current.position.ReadValue();
        Vector3 world = cam.ScreenToWorldPoint(mouse);
        world.z = 0f;

        Vector2 dir = (Vector2)(world - turret.position);
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + angleOffset;

        Quaternion target = Quaternion.AngleAxis(ang, Vector3.forward);
        turret.rotation = Quaternion.Lerp(turret.rotation, target, Time.deltaTime * turnLerp);
    }

}
