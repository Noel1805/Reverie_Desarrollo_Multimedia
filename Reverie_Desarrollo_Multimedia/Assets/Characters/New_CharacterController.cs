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

    [Header("Plataformas Verticales")]
    [Tooltip("Multiplicador para seguir plataformas verticales")]
    [Range(1f, 3f)]
    public float verticalPlatformStickiness = 1.5f;

    [Header("Referenciación")]
    [Tooltip("Transform de la cámara (Main Camera)")]
    public Transform cameraTransform;
    public Animator animator;

    private CharacterController characterController;
    private Vector3 Velocity;
    private float currentSpeed;
    private Vector3 externalVelocity = Vector3.zero;
    private float turnSmoothVelocity;

    // Detección de plataformas verticales
    private Transform verticalPlatform;
    private Vector3 lastPlatformPosition;
    private bool wasOnVerticalPlatform;

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
        DetectVerticalPlatform();
        HandleMovement();
        UpdateAnimator();
    }

    void DetectVerticalPlatform()
    {
        // Solo detectar si estamos en el suelo
        if (!characterController.isGrounded)
        {
            verticalPlatform = null;
            wasOnVerticalPlatform = false;
            return;
        }

        // Raycast corto hacia abajo
        RaycastHit hit;
        float rayDistance = characterController.height / 2f + 0.2f;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, rayDistance))
        {
            // Buscar MovingPlatformRangeStable
            MovingPlatformRangeStable platform = hit.collider.GetComponentInParent<MovingPlatformRangeStable>();

            if (platform != null)
            {
                // Si es una nueva plataforma
                if (verticalPlatform != platform.transform)
                {
                    verticalPlatform = platform.transform;
                    lastPlatformPosition = verticalPlatform.position;
                    wasOnVerticalPlatform = false;
                }
                else
                {
                    // Calcular movimiento de la plataforma
                    Vector3 platformDelta = verticalPlatform.position - lastPlatformPosition;

                    // Mover al jugador con la plataforma (especialmente en Y)
                    if (platformDelta.magnitude > 0.0001f)
                    {
                        characterController.Move(platformDelta * verticalPlatformStickiness);
                    }

                    lastPlatformPosition = verticalPlatform.position;
                    wasOnVerticalPlatform = true;
                }
            }
            else
            {
                verticalPlatform = null;
                wasOnVerticalPlatform = false;
            }
        }
        else
        {
            verticalPlatform = null;
            wasOnVerticalPlatform = false;
        }
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

            // Ángulo según la cámara
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

        // Salto
        if (Input.GetButtonDown("Jump") && IsGrounded)
        {
            Velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            if (animator != null)
                animator.SetBool("IsJumping", true);
        }

        // Aplicar gravedad (reducida si estamos en plataforma vertical)
        float gravityMultiplier = wasOnVerticalPlatform ? 0.3f : 1f;
        Velocity.y += gravity * gravityMultiplier * Time.deltaTime;

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

    // Método público para aplicar fuerzas externas
    public void AddExternalVelocity(Vector3 velocity)
    {
        externalVelocity = velocity;
    }

    // Debug visual
    void OnDrawGizmos()
    {
        if (!Application.isPlaying || characterController == null) return;

        // Raycast de detección
        Gizmos.color = wasOnVerticalPlatform ? Color.green : Color.yellow;
        float rayDistance = characterController.height / 2f + 0.2f;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * rayDistance);
    }
}