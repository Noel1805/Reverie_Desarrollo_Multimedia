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


        ActualizarListaEnemigos();
    }

    void Update()
    {
        if (arenaActiva)
        {

            ActualizarListaEnemigos();

            if (requiereEnemigosVivos && enemigosVivos.Count == 0)
            {
                DesactivarArena();
            }
        }
        else
        {

            if (activarAlDetectarJugador && jugador != null)
            {
                float distancia = Vector3.Distance(centroArena.position, jugador.transform.position);

                if (distancia <= rangoDeteccion)
                {

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

            GameObject[] todosEnemigos = GameObject.FindGameObjectsWithTag(tagEnemigo);

            foreach (var enemigo in todosEnemigos)
            {
                if (enemigo != null && enemigo.activeSelf)
                {
                    float distancia = Vector3.Distance(centroArena.position, enemigo.transform.position);
                    if (distancia <= rangoDeteccion * 1.5f) 
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


        if (efectoActivacionArena != null)
        {
            Instantiate(efectoActivacionArena, centroArena.position, Quaternion.identity);
        }


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


    public void ActivarManualmente()
    {
        ActivarArena();
    }


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


        Gizmos.color = arenaActiva ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(centro.position, rangoDeteccion);


        Gizmos.color = new Color(1f, 0f, 0f, 0.1f);
        Gizmos.DrawWireSphere(centro.position, rangoDeteccion * 1.5f);


        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(centro.position, centro.position + Vector3.up * 5f);
    }
}