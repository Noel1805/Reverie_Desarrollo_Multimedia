using UnityEngine;

public class CajaInteractuable : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private GameObject pollo; // El pollo que está dentro como hijo
    [SerializeField] private float distanciaInteraccion = 3f;
    [SerializeField] private KeyCode teclaAbrir = KeyCode.E;

    [Header("UI (Opcional)")]
    [SerializeField] private GameObject indicadorUI; // Para mostrar "Presiona E"

    private bool yaAbierta = false;
    private Transform jugador;
    private bool enRango = false;

    void Start()
    {
        // Buscar al jugador por tag
        GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");
        if (jugadorObj != null)
        {
            jugador = jugadorObj.transform;
        }

        // Asegurarse de que el pollo esté oculto al inicio
        if (pollo != null)
        {
            pollo.SetActive(false);
        }

        if (indicadorUI != null)
        {
            indicadorUI.SetActive(false);
        }
    }

    void Update()
    {
        if (yaAbierta || jugador == null) return;

        // Verificar distancia al jugador
        float distancia = Vector3.Distance(transform.position, jugador.position);
        enRango = distancia <= distanciaInteraccion;

        // Mostrar/ocultar indicador
        if (indicadorUI != null)
        {
            indicadorUI.SetActive(enRango);
        }

        // Detectar input para abrir
        if (enRango && Input.GetKeyDown(teclaAbrir))
        {
            AbrirCaja();
        }
    }

    void AbrirCaja()
    {
        yaAbierta = true;

        // IMPORTANTE: Sacar el pollo de la jerarquía de la caja ANTES de destruirla
        if (pollo != null)
        {
            // Hacer que el pollo sea independiente (sin padre)
            pollo.transform.SetParent(null);

            // Revelar el pollo
            pollo.SetActive(true);

            // Opcional: hacer que el pollo "salte" al revelarse
            Rigidbody polloRb = pollo.GetComponent<Rigidbody>();
            if (polloRb != null)
            {
                polloRb.AddForce(Vector3.up * 3f, ForceMode.Impulse);
            }
        }

        // Ahora sí destruir la caja
        Destroy(gameObject);

        Debug.Log("¡Caja abierta! Pollo revelado.");
    }

    // Para visualizar el rango en el editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaInteraccion);
    }
}