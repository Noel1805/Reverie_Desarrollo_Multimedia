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
    [SerializeField] private Image imagenFondo; // Opcional: si hay una imagen de fondo específica

    private int frutasRecogidas = 0;
    private Camera mainCamera;

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
        mainCamera = Camera.main;
        ActualizarTexto();

        // Ajustar color inicial
        if (ajustarColorAutomatico)
        {
            AjustarColorTextoSegunFondo();
        }
    }

    void Update()
    {
        // Actualizar color del texto cada frame si está habilitado
        if (ajustarColorAutomatico)
        {
            AjustarColorTextoSegunFondo();
        }
    }

    public void AgregarFruta()
    {
        frutasRecogidas++;

        // Evita que pase de 9/9 si por algún motivo se recoge más
        if (frutasRecogidas > totalFrutas)
            frutasRecogidas = totalFrutas;

        ActualizarTexto();
    }

    /// <summary>
    /// Método público para obtener las frutas recogidas
    /// </summary>
    public int GetFrutasRecogidas()
    {
        return frutasRecogidas;
    }

    /// <summary>
    /// Método público para verificar si tiene todas las frutas
    /// </summary>
    public bool TieneTodasLasFrutas()
    {
        return frutasRecogidas >= totalFrutas;
    }

    /// <summary>
    /// Método público para obtener el total de frutas
    /// </summary>
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

    /// <summary>
    /// NUEVO: Ajusta automáticamente el color del texto según la luminosidad del fondo
    /// </summary>
    private void AjustarColorTextoSegunFondo()
    {
        if (textoFrutas == null) return;

        Color colorFondo = ObtenerColorFondo();
        float luminosidad = CalcularLuminosidad(colorFondo);

        // Si el fondo es claro (luminosidad alta) → texto negro
        // Si el fondo es oscuro (luminosidad baja) → texto blanco
        if (luminosidad > umbralLuminosidad)
        {
            textoFrutas.color = colorTextoFondoClaro; // Fondo claro → texto negro
        }
        else
        {
            textoFrutas.color = colorTextoFondoOscuro; // Fondo oscuro → texto blanco
        }
    }

    /// <summary>
    /// Obtiene el color del fondo detrás del texto
    /// </summary>
    private Color ObtenerColorFondo()
    {
        // Opción 1: Si hay una imagen de fondo asignada
        if (imagenFondo != null)
        {
            return imagenFondo.color;
        }

        // Opción 2: Detectar color de la cámara (skybox/background)
        if (mainCamera != null)
        {
            return mainCamera.backgroundColor;
        }

        // Opción 3: Buscar imagen de fondo en el padre
        Image parentImage = GetComponentInParent<Image>();
        if (parentImage != null)
        {
            return parentImage.color;
        }

        // Por defecto: asumir fondo claro
        return Color.white;
    }

    /// <summary>
    /// Calcula la luminosidad de un color (0 = oscuro, 1 = claro)
    /// Usa la fórmula estándar de luminosidad percibida
    /// </summary>
    private float CalcularLuminosidad(Color color)
    {
        // Fórmula de luminosidad percibida (ITU-R BT.709)
        return (0.2126f * color.r) + (0.7152f * color.g) + (0.0722f * color.b);
    }

    /// <summary>
    /// NUEVO: Método público para forzar actualización de color
    /// Útil si cambias el fondo manualmente
    /// </summary>
    public void ActualizarColorTexto()
    {
        if (ajustarColorAutomatico)
        {
            AjustarColorTextoSegunFondo();
        }
    }
}