using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float walkSpeed = 5f;           
    public float sprintSpeed = 10f;         
    public float rotationSpeed = 10f;     
    public float jumpHeight = 0.1f;        
    public float gravity = -20f;         
    
    [Header("Referencias")]
    public Transform cameraTransform;      
    public Animator animator;               

    private CharacterController controller;
    private float turnSmoothVelocity;
    private Vector3 velocity;               
    private bool isGrounded;
    private float currentSpeed;            
    private bool isMoving;                 

    void Start()
    {
        controller = GetComponent<CharacterController>();


        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;


        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {

        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f; 


        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;

        isMoving = inputDir.magnitude >= 0.1f;

        if (isMoving)
        {

            bool isSprinting = Input.GetKey(KeyCode.LeftShift);
            currentSpeed = isSprinting ? sprintSpeed : walkSpeed;


            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, rotationSpeed * Time.deltaTime);


            transform.rotation = Quaternion.Euler(0f, angle, 0f);


            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);
        }


        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            

            if (animator != null)
                animator.SetBool("IsJumping", true);
        }


        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);


        UpdateAnimator();
    }

    void UpdateAnimator()
    {
        if (animator == null) return;


        float speedPercent = 0f;
        if (isMoving)
        {
            speedPercent = (currentSpeed == sprintSpeed) ? 1f : 0.5f;
        }
        animator.SetFloat("Speed", speedPercent, 0.1f, Time.deltaTime);


        animator.SetBool("IsGrounded", isGrounded);


        animator.SetFloat("VerticalSpeed", velocity.y);


        if (isGrounded && velocity.y <= 0f)
        {
            animator.SetBool("IsJumping", false);
        }
    }
}