using UnityEngine;

// SCRIPT 2: Colocar este script en el JUGADOR (el personaje)
public class EquipadorBaculo : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform puntoAgarre; // Arrastra aquí tu PuntoAgarre desde la jerarquía

    [Header("Ajustes de Posición")]
    [SerializeField] private Vector3 posicionLocal = Vector3.zero;
    [SerializeField] private Vector3 rotacionLocal = Vector3.zero;

    private GameObject baculoEquipado;

    public void EquiparBaculo(GameObject baculo)
    {
        if (puntoAgarre == null)
        {
            Debug.LogError("¡No se ha asignado el PuntoAgarre!");
            return;
        }

        // Desactivar físicas si las tiene
        Rigidbody rb = baculo.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        Collider col = baculo.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Parentear el báculo al PuntoAgarre
        baculo.transform.SetParent(puntoAgarre);

        // Aplicar posición y rotación personalizadas
        baculo.transform.localPosition = posicionLocal;
        baculo.transform.localRotation = Quaternion.Euler(rotacionLocal);

        baculoEquipado = baculo;

        Debug.Log("Báculo equipado en PuntoAgarre");
    }

    public bool TieneBaculoEquipado()
    {
        return baculoEquipado != null;
    }
}