using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Week 5 - Driving City: player car movement.
// W / Up = forward, S / Down = reverse, A / Left = turn left, D / Right = turn right.
// Sprites face UP. The car stays inside the playable area (min/max bounds).
public class DrivingCarController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float maxReverseSpeed = 3f;
    [SerializeField] private float acceleration = 6f;
    [SerializeField] private float friction = 4f;

    [Header("Turning")]
    [SerializeField] private float turnSpeed = 140f;

    [Header("Playable Area (world units)")]
    [SerializeField] private Vector2 minBounds = new Vector2(1f, 1f);
    [SerializeField] private Vector2 maxBounds = new Vector2(71f, 47f);

    private float currentSpeed = 0f;

    void Update()
    {
        float drive = 0f;
        float turn = 0f;
        ReadInput(out drive, out turn);

        // speed grows with the key, and slowly returns to 0 without a key
        if (drive != 0f)
        {
            currentSpeed = currentSpeed + drive * acceleration * Time.deltaTime;
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, friction * Time.deltaTime);
        }
        currentSpeed = Mathf.Clamp(currentSpeed, -maxReverseSpeed, maxSpeed);

        // turning depends on speed: standing = no turn, reverse = opposite direction
        float speedFactor = currentSpeed / maxSpeed;
        transform.Rotate(0f, 0f, turn * turnSpeed * speedFactor * Time.deltaTime);

        // move forward (up of the sprite)
        transform.Translate(Vector3.up * (currentSpeed * Time.deltaTime));

        // keep the car inside the city
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
        pos.y = Mathf.Clamp(pos.y, minBounds.y, maxBounds.y);
        transform.position = pos;
    }

    private void ReadInput(out float drive, out float turn)
    {
#if ENABLE_INPUT_SYSTEM
        var keys = Keyboard.current;
        if (keys == null) { drive = 0f; turn = 0f; return; }
        drive = 0f; turn = 0f;
        if (keys.wKey.isPressed || keys.upArrowKey.isPressed) drive += 1f;
        if (keys.sKey.isPressed || keys.downArrowKey.isPressed) drive -= 1f;
        if (keys.aKey.isPressed || keys.leftArrowKey.isPressed) turn += 1f;
        if (keys.dKey.isPressed || keys.rightArrowKey.isPressed) turn -= 1f;
#else
        drive = Input.GetAxisRaw("Vertical");
        turn = -Input.GetAxisRaw("Horizontal");
#endif
    }
}
