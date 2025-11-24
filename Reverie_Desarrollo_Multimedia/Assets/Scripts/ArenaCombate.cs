using UnityEngine;
using System.Collections.Generic;

public class ArenaCombate : MonoBehaviour
{
    [Header("Configuración de Arena")]
    [SerializeField] private Transform centroArena;
    [SerializeField] private float rangoDeteccion = 10f;

    [Header("Condiciones de Activación")]
    [SerializeField] private bool activarAlDetectarJugador = true;
    [SerializeField] private bool requiereEnemigosVivos = true;
    [SerializeField] private int minimoEnemigosParaActivar = 1;

    [Header("Paredes")]
    [SerializeField] private ParedActivable[] paredes;
    [SerializeField] private bool buscarParedesAutomaticamente = true;

    [Header("Enemigos en la Arena")]
    [SerializeField] private GameObject[] enemigosAsignados;
    [SerializeField] private string tagEnemigo = "Enemy";

    [Header("Efectos")]
    [SerializeField] private GameObject efectoActivacionArena;
    [SerializeField] private AudioClip sonidoActivacionArena;

    [Header("Debug")]
    [SerializeField] private bool mostrarDebug = true;

    private GameObject jugador;
    private bool arenaActiva = false;
    private List<GameObject> enemigosVivos = new List<GameObject>();
    private AudioSource audioSource;

    void Start()
    {
        jugador = GameObject.FindGameObjectWithTag("Player");

        if (centroArena == null)
        {
            centroArena = transform;
        }

        // Buscar paredes automáticamente si está habilitado
        if (buscarParedesAutomaticamente && (paredes == null || paredes.Length == 0))
        {
            paredes = GetComponentsInChildren<ParedActivable>();
            if (mostrarDebug)
            {
                Debug.Log($"[Arena] Paredes encontradas automáticamente: {paredes.Length}");
            }
        }

        // Audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && sonidoActivacionArena != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Inicializar lista de enemigos
        ActualizarListaEnemigos();
    }

    void Update()
    {
        if (arenaActiva)
        {
            // Verificar si todos los enemigos están muertos
            ActualizarListaEnemigos();

            if (requiereEnemigosVivos && enemigosVivos.Count == 0)
            {
                DesactivarArena();
            }
        }
        else
        {
            // Verificar si el jugador está en rango
            if (activarAlDetectarJugador && jugador != null)
            {
                float distancia = Vector3.Distance(centroArena.position, jugador.transform.position);

                if (distancia <= rangoDeteccion)
                {
                    // Verificar condición de enemigos
                    ActualizarListaEnemigos();

                    if (!requiereEnemigosVivos || enemigosVivos.Count >= minimoEnemigosParaActivar)
                    {
                        ActivarArena();
                    }
                }
            }
        }
    }

    void ActualizarListaEnemigos()
    {
        enemigosVivos.Clear();

        // Usar enemigos asignados si existen
        if (enemigosAsignados != null && enemigosAsignados.Length > 0)
        {
            foreach (var enemigo in enemigosAsignados)
            {
                if (enemigo != null && enemigo.activeSelf)
                {
                    enemigosVivos.Add(enemigo);
                }
            }
        }
        else
        {
            // Buscar enemigos por tag en el área
            GameObject[] todosEnemigos = GameObject.FindGameObjectsWithTag(tagEnemigo);

            foreach (var enemigo in todosEnemigos)
            {
                if (enemigo != null && enemigo.activeSelf)
                {
                    float distancia = Vector3.Distance(centroArena.position, enemigo.transform.position);
                    if (distancia <= rangoDeteccion * 1.5f) // Un poco más de rango para enemigos
                    {
                        enemigosVivos.Add(enemigo);
                    }
                }
            }
        }
    }

    public void ActivarArena()
    {
        if (arenaActiva) return;

        arenaActiva = true;

        // Activar todas las paredes
        if (paredes != null)
        {
            foreach (var pared in paredes)
            {
                if (pared != null)
                {
                    pared.Activar();
                }
            }
        }

        // Efecto visual
        if (efectoActivacionArena != null)
        {
            Instantiate(efectoActivacionArena, centroArena.position, Quaternion.identity);
        }

        // Sonido
        if (sonidoActivacionArena != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoActivacionArena);
        }

        if (mostrarDebug)
        {
            Debug.Log($"[Arena] Arena ACTIVADA - Enemigos vivos: {enemigosVivos.Count}");
        }
    }

    public void DesactivarArena()
    {
        if (!arenaActiva) return;

        arenaActiva = false;

        // Desactivar todas las paredes
        if (paredes != null)
        {
            foreach (var pared in paredes)
            {
                if (pared != null)
                {
                    pared.Desactivar();
                }
            }
        }

        if (mostrarDebug)
        {
            Debug.Log($"[Arena] Arena DESACTIVADA - Combate terminado");
        }
    }

    // Método para activar manualmente desde otro script
    public void ActivarManualmente()
    {
        ActivarArena();
    }

    // Método para desactivar manualmente
    public void DesactivarManualmente()
    {
        DesactivarArena();
    }

    public bool EstaActiva()
    {
        return arenaActiva;
    }

    void OnDrawGizmosSelected()
    {
        Transform centro = centroArena != null ? centroArena : transform;

        // Rango de detección
        Gizmos.color = arenaActiva ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(centro.position, rangoDeteccion);

        // Área de combate (más grande)
        Gizmos.color = new Color(1f, 0f, 0f, 0.1f);
        Gizmos.DrawWireSphere(centro.position, rangoDeteccion * 1.5f);

        // Línea hacia arriba para identificar el centro
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(centro.position, centro.position + Vector3.up * 5f);
    }
}