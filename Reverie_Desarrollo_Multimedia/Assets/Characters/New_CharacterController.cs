using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class New_CharacterController : MonoBehaviour
{
    [Header("Movimiento")]
    public float WalkSpeed = 0.67f;      // Reducido a 1/6 (4/6 = 0.67)
    public float SrpintSpeed = 1f;       // Reducido a 1/6 (6/6 = 1)
    public float jumpHeight = 0.33f;     // Reducido a 1/6 (2/6 = 0.33)
    public float rotationSpeed = 10f;
    public float gravity = -20f;

    [Header("Referenciación")]
    public Transform cameraTransform;
    public Animator animator;

    private CharacterController characterController;
    private Vector3 Velocity;
    private float currentSpeed;
    private Vector3 externalVelocity = Vector3.zero;
    private float turnSmoothVelocity;  // Para rotación suave

    public bool IsMoving { get; private set; }
    public Vector2 CurrentInput { get; private set; }
    public bool IsGrounded { get; private set; }

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Si no se asigna cámara, usa la principal
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        HandleMovement();
        updateAnimator();
    }

    void HandleMovement()
    {
        IsGrounded = characterController.isGrounded;

        if (IsGrounded && Velocity.y < 0)
        {
            if (externalVelocity.y > -0.05f && externalVelocity.y < 0.05f)
                Velocity.y = 0;
            else
                Velocity.y = -2f;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;
        IsMoving = inputDirection.magnitude > 0.1f;

        Vector3 moveDirection = Vector3.zero;

        if (IsMoving)
        {
            // Detectar sprint
            bool isSprinting = Input.GetKey(KeyCode.LeftShift);
            currentSpeed = isSprinting ? SrpintSpeed : WalkSpeed;

            // Calcular dirección de movimiento relativa a la cámara
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;

            // Rotación suave del personaje hacia la dirección de movimiento
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Dirección de movimiento basada en la cámara
            moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        }
        else
        {
            currentSpeed = 0f;
        }

        // Salto
        if (Input.GetButtonDown("Jump") && IsGrounded)
        {
            Velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            // CORREGIDO: Usar "IsJumping" con I mayúscula (según tu Animator)
            if (animator != null)
                animator.SetBool("IsJumping", true);
        }

        // Aplicar gravedad
        Velocity.y += gravity * Time.deltaTime;

        // Movimiento final
        Vector3 finalMovement = (moveDirection * currentSpeed + externalVelocity) * Time.deltaTime;
        finalMovement.y += Velocity.y * Time.deltaTime;

        characterController.Move(finalMovement);

        // Desactivar animación de salto al tocar suelo
        if (IsGrounded && Velocity.y < 0f)
        {
            if (animator != null)
                animator.SetBool("IsJumping", false);
        }
    }

    void updateAnimator()
    {
        if (animator == null) return;

        float SpeedPercent = IsMoving ? (currentSpeed == SrpintSpeed ? 1f : 0.5f) : 0f;
        animator.SetFloat("Speed", SpeedPercent, 0.1f, Time.deltaTime);
        animator.SetBool("IsGrounded", IsGrounded);
        animator.SetFloat("VerticalSpeed", Velocity.y);
    }
}