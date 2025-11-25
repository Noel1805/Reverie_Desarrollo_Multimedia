using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FruitCounterUI : MonoBehaviour
{
    public static FruitCounterUI Instance;

    [Header("Configuración del Contador")]
    [SerializeField] private TextMeshProUGUI textoFrutas;
    [SerializeField] private int totalFrutas = 9;

    [Header("Detección Automática de Contraste")]
    [SerializeField] private bool ajustarColorAutomatico = true;
    [SerializeField] private Color colorTextoFondoClaro = Color.black;
    [SerializeField] private Color colorTextoFondoOscuro = Color.white;
    [Tooltip("Umbral de luminosidad (0-1). Valores menores = oscuro, mayores = claro")]
    [SerializeField] private float umbralLuminosidad = 0.5f;
    [SerializeField] private Image imagenFondo; 

    private int frutasRecogidas = 0;
    private Camera mainCamera;

    void Awake()
    {

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
        mainCamera = Camera.main;
        ActualizarTexto();


        if (ajustarColorAutomatico)
        {
            AjustarColorTextoSegunFondo();
        }
    }

    void Update()
    {

        if (ajustarColorAutomatico)
        {
            AjustarColorTextoSegunFondo();
        }
    }

    public void AgregarFruta()
    {
        frutasRecogidas++;


        if (frutasRecogidas > totalFrutas)
            frutasRecogidas = totalFrutas;

        ActualizarTexto();
    }


    public int GetFrutasRecogidas()
    {
        return frutasRecogidas;
    }


    public bool TieneTodasLasFrutas()
    {
        return frutasRecogidas >= totalFrutas;
    }


    public int GetTotalFrutas()
    {
        return totalFrutas;
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


    private void AjustarColorTextoSegunFondo()
    {
        if (textoFrutas == null) return;

        Color colorFondo = ObtenerColorFondo();
        float luminosidad = CalcularLuminosidad(colorFondo);


        if (luminosidad > umbralLuminosidad)
        {
            textoFrutas.color = colorTextoFondoClaro; 
        }
        else
        {
            textoFrutas.color = colorTextoFondoOscuro; 
        }
    }


    private Color ObtenerColorFondo()
    {

        if (imagenFondo != null)
        {
            return imagenFondo.color;
        }


        if (mainCamera != null)
        {
            return mainCamera.backgroundColor;
        }


        Image parentImage = GetComponentInParent<Image>();
        if (parentImage != null)
        {
            return parentImage.color;
        }


        return Color.white;
    }


    private float CalcularLuminosidad(Color color)
    {

        return (0.2126f * color.r) + (0.7152f * color.g) + (0.0722f * color.b);
    }


    public void ActualizarColorTexto()
    {
        if (ajustarColorAutomatico)
        {
            AjustarColorTextoSegunFondo();
        }
    }
}