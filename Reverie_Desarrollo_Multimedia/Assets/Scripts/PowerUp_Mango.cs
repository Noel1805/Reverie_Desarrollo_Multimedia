using UnityEngine;
using System.Collections;

public class PowerUp_Mango : MonoBehaviour
{
    [Header("Configuración del Power-Up")]
    [SerializeField] private float multiplicadorDaño = 1.2f;
    [SerializeField] private float duracion = 20f;
    [SerializeField] private KeyCode teclaRecoger = KeyCode.E;
    [SerializeField] private float distanciaRecoger = 2f; // Distancia para recoger

    [Header("Efectos Visuales (Opcional)")]
    [SerializeField] private GameObject efectoRecoger;
    [SerializeField] private AudioClip sonidoRecoger;

    [Header("Respawn (Opcional)")]
    [SerializeField] private bool respawnear = true;
    [SerializeField] private float tiempoRespawn = 30f;

    [Header("Debug")]
    [SerializeField] private bool mostrarDebug = true;

    private GameObject jugador;
    private bool estaActivo = true;

    // 🔹 Ahora son arreglos para manejar TODOS los meshes/colliders
    private MeshRenderer[] meshRenderers;
    private Collider[] colliders;
    private AudioSource audioSource;

    void Start()
    {
        // Buscar TODOS los renderers y colliders del mango (en el root y en los hijos)
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        colliders = GetComponentsInChildren<Collider>();

        // Buscar al jugador por tag
        jugador = GameObject.FindGameObjectWithTag("Player");

        if (mostrarDebug)
        {
            Debug.Log($"[Mango START] ✓ Inicializado en {gameObject.name}");
            Debug.Log($"[Mango START] Jugador encontrado: {(jugador != null ? jugador.name : "NO ENCONTRADO")}");
            Debug.Log($"[Mango START] Distancia de recolección: {distanciaRecoger}m");
            Debug.Log($"[Mango START] MeshRenderers encontrados: {meshRenderers.Length}");
            Debug.Log($"[Mango START] Colliders encontrados: {colliders.Length}");
        }

        if (jugador == null)
        {
            Debug.LogError("[Mango START] ❌ No se encontró ningún objeto con tag 'Player'!");
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

        // Centro del mango usando el primer collider si existe
        Vector3 centroMango = transform.position;
        if (colliders != null && colliders.Length > 0 && colliders[0] != null)
        {
            centroMango = colliders[0].bounds.center;
        }

        float distancia = Vector3.Distance(centroMango, jugador.transform.position);

        if (mostrarDebug && Time.frameCount % 60 == 0)
        {
            Debug.Log($"[Mango] Distancia al jugador: {distancia:F2}m (Necesita: ≤{distanciaRecoger}m)");
        }

        if (distancia <= distanciaRecoger)
        {
            if (mostrarDebug && Time.frameCount % 30 == 0)
            {
                Debug.Log($"[Mango] 🟢 ¡En rango! ({distancia:F2}m) Presiona {teclaRecoger} para recoger");
            }

            if (Input.GetKeyDown(teclaRecoger))
            {
                if (mostrarDebug)
                    Debug.Log($"[Mango] ✓ Tecla {teclaRecoger} detectada! Distancia exacta: {distancia:F2}m");

                RecogerPowerUp();
            }
        }
    }

    void RecogerPowerUp()
    {
        if (jugador == null)
        {
            if (mostrarDebug)
                Debug.LogWarning("[Mango RECOGER] Jugador es NULL");
            return;
        }

        if (mostrarDebug)
            Debug.Log($"[Mango RECOGER] Intentando recoger... Jugador: {jugador.name}");

        // Buscar el script de ataque (en el jugador o sus hijos)
        AtaqueBaculo ataqueBaculo = jugador.GetComponent<AtaqueBaculo>();
        if (ataqueBaculo == null)
            ataqueBaculo = jugador.GetComponentInChildren<AtaqueBaculo>();

        if (ataqueBaculo == null)
        {
            Debug.LogError($"[Mango RECOGER] ❌ No se encontró AtaqueBaculo en {jugador.name}");

            if (mostrarDebug)
            {
                Debug.Log("[Mango DEBUG] Componentes en el jugador:");
                foreach (var comp in jugador.GetComponentsInChildren<Component>())
                {
                    Debug.Log($"  - {comp.GetType().Name}");
                }
            }

            // Aun así consumimos el mango
            ConsumirYRespawnear();
            return;
        }

        if (mostrarDebug)
            Debug.Log($"[Mango RECOGER] ✓ AtaqueBaculo encontrado en {ataqueBaculo.gameObject.name}");

        var metodo = ataqueBaculo.GetType().GetMethod("AplicarBuffDaño");

        if (metodo != null)
        {
            if (mostrarDebug)
                Debug.Log($"[Mango RECOGER] ✓ Método AplicarBuffDaño encontrado");

            metodo.Invoke(ataqueBaculo, new object[] { multiplicadorDaño, duracion });
            Debug.Log($"[Mango] 🥭 Power-up recogido! Daño aumentado {(multiplicadorDaño - 1f) * 100f}% por {duracion} segundos");
        }
        else
        {
            if (mostrarDebug)
                Debug.LogWarning("[Mango] ⚠ Método AplicarBuffDaño no encontrado. Aplicando buff manualmente...");

            StartCoroutine(AplicarBuffManual(ataqueBaculo));
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

        OcultarMango();

        if (respawnear)
        {
            Invoke(nameof(ReaparecerMango), tiempoRespawn);
            if (mostrarDebug)
                Debug.Log($"[Mango] ⏰ Reaparecerá en {tiempoRespawn} segundos");
        }
    }

    private IEnumerator AplicarBuffManual(AtaqueBaculo ataque)
    {
        var tipo = ataque.GetType();

        var campoDaño1 = tipo.GetField("dañoAtaque1", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var campoDaño2 = tipo.GetField("dañoAtaque2", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var campoDaño3 = tipo.GetField("dañoAtaque3", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (campoDaño1 != null && campoDaño2 != null && campoDaño3 != null)
        {
            float dañoOriginal1 = (float)campoDaño1.GetValue(ataque);
            float dañoOriginal2 = (float)campoDaño2.GetValue(ataque);
            float dañoOriginal3 = (float)campoDaño3.GetValue(ataque);

            campoDaño1.SetValue(ataque, dañoOriginal1 * multiplicadorDaño);
            campoDaño2.SetValue(ataque, dañoOriginal2 * multiplicadorDaño);
            campoDaño3.SetValue(ataque, dañoOriginal3 * multiplicadorDaño);

            Debug.Log($"[Mango] 🥭 Buff aplicado manualmente: x{multiplicadorDaño} por {duracion}s");

            yield return new WaitForSeconds(duracion);

            campoDaño1.SetValue(ataque, dañoOriginal1);
            campoDaño2.SetValue(ataque, dañoOriginal2);
            campoDaño3.SetValue(ataque, dañoOriginal3);

            Debug.Log("[Mango] ⏰ Buff terminado. Daño restaurado.");
        }
        else
        {
            Debug.LogError("[Mango] ❌ No se pudieron encontrar los campos de daño");
        }
    }

    void OcultarMango()
    {
        estaActivo = false;

        // 🔹 Apagar TODOS los MeshRenderers
        if (meshRenderers != null)
        {
            foreach (var mr in meshRenderers)
            {
                if (mr != null)
                    mr.enabled = false;
            }
        }

        // 🔹 Desactivar TODOS los Colliders del mango
        if (colliders != null)
        {
            foreach (var c in colliders)
            {
                if (c != null)
                    c.enabled = false;
            }
        }

        if (mostrarDebug)
            Debug.Log("[Mango] 👻 Mango ocultado (todos los meshes y colliders)");
    }

    void ReaparecerMango()
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
            Debug.Log("[Mango] ✨ Power-up ha reaparecido (todos los meshes y colliders)");
    }

    void OnDrawGizmosSelected()
    {
        // Centro usando el primer collider si existe
        Collider c = GetComponentInChildren<Collider>();
        Vector3 centro = c != null ? c.bounds.center : transform.position;

        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawSphere(centro, distanciaRecoger);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(centro, distanciaRecoger);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(centro, centro + Vector3.up * 3f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(centro, 0.1f);

        if (Application.isPlaying && jugador != null)
        {
            float dist = Vector3.Distance(centro, jugador.transform.position);
            Gizmos.color = dist <= distanciaRecoger ? Color.green : Color.red;

            Gizmos.DrawLine(centro, jugador.transform.position);

            Vector3 midPoint = (centro + jugador.transform.position) / 2f;
            Gizmos.DrawWireSphere(midPoint, 0.2f);
        }
    }
}
