using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float turnSpeed = 10f;

    [Header("Gravity")]
    public float gravity = -9.81f;
    public float groundDistance = 0.3f;
    public LayerMask groundMask;

    [Header("Jump")]
    public float jumpHeight = 2f;

    [Header("Mobile Controls")]
    public VirtualJoystick joystick;
    public MobileButton sprintButton;
    public MobileButton jumpButton;

    [Header("Camera Reference")]
    public ThirdPersonCamera thirdPersonCamera;
    public FirstPersonCameraFollow firstPersonCamera;
    public bool isFirstPerson = false;

    [Header("Animator")]
    public Animator animator;

    private CharacterController cc;
    private Vector3 velocity;
    private bool isGrounded;
    private bool jumpQueued;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    void Start()
    {
        cc = GetComponent<CharacterController>();
        animator = animator != null ? animator : GetComponentInChildren<Animator>();

        if (jumpButton != null)
            jumpButton.OnPressed += () => jumpQueued = true;
    }

    void OnDestroy()
    {
        if (jumpButton != null)
            jumpButton.OnPressed -= () => jumpQueued = true;
    }

    void Update()
    {
        HandleGravity();
        HandleMovement();
    }

    void HandleGravity()
    {
        isGrounded = cc.isGrounded;

        if (isGrounded && velocity.y < 0f)
            velocity.y = -1f;

        bool jumpPressed = Input.GetKeyDown(KeyCode.Space) || jumpQueued;
        jumpQueued = false;
        if (isGrounded && jumpPressed)
            velocity.y = Mathf.Sqrt(jumpHeight * -1f * gravity);

        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);
    }

    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // blend in joystick only when keyboard has no input on that axis
        if (joystick != null)
        {
            if (Mathf.Abs(h) < 0.1f) h = joystick.Horizontal;
            if (Mathf.Abs(v) < 0.1f) v = joystick.Vertical;
        }

        if (Mathf.Abs(h) < 0.01f && Mathf.Abs(v) < 0.01f)
        {
            UpdateAnimator(0f);
            return;
        }

        bool isRunning = Input.GetKey(KeyCode.LeftShift)
                      || (sprintButton != null && sprintButton.IsHeld);
        float speed = isRunning ? runSpeed : walkSpeed;

        Vector3 moveDir = GetCameraRelativeDirection(h, v);
        cc.Move(moveDir * speed * Time.deltaTime);

        // rotate character to face movement direction
        if (moveDir != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(moveDir),
                turnSpeed * Time.deltaTime
            );
        }

        UpdateAnimator(isRunning ? 1f : 0.5f);
    }

    Vector3 GetCameraRelativeDirection(float h, float v)
    {
        // pick the active camera transform
        Transform camTransform = isFirstPerson && firstPersonCamera != null
            ? firstPersonCamera.transform
            : thirdPersonCamera != null
                ? thirdPersonCamera.transform
                : null;

        if (camTransform == null)
            return new Vector3(h, 0f, v).normalized;

        // flatten camera vectors to horizontal plane
        Vector3 forward = camTransform.forward;
        Vector3 right = camTransform.right;
        forward.y = 0f;
        right.y = 0f;

        // handle edge case when camera looks straight up/down
        if (forward.magnitude < 0.1f)
            forward = new Vector3(camTransform.right.z, 0f, -camTransform.right.x);

        forward.Normalize();
        right.Normalize();

        return (forward * v + right * h).normalized;
    }

    void UpdateAnimator(float speed)
    {
        if (animator != null)
            animator.SetFloat(SpeedHash, speed, 0.1f, Time.deltaTime);
    }

    public void EnableController() => enabled = true;
    public void DisableController() => enabled = false;
}