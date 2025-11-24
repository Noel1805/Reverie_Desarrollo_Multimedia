using UnityEngine;

public class ParedActivable : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private bool empiezaActiva = false;
    [SerializeField] private Color colorInactivo = new Color(0f, 1f, 0f, 0.3f); // Verde
    [SerializeField] private Color colorActivo = new Color(1f, 0f, 0f, 0.5f);   // Rojo

    [Header("Efectos Visuales (Opcional)")]
    [SerializeField] private GameObject efectoActivacion;
    [SerializeField] private GameObject efectoDesactivacion;
    [SerializeField] private AudioClip sonidoActivacion;
    [SerializeField] private AudioClip sonidoDesactivacion;

    [Header("Debug")]
    [SerializeField] private bool mostrarDebug = true;

    private Collider[] colliders;
    private AudioSource audioSource;
    private bool estaActiva;

    void Start()
    {
        // Obtener todos los colliders (por si hay múltiples)
        colliders = GetComponents<Collider>();
        if (colliders.Length == 0)
        {
            colliders = GetComponentsInChildren<Collider>();
        }

        // Asegurar que no tenga mesh renderer visible
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (sonidoActivacion != null || sonidoDesactivacion != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Estado inicial
        if (empiezaActiva)
        {
            ActivarInmediato();
        }
        else
        {
            DesactivarInmediato();
        }
    }

    public void Activar()
    {
        if (estaActiva) return;

        estaActiva = true;

        // Activar colliders
        foreach (var col in colliders)
        {
            if (col != null)
            {
                col.enabled = true;
            }
        }

        // Efectos visuales
        if (efectoActivacion != null)
        {
            Instantiate(efectoActivacion, transform.position, Quaternion.identity);
        }

        // Sonido
        if (sonidoActivacion != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoActivacion);
        }

        if (mostrarDebug)
        {
            Debug.Log($"[Pared] {gameObject.name} ACTIVADA ❌");
        }
    }

    public void Desactivar()
    {
        if (!estaActiva) return;

        estaActiva = false;

        // Desactivar colliders
        foreach (var col in colliders)
        {
            if (col != null)
            {
                col.enabled = false;
            }
        }

        // Efectos visuales
        if (efectoDesactivacion != null)
        {
            Instantiate(efectoDesactivacion, transform.position, Quaternion.identity);
        }

        // Sonido
        if (sonidoDesactivacion != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoDesactivacion);
        }

        if (mostrarDebug)
        {
            Debug.Log($"[Pared] {gameObject.name} DESACTIVADA ✅");
        }
    }

    private void ActivarInmediato()
    {
        estaActiva = true;
        foreach (var col in colliders)
        {
            if (col != null) col.enabled = true;
        }
    }

    private void DesactivarInmediato()
    {
        estaActiva = false;
        foreach (var col in colliders)
        {
            if (col != null) col.enabled = false;
        }
    }

    public bool EstaActiva()
    {
        return estaActiva;
    }

    void OnDrawGizmos()
    {
        Collider c = GetComponent<Collider>();
        if (c == null) return;

        // Color según estado (en play mode)
        if (Application.isPlaying)
        {
            Gizmos.color = estaActiva ? colorActivo : colorInactivo;
        }
        else
        {
            // En editor, mostrar según configuración inicial
            Gizmos.color = empiezaActiva ? colorActivo : colorInactivo;
        }

        if (c is BoxCollider)
        {
            BoxCollider box = c as BoxCollider;
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.DrawWireCube(box.center, box.size);
            Gizmos.matrix = oldMatrix;
        }
    }

    void OnDrawGizmosSelected()
    {
        Collider c = GetComponent<Collider>();
        if (c == null) return;

        Gizmos.color = Color.yellow;

        if (c is BoxCollider)
        {
            BoxCollider box = c as BoxCollider;
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.center, box.size);
            Gizmos.matrix = oldMatrix;
        }
    }
}