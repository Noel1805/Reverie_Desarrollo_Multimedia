using UnityEngine;
using System.Collections;

public class PowerUp_Pera : MonoBehaviour
{
    [Header("Configuración de la Pera")]
    [SerializeField] private float duracionInvulnerabilidad = 15f;
    [SerializeField] private KeyCode teclaRecoger = KeyCode.E;
    [SerializeField] private float distanciaRecoger = 2f;

    [Header("Aura de Escudo (Opcional)")]
    [Tooltip("Prefab del aura (un círculo / esfera de escudo) que se instancia alrededor del jugador.")]
    [SerializeField] private GameObject auraEscudoPrefab;
    [SerializeField] private Vector3 offsetAura = Vector3.zero;

    [Header("Efectos Visuales (Opcional)")]
    [SerializeField] private GameObject efectoRecoger;
    [SerializeField] private AudioClip sonidoRecoger;

    [Header("Respawn (Opcional)")]
    [SerializeField] private bool respawnear = true;
    [SerializeField] private float tiempoRespawn = 45f;

    [Header("Debug")]
    [SerializeField] private bool mostrarDebug = true;

    private GameObject jugador;
    private VidaKaven vidaKaven;
    private bool estaActivo = true;

    private MeshRenderer[] meshRenderers;
    private Collider[] colliders;
    private AudioSource audioSource;

    void Start()
    {
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        colliders = GetComponentsInChildren<Collider>();

        jugador = GameObject.FindGameObjectWithTag("Player");
        if (jugador != null)
        {
            vidaKaven = jugador.GetComponent<VidaKaven>();
            if (vidaKaven == null)
            {
                vidaKaven = jugador.GetComponentInChildren<VidaKaven>();
            }
        }

        if (mostrarDebug)
        {
            Debug.Log($"[Pera START] ✓ Inicializado en {gameObject.name}");
            Debug.Log($"[Pera START] Jugador: {(jugador != null ? jugador.name : "NO ENCONTRADO")}");
            Debug.Log($"[Pera START] VidaKaven: {(vidaKaven != null ? "ENCONTRADO" : "NO ENCONTRADO")}");
            Debug.Log($"[Pera START] Distancia recogida: {distanciaRecoger}m");
            Debug.Log($"[Pera START] Renderers: {meshRenderers.Length}, Colliders: {colliders.Length}");
        }

        if (jugador == null)
        {
            Debug.LogError("[Pera START] ❌ No se encontró ningún objeto con tag 'Player'!");
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && sonidoRecoger != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Update()
    {
        if (!estaActivo || jugador == null)
            return;

        Vector3 centroPera = transform.position;

        float distancia = Vector3.Distance(centroPera, jugador.transform.position);

        if (mostrarDebug && Time.frameCount % 60 == 0)
        {
            Debug.Log($"[Pera] Distancia al jugador: {distancia:F2}m (Necesita ≤ {distanciaRecoger}m)");
        }

        if (distancia <= distanciaRecoger)
        {
            if (mostrarDebug && Time.frameCount % 30 == 0)
            {
                Debug.Log($"[Pera] 🛡 En rango ({distancia:F2}m). Presiona {teclaRecoger} para obtener ESCUDO");
            }

            if (Input.GetKeyDown(teclaRecoger))
            {
                if (mostrarDebug)
                    Debug.Log($"[Pera] ✓ Tecla {teclaRecoger} detectada. Activando escudo.");

                RecogerPera();
            }
        }
    }

    void RecogerPera()
    {
        if (vidaKaven != null && vidaKaven.EstaVivo())
        {
            vidaKaven.ActivarInvulnerabilidadTemporal(duracionInvulnerabilidad);

            if (mostrarDebug)
                Debug.Log($"[Pera] 🛡 Invulnerabilidad aplicada por {duracionInvulnerabilidad} segundos.");
        }
        else
        {
            if (mostrarDebug)
                Debug.LogWarning("[Pera] No se pudo activar invulnerabilidad (VidaKaven nulo o muerto).");
        }

        if (auraEscudoPrefab != null && jugador != null)
        {
            GameObject aura = Instantiate(
                auraEscudoPrefab,
                jugador.transform.position + offsetAura,
                Quaternion.identity
            );

            aura.transform.SetParent(jugador.transform, true);

            Destroy(aura, duracionInvulnerabilidad);

            if (mostrarDebug)
                Debug.Log("[Pera] ✨ Aura de escudo instanciada y parentada al jugador.");
        }

        ConsumirYRespawnear();
    }

    void ConsumirYRespawnear()
    {
        if (efectoRecoger != null)
        {
            Instantiate(efectoRecoger, transform.position, Quaternion.identity);
        }

        if (sonidoRecoger != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoRecoger);
        }


        GetComponent<FruitCollectNotifier>()?.NotificarRecoleccion();

        OcultarPera();

        if (respawnear)
        {
            Invoke(nameof(ReaparecerPera), tiempoRespawn);
            if (mostrarDebug)
                Debug.Log($"[Pera] ⏰ Reaparecerá en {tiempoRespawn} segundos");
        }
    }

    void OcultarPera()
    {
        estaActivo = false;

        if (meshRenderers != null)
        {
            foreach (var mr in meshRenderers)
            {
                if (mr != null) mr.enabled = false;
            }
        }

        if (colliders != null)
        {
            foreach (var c in colliders)
            {
                if (c != null) c.enabled = false;
            }
        }

        if (mostrarDebug)
            Debug.Log("[Pera] 👻 Pera oculta (todos los meshes y colliders)");
    }

    void ReaparecerPera()
    {
        estaActivo = true;

        if (meshRenderers != null)
        {
            foreach (var mr in meshRenderers)
            {
                if (mr != null) mr.enabled = true;
            }
        }

        if (colliders != null)
        {
            foreach (var c in colliders)
            {
                if (c != null) c.enabled = true;
            }
        }

        if (mostrarDebug)
            Debug.Log("[Pera] ✨ Power-up de Pera ha reaparecido");
    }

    void OnDrawGizmosSelected()
    {
        Vector3 centro = transform.position;

        Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.25f);
        Gizmos.DrawSphere(centro, distanciaRecoger);

        Gizmos.color = new Color(0.3f, 0.8f, 1f, 1f);
        Gizmos.DrawWireSphere(centro, distanciaRecoger);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(centro, centro + Vector3.up * 2f);

        Gizmos.color = Color.white;
        Gizmos.DrawSphere(centro, 0.1f);
    }
}