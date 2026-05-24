using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float turnSpeed = 150f;

    private Animator animator;
    private CharacterController controller;

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Check if any movement key is pressed
        bool isInputActive = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || 
                             Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);

        // Interrupt wave ONLY if the wave animation is actively playing
        if (isInputActive && animator.GetCurrentAnimatorStateInfo(0).IsName("wave"))
        {
            animator.Play("idle"); 
        }

        // Rotate left/right with A and D
        float turnInput = Input.GetAxis("Horizontal");
        transform.Rotate(0, turnInput * turnSpeed * Time.deltaTime, 0);

        float currentSpeed = 0f;
        float animatorSpeedTarget = 0f;

        // W walks forward, S runs forward
        if (Input.GetKey(KeyCode.W))
        {
            currentSpeed = walkSpeed;
            animatorSpeedTarget = 0.5f; 
        }
        else if (Input.GetKey(KeyCode.S))
        {
            currentSpeed = runSpeed;
            animatorSpeedTarget = 1.0f; 
        }

        // Apply movement and gravity
        Vector3 moveDirection = transform.forward * currentSpeed;
        moveDirection.y = Physics.gravity.y; 
        controller.Move(moveDirection * Time.deltaTime);

        // Update the Animator Speed parameter
        animator.SetFloat("Speed", animatorSpeedTarget);
    }
}