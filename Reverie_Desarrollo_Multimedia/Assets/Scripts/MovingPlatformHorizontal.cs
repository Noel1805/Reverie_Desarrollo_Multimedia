using UnityEngine;
using System.Collections.Generic;

public class MovingPlatformHorizontal : MonoBehaviour
{
    public enum StartPoint { PositionA, PositionB }
    public enum HorizontalDirection
    {
        Right,      // Derecha (X+)
        Left,       // Izquierda (X-)
        Forward,    // Adelante (Z+)
        Backward    // Atrás (Z-)
    }

    [Header("Dirección Horizontal")]
    [Tooltip("Dirección del movimiento horizontal.")]
    public HorizontalDirection horizontalDirection = HorizontalDirection.Right;

    [Header("Distancia")]
    [Tooltip("Distancia total del recorrido desde A hasta B.")]
    [Min(0f)] public float distance = 5f;

    [Header("Velocidad")]
    [Tooltip("Tiempo de TRAYECTO entre A y B (y también entre B y A). No incluye esperas.")]
    [Min(0.0001f)] public float travelTime = 3f;

    [Header("Tiempos de espera")]
    [Min(0f)] public float waitAtA = 2.0f;
    [Min(0f)] public float waitAtB = 2.0f;

    [Header("Arranque")]
    public StartPoint startAt = StartPoint.PositionA;

    [Header("Movimiento con físicas (opcional)")]
    [Tooltip("Si hay Rigidbody (recomendado isKinematic=true), mover con MovePosition en FixedUpdate.")]
    public bool useRigidbody = false;

    [Header("Detección de Jugador")]
    [Tooltip("Altura del trigger detector sobre la plataforma")]
    public float detectorHeight = 0.5f;

    [Header("Método de Movimiento")]
    [Tooltip("TRUE: Hacer hijo (mejor para rotaciones). FALSE: Mover con delta (mejor para CharacterController)")]
    public bool useParenting = true; // ⭐ CAMBIADO A TRUE POR DEFECTO

    // ---- Internos ----
    private Vector3 A;
    private Vector3 B;
    private float cycle;
    private float phase;
    private Rigidbody rb;
    private Vector3 finalDirection;

    // Sistema de jugadores
    private Vector3 lastPosition;
    private Dictionary<Transform, Transform> playersOriginalParent = new Dictionary<Transform, Transform>();
    private HashSet<Transform> playersOnPlatform = new HashSet<Transform>();
    private GameObject triggerDetector;

    // ⭐ NUEVO: Para almacenar el delta de movimiento
    private Vector3 platformDelta;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        A = transform.position;
        lastPosition = transform.position;

        // Determinar dirección
        finalDirection = GetHorizontalDirection();
        finalDirection = finalDirection.normalized;
        distance = Mathf.Abs(distance);
        B = A + finalDirection * distance;

        // Construir ciclo
        cycle = waitAtA + travelTime + waitAtB + travelTime;
        if (cycle < 0.0001f) cycle = 0.0001f;

        // Inicializar phase
        switch (startAt)
        {
            case StartPoint.PositionA:
                phase = 0f;
                SetPositionImmediate(A);
                break;
            case StartPoint.PositionB:
                phase = waitAtA + travelTime;
                SetPositionImmediate(B);
                break;
        }

        // Crear detector de jugadores
        CrearTriggerDetector();
    }

    void CrearTriggerDetector()
    {
        triggerDetector = new GameObject("PlayerDetector_Horizontal");
        triggerDetector.transform.SetParent(transform);
        triggerDetector.transform.localPosition = Vector3.zero;
        triggerDetector.layer = gameObject.layer;

        BoxCollider triggerCollider = triggerDetector.AddComponent<BoxCollider>();
        triggerCollider.isTrigger = true;

        BoxCollider platformCollider = GetComponent<BoxCollider>();
        if (platformCollider != null)
        {
            triggerCollider.size = new Vector3(
                platformCollider.size.x,
                detectorHeight,
                platformCollider.size.z
            );
            triggerCollider.center = new Vector3(
                platformCollider.center.x,
                platformCollider.center.y + (platformCollider.size.y / 2) + (detectorHeight / 2),
                platformCollider.center.z
            );
        }
        else
        {
            triggerCollider.size = new Vector3(1, detectorHeight, 1);
            triggerCollider.center = new Vector3(0, detectorHeight / 2, 0);
        }

        HorizontalPlatformDetector detector = triggerDetector.AddComponent<HorizontalPlatformDetector>();
        detector.Initialize(this);
    }

    Vector3 GetHorizontalDirection()
    {
        switch (horizontalDirection)
        {
            case HorizontalDirection.Right:
                return Vector3.right;
            case HorizontalDirection.Left:
                return Vector3.left;
            case HorizontalDirection.Forward:
                return Vector3.forward;
            case HorizontalDirection.Backward:
                return Vector3.back;
            default:
                return Vector3.right;
        }
    }

    void Update()
    {
        if (useRigidbody) return;

        AvanzarLineaDeTiempo(Time.deltaTime);
        AplicarPosicion();
    }

    void FixedUpdate()
    {
        if (!useRigidbody) return;

        AvanzarLineaDeTiempo(Time.fixedDeltaTime);
        AplicarPosicion(true);
    }

    // ⭐ NUEVO: LateUpdate para mover jugadores DESPUÉS de que se hayan movido ellos mismos
    void LateUpdate()
    {
        if (!useParenting)
        {
            MoverJugadores();
        }
    }

    void AvanzarLineaDeTiempo(float dt)
    {
        phase += dt;
        if (phase >= cycle)
        {
            phase -= cycle * Mathf.Floor(phase / cycle);
        }
    }

    void AplicarPosicion(bool viaRigidbody = false)
    {
        // ⭐ Guardar posición ANTES de mover
        Vector3 posicionAnterior = transform.position;

        float p = phase;

        if (p < waitAtA)
        {
            SetPosition(A, viaRigidbody);
        }
        else
        {
            p -= waitAtA;

            if (p < travelTime)
            {
                float t = p / travelTime;
                Vector3 target = Vector3.LerpUnclamped(A, B, t);
                SetPosition(target, viaRigidbody);
            }
            else
            {
                p -= travelTime;

                if (p < waitAtB)
                {
                    SetPosition(B, viaRigidbody);
                }
                else
                {
                    p -= waitAtB;
                    float t = p / travelTime;
                    Vector3 target = Vector3.LerpUnclamped(B, A, t);
                    SetPosition(target, viaRigidbody);
                }
            }
        }

        // ⭐ Calcular delta DESPUÉS de mover
        platformDelta = transform.position - posicionAnterior;
    }

    void SetPosition(Vector3 pos, bool viaRigidbody)
    {
        if (viaRigidbody && rb != null)
        {
            rb.MovePosition(pos);
        }
        else
        {
            transform.position = pos;
        }
    }

    void SetPositionImmediate(Vector3 pos)
    {
        if (useRigidbody && rb != null && rb.isKinematic)
        {
            rb.position = pos;
        }
        else
        {
            transform.position = pos;
        }
    }

    void MoverJugadores()
    {
        // ⭐ Usar el delta calculado en AplicarPosicion
        if (platformDelta.sqrMagnitude > 0.0001f)
        {
            foreach (Transform player in playersOnPlatform)
            {
                if (player != null)
                {
                    CharacterController cc = player.GetComponent<CharacterController>();
                    if (cc != null)
                    {
                        // ⭐ Mover con CharacterController
                        cc.Move(platformDelta);
                    }
                    else
                    {
                        // ⭐ Si no tiene CharacterController, mover directamente
                        player.position += platformDelta;
                    }
                }
            }
        }

        // ⭐ Resetear delta
        platformDelta = Vector3.zero;
    }

    public void AddPlayer(Transform player)
    {
        if (playersOnPlatform.Contains(player))
            return;

        playersOnPlatform.Add(player);

        if (useParenting)
        {
            // Guardar padre original
            playersOriginalParent[player] = player.parent;

            // Hacer hijo de la plataforma
            player.SetParent(transform);

            // ⭐ NUEVO: Debug para confirmar
            Debug.Log($"Jugador {player.name} subió a la plataforma (PARENTING)");
        }
        else
        {
            Debug.Log($"Jugador {player.name} subió a la plataforma (DELTA MOVEMENT)");
        }
    }

    public void RemovePlayer(Transform player)
    {
        if (!playersOnPlatform.Contains(player))
            return;

        playersOnPlatform.Remove(player);

        if (useParenting)
        {
            // Restaurar padre original
            if (playersOriginalParent.ContainsKey(player))
            {
                player.SetParent(playersOriginalParent[player]);
                playersOriginalParent.Remove(player);
            }
            else
            {
                player.SetParent(null);
            }

            Debug.Log($"Jugador {player.name} bajó de la plataforma (PARENTING)");
        }
        else
        {
            Debug.Log($"Jugador {player.name} bajó de la plataforma (DELTA MOVEMENT)");
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 a = Application.isPlaying ? A : transform.position;
        Vector3 dir = Application.isPlaying ? finalDirection : GetHorizontalDirection();
        float d = distance;
        Vector3 b = a + dir * d;

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(a, b);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(a, 0.15f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(b, 0.15f);

        Vector3 midPoint = (a + b) / 2f;
        Gizmos.color = Color.cyan;
        DrawArrow(midPoint, dir * 0.5f);

        if (!Application.isPlaying)
        {
            BoxCollider platformCollider = GetComponent<BoxCollider>();
            if (platformCollider != null)
            {
                Gizmos.color = new Color(0, 1, 0, 0.3f);
                Vector3 detectorSize = new Vector3(
                    platformCollider.size.x,
                    detectorHeight,
                    platformCollider.size.z
                );
                Vector3 detectorCenter = transform.position + new Vector3(
                    platformCollider.center.x,
                    platformCollider.center.y + (platformCollider.size.y / 2) + (detectorHeight / 2),
                    platformCollider.center.z
                );
                Gizmos.DrawCube(detectorCenter, detectorSize);
            }
        }

#if UNITY_EDITOR
        UnityEditor.Handles.Label(midPoint + Vector3.up * 0.3f, horizontalDirection.ToString());
#endif
    }

    void DrawArrow(Vector3 pos, Vector3 direction)
    {
        Gizmos.DrawRay(pos, direction);
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + 20, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - 20, 0) * new Vector3(0, 0, 1);
        Gizmos.DrawRay(pos + direction, right * 0.3f);
        Gizmos.DrawRay(pos + direction, left * 0.3f);
    }
}

public class HorizontalPlatformDetector : MonoBehaviour
{
    private MovingPlatformHorizontal platform;

    public void Initialize(MovingPlatformHorizontal plat)
    {
        platform = plat;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            platform.AddPlayer(other.transform);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            platform.RemovePlayer(other.transform);
        }
    }
}