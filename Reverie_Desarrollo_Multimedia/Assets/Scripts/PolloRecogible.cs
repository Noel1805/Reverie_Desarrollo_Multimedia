using UnityEngine;

public class PolloRecogible : MonoBehaviour
{
    [Header("Configuración de Interacción")]
    [SerializeField] private float distanciaRecogida = 2f;
    [SerializeField] private KeyCode teclaRecoger = KeyCode.E;

    [Header("UI (Opcional)")]
    [SerializeField] private GameObject indicadorUI;

    private Transform jugador;
    private bool enRango = false;


    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;

    void Awake()
    {

        posicionInicial = transform.position;
        rotacionInicial = transform.rotation;
        Debug.Log($"PolloRecogible: Posición inicial guardada en {posicionInicial}");
    }

    void Start()
    {
        BuscarJugador();

        if (indicadorUI != null)
        {
            indicadorUI.SetActive(false);
        }
    }

    void OnEnable()
    {
        BuscarJugador();
        enRango = false;

        if (indicadorUI != null)
        {
            indicadorUI.SetActive(false);
        }
    }

    void BuscarJugador()
    {
        if (jugador == null)
        {
            GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");
            if (jugadorObj != null)
            {
                jugador = jugadorObj.transform;
                Debug.Log("PolloRecogible: Jugador encontrado");
            }
            else
            {
                Debug.LogWarning("PolloRecogible: No se encontró un objeto con tag 'Player'");
            }
        }
    }

    void Update()
    {
        if (jugador == null)
        {
            BuscarJugador();
            return;
        }

        float distancia = Vector3.Distance(transform.position, jugador.position);
        enRango = distancia <= distanciaRecogida;

        if (indicadorUI != null)
        {
            indicadorUI.SetActive(enRango);
        }

        if (enRango && Input.GetKeyDown(teclaRecoger))
        {
            RecogerPollo();
        }
    }

    void RecogerPollo()
    {
        New_CharacterController jugadorScript = jugador.GetComponent<New_CharacterController>();

        if (jugadorScript != null)
        {
            if (!jugadorScript.TienePollo())
            {

                jugadorScript.EquiparPollo(gameObject, posicionInicial, rotacionInicial);
            }
            else
            {
                Debug.Log("El jugador ya tiene un pollo equipado");
            }
        }
        else
        {
            Debug.LogError("No se encontró el script New_CharacterController en el jugador!");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, distanciaRecogida);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(posicionInicial, 0.3f);
            Gizmos.DrawLine(transform.position, posicionInicial);
        }
    }
}