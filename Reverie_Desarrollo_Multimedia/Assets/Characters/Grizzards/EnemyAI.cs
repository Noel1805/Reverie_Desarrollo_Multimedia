using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Detección del Jugador")]
    [Tooltip("Radio de detección circular")]
    public float detectionRadius = 15f;
    [Tooltip("Radio de ataque")]
    public float attackRadius = 2.5f;
    [Tooltip("Tag del jugador")]
    public string playerTag = "Player";

    [Header("Comportamiento")]
    public float walkSpeed = 2f;
    public float chaseSpeed = 3.5f;
    [Tooltip("Tiempo entre ataques")]
    public float attackCooldown = 2f;

    [Header("Referencias")]
    public Animator animator;
    public Transform player;

    private NavMeshAgent agent;
    private float lastAttackTime = -999f;
    private bool isAttacking = false;

    private enum EnemyState
    {
        Idle,
        Chasing,
        Attacking
    }
    private EnemyState currentState = EnemyState.Idle;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError("❌❌❌ NO HAY NAV MESH AGENT EN " + gameObject.name + " ❌❌❌");
            Debug.LogError("SOLUCIÓN: Selecciona el enemigo → Add Component → Nav Mesh Agent");
            enabled = false;
            return;
        }

        // Buscar animator
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            if (animator != null)
                Debug.Log("✅ Animator encontrado en: " + animator.gameObject.name);
            else
                Debug.LogWarning("⚠️ No se encontró Animator");
        }

        // Buscar jugador
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("✅ Jugador encontrado: " + player.name);
            }
            else
            {
                Debug.LogError("❌ No se encontró jugador con tag: " + playerTag);
                Debug.LogError("SOLUCIÓN: Selecciona tu jugador → Tag → Player");
            }
        }

        // CONFIGURACIÓN CRÍTICA DEL NAVMESHAGENT
        if (agent != null)
        {
            agent.speed = chaseSpeed;
            agent.acceleration = 8f;
            agent.angularSpeed = 200f;

            // CRÍTICO: Stopping Distance debe ser MAYOR que Attack Radius
            agent.stoppingDistance = attackRadius + 0.5f;  // 2.5 + 0.5 = 3

            agent.autoBraking = true;
            agent.updateRotation = true;
            agent.radius = 0.3f;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

            Debug.Log("✅ NavMeshAgent configurado correctamente");
            Debug.Log($"Speed: {agent.speed} | StoppingDist: {agent.stoppingDistance} | AttackRadius: {attackRadius}");
        }

        // Verificar que está en NavMesh
        if (agent != null)
        {
            if (agent.isOnNavMesh)
            {
                Debug.Log("✅ Enemigo está EN NavMesh correctamente");
            }
            else
            {
                Debug.LogWarning("⚠️ Enemigo NO está en NavMesh, intentando reposicionar...");

                NavMeshHit hit;
                if (NavMesh.SamplePosition(transform.position, out hit, 10f, NavMesh.AllAreas))
                {
                    transform.position = hit.position;
                    Debug.Log("✅ Enemigo reposicionado en NavMesh en: " + hit.position);
                }
                else
                {
                    Debug.LogError("❌❌❌ NO SE ENCONTRÓ NAVMESH CERCA! ❌❌❌");
                    Debug.LogError("SOLUCIÓN: Bakea el NavMesh en el suelo de la isla");
                }
            }
        }
    }

    void Update()
    {
        if (player == null || agent == null) return;

        if (!agent.isOnNavMesh)
        {
            Debug.LogError("❌ Enemigo NO está en NavMesh!");
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdleState(distanceToPlayer);
                break;
            case EnemyState.Chasing:
                HandleChasingState(distanceToPlayer);
                break;
            case EnemyState.Attacking:
                HandleAttackingState(distanceToPlayer);
                break;
        }

        UpdateAnimator();
    }

    void HandleIdleState(float distance)
    {
        if (distance <= detectionRadius)
        {
            Debug.Log("🔍 JUGADOR DETECTADO A " + distance.ToString("F1") + "m - Iniciando persecución");
            currentState = EnemyState.Chasing;
            agent.isStopped = false;
        }
    }

    void HandleChasingState(float distance)
    {
        // Asegurarse que está activo
        if (agent.isStopped)
        {
            agent.isStopped = false;
        }

        // Actualizar destino constantemente
        agent.SetDestination(player.position);

        // Diagnóstico completo
        float velocity = agent.velocity.magnitude;
        NavMeshPathStatus pathStatus = agent.pathStatus;
        float remainingDistance = agent.hasPath ? agent.remainingDistance : distance;

        // Si no puede calcular path, intentar posición cercana en NavMesh
        if (pathStatus == NavMeshPathStatus.PathInvalid)
        {
            Debug.LogError("❌ NO PUEDE CALCULAR CAMINO!");

            NavMeshHit hit;
            if (NavMesh.SamplePosition(player.position, out hit, 5f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                Debug.Log("🔄 Usando posición cercana en NavMesh");
            }
            else
            {
                Debug.LogError("❌ El JUGADOR está FUERA del NavMesh!");
                Debug.LogError("SOLUCIÓN: Agrega Nav Mesh Obstacle al jugador");
            }
        }

        Debug.Log($"🏃 PERSIGUIENDO | Dist: {distance:F1}m | Vel: {velocity:F2} | Remaining: {remainingDistance:F1} | pathStatus: {pathStatus}");

        // LÓGICA MEJORADA: Verificar si está suficientemente cerca
        // Usar la distancia directa Y el remainingDistance del path
        bool closeEnough = distance <= attackRadius;
        bool pathAlmostComplete = agent.hasPath && !agent.pathPending && remainingDistance <= agent.stoppingDistance + 0.5f;

        if (closeEnough || pathAlmostComplete)
        {
            Debug.Log("⚔️ EN RANGO DE ATAQUE - DETENIENDO COMPLETAMENTE");
            currentState = EnemyState.Attacking;

            // DETENCIÓN COMPLETA Y FORZADA
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }
        else if (distance > detectionRadius)
        {
            Debug.Log("👋 Jugador fuera de rango de detección");
            currentState = EnemyState.Idle;
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    void HandleAttackingState(float distance)
    {
        // FORZAR DETENCIÓN TOTAL - Verificar en cada frame
        if (!agent.isStopped || agent.velocity.magnitude > 0.1f)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }

        // Rotar manualmente hacia el jugador (sin NavMeshAgent)
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;  // Mantener rotación horizontal solamente

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        }

        // Atacar si puede
        if (Time.time >= lastAttackTime + attackCooldown && !isAttacking)
        {
            StartAttack();
        }

        // Si el jugador se aleja significativamente, volver a perseguir
        if (distance > attackRadius + 2f)  // Histeresis amplio para evitar parpadeo
        {
            Debug.Log("🏃 Jugador se alejó mucho (" + distance.ToString("F1") + "m) - Volviendo a perseguir");
            currentState = EnemyState.Chasing;
            agent.isStopped = false;
        }
    }

    void StartAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        Debug.Log("💥 ATACANDO!");

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        else
        {
            Debug.LogWarning("⚠️ No hay Animator para reproducir animación de ataque");
        }

        // Ajusta este valor a la duración real de tu animación de ataque
        Invoke("OnAttackComplete", 1.5f);
    }

    public void OnAttackComplete()
    {
        isAttacking = false;
        Debug.Log("✅ Ataque completado");
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        // SISTEMA DE 3 ANIMACIONES
        // Speed: 0 = Idle, 0.5 = Walk, 1 = Run

        float agentSpeed = agent.velocity.magnitude;
        float speedValue = 0f;

        if (agentSpeed > 0.1f)
        {
            // Si se mueve lento (patrullando): Walk = 0.5
            if (agentSpeed < 2.5f)
            {
                speedValue = 0.5f;
            }
            // Si se mueve rápido (persiguiendo): Run = 1
            else
            {
                speedValue = 1f;
            }
        }
        // Si está quieto: Idle = 0
        else
        {
            speedValue = 0f;
        }

        animator.SetFloat("Speed", speedValue, 0.1f, Time.deltaTime);
        animator.SetBool("IsAttacking", isAttacking);
    }

    // Visualización en el editor
    void OnDrawGizmosSelected()
    {
        // Radio de detección (amarillo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Radio de ataque (rojo)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        // Stopping distance (naranja)
        if (agent != null)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, agent.stoppingDistance);
        }

        // Línea hacia el jugador
        if (player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, player.position);

            // Mostrar distancia actual
            Vector3 midPoint = (transform.position + player.position) / 2f;
            Gizmos.DrawSphere(midPoint, 0.3f);
        }

        // Dibujar el path del NavMesh
        if (Application.isPlaying && agent != null && agent.hasPath)
        {
            Gizmos.color = Color.cyan;
            Vector3[] path = agent.path.corners;

            // Línea desde enemigo al primer punto del path
            if (path.Length > 0)
            {
                Gizmos.DrawLine(transform.position, path[0]);
            }

            // Dibujar el resto del path
            for (int i = 0; i < path.Length - 1; i++)
            {
                Gizmos.DrawLine(path[i], path[i + 1]);
                Gizmos.DrawSphere(path[i], 0.15f);
            }

            // Último punto del path
            if (path.Length > 0)
            {
                Gizmos.DrawSphere(path[path.Length - 1], 0.15f);
            }
        }

        // Dibujar radio del agente
        if (agent != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Gizmos.DrawWireSphere(transform.position, agent.radius);
        }
    }

    // Métodos públicos opcionales para control externo
    public void StopEnemy()
    {
        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            currentState = EnemyState.Idle;
        }
    }

    public void ResumeEnemy()
    {
        if (agent != null && player != null)
        {
            agent.isStopped = false;
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= detectionRadius)
            {
                currentState = EnemyState.Chasing;
            }
        }
    }

    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
        Debug.Log("✅ Nuevo jugador asignado: " + player.name);
    }

    // Método para ajustar velocidades desde el Inspector
    void OnValidate()
    {
        // Asegurarse que Attack Radius sea menor que Stopping Distance
        if (agent != null)
        {
            if (attackRadius > agent.stoppingDistance)
            {
                Debug.LogWarning("⚠️ Attack Radius (" + attackRadius + ") debería ser MENOR que Stopping Distance (" + agent.stoppingDistance + ")");
            }
        }
    }
}
