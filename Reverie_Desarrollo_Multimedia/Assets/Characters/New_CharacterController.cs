using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class New_CharacterController : MonoBehaviour
{
    [Header("Movimiento")]
    public float WalkSpeed = 0.67f;
    public float SrpintSpeed = 1.2f;
    public float jumpHeight = 0.6f;
    public float rotationSpeed = 10f;
    public float gravity = -20f;

    [Header("Referenciación")]
    [Tooltip("Transform de la cámara (Main Camera)")]
    public Transform cameraTransform;
    public Animator animator;

    private CharacterController characterController;
    private Vector3 Velocity;
    private float currentSpeed;
    private Vector3 externalVelocity = Vector3.zero;
    private float turnSmoothVelocity;

    public bool IsMoving { get; private set; }
    public Vector2 CurrentInput { get; private set; }
    public bool IsGrounded { get; private set; }

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Si no se asigna cámara, usa la principal
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        // Si no se asigna animator, intenta obtenerlo del objeto
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        HandleMovement();
        UpdateAnimator();
    }

    void HandleMovement()
    {
        // Detectar si está en el suelo
        IsGrounded = characterController.isGrounded;

        if (IsGrounded && Velocity.y < 0)
        {
            Velocity.y = -2f; // mantenerlo pegado al suelo
        }

        // Input de movimiento
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        CurrentInput = new Vector2(horizontal, vertical);

        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;
        IsMoving = inputDirection.magnitude >= 0.1f;

        Vector3 moveDir = Vector3.zero;

        if (IsMoving)
        {
            // Detectar sprint
            bool isSprinting = Input.GetKey(KeyCode.LeftShift);
            currentSpeed = isSprinting ? SrpintSpeed : WalkSpeed;

            // Ángulo según la cámara (SOLO usamos su yaw)
            float camYaw = (cameraTransform != null) ? cameraTransform.eulerAngles.y : 0f;
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + camYaw;

            float angle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref turnSmoothVelocity,
                rotationSpeed * Time.deltaTime
            );

            // Girar el personaje
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Mover según la dirección de la cámara
            moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        }
        else
        {
            currentSpeed = 0f;
        }

        // Salto (con tecla Espacio)
        if (Input.GetButtonDown("Jump") && IsGrounded)
        {
            Velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            if (animator != null)
                animator.SetBool("IsJumping", true);
        }

        // Aplicar gravedad
        Velocity.y += gravity * Time.deltaTime;

        // Movimiento final
        Vector3 finalMovement = (moveDir * currentSpeed + externalVelocity) * Time.deltaTime;
        finalMovement.y += Velocity.y * Time.deltaTime;

        characterController.Move(finalMovement);

        // Desactivar IsJumping cuando toca el suelo
        if (IsGrounded && Velocity.y <= 0f)
        {
            if (animator != null)
                animator.SetBool("IsJumping", false);
        }
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        // Speed: 0 = idle, 0.5 = walk, 1 = sprint
        float SpeedPercent = 0f;
        if (IsMoving)
        {
            SpeedPercent = (currentSpeed == SrpintSpeed) ? 1f : 0.5f;
        }

        animator.SetFloat("Speed", SpeedPercent, 0.1f, Time.deltaTime);
        animator.SetBool("IsGrounded", IsGrounded);
        animator.SetFloat("VerticalSpeed", Velocity.y);
    }

    // Método público para aplicar fuerzas externas (si lo usas)
    public void AddExternalVelocity(Vector3 velocity)
    {
        externalVelocity = velocity;
    }
}
