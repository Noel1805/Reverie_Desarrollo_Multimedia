using UnityEngine;

/// <summary>
/// Script para el jugador que maneja el sistema de respawn en checkpoints
/// OPTIMIZADO PARA CHARACTER CONTROLLER + SISTEMA DE MONTAJE MAGGI
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
    private bool estaRespawneando = false; // Bandera para evitar múltiples respawns

    // NUEVO: Referencias para el modelo del jugador
    private Transform playerModel;
    private Quaternion modelRotacionOriginal;

    void Start()
    {
        // Obtener Character Controller
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("¡El jugador necesita un Character Controller!");
        }

        // NUEVO: Buscar el modelo visual del jugador (Player_Gri)
        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            playerModel = animator.transform;
            modelRotacionOriginal = playerModel.localRotation;
            Debug.Log($"✅ Modelo del jugador encontrado: {playerModel.name}");
        }
        else if (transform.childCount > 0)
        {
            playerModel = transform.GetChild(0);
            modelRotacionOriginal = playerModel.localRotation;
            Debug.Log($"✅ Modelo del jugador encontrado (primer hijo): {playerModel.name}");
        }

        // Guardar posición inicial como primer checkpoint
        GuardarCheckpoint(transform.position);
        tiempoUltimoRespawn = -tiempoInvulnerabilidad;
    }

    void Update()
    {
        // Solo verificar caída si NO estamos en proceso de respawn
        if (estaRespawneando) return;

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
    /// MEJORADO: Detecta si está montado en Maggi y resetea la rotación del modelo correctamente
    /// </summary>
    private void Respawnear()
    {
        // Activar bandera de respawn
        estaRespawneando = true;

        Debug.Log("🔄 Iniciando respawn...");

        // Verificar si está montado en Maggi
        bool estabaMontado = transform.parent != null;
        MountableChicken_New maggiScript = null;

        if (estabaMontado)
        {
            // Buscar el script de Maggi en el padre o arriba
            Transform current = transform.parent;
            while (current != null && maggiScript == null)
            {
                maggiScript = current.GetComponent<MountableChicken_New>();
                current = current.parent;
            }

            if (maggiScript != null)
            {
                Debug.Log("🐔 Jugador estaba montado en Maggi - Respawneando a ambos");
                // Llamar al respawn de Maggi (que incluye desmontar)
                maggiScript.OnPlayerRespawn();
            }
        }

        // Desactivar Character Controller temporalmente
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        // Mover al checkpoint
        transform.position = ultimoCheckpoint;
        transform.rotation = Quaternion.identity; // NUEVO: Resetear rotación del transform

        // NUEVO: RESTAURAR LA ROTACIÓN ORIGINAL DEL MODELO VISUAL
        if (playerModel != null)
        {
            playerModel.localRotation = modelRotacionOriginal;
            Debug.Log("✅ Rotación del modelo restaurada");
        }

        // Reactivar Character Controller
        if (characterController != null)
        {
            characterController.enabled = true;
        }

        tiempoUltimoRespawn = Time.time;

        // Quitar vida SOLO UNA VEZ por caída
        VidaKaven vidaKaven = GetComponent<VidaKaven>();
        if (vidaKaven != null && vidaKaven.EstaVivo())
        {
            vidaKaven.RecibirDano(2f, true); // Quita 1 corazón completo (2 puntos) - ignorar invulnerabilidad
            Debug.Log("💔 Se quitó 1 corazón por caída");
        }

        Debug.Log($"✅ Jugador respawneado en: {ultimoCheckpoint}");

        // Desactivar bandera después de un frame
        Invoke(nameof(DesactivarBanderaRespawn), 0.2f);
    }

    private void DesactivarBanderaRespawn()
    {
        estaRespawneando = false;
        Debug.Log("✓ Respawn completado - Sistema listo");
    }

    // Método público por si necesitas forzar un respawn desde otro script
    public void ForzarRespawn()
    {
        if (checkpointActivo && !estaRespawneando)
        {
            Respawnear();
        }
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            new Vector3(-1000, limiteY, 0),
            new Vector3(1000, limiteY, 0)
        );

        if (checkpointActivo)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(ultimoCheckpoint, 0.5f);
        }
    }
}