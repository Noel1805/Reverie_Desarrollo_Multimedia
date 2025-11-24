using UnityEngine;

// Script para CameraTarget - Rota automáticamente con el movimiento del jugador
public class CameraFollowPlayer : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player; // Arrastra aquí al Player

    [Header("Seguimiento")]
    public float followSpeed = 10f;
    public Vector3 offset = new Vector3(0, 2, 0); // Altura sobre el jugador

    [Header("Rotación Automática")]
    public bool autoRotate = true;
    public float rotationSpeed = 3f;

    private Vector3 lastPlayerPosition;
    private float idleTime = 0f;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        if (player != null)
        {
            lastPlayerPosition = player.position;
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Seguir posición del jugador
        Vector3 targetPosition = player.position + offset;
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );

        // Rotar basándose en el movimiento del jugador
        if (autoRotate)
        {
            Vector3 playerMovement = player.position - lastPlayerPosition;
            playerMovement.y = 0; // Ignorar movimiento vertical

            // Si el jugador se está moviendo
            if (playerMovement.magnitude > 0.01f)
            {
                idleTime = 0f;

                // Calcular dirección de movimiento
                Vector3 direction = playerMovement.normalized;

                // Crear rotación objetivo mirando en esa dirección
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                // Rotar suavemente hacia esa dirección
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
            else
            {
                // Si el jugador está quieto, rotar hacia donde mira
                idleTime += Time.deltaTime;

                if (idleTime > 0.3f) // Pequeño delay antes de rotar cuando está quieto
                {
                    Vector3 playerForward = player.forward;
                    playerForward.y = 0;

                    if (playerForward.magnitude > 0.1f)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(playerForward);
                        transform.rotation = Quaternion.Slerp(
                            transform.rotation,
                            targetRotation,
                            rotationSpeed * 0.5f * Time.deltaTime
                        );
                    }
                }
            }

            lastPlayerPosition = player.position;
        }
    }
}