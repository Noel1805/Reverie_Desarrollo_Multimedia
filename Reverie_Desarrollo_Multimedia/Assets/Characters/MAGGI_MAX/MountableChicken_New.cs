using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MountableChicken_New : MonoBehaviour
{
    [Header("🐔 Movimiento")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float jumpHeight = 1.5f;

    [Header("🎮 Controles")]
    [SerializeField] private KeyCode mountKey = KeyCode.E;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private float mountRadius = 2f; // ✅ CAMBIADO de 2.5f a 2f

    [Header("📍 Referencias")]
    [SerializeField] private Transform seatPoint;
    [SerializeField] private Animator maggiAnimator;

    [Header("📷 Cámara (opcional)")]
    [SerializeField] private Transform cameraOverride;

    [Header("🔧 Ajustes de Rotación")]
    [Tooltip("Offset de rotación de MAGGI en Y (ajusta si Maggi mira de lado). Prueba 0, 90, 180, -90")]
    [SerializeField] private float maggiRotationOffset = 0f;

    [Tooltip("Offset de rotación del JUGADOR en Y (ajusta si el jugador mira de lado). Prueba 0, 90, 180, -90")]
    [SerializeField] private float playerRotationOffset = 0f;

    [Header("📍 Ajuste de Posición del Jugador")]
    [Tooltip("Offset de posición local del jugador respecto al SeatPoint (X, Y, Z)")]
    [SerializeField] private Vector3 playerPositionOffset = Vector3.zero;

    [Header("🔄 Sistema de Respawn")]
    [Tooltip("Límite Y para detectar caída de Maggi (debe coincidir con PlayerCheckpointSystem)")]
    [SerializeField] private float limiteY = -10f;

    // Referencias privadas
    private CharacterController chickenController;
    private Transform player;
    private Transform playerModel;
    private Transform cameraTarget;
    private CharacterController playerCC;
    private New_CharacterController playerMovement;
    private Animator playerAnimator;

    private bool isMounted = false;
    private Vector3 velocity = Vector3.zero;
    private float turnSmoothVelocity;

    // Para respawn
    private Vector3 spawnPos;
    private Quaternion spawnRot;
    private Quaternion originalPlayerModelRotation;

    void Awake()
    {
        chickenController = GetComponent<CharacterController>();
        spawnPos = transform.position;
        spawnRot = transform.rotation;
    }

    void Start()
    {
        // Buscar jugador
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogError("❌ NO SE ENCONTRÓ JUGADOR CON TAG 'Player'");
            enabled = false;
            return;
        }

        player = playerObj.transform;
        playerCC = player.GetComponent<CharacterController>();
        playerMovement = player.GetComponent<New_CharacterController>();
        playerAnimator = player.GetComponentInChildren<Animator>();

        // Buscar modelo visual del jugador
        if (playerAnimator != null)
        {
            playerModel = playerAnimator.transform;
        }
        else if (player.childCount > 0)
        {
            playerModel = player.GetChild(0);
        }

        if (playerModel != null)
        {
            originalPlayerModelRotation = playerModel.localRotation;
        }

        // Buscar cámara
        if (cameraOverride != null)
        {
            cameraTarget = cameraOverride;
        }
        else if (playerMovement != null && playerMovement.cameraTransform != null)
        {
            cameraTarget = playerMovement.cameraTransform;
        }
        else if (Camera.main != null)
        {
            cameraTarget = Camera.main.transform;
        }

        if (cameraTarget != null)
        {
            Debug.Log($"✅ Cámara encontrada: {cameraTarget.name}");
        }
        else
        {
            Debug.LogError("❌ NO SE ENCONTRÓ CÁMARA");
        }
    }

    void Update()
    {
        if (!isMounted)
        {
            CheckMount();
        }
        else
        {
            HandleMovement();
            CheckDismount();
            CheckFall(); // NUEVO: Verificar si Maggi cayó mientras está montado
        }
    }

    void CheckMount()
    {
        if (player == null) return;

        float dist = Vector3.Distance(player.position, transform.position);
        if (dist <= mountRadius && Input.GetKeyDown(mountKey))
        {
            Mount();
        }
    }

    void Mount()
    {
        Debug.Log("🐔 Montando en Maggi...");

        // Desactivar controles del jugador
        if (playerCC != null) playerCC.enabled = false;
        if (playerMovement != null) playerMovement.enabled = false;
        if (playerAnimator != null) playerAnimator.enabled = false;

        // Hacer hijo del SeatPoint
        Transform parent = seatPoint != null ? seatPoint : transform;
        player.SetParent(parent);

        // APLICAR OFFSET DE POSICIÓN (ajustable en Inspector)
        player.localPosition = playerPositionOffset;
        player.localRotation = Quaternion.identity;

        // Alinear modelo del jugador con Maggi
        if (playerModel != null)
        {
            Vector3 maggiForward = transform.forward;
            Quaternion targetRotation = Quaternion.LookRotation(maggiForward, Vector3.up);
            targetRotation *= Quaternion.Euler(0f, playerRotationOffset, 0f);
            playerModel.rotation = targetRotation;
        }

        isMounted = true;
        SetAnimation(0f);

        Debug.Log("✅ Montado en Maggi");
    }

    void CheckDismount()
    {
        if (Input.GetKeyDown(mountKey))
        {
            Dismount();
        }
    }

    void Dismount()
    {
        player.SetParent(null);

        Vector3 dismountPos = transform.position + (transform.right * 0.5f);
        player.position = dismountPos;
        player.rotation = transform.rotation;

        if (playerModel != null)
        {
            playerModel.localRotation = originalPlayerModelRotation;
        }

        if (playerCC != null) playerCC.enabled = true;
        if (playerMovement != null) playerMovement.enabled = true;
        if (playerAnimator != null) playerAnimator.enabled = true;

        isMounted = false;
        SetAnimation(0f);

        Debug.Log("✅ Desmontado");
    }

    void HandleMovement()
    {
        if (cameraTarget == null)
        {
            Debug.LogError("❌ No hay cámara");
            return;
        }

        // ===== DETECTAR SI ESTÁ EN EL SUELO =====
        bool isGrounded = chickenController.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // ===== INPUT =====
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;
        bool isMoving = inputDirection.magnitude >= 0.1f;

        Vector3 moveDir = Vector3.zero;

        if (isMoving)
        {
            // ===== CALCULAR ÁNGULO SEGÚN LA CÁMARA =====
            float camYaw = cameraTarget.eulerAngles.y;
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + camYaw;

            // ===== APLICAR OFFSET DE ROTACIÓN A MAGGI =====
            float maggiTargetAngle = targetAngle + maggiRotationOffset;

            // ===== ROTAR SUAVEMENTE MAGGI =====
            float angle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                maggiTargetAngle,
                ref turnSmoothVelocity,
                rotationSpeed * Time.deltaTime
            );

            // ===== APLICAR ROTACIÓN A MAGGI =====
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Dirección de movimiento según el ángulo ORIGINAL (sin offset)
            moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            // ===== MANTENER AL JUGADOR ALINEADO =====
            if (playerModel != null)
            {
                Vector3 maggiForward = transform.forward;
                Quaternion modelRotation = Quaternion.LookRotation(maggiForward, Vector3.up);
                modelRotation *= Quaternion.Euler(0f, playerRotationOffset, 0f);
                playerModel.rotation = modelRotation;
            }
        }

        // ===== SALTO =====
        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            Debug.Log("🐔 ¡Maggi saltó!");
        }

        // ===== APLICAR GRAVEDAD =====
        velocity.y += gravity * Time.deltaTime;

        // ===== MOVIMIENTO FINAL =====
        Vector3 finalMovement = moveDir * walkSpeed * Time.deltaTime;
        finalMovement.y += velocity.y * Time.deltaTime;

        chickenController.Move(finalMovement);

        // ===== ANIMACIÓN =====
        SetAnimation(isMoving ? 1f : 0f);
    }

    /// <summary>
    /// NUEVO: Verifica si Maggi cayó por debajo del límite Y
    /// </summary>
    void CheckFall()
    {
        if (transform.position.y < limiteY)
        {
            Debug.LogWarning("🐔 ¡Maggi cayó! El sistema de checkpoint del jugador manejará el respawn");
            // No hacemos nada aquí - el PlayerCheckpointSystem detectará al jugador cayendo
            // y llamará a OnPlayerRespawn()
        }
    }

    /// <summary>
    /// MEJORADO: Método llamado cuando el jugador respawnea
    /// Ahora también respawnea a Maggi en su posición inicial
    /// </summary>
    public void OnPlayerRespawn()
    {
        Debug.Log("🔄 Iniciando respawn de Maggi...");

        // Si está montado, desmontar primero
        if (isMounted && player != null)
        {
            player.SetParent(null);
            if (playerModel != null) playerModel.localRotation = originalPlayerModelRotation;
            if (playerCC != null) playerCC.enabled = true;
            if (playerMovement != null) playerMovement.enabled = true;
            if (playerAnimator != null) playerAnimator.enabled = true;
        }

        isMounted = false;

        // Desactivar CharacterController de Maggi temporalmente
        chickenController.enabled = false;

        // Respawnear Maggi en su posición inicial
        transform.position = spawnPos;
        transform.rotation = spawnRot;

        // Reactivar CharacterController
        chickenController.enabled = true;

        velocity = Vector3.zero;
        SetAnimation(0f);

        Debug.Log("✅ Maggi reseteada en posición inicial");
    }

    /// <summary>
    /// NUEVO: Actualiza la posición de spawn de Maggi
    /// Úsalo cuando quieras que Maggi respawnee en una nueva ubicación
    /// </summary>
    public void ActualizarSpawnPoint(Vector3 nuevaPosicion, Quaternion nuevaRotacion)
    {
        spawnPos = nuevaPosicion;
        spawnRot = nuevaRotacion;
        Debug.Log($"🐔 Spawn de Maggi actualizado a: {nuevaPosicion}");
    }

    void SetAnimation(float value)
    {
        if (maggiAnimator != null)
        {
            maggiAnimator.SetFloat("Blend", value);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Radio de montaje
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, mountRadius);

        // Dirección de Maggi
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + Vector3.up, transform.forward * 2f);

        // Visualizar posición del jugador en el SeatPoint
        if (seatPoint != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 playerPos = seatPoint.position + seatPoint.TransformDirection(playerPositionOffset);
            Gizmos.DrawWireSphere(playerPos, 0.3f);
            Gizmos.DrawLine(seatPoint.position, playerPos);
        }
    }
}