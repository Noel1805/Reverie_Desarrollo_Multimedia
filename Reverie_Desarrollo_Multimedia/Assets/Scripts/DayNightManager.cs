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
    public Color diaColor = new Color(1f, 0.95f, 0.8f); 
    public float diaIntensity = 1.5f;
    public float diaRotationX = 50f;

    [Header("Configuración de Atardecer (Otoño)")]
    public Color atardecerColor = new Color(1f, 0.6f, 0.3f); 
    public float atardecerIntensity = 0.8f;
    public float atardecerRotationX = 10f; 

    [Header("Configuración de Noche (Invierno)")]
    public Color nocheColor = new Color(0.3f, 0.4f, 0.6f); 
    public float nocheIntensity = 0.3f;
    public float nocheRotationX = -30f; 

    [Header("Configuración de Transición")]
    [Tooltip("Velocidad de transición entre estados")]
    public float transicionSuavidad = 2f;


    private Color targetColor;
    private float targetIntensity;
    private float targetRotationX;

    void Start()
    {

        if (directionalLight == null)
        {
            Debug.LogError("¡Falta asignar la Directional Light en el Inspector!");
        }

        if (player == null)
        {

            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null)
            {
                Debug.LogError("¡No se encontró el jugador! Asegúrate de asignar el player o que tenga el tag 'Player'");
            }
        }


        targetColor = diaColor;
        targetIntensity = diaIntensity;
        targetRotationX = diaRotationX;
    }

    void Update()
    {
        if (player == null || directionalLight == null) return;


        DeterminarZonaYConfigurar();


        AplicarTransiciones();
    }

    void DeterminarZonaYConfigurar()
    {
        float posZ = player.position.z;


        if (posZ < primaveraEndZ)
        {
            targetColor = diaColor;
            targetIntensity = diaIntensity;
            targetRotationX = diaRotationX;
        }

        else if (posZ >= primaveraEndZ && posZ < otonoEndZ)
        {
            float t = (posZ - primaveraEndZ) / (otonoEndZ - primaveraEndZ);
            targetColor = Color.Lerp(diaColor, atardecerColor, t);
            targetIntensity = Mathf.Lerp(diaIntensity, atardecerIntensity, t);
            targetRotationX = Mathf.Lerp(diaRotationX, atardecerRotationX, t);
        }

        else if (posZ >= otonoEndZ && posZ < inviernoStartZ + 20f)
        {
            float t = (posZ - otonoEndZ) / 20f;
            targetColor = Color.Lerp(atardecerColor, nocheColor, t);
            targetIntensity = Mathf.Lerp(atardecerIntensity, nocheIntensity, t);
            targetRotationX = Mathf.Lerp(atardecerRotationX, nocheRotationX, t);
        }

        else
        {
            targetColor = nocheColor;
            targetIntensity = nocheIntensity;
            targetRotationX = nocheRotationX;
        }
    }

    void AplicarTransiciones()
    {

        directionalLight.color = Color.Lerp(
            directionalLight.color,
            targetColor,
            Time.deltaTime * transicionSuavidad
        );


        directionalLight.intensity = Mathf.Lerp(
            directionalLight.intensity,
            targetIntensity,
            Time.deltaTime * transicionSuavidad
        );


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


    void OnDrawGizmosSelected()
    {

        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            new Vector3(-100, 0, 0),
            new Vector3(100, 0, 0)
        );
        Gizmos.DrawLine(
            new Vector3(-100, 0, primaveraEndZ),
            new Vector3(100, 0, primaveraEndZ)
        );


        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawLine(
            new Vector3(-100, 0, otonoEndZ),
            new Vector3(100, 0, otonoEndZ)
        );


        Gizmos.color = Color.blue;
        Gizmos.DrawLine(
            new Vector3(-100, 0, inviernoStartZ),
            new Vector3(100, 0, inviernoStartZ)
        );
    }
}