using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float walkSpeed = 5f;            // Velocidad de caminar
    public float sprintSpeed = 8f;          // Velocidad de correr
    public float rotationSpeed = 10f;       // Suavidad de giro
    public float jumpHeight = 2f;           // Altura del salto
    public float gravity = -9.81f;          // Gravedad personalizada

    [Header("Referencias")]
    public Transform cameraTransform;       // Referencia a la cámara
    public Animator animator;               // Referencia al Animator

    private CharacterController controller;
    private float turnSmoothVelocity;
    private Vector3 velocity;               // Controla la velocidad vertical
    private bool isGrounded;
    private float currentSpeed;             // Velocidad actual (walk o sprint)
    private bool isMoving;                  // Si el personaje se está moviendo

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Si no se asigna cámara, usa la principal
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        // Si no se asigna animator, intenta obtenerlo del objeto
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Detectar si está en el suelo
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f; // mantenerlo pegado al suelo

        // Movimiento según cámara
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;

        isMoving = inputDir.magnitude >= 0.1f;

        if (isMoving)
        {
            // Detectar si está corriendo (Shift izquierdo)
            bool isSprinting = Input.GetKey(KeyCode.LeftShift);
            currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

            // Calcula ángulo según la cámara
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, rotationSpeed * Time.deltaTime);

            // Gira el personaje
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Mueve según la dirección de la cámara
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);
        }

        // Salto (con tecla Espacio)
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            // Activar animación de salto
            if (animator != null)
                animator.SetBool("isJumping", true);
        }

        // Aplicar gravedad
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Actualizar animaciones
        UpdateAnimator();
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        // Speed: 0 = idle, 0.5 = walk, 1 = sprint
        float speedPercent = 0f;
        if (isMoving)
        {
            speedPercent = (currentSpeed == sprintSpeed) ? 1f : 0.5f;
        }
        animator.SetFloat("Speed", speedPercent, 0.1f, Time.deltaTime);

        // IsGrounded
        animator.SetBool("IsGrounded", isGrounded);

        // VerticalSpeed (para animaciones de caída/salto)
        animator.SetFloat("VerticalSpeed", velocity.y);

        // Desactivar isJumping cuando toca el suelo
        if (isGrounded && velocity.y <= 0f)
        {
            animator.SetBool("isJumping", false);
        }
    }
}