using UnityEngine;
using System.Collections;

public class PowerUp_Mora : MonoBehaviour
{
    [Header("Configuración de la Mora")]
    [SerializeField] private float cantidadPorTick = 1f; // 0.5 corazón (1 punto de vida)
    [SerializeField] private int numeroTicks = 2;        // 2 ticks -> 1 corazón total
    [SerializeField] private float intervaloEntreTicks = 2f; // cada 2 segundos
    [SerializeField] private KeyCode teclaRecoger = KeyCode.E;
    [SerializeField] private float distanciaRecoger = 2f;

    [Header("Efectos Visuales (Opcional)")]
    [SerializeField] private GameObject efectoRecoger;
    [SerializeField] private AudioClip sonidoRecoger;

    [Header("Respawn (Opcional)")]
    [SerializeField] private bool respawnear = true;
    [SerializeField] private float tiempoRespawn = 30f;

    [Header("Debug")]
    [SerializeField] private bool mostrarDebug = true;

    private GameObject jugador;
    private VidaKaven vidaKaven;
    private bool estaActivo = true;

    // Para ocultar completamente la mora
    private MeshRenderer[] meshRenderers;
    private Collider[] colliders;
    private AudioSource audioSource;

    void Start()
    {
        // Buscar TODOS los renderers y colliders de la mora (root + hijos)
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        colliders = GetComponentsInChildren<Collider>();

        // Buscar al jugador por tag y su script de vida
        jugador = GameObject.FindGameObjectWithTag("Player");

        if (jugador != null)
        {
            vidaKaven = jugador.GetComponent<VidaKaven>();
            if (vidaKaven == null)
            {
                // Por si el script está en un hijo
                vidaKaven = jugador.GetComponentInChildren<VidaKaven>();
            }
        }

        if (mostrarDebug)
        {
            Debug.Log($"[Mora START] ✓ Inicializado en {gameObject.name}");
            Debug.Log($"[Mora START] Jugador encontrado: {(jugador != null ? jugador.name : "NO ENCONTRADO")}");
            Debug.Log($"[Mora START] VidaKaven: {(vidaKaven != null ? "ENCONTRADO" : "NO ENCONTRADO")}");
            Debug.Log($"[Mora START] Distancia de recolección: {distanciaRecoger}m");
            Debug.Log($"[Mora START] MeshRenderers encontrados: {meshRenderers.Length}");
            Debug.Log($"[Mora START] Colliders encontrados: {colliders.Length}");
        }

        if (jugador == null)
        {
            Debug.LogError("[Mora START] ❌ No se encontró ningún objeto con tag 'Player'!");
        }

        // Crear / obtener AudioSource si hace falta
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

        // Centro de la mora usando el primer collider si existe
        Vector3 centroMora = transform.position;
        if (colliders != null && colliders.Length > 0 && colliders[0] != null)
        {
            centroMora = colliders[0].bounds.center;
        }

        float distancia = Vector3.Distance(centroMora, jugador.transform.position);

        if (mostrarDebug && Time.frameCount % 60 == 0)
        {
            Debug.Log($"[Mora] Distancia al jugador: {distancia:F2}m (Necesita: ≤{distanciaRecoger}m)");
        }

        if (distancia <= distanciaRecoger)
        {
            if (mostrarDebug && Time.frameCount % 30 == 0)
            {
                Debug.Log($"[Mora] 🟣 En rango ({distancia:F2}m) - Presiona {teclaRecoger} para comer la mora");
            }

            if (Input.GetKeyDown(teclaRecoger))
            {
                if (mostrarDebug)
                    Debug.Log($"[Mora] ✓ Tecla {teclaRecoger} detectada! Distancia exacta: {distancia:F2}m");

                RecogerMora();
            }
        }
    }

    void RecogerMora()
    {
        if (mostrarDebug)
            Debug.Log("[Mora RECOGER] Intentando aplicar curación progresiva...");

        if (vidaKaven != null && vidaKaven.EstaVivo())
        {
            StartCoroutine(CurarProgresivamente());
        }
        else
        {
            if (mostrarDebug)
                Debug.LogWarning("[Mora RECOGER] No se encontró VidaKaven o el jugador no está vivo. Solo se consumirá la mora.");
        }

        ConsumirYRespawnear();
    }

    private IEnumerator CurarProgresivamente()
    {
        if (mostrarDebug)
        {
            Debug.Log($"[Mora CURAR] Iniciando curación progresiva: " +
                      $"{numeroTicks} ticks x {cantidadPorTick} vida cada {intervaloEntreTicks}s");
        }

        for (int i = 0; i < numeroTicks; i++)
        {
            if (vidaKaven == null || !vidaKaven.EstaVivo())
            {
                if (mostrarDebug)
                    Debug.Log("[Mora CURAR] Se detiene curación: VidaKaven es null o Kaven está muerto.");
                yield break;
            }

            // Curar medio corazón (1 punto de vida)
            vidaKaven.Curar(cantidadPorTick);

            if (mostrarDebug)
            {
                Debug.Log($"[Mora CURAR] Tick {i + 1}/{numeroTicks}: Curado {cantidadPorTick} vida.");
            }

            // Esperar entre ticks, menos después del último
            if (i < numeroTicks - 1 && intervaloEntreTicks > 0f)
                yield return new WaitForSeconds(intervaloEntreTicks);
        }

        if (mostrarDebug)
            Debug.Log("[Mora CURAR] Curación progresiva completada ✅");
    }

    void ConsumirYRespawnear()
    {
        // Efectos visuales y sonido
        if (efectoRecoger != null)
        {
            Instantiate(efectoRecoger, transform.position, Quaternion.identity);
        }

        if (sonidoRecoger != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoRecoger);
        }

        OcultarMora();

        if (respawnear)
        {
            Invoke(nameof(ReaparecerMora), tiempoRespawn);
            if (mostrarDebug)
                Debug.Log($"[Mora] ⏰ Reaparecerá en {tiempoRespawn} segundos");
        }
    }

    void OcultarMora()
    {
        estaActivo = false;

        if (meshRenderers != null)
        {
            foreach (var mr in meshRenderers)
            {
                if (mr != null)
                    mr.enabled = false;
            }
        }

        if (colliders != null)
        {
            foreach (var c in colliders)
            {
                if (c != null)
                    c.enabled = false;
            }
        }

        if (mostrarDebug)
            Debug.Log("[Mora] 👻 Mora ocultada (todos los meshes y colliders)");
    }

    void ReaparecerMora()
    {
        estaActivo = true;

        if (meshRenderers != null)
        {
            foreach (var mr in meshRenderers)
            {
                if (mr != null)
                    mr.enabled = true;
            }
        }

        if (colliders != null)
        {
            foreach (var c in colliders)
            {
                if (c != null)
                    c.enabled = true;
            }
        }

        if (mostrarDebug)
            Debug.Log("[Mora] ✨ Power-up de mora ha reaparecido");
    }

    // Gizmos solo en Scene View
    void OnDrawGizmosSelected()
    {
        // Centro usando primer collider si existe
        Collider c = GetComponentInChildren<Collider>();
        Vector3 centro = c != null ? c.bounds.center : transform.position;

        // Área de recolección
        Gizmos.color = new Color(0.5f, 0, 1f, 0.25f); // moradito translúcido
        Gizmos.DrawSphere(centro, distanciaRecoger);

        Gizmos.color = new Color(0.7f, 0.2f, 1f, 1f);
        Gizmos.DrawWireSphere(centro, distanciaRecoger);

        // Línea hacia arriba para identificar
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(centro, centro + Vector3.up * 2f);

        // Punto central
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(centro, 0.1f);
    }
}
