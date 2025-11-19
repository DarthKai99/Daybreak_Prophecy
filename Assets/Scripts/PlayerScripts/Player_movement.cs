using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;



[RequireComponent(typeof(Rigidbody2D))]
public class Player_movement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 6f;
    [SerializeField] private float sprintMultiplier = 1.5f; // how much faster when holding Shift
    [SerializeField] private bool fourWayOnly = false;

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

        // 4-way snap (DISABLE this to allow diagonals)
        if (fourWayOnly && movement != Vector2.zero)
        {
            if (Mathf.Abs(movement.x) >= Mathf.Abs(movement.y)) { movement = new Vector2(Mathf.Sign(movement.x), 0f); }
            else { movement = new Vector2(0f, Mathf.Sign(movement.y)); }
        }

        // normalize so diagonals aren't faster
        if (movement.sqrMagnitude > 1f) movement = movement.normalized;

        // FacingDir = last non-zero input (now supports diagonals)
        if (movement != Vector2.zero)
            FacingDir = movement;

        // Update your left/right eye indicator if you still want it
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
        float currentSpeed = speed;

        // Check if shift is held (new Input System)
        if (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed)
            currentSpeed *= sprintMultiplier;

        Vector2 targetVel = movement * currentSpeed;
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVel, 0.2f);
    }



}
