using UnityEngine;

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
    [Min(0f)] public float waitAtA = 2.0f; // pausa en A
    [Min(0f)] public float waitAtB = 2.0f; // pausa en B

    [Header("Arranque")]
    public StartPoint startAt = StartPoint.PositionA;

    [Header("Movimiento con físicas (opcional)")]
    [Tooltip("Si hay Rigidbody (recomendado isKinematic=true), mover con MovePosition en FixedUpdate.")]
    public bool useRigidbody = false;

    // ---- Internos ----
    private Vector3 A;      // posición base (Editor)
    private Vector3 B;      // A + dirección horizontal * distance
    private float cycle;    // duración total del ciclo
    private float phase;    // línea de tiempo acumulada
    private Rigidbody rb;
    private Vector3 finalDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Capturar A al iniciar
        A = transform.position;

        // Determinar la dirección según el tipo seleccionado
        finalDirection = GetHorizontalDirection();

        // Normalizar y calcular B
        finalDirection = finalDirection.normalized;
        distance = Mathf.Abs(distance);
        B = A + finalDirection * distance;

        // Construir ciclo
        cycle = waitAtA + travelTime + waitAtB + travelTime;
        if (cycle < 0.0001f) cycle = 0.0001f;

        // Inicializar phase según arranque
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
    }

    // Obtener la dirección horizontal según el tipo seleccionado
    Vector3 GetHorizontalDirection()
    {
        switch (horizontalDirection)
        {
            case HorizontalDirection.Right:
                return Vector3.right;       // (1, 0, 0)

            case HorizontalDirection.Left:
                return Vector3.left;        // (-1, 0, 0)

            case HorizontalDirection.Forward:
                return Vector3.forward;     // (0, 0, 1)

            case HorizontalDirection.Backward:
                return Vector3.back;        // (0, 0, -1)

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
            // Pausa en A
            SetPosition(A, viaRigidbody);
            return;
        }
        p -= waitAtA;

        if (p < travelTime)
        {
            // Movimiento A -> B
            float t = p / travelTime;
            Vector3 target = Vector3.LerpUnclamped(A, B, t);
            SetPosition(target, viaRigidbody);
            return;
        }
        p -= travelTime;

        if (p < waitAtB)
        {
            // Pausa en B
            SetPosition(B, viaRigidbody);
            return;
        }
        p -= waitAtB;

        // Movimiento B -> A
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

    void OnDrawGizmosSelected()
    {
        // En Editor: mostrar la trayectoria horizontal
        Vector3 a = Application.isPlaying ? A : transform.position;
        Vector3 dir = Application.isPlaying ? finalDirection : GetHorizontalDirection();
        float d = distance;
        Vector3 b = a + dir * d;

        // Línea de trayectoria
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(a, b);

        // Punto A (inicio)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(a, 0.15f);

        // Punto B (final)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(b, 0.15f);

        // Flecha para indicar dirección
        Vector3 midPoint = (a + b) / 2f;
        Gizmos.color = Color.cyan;
        DrawArrow(midPoint, dir * 0.5f);

        // Etiqueta de dirección
#if UNITY_EDITOR
        UnityEditor.Handles.Label(midPoint + Vector3.up * 0.3f, horizontalDirection.ToString());
#endif
    }

    // Dibujar una flecha en los Gizmos
    void DrawArrow(Vector3 pos, Vector3 direction)
    {
        Gizmos.DrawRay(pos, direction);
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + 20, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - 20, 0) * new Vector3(0, 0, 1);
        Gizmos.DrawRay(pos + direction, right * 0.3f);
        Gizmos.DrawRay(pos + direction, left * 0.3f);
    }
}