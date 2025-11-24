using UnityEngine;
using TMPro;

public class FruitCounterUI : MonoBehaviour
{
    public static FruitCounterUI Instance;

    [Header("Configuración del Contador")]
    [SerializeField] private TextMeshProUGUI textoFrutas;

    [SerializeField] private int totalFrutas = 9;
    private int frutasRecogidas = 0;

    void Awake()
    {
        // Patrón Singleton simple para acceder desde cualquier fruta
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ActualizarTexto();
    }

    public void AgregarFruta()
    {
        frutasRecogidas++;

        // Evita que pase de 9/9 si por algún motivo se recoge más
        if (frutasRecogidas > totalFrutas)
            frutasRecogidas = totalFrutas;

        ActualizarTexto();
    }

    private void ActualizarTexto()
    {
        if (textoFrutas != null)
        {
            textoFrutas.text = frutasRecogidas + "/" + totalFrutas;
        }
        else
        {
            Debug.LogWarning("[FruitCounterUI] No se asignó el TextMeshPro en el Inspector.");
        }
    }
}