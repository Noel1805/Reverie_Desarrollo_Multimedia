using UnityEngine;

public class DayNightController : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("La luz direccional que representa el sol")]
    public Light directionalLight;

    [Tooltip("El transform del jugador")]
    public Transform player;

    [Header("Configuración de Zonas")]
    [Tooltip("Posición Z donde termina la zona de primavera (día)")]
    public float primaveraEndZ = 50f;

    [Tooltip("Posición Z donde termina la zona de otoño (atardecer)")]
    public float otonoEndZ = 100f;

    [Tooltip("Posición Z donde comienza la zona de invierno (noche)")]
    public float inviernoStartZ = 100f;

    [Header("Configuración de Día (Primavera)")]
    public Color diaColor = new Color(1f, 0.95f, 0.8f); // Amarillo suave
    public float diaIntensity = 1.5f;
    public float diaRotationX = 50f; // Ángulo del sol en el cielo

    [Header("Configuración de Atardecer (Otoño)")]
    public Color atardecerColor = new Color(1f, 0.6f, 0.3f); // Naranja
    public float atardecerIntensity = 0.8f;
    public float atardecerRotationX = 10f; // Sol más bajo

    [Header("Configuración de Noche (Invierno)")]
    public Color nocheColor = new Color(0.3f, 0.4f, 0.6f); // Azul oscuro
    public float nocheIntensity = 0.3f;
    public float nocheRotationX = -30f; // Sol por debajo del horizonte

    [Header("Configuración de Transición")]
    [Tooltip("Velocidad de transición entre estados")]
    public float transicionSuavidad = 2f;

    // Variables internas
    private Color targetColor;
    private float targetIntensity;
    private float targetRotationX;

    void Start()
    {
        // Validaciones
        if (directionalLight == null)
        {
            Debug.LogError("¡Falta asignar la Directional Light en el Inspector!");
        }

        if (player == null)
        {
            // Intentar encontrar al jugador automáticamente
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null)
            {
                Debug.LogError("¡No se encontró el jugador! Asegúrate de asignar el player o que tenga el tag 'Player'");
            }
        }

        // Inicializar con valores de día
        targetColor = diaColor;
        targetIntensity = diaIntensity;
        targetRotationX = diaRotationX;
    }

    void Update()
    {
        if (player == null || directionalLight == null) return;

        // Determinar en qué zona está el jugador y establecer los valores objetivo
        DeterminarZonaYConfigurar();

        // Aplicar transiciones suaves
        AplicarTransiciones();
    }

    void DeterminarZonaYConfigurar()
    {
        float posZ = player.position.z;

        // Zona de Primavera (Día)
        if (posZ < primaveraEndZ)
        {
            targetColor = diaColor;
            targetIntensity = diaIntensity;
            targetRotationX = diaRotationX;
        }
        // Transición de Primavera a Otoño (Día a Atardecer)
        else if (posZ >= primaveraEndZ && posZ < otonoEndZ)
        {
            float t = (posZ - primaveraEndZ) / (otonoEndZ - primaveraEndZ);
            targetColor = Color.Lerp(diaColor, atardecerColor, t);
            targetIntensity = Mathf.Lerp(diaIntensity, atardecerIntensity, t);
            targetRotationX = Mathf.Lerp(diaRotationX, atardecerRotationX, t);
        }
        // Transición de Otoño a Invierno (Atardecer a Noche)
        else if (posZ >= otonoEndZ && posZ < inviernoStartZ + 20f)
        {
            float t = (posZ - otonoEndZ) / 20f;
            targetColor = Color.Lerp(atardecerColor, nocheColor, t);
            targetIntensity = Mathf.Lerp(atardecerIntensity, nocheIntensity, t);
            targetRotationX = Mathf.Lerp(atardecerRotationX, nocheRotationX, t);
        }
        // Zona de Invierno (Noche)
        else
        {
            targetColor = nocheColor;
            targetIntensity = nocheIntensity;
            targetRotationX = nocheRotationX;
        }
    }

    void AplicarTransiciones()
    {
        // Transición suave del color
        directionalLight.color = Color.Lerp(
            directionalLight.color,
            targetColor,
            Time.deltaTime * transicionSuavidad
        );

        // Transición suave de la intensidad
        directionalLight.intensity = Mathf.Lerp(
            directionalLight.intensity,
            targetIntensity,
            Time.deltaTime * transicionSuavidad
        );

        // Transición suave de la rotación
        Vector3 currentRotation = directionalLight.transform.eulerAngles;
        float newRotationX = Mathf.LerpAngle(
            currentRotation.x,
            targetRotationX,
            Time.deltaTime * transicionSuavidad
        );

        directionalLight.transform.eulerAngles = new Vector3(
            newRotationX,
            currentRotation.y,
            currentRotation.z
        );
    }

    // Método auxiliar para visualizar las zonas en el editor
    void OnDrawGizmosSelected()
    {
        // Zona de Primavera (Verde)
        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            new Vector3(-100, 0, 0),
            new Vector3(100, 0, 0)
        );
        Gizmos.DrawLine(
            new Vector3(-100, 0, primaveraEndZ),
            new Vector3(100, 0, primaveraEndZ)
        );

        // Zona de Otoño (Naranja)
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawLine(
            new Vector3(-100, 0, otonoEndZ),
            new Vector3(100, 0, otonoEndZ)
        );

        // Zona de Invierno (Azul)
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(
            new Vector3(-100, 0, inviernoStartZ),
            new Vector3(100, 0, inviernoStartZ)
        );
    }
}