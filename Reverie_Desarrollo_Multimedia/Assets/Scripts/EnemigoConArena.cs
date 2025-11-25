using UnityEngine;

public class EnemigoConArena : MonoBehaviour
{
    [Header("Arena")]
    [SerializeField] private ArenaCombate arenaAsociada;
    [SerializeField] private bool buscarArenaAutomaticamente = true;

    [Header("Detección")]
    [SerializeField] private float rangoDeteccion = 8f;

    private GameObject jugador;
    private bool arenaActivada = false;

    void Start()
    {
        jugador = GameObject.FindGameObjectWithTag("Player");


        if (arenaAsociada == null && buscarArenaAutomaticamente)
        {
            arenaAsociada = FindObjectOfType<ArenaCombate>();
        }
    }

    void Update()
    {
        if (arenaActivada || jugador == null || arenaAsociada == null)
            return;

        float distancia = Vector3.Distance(transform.position, jugador.transform.position);

        if (distancia <= rangoDeteccion)
        {
            arenaAsociada.ActivarManualmente();
            arenaActivada = true;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
    }
}
