using UnityEngine;

public class CajaInteractuable : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Prefabs de los ítems que suelta la caja (mango, pera, mora, etc).")]
    [SerializeField] private GameObject[] prefabsItems;

    [Tooltip("Punto exacto dentro de la caja donde aparecerán los ítems.")]
    [SerializeField] private Transform spawnPoint;

    [SerializeField] private float distanciaInteraccion = 3f;
    [SerializeField] private KeyCode teclaAbrir = KeyCode.E;

    [Header("UI (Opcional)")]
    [SerializeField] private GameObject indicadorUI;

    private bool yaAbierta = false;
    private Transform jugador;
    private bool enRango = false;

    private GameObject[] itemsClonados;

    void Start()
    {
        GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");
        if (jugadorObj != null)
            jugador = jugadorObj.transform;

        // Proteger por si se olvidó asignar spawnPoint
        if (spawnPoint == null)
            spawnPoint = this.transform;

        // Instanciar clones SIN padre para mantener escala correcta
        itemsClonados = new GameObject[prefabsItems.Length];

        for (int i = 0; i < prefabsItems.Length; i++)
        {
            GameObject prefab = prefabsItems[i];
            if (prefab == null) continue;

            // Instanciar EXACTAMENTE donde está el SpawnPoint
            GameObject clon = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

            // Mantener escala del prefab
            clon.transform.localScale = prefab.transform.localScale;

            // Guardarlo y ocultarlo
            clon.SetActive(false);
            itemsClonados[i] = clon;
        }

        if (indicadorUI != null)
            indicadorUI.SetActive(false);
    }

    void Update()
    {
        if (yaAbierta || jugador == null) return;

        float distancia = Vector3.Distance(transform.position, jugador.position);
        enRango = distancia <= distanciaInteraccion;

        if (indicadorUI != null)
            indicadorUI.SetActive(enRango);

        if (enRango && Input.GetKeyDown(teclaAbrir))
            AbrirCaja();
    }

    void AbrirCaja()
    {
        yaAbierta = true;

        foreach (GameObject item in itemsClonados)
        {
            if (item == null) continue;

            item.SetActive(true);

            // Fuerza visual opcional
            Rigidbody rb = item.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddForce(Vector3.up * 2f, ForceMode.Impulse);
        }

        Destroy(gameObject);
    }

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
