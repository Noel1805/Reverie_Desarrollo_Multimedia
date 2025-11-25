using UnityEngine;

public class ParedActivable : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private bool empiezaActiva = false;
    [SerializeField] private Color colorInactivo = new Color(0f, 1f, 0f, 0.3f); 
    [SerializeField] private Color colorActivo = new Color(1f, 0f, 0f, 0.5f);   

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

        colliders = GetComponents<Collider>();
        if (colliders.Length == 0)
        {
            colliders = GetComponentsInChildren<Collider>();
        }


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


        foreach (var col in colliders)
        {
            if (col != null)
            {
                col.enabled = true;
            }
        }


        if (efectoActivacion != null)
        {
            Instantiate(efectoActivacion, transform.position, Quaternion.identity);
        }


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


        foreach (var col in colliders)
        {
            if (col != null)
            {
                col.enabled = false;
            }
        }


        if (efectoDesactivacion != null)
        {
            Instantiate(efectoDesactivacion, transform.position, Quaternion.identity);
        }


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

        if (Application.isPlaying) return;

        Collider c = GetComponent<Collider>();
        if (c == null) return;


        Gizmos.color = empiezaActiva ? colorActivo : colorInactivo;

        if (c is BoxCollider box)
        {
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.DrawWireCube(box.center, box.size);
            Gizmos.matrix = oldMatrix;
        }
    }

    void OnDrawGizmosSelected()
    {

        if (Application.isPlaying) return;

        Collider c = GetComponent<Collider>();
        if (c == null) return;

        Gizmos.color = Color.yellow;

        if (c is BoxCollider box)
        {
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.center, box.size);
            Gizmos.matrix = oldMatrix;
        }
    }
}