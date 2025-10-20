using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(Rigidbody2D))]
public class Player_movement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 6f;
    [SerializeField] private bool fourWayOnly = true;

    private Rigidbody2D rb;
    private Vector2 movement;
    private int facing = 1;

    // Add this property at the top-level of the class
    public Vector2 FacingDir { get; private set; } = Vector2.right;



    [Header("Eye")]
    [SerializeField] private Transform eye;
    [SerializeField] private float eyeOffsetX = 0.25f;
    [SerializeField] private float eyeOffsetY = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.linearDamping = 0f;  // turn off physics drag completely
        rb.angularDamping = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        var k = Keyboard.current;
        if (k == null) return;

        float x = (k.dKey.isPressed || k.rightArrowKey.isPressed ? 1 : 0) -
                  (k.aKey.isPressed || k.leftArrowKey.isPressed ? 1 : 0);
        float y = (k.wKey.isPressed || k.upArrowKey.isPressed ? 1 : 0) -
                  (k.sKey.isPressed || k.downArrowKey.isPressed ? 1 : 0);

        movement = new Vector2(x, y);

        // In Update(), after you compute 'movement':
        if (movement != Vector2.zero)
        {
          FacingDir = movement; // last non-zero direction (4-way already)
        }

        // 4-way mode
        if (fourWayOnly && movement != Vector2.zero)
        {
            if (Mathf.Abs(movement.x) >= Mathf.Abs(movement.y)) { movement.y = 0; movement.x = Mathf.Sign(movement.x); }
            else { movement.x = 0; movement.y = Mathf.Sign(movement.y); }
        }

        if (movement.sqrMagnitude > 1f) movement.Normalize();

        if (movement.x > 0.01f) facing = 1;
        else if (movement.x < -0.01f) facing = -1;


    }

    void LateUpdate()
    {
        if (eye != null)
            eye.localPosition = new Vector3(eyeOffsetX * facing, eyeOffsetY, 0f);
    }

    void FixedUpdate()
    {
        Vector2 targetVel = movement * speed;
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVel, 0.2f); // 0.2 = smoothing factor
    }



}
