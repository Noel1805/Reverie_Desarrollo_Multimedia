using UnityEngine;

public class MovingPlatformDiagonal : MonoBehaviour
{
    public enum StartPoint { PositionA, PositionB }
    public enum DiagonalDirection
    {
        UpRight,        
        UpLeft,         
        DownRight,      
        DownLeft,       
        ForwardUp,     
        ForwardDown,    
        BackwardUp,     
        BackwardDown,   
        Custom          
    }

    [Header("Dirección Diagonal")]
    [Tooltip("Tipo de movimiento diagonal predefinido.")]
    public DiagonalDirection diagonalType = DiagonalDirection.UpRight;

    [Tooltip("Solo usado si DiagonalType = Custom. Define la dirección exacta.")]
    public Vector3 customDirection = new Vector3(1f, 1f, 0f);

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


    private Vector3 A;      
    private Vector3 B;      
    private float cycle;    
    private float phase;    
    private Rigidbody rb;
    private Vector3 finalDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();


        A = transform.position;


        finalDirection = GetDiagonalDirection();


        finalDirection = finalDirection.normalized;
        distance = Mathf.Abs(distance);
        B = A + finalDirection * distance;


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
    }


    Vector3 GetDiagonalDirection()
    {
        switch (diagonalType)
        {
            case DiagonalDirection.UpRight:
                return new Vector3(1f, 1f, 0f);     

            case DiagonalDirection.UpLeft:
                return new Vector3(-1f, 1f, 0f);   

            case DiagonalDirection.DownRight:
                return new Vector3(1f, -1f, 0f);    

            case DiagonalDirection.DownLeft:
                return new Vector3(-1f, -1f, 0f);  

            case DiagonalDirection.ForwardUp:
                return new Vector3(0f, 1f, 1f);    

            case DiagonalDirection.ForwardDown:
                return new Vector3(0f, -1f, 1f);   

            case DiagonalDirection.BackwardUp:
                return new Vector3(0f, 1f, -1f);    

            case DiagonalDirection.BackwardDown:
                return new Vector3(0f, -1f, -1f);  

            case DiagonalDirection.Custom:
                return customDirection;

            default:
                return new Vector3(1f, 1f, 0f);
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

    void OnDrawGizmosSelected()
    {

        Vector3 a = Application.isPlaying ? A : transform.position;
        Vector3 dir = Application.isPlaying ? finalDirection : GetDiagonalDirection().normalized;
        float d = distance;
        Vector3 b = a + dir * d;


        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(a, b);


        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(a, 0.15f);


        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(b, 0.15f);


        Vector3 midPoint = (a + b) / 2f;
        Gizmos.color = Color.cyan;
        DrawArrow(midPoint, dir * 0.5f);
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