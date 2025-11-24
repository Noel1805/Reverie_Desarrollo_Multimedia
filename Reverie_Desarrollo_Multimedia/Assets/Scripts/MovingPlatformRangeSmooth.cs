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
    [Min(0f)] public float waitAtA = 2.0f;
    [Min(0f)] public float waitAtB = 2.0f;

    [Header("Arranque")]
    public StartPoint startAt = StartPoint.PositionA;

    [Header("Movimiento con físicas (opcional)")]
    [Tooltip("Si hay Rigidbody (recomendado isKinematic=true), mover con MovePosition.")]
    public bool useRigidbody = false;

    // ---- Internos ----
    private Vector3 A;
    private Vector3 B;
    private float cycle;
    private float phase;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null && useRigidbody)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        A = transform.position;

        // Normalizar dirección, y si viene cero, usar arriba
        direction = (direction.sqrMagnitude < 1e-6f) ? Vector3.up : direction.normalized;
        distance = Mathf.Abs(distance);

        float effectiveDistance = Mathf.Max(0f, distance - 1f);
        B = A + direction * effectiveDistance;

        // Construcción del ciclo
        cycle = waitAtA + travelTime + waitAtB + travelTime;
        if (cycle < 0.0001f) cycle = 0.0001f;

        // Posición inicial según StartPoint
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

    void Update()
    {
        AvanzarLineaDeTiempo(Time.deltaTime);
        AplicarPosicion();
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
        if (useRigidbody && rb != null)
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
        Vector3 a = Application.isPlaying ? A : transform.position;
        Vector3 dir = (direction.sqrMagnitude < 1e-6f) ? Vector3.up : direction.normalized;
        float d = Mathf.Max(0f, distance - 1f);
        Vector3 b = a + dir * d;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(a, b);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(a, 0.12f);
        Gizmos.DrawWireSphere(b, 0.12f);
    }
}
