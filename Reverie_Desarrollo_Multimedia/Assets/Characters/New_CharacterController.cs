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

    [Header("Sistema de Pollo")]
    [Tooltip("Punto donde se equipará el pollo (en la cabeza)")]
    public Transform puntoEquipoPollo;
    [Tooltip("Multiplicador de salto cuando tiene el pollo")]
    public float multiplicadorSaltoPollo = 2.5f;
    [Tooltip("Multiplicador de gravedad cuando tiene el pollo (menor = cae más lento)")]
    public float multiplicadorGravedadPollo = 0.4f;

    private CharacterController characterController;
    private Vector3 Velocity;
    private float currentSpeed;
    private Vector3 externalVelocity = Vector3.zero;
    private float turnSmoothVelocity;


    private Transform verticalPlatform;
    private Vector3 lastPlatformPosition;
    private bool wasOnVerticalPlatform;


    private GameObject polloEquipado;
    private bool tienePollo = false;
    private float jumpHeightOriginal;
    private float gravityOriginal;
    private int saltosRestantesPollo = 4;
    private Vector3 posicionInicialPollo;
    private Quaternion rotacionInicialPollo;

    public bool IsMoving { get; private set; }
    public Vector2 CurrentInput { get; private set; }
    public bool IsGrounded { get; private set; }

    void Start()
    {
        characterController = GetComponent<CharacterController>();


        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;


        if (animator == null)
            animator = GetComponent<Animator>();


        if (puntoEquipoPollo == null)
        {
            GameObject puntoTemp = new GameObject("PuntoEquipoPollo");
            puntoTemp.transform.SetParent(transform);
            puntoTemp.transform.localPosition = new Vector3(0, 2f, 0); 
            puntoEquipoPollo = puntoTemp.transform;
        }


        jumpHeightOriginal = jumpHeight;
        gravityOriginal = gravity;
    }

    void Update()
    {
        DetectVerticalPlatform();
        HandleMovement();
        UpdateAnimator();
    }

    void DetectVerticalPlatform()
    {

        if (!characterController.isGrounded)
        {
            verticalPlatform = null;
            wasOnVerticalPlatform = false;
            return;
        }


        RaycastHit hit;
        float rayDistance = characterController.height / 2f + 0.2f;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, rayDistance))
        {

            MovingPlatformRangeStable platform = hit.collider.GetComponentInParent<MovingPlatformRangeStable>();

            if (platform != null)
            {

                if (verticalPlatform != platform.transform)
                {
                    verticalPlatform = platform.transform;
                    lastPlatformPosition = verticalPlatform.position;
                    wasOnVerticalPlatform = false;
                }
                else
                {

                    Vector3 platformDelta = verticalPlatform.position - lastPlatformPosition;


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

        IsGrounded = characterController.isGrounded;

        if (IsGrounded && Velocity.y < 0)
        {
            Velocity.y = -2f; 
        }


        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        CurrentInput = new Vector2(horizontal, vertical);

        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;
        IsMoving = inputDirection.magnitude >= 0.1f;

        Vector3 moveDir = Vector3.zero;

        if (IsMoving)
        {

            bool isSprinting = Input.GetKey(KeyCode.LeftShift);
            currentSpeed = isSprinting ? SrpintSpeed : WalkSpeed;


            float camYaw = (cameraTransform != null) ? cameraTransform.eulerAngles.y : 0f;
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + camYaw;

            float angle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref turnSmoothVelocity,
                rotationSpeed * Time.deltaTime
            );


            transform.rotation = Quaternion.Euler(0f, angle, 0f);


            moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        }
        else
        {
            currentSpeed = 0f;
        }


        if (Input.GetButtonDown("Jump") && IsGrounded)
        {

            if (tienePollo)
            {
                if (saltosRestantesPollo > 0)
                {
                    Velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                    saltosRestantesPollo--;

                    Debug.Log($"Saltos restantes: {saltosRestantesPollo}");

                    if (animator != null)
                        animator.SetBool("IsJumping", true);
                }
                else
                {
                    Debug.Log("¡No quedan saltos!");
                }
            }
            else
            {

                Velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

                if (animator != null)
                    animator.SetBool("IsJumping", true);
            }
        }


        float gravityMultiplier = wasOnVerticalPlatform ? 0.3f : 1f;
        Velocity.y += gravity * gravityMultiplier * Time.deltaTime;


        Vector3 finalMovement = (moveDir * currentSpeed + externalVelocity) * Time.deltaTime;
        finalMovement.y += Velocity.y * Time.deltaTime;

        characterController.Move(finalMovement);


        if (IsGrounded && Velocity.y <= 0f)
        {
            if (animator != null)
                animator.SetBool("IsJumping", false);


            if (tienePollo && saltosRestantesPollo <= 0)
            {
                RegresarPolloAPosicionInicial();
            }
        }
    }

    void UpdateAnimator()
    {
        if (animator == null) return;


        float SpeedPercent = 0f;
        if (IsMoving)
        {
            SpeedPercent = (currentSpeed == SrpintSpeed) ? 1f : 0.5f;
        }

        animator.SetFloat("Speed", SpeedPercent, 0.1f, Time.deltaTime);
        animator.SetBool("IsGrounded", IsGrounded);
        animator.SetFloat("VerticalSpeed", Velocity.y);
    }

    // ==================== MÉTODOS DEL SISTEMA DEL POLLO ====================

    public void EquiparPollo(GameObject pollo, Vector3 posInicial, Quaternion rotInicial)
    {
        if (tienePollo) return;

        tienePollo = true;
        polloEquipado = pollo;
        saltosRestantesPollo = 4;


        posicionInicialPollo = posInicial;
        rotacionInicialPollo = rotInicial;

        Debug.Log($"Posición inicial del pollo recibida: {posicionInicialPollo}");


        Rigidbody polloRb = pollo.GetComponent<Rigidbody>();
        if (polloRb != null)
            Destroy(polloRb);

        Collider polloCollider = pollo.GetComponent<Collider>();
        if (polloCollider != null)
            polloCollider.enabled = false;


        PolloRecogible polloScript = pollo.GetComponent<PolloRecogible>();
        if (polloScript != null)
            polloScript.enabled = false;


        pollo.transform.SetParent(puntoEquipoPollo);
        pollo.transform.localPosition = new Vector3(0, -0.2f, 0);
        pollo.transform.localRotation = Quaternion.Euler(0, 90, 0);
        pollo.transform.localScale = Vector3.one * 2f;


        jumpHeight = jumpHeightOriginal * multiplicadorSaltoPollo;
        gravity = gravityOriginal * multiplicadorGravedadPollo;

        Debug.Log($"¡Pollo equipado! {saltosRestantesPollo} saltos disponibles.");
    }
    private void RegresarPolloAPosicionInicial()
    {
        if (!tienePollo || polloEquipado == null)
        {
            Debug.LogWarning("No se puede regresar el pollo");
            return;
        }

        Debug.Log("=== Iniciando regreso del pollo ===");
        tienePollo = false;


        jumpHeight = jumpHeightOriginal;
        gravity = gravityOriginal;

 
        polloEquipado.transform.SetParent(null);


        polloEquipado.transform.localScale = Vector3.one;


        polloEquipado.transform.position = posicionInicialPollo;
        polloEquipado.transform.rotation = rotacionInicialPollo;

        Debug.Log($"Pollo reposicionado en: {posicionInicialPollo}");


        Collider polloCollider = polloEquipado.GetComponent<Collider>();
        if (polloCollider != null)
        {
            polloCollider.enabled = true;
            Debug.Log($"✓ Collider restaurado: {polloCollider.GetType().Name}");
        }


        PolloRecogible polloScript = polloEquipado.GetComponent<PolloRecogible>();
        if (polloScript != null)
        {
            polloScript.enabled = true;
            Debug.Log("✓ Script PolloRecogible reactivado");
        }

        polloEquipado.SetActive(true);

        Debug.Log("=== ✓ Pollo regresado a su posición original (sin Rigidbody) ===");

        polloEquipado = null;
    }

    public void DesequiparPollo()
    {
        if (!tienePollo || polloEquipado == null) return;

        tienePollo = false;


        jumpHeight = jumpHeightOriginal;
        gravity = gravityOriginal;


        Destroy(polloEquipado);
        polloEquipado = null;

        Debug.Log("Pollo desequipado. Valores normales restaurados.");
    }

    public bool TienePollo()
    {
        return tienePollo;
    }

    public int GetSaltosRestantes()
    {
        return tienePollo ? saltosRestantesPollo : 0;
    }


    public void AddExternalVelocity(Vector3 velocity)
    {
        externalVelocity = velocity;
    }

    void OnDrawGizmos()
    {

        if (Application.isPlaying) return;

        if (characterController == null) return;

        Gizmos.color = wasOnVerticalPlatform ? Color.green : Color.yellow;
        float rayDistance = characterController.height / 2f + 0.2f;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * rayDistance);

        /*
        if (puntoEquipoPollo != null)
        {
            Gizmos.color = tienePollo ? Color.cyan : Color.gray;
            Gizmos.DrawWireSphere(puntoEquipoPollo.position, 0.2f);
        }
        */
    }
}