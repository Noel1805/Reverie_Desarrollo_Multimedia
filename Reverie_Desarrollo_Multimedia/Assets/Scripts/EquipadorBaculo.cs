using UnityEngine;


public class EquipadorBaculo : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform puntoAgarre; 

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


        baculo.transform.SetParent(puntoAgarre);


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