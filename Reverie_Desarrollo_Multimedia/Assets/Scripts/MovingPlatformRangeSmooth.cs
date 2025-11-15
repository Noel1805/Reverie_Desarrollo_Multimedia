using UnityEngine;

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
    [Min(0f)] public float waitAtA = 2.0f; // pausa en A
    [Min(0f)] public float waitAtB = 2.0f; // pausa en B

    [Header("Arranque")]
    public StartPoint startAt = StartPoint.PositionA;

    [Header("Movimiento con físicas (opcional)")]
    [Tooltip("Si hay Rigidbody (recomendado isKinematic=true), mover con MovePosition en FixedUpdate.")]
    public bool useRigidbody = false;

    // ---- Internos ----
    private Vector3 A;      // posición base (Editor)
    private Vector3 B;      // A + dir * (distance - 1f)
    private float cycle;  // duración total del ciclo: A_wait + A->B + B_wait + B->A

    // Línea de tiempo acumulada (siempre crece, se pliega con % cycle)
    private float phase;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Capturar A una sola vez al iniciar Play
        A = transform.position;

        // Normalizar dirección y calcular B UNA sola vez
        direction = (direction.sqrMagnitude < 1e-6f) ? Vector3.up : direction.normalized;
        distance = Mathf.Abs(distance);

        // 🔹 Recorrer 1 unidad menos
        float effectiveDistance = Mathf.Max(0f, distance - 1f);
        B = A + direction * effectiveDistance;

        // Construir ciclo (espera A + viaje A->B + espera B + viaje B->A)
        cycle = waitAtA + travelTime + waitAtB + travelTime;
        if (cycle < 0.0001f) cycle = 0.0001f;

        // Inicializar phase según arranque
        switch (startAt)
        {
            case StartPoint.PositionA:
                phase = 0f; // empieza en A (dentro de la primera fase de espera en A)
                SetPositionImmediate(A);
                break;
            case StartPoint.PositionB:
                phase = waitAtA + travelTime; // justo al inicio de la espera en B
                SetPositionImmediate(B);
                break;
        }
    }

    void Update()
    {
        if (useRigidbody) return; // si usamos físicas, movemos en FixedUpdate

        AvanzarLineaDeTiempo(Time.deltaTime);
        AplicarPosicion();
    }

    void FixedUpdate()
    {
        if (!useRigidbody) return;

        // Para físicas, usar el delta fijo
        AvanzarLineaDeTiempo(Time.fixedDeltaTime);
        AplicarPosicion(true);
    }

    // --------- Núcleo: timeline sin coroutines ni resets ---------

    void AvanzarLineaDeTiempo(float dt)
    {
        // Avanza fase y dóblala dentro del ciclo
        phase += dt;
        if (phase >= cycle)
        {
            // equivalente a phase = phase % cycle, pero más estable para valores pequeños
            phase -= cycle * Mathf.Floor(phase / cycle);
        }
    }

    void AplicarPosicion(bool viaRigidbody = false)
    {
        // Fases:
        // [0            , waitAtA)                 => pausa en A
        // [waitAtA      , waitAtA+travelTime)      => mov A->B
        // [waitAtA+travelTime, waitAtA+travelTime+waitAtB) => pausa en B
        // [waitAtA+travelTime+waitAtB, cycle)      => mov B->A

        float p = phase; // fase actual 0..cycle

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
            float t = p / travelTime; // 0..1
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
            float t = p / travelTime; // 0..1
            Vector3 target = Vector3.LerpUnclamped(B, A, t);
            SetPosition(target, viaRigidbody);
        }
    }

    // --------- Setters de posición seguros ---------

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

    // --------- Gizmos ---------
    void OnDrawGizmosSelected()
    {
        // En Editor (sin Play): A es la posición actual
        Vector3 a = Application.isPlaying ? A : transform.position;
        Vector3 dir = (direction.sqrMagnitude < 1e-6f) ? Vector3.up : direction.normalized;
        float d = Mathf.Max(0f, distance - 1f); // 🔹 mismo recorrido 1f menos
        Vector3 b = a + dir * d;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(a, b);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(a, 0.12f);
        Gizmos.DrawWireSphere(b, 0.12f);
    }


}
