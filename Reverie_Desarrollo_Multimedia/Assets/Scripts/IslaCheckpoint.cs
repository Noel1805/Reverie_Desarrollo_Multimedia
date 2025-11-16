using UnityEngine;

/// <summary>
/// Script para las islas que detectan cuando el jugador las toca
/// VERSIÓN SIMPLIFICADA PARA CHARACTER CONTROLLER
/// </summary>
public class IslaCheckpoint : MonoBehaviour
{
    [Header("Configuración de Detección")]
    [SerializeField] private bool esCheckpointInicial = false;
    [SerializeField] private Color colorGizmo = Color.cyan;

    private float ultimoTiempoGuardado = 0f;
    private float cooldownGuardado = 0.3f; // Evitar spam de guardados
    private bool checkpointGuardado = false;

    /// <summary>
    /// Método llamado por el jugador cuando toca la isla
    /// </summary>
    public void RegistrarJugadorEnIsla(Vector3 puntoContacto)
    {
        // Cooldown para evitar guardar múltiples veces muy rápido
        if (Time.time - ultimoTiempoGuardado < cooldownGuardado)
        {
            return;
        }

        // Buscar el componente del jugador
        PlayerCheckpointSystem checkpointSystem = FindObjectOfType<PlayerCheckpointSystem>();

        if (checkpointSystem != null)
        {
            checkpointSystem.GuardarCheckpoint(puntoContacto);
            ultimoTiempoGuardado = Time.time;
            checkpointGuardado = true;

            Debug.Log($"✓✓✓ Isla '{gameObject.name}' registró al jugador ✓✓✓");
        }
        else
        {
            Debug.LogError("No se encontró PlayerCheckpointSystem en la escena!");
        }
    }

    // Visualización en el editor
    void OnDrawGizmos()
    {
        Gizmos.color = checkpointGuardado ? Color.green : colorGizmo;

        // Dibujar el collider aproximado
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
        else
        {
            Gizmos.DrawWireCube(transform.position, transform.localScale);
        }

        if (esCheckpointInicial)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2, 1f);
        }
    }
}