using UnityEngine;

/// <summary>
/// Script para el jugador que maneja el sistema de respawn en checkpoints
/// OPTIMIZADO PARA CHARACTER CONTROLLER
/// </summary>
public class PlayerCheckpointSystem : MonoBehaviour
{
    [Header("Configuración de Respawn")]
    [SerializeField] private float limiteY = -10f; // Límite Y para activar respawn
    [SerializeField] private float alturaRespawn = 2f; // Altura adicional al respawnear
    [SerializeField] private float tiempoInvulnerabilidad = 1f; // Tiempo sin detectar caídas después de respawn

    private Vector3 ultimoCheckpoint;
    private bool checkpointActivo = false;
    private float tiempoUltimoRespawn;
    private CharacterController characterController;

    void Start()
    {
        // Obtener Character Controller
        characterController = GetComponent<CharacterController>();

        if (characterController == null)
        {
            Debug.LogError("¡El jugador necesita un Character Controller!");
        }

        // Guardar posición inicial como primer checkpoint
        GuardarCheckpoint(transform.position);

        tiempoUltimoRespawn = -tiempoInvulnerabilidad;
    }

    void Update()
    {
        // Verificar si el jugador cayó por debajo del límite
        if (transform.position.y < limiteY && checkpointActivo)
        {
            // Evitar múltiples respawns seguidos
            if (Time.time - tiempoUltimoRespawn > tiempoInvulnerabilidad)
            {
                Respawnear();
            }
        }
    }

    /// <summary>
    /// CHARACTER CONTROLLER usa OnControllerColliderHit en lugar de OnCollisionEnter
    /// </summary>
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Verificar si tocó una isla (que tenga el componente IslaCheckpoint)
        IslaCheckpoint isla = hit.gameObject.GetComponent<IslaCheckpoint>();

        if (isla != null)
        {
            // Verificar que está tocando desde arriba (no los lados)
            if (hit.normal.y > 0.5f) // Normal apuntando hacia arriba
            {
                isla.RegistrarJugadorEnIsla(hit.point);
            }
        }
    }

    /// <summary>
    /// Guarda una nueva posición de checkpoint
    /// </summary>
    public void GuardarCheckpoint(Vector3 nuevaPosicion)
    {
        ultimoCheckpoint = nuevaPosicion + Vector3.up * alturaRespawn;
        checkpointActivo = true;

        Debug.Log($"✓✓✓ Checkpoint guardado en: {ultimoCheckpoint} ✓✓✓");
    }

    /// <summary>
    /// Respawnea al jugador en el último checkpoint
    /// </summary>
    private void Respawnear()
    {
        // Desactivar Character Controller temporalmente
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        // Mover al checkpoint
        transform.position = ultimoCheckpoint;

        // Reactivar Character Controller
        if (characterController != null)
        {
            characterController.enabled = true;
        }

        tiempoUltimoRespawn = Time.time;

        Debug.Log($"Jugador respawneado en: {ultimoCheckpoint}");
    }

    // Método público por si necesitas forzar un respawn desde otro script
    public void ForzarRespawn()
    {
        if (checkpointActivo)
        {
            Respawnear();
        }
    }

    // Para visualizar el límite de caída en el editor
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            new Vector3(-1000, limiteY, 0),
            new Vector3(1000, limiteY, 0)
        );

        if (checkpointActivo && Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(ultimoCheckpoint, 0.5f);
        }
    }
}