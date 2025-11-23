using UnityEngine;

public class VidaEnemigo : MonoBehaviour
{
    [SerializeField] private float vidaMaxima = 100f;
    private float vidaActual;
    private bool estaMuerto = false;

    void Start()
    {
        vidaActual = vidaMaxima;
    }

    public void RecibirDaño(float cantidad)
    {
        if (estaMuerto) return;

        vidaActual -= cantidad;
        Debug.Log($"💥 {gameObject.name} recibió {cantidad} de daño. Vida restante: {vidaActual}");

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    // NUEVO: Método para obtener la vida actual
    public float GetVidaActual()
    {
        return vidaActual;
    }

    // NUEVO: Método para verificar si está muerto
    public bool EstaMuerto()
    {
        return estaMuerto;
    }

    void Morir()
    {
        if (estaMuerto) return;

        estaMuerto = true;
        Debug.Log($"💀 {gameObject.name} ha muerto");

        // NO destruir aquí, el GrizartController lo maneja
        // Destroy(gameObject);
    }
}