using UnityEngine;
using System.Collections.Generic;

public class MovingPlatformRangeStable : MonoBehaviour
{
    public enum StartPoint { PositionA, PositionB }

    [Header("Dirección y Distancia")]
    [Tooltip("Vector de movimiento. (0,1,0) para arriba.")]
    public Vector3 direction = Vector3.up;

    [Tooltip("Distancia total desde la posición inicial del Editor hasta el punto B.")]
    [Min(0f)] public float distance = 2f;

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

    // ---- Internos ----
    private Vector3 A;
    private Vector3 B;
    private float cycle;
    private float phase;
    private Rigidbody rb;

    // Para mover al jugador con la plataforma
    private Vector3 lastPosition;
    private HashSet<Transform> playersOnPlatform = new HashSet<Transform>();
    private GameObject triggerDetector;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        A = transform.position;
        lastPosition = transform.position;

        direction = (direction.sqrMagnitude < 1e-6f) ? Vector3.up : direction.normalized;
        distance = Mathf.Abs(distance);

        float effectiveDistance = Mathf.Max(0f, distance - 1f);
        B = A + direction * effectiveDistance;

        cycle = waitAtA + travelTime + waitAtB + travelTime;
        if (cycle < 0.0001f) cycle = 0.0001f;

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

        // Crear trigger detector automáticamente
        CrearTriggerDetector();
    }

    void CrearTriggerDetector()
    {
        // Crear un GameObject hijo para el trigger
        triggerDetector = new GameObject("PlayerDetector");
        triggerDetector.transform.SetParent(transform);
        triggerDetector.transform.localPosition = Vector3.zero;
        triggerDetector.layer = gameObject.layer;

        // Añadir BoxCollider como trigger
        BoxCollider triggerCollider = triggerDetector.AddComponent<BoxCollider>();
        triggerCollider.isTrigger = true;

        // Copiar el tamaño del collider de la plataforma
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
            // Tamaño por defecto si no hay collider
            triggerCollider.size = new Vector3(1, detectorHeight, 1);
            triggerCollider.center = new Vector3(0, detectorHeight / 2, 0);
        }

        // Añadir el componente detector
        triggerDetector.AddComponent<PlatformPlayerDetector>().Initialize(this);
    }

    void Update()
    {
        if (useRigidbody) return;

        AvanzarLineaDeTiempo(Time.deltaTime);
        AplicarPosicion();
        MoverJugadores();
    }

    void FixedUpdate()
    {
        if (!useRigidbody) return;

        AvanzarLineaDeTiempo(Time.fixedDeltaTime);
        AplicarPosicion(true);
        MoverJugadores();
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
        float p = phase;

        if (p < waitAtA)
        {
            SetPosition(A, viaRigidbody);
            return;
        }
        p -= waitAtA;

        if (p < travelTime)
        {
            float t = p / travelTime;
            Vector3 target = Vector3.LerpUnclamped(A, B, t);
            SetPosition(target, viaRigidbody);
            return;
        }
        p -= travelTime;

        if (p < waitAtB)
        {
            SetPosition(B, viaRigidbody);
            return;
        }
        p -= waitAtB;

        {
            float t = p / travelTime;
            Vector3 target = Vector3.LerpUnclamped(B, A, t);
            SetPosition(target, viaRigidbody);
        }
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
        Vector3 platformDelta = transform.position - lastPosition;

        if (platformDelta.sqrMagnitude > 0.0001f)
        {
            foreach (Transform player in playersOnPlatform)
            {
                if (player != null)
                {
                    CharacterController cc = player.GetComponent<CharacterController>();
                    if (cc != null)
                    {
                        cc.Move(platformDelta);
                    }
                    else
                    {
                        player.position += platformDelta;
                    }
                }
            }
        }

        lastPosition = transform.position;
    }

    public void AddPlayer(Transform player)
    {
        playersOnPlatform.Add(player);
    }

    public void RemovePlayer(Transform player)
    {
        playersOnPlatform.Remove(player);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 a = Application.isPlaying ? A : transform.position;
        Vector3 dir = (direction.sqrMagnitude < 1e-6f) ? Vector3.up : direction.normalized;
        float d = Mathf.Max(0f, distance - 1f);
        Vector3 b = a + dir * d;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(a, b);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(a, 0.12f);
        Gizmos.DrawWireSphere(b, 0.12f);

        // Dibujar área del detector
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
    }
}

// Componente auxiliar para detectar jugadores con trigger
public class PlatformPlayerDetector : MonoBehaviour
{
    private MovingPlatformRangeStable platform;

    public void Initialize(MovingPlatformRangeStable plat)
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