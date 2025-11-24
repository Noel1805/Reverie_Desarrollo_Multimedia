using UnityEngine;

public class CajaInteractuable : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Prefabs de los ítems que suelta la caja (mango, pera, mora, etc).")]
    [SerializeField] private GameObject[] prefabsItems;

    [Tooltip("Punto exacto dentro de la caja donde aparecerá el ítem.")]
    [SerializeField] private Transform spawnPoint;

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
            jugador = jugadorObj.transform;

        // Si se olvidan de asignar spawnPoint, usamos la posición de la caja
        if (spawnPoint == null)
            spawnPoint = this.transform;

        if (indicadorUI != null)
            indicadorUI.SetActive(false);
    }

    void Update()
    {
        if (yaAbierta || jugador == null) return;

        // Verificar distancia al jugador
        float distancia = Vector3.Distance(transform.position, jugador.position);
        enRango = distancia <= distanciaInteraccion;

        // Mostrar/ocultar indicador
        if (indicadorUI != null)
            indicadorUI.SetActive(enRango);

        // Detectar input para abrir
        if (enRango && Input.GetKeyDown(teclaAbrir))
            AbrirCaja();
    }

    void AbrirCaja()
    {
        yaAbierta = true;

        if (prefabsItems == null || prefabsItems.Length == 0)
        {
            Debug.LogWarning("CajaInteractuable: No hay prefabs asignados en prefabsItems.");
        }
        else
        {
            // Elegir un ítem aleatorio
            int index = Random.Range(0, prefabsItems.Length);
            GameObject prefab = prefabsItems[index];

            if (prefab != null)
            {
                // Instanciar clon en el spawnPoint
                GameObject clon = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

                // Mantener escala original del prefab
                clon.transform.localScale = prefab.transform.localScale;

                // Opcional: salto hacia arriba
                Rigidbody rb = clon.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddForce(Vector3.up * 2f, ForceMode.Impulse);
                }

                Debug.Log($"¡Caja abierta! Ítem aleatorio: {prefab.name}");
            }
        }

        // Destruir la caja
        Destroy(gameObject);
    }

    // Para visualizar el rango en el editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaInteraccion);

        if (spawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(spawnPoint.position, 0.1f);
        }
    }
}
