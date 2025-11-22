using UnityEngine;

public class PolloRecogible : MonoBehaviour
{
    [Header("Configuración de Interacción")]
    [SerializeField] private float distanciaRecogida = 2f;
    [SerializeField] private KeyCode teclaRecoger = KeyCode.E;

    [Header("UI (Opcional)")]
    [SerializeField] private GameObject indicadorUI; // Para mostrar "Presiona E"

    private Transform jugador;
    private bool enRango = false;

    void Start()
    {
        GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");
        if (jugadorObj != null)
        {
            jugador = jugadorObj.transform;
        }

        if (indicadorUI != null)
        {
            indicadorUI.SetActive(false);
        }
    }

    void Update()
    {
        if (jugador == null) return;

        float distancia = Vector3.Distance(transform.position, jugador.position);
        enRango = distancia <= distanciaRecogida;

        // Mostrar/ocultar indicador
        if (indicadorUI != null)
        {
            indicadorUI.SetActive(enRango);
        }

        // Detectar input para recoger
        if (enRango && Input.GetKeyDown(teclaRecoger))
        {
            RecogerPollo();
        }
    }

    void RecogerPollo()
    {
        // Buscar el componente correcto del jugador
        New_CharacterController jugadorScript = jugador.GetComponent<New_CharacterController>();

        if (jugadorScript != null)
        {
            jugadorScript.EquiparPollo(gameObject);
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
    }
}