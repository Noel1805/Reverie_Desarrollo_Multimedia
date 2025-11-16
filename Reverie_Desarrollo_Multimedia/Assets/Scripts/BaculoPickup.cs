using UnityEngine;

// Script para el báculo (colocar en el objeto del báculo en el suelo)
public class BaculoPickup : MonoBehaviour
{
    [SerializeField] private float distanciaInteraccion = 2f;

    private Transform jugador;
    private bool jugadorCerca = false;
    private bool recogido = false;

    void Start()
    {
        jugador = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (recogido) return;

        if (jugador != null)
        {
            float distancia = Vector3.Distance(transform.position, jugador.position);
            jugadorCerca = distancia <= distanciaInteraccion;

            if (jugadorCerca && Input.GetKeyDown(KeyCode.E))
            {
                RecogerBaculo();
            }
        }
    }

    void RecogerBaculo()
    {
        EquipadorBaculo equipador = jugador.GetComponent<EquipadorBaculo>();

        if (equipador != null)
        {
            equipador.EquiparBaculo(gameObject);
            recogido = true;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaInteraccion);
    }
}

