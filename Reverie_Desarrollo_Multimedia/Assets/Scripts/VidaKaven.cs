using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class VidaKaven : MonoBehaviour
{
    [Header("Configuración de Vida")]
    [SerializeField] private float vidaMaxima = 6f; // 3 corazones x 2 puntos cada uno
    private float vidaActual;

    [Header("Referencias UI")]
    [SerializeField] private Image corazon1;
    [SerializeField] private Image corazon2;
    [SerializeField] private Image corazon3;

    [Header("Sprites de Corazones")]
    [SerializeField] private Sprite corazonLleno;
    [SerializeField] private Sprite corazonMedio;
    [SerializeField] private Sprite corazonVacio;

    [Header("Configuración de Daño")]
    [SerializeField] private float tiempoInvulnerabilidad = 1f;
    private float tiempoUltimoDano = -10f;

    [Header("Configuración de Muerte")]
    [SerializeField] private bool reiniciarNivelAlMorir = true;
    [SerializeField] private float tiempoAntesDeReiniciar = 2f;

    private CharacterController characterController;
    private bool estaMuerto = false;

    void Start()
    {
        vidaActual = vidaMaxima;
        characterController = GetComponent<CharacterController>();
        ActualizarCorazones();
    }

    /// <summary>
    /// Método público para que otros scripts hagan daño a Kaven
    /// </summary>
    public void RecibirDano(float cantidad)
    {
        if (estaMuerto) return;

        // Verificar invulnerabilidad
        if (Time.time - tiempoUltimoDano < tiempoInvulnerabilidad)
        {
            Debug.Log("Kaven está invulnerable");
            return;
        }

        vidaActual -= cantidad;
        vidaActual = Mathf.Clamp(vidaActual, 0, vidaMaxima);
        tiempoUltimoDano = Time.time;

        Debug.Log($"Kaven recibió {cantidad} de daño. Vida restante: {vidaActual}/{vidaMaxima}");

        ActualizarCorazones();

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    /// <summary>
    /// Actualiza la visualización de los 3 corazones individualmente
    /// </summary>
    void ActualizarCorazones()
    {
        if (corazon1 == null || corazon2 == null || corazon3 == null)
        {
            Debug.LogError("¡Faltan referencias de corazones en el Inspector!");
            return;
        }

        if (corazonLleno == null || corazonMedio == null || corazonVacio == null)
        {
            Debug.LogError("¡Faltan sprites de corazones en el Inspector!");
            return;
        }

        // Actualizar Corazón 1 (primeros 2 puntos de vida)
        ActualizarCorazonIndividual(corazon1, vidaActual);

        // Actualizar Corazón 2 (siguientes 2 puntos de vida)
        ActualizarCorazonIndividual(corazon2, vidaActual - 2f);

        // Actualizar Corazón 3 (últimos 2 puntos de vida)
        ActualizarCorazonIndividual(corazon3, vidaActual - 4f);
    }

    /// <summary>
    /// Actualiza un corazón individual según la vida que le corresponde
    /// </summary>
    void ActualizarCorazonIndividual(Image corazon, float vidaParaEsteCorazon)
    {
        if (vidaParaEsteCorazon >= 2f)
        {
            // Corazón lleno (2 puntos o más)
            corazon.sprite = corazonLleno;
        }
        else if (vidaParaEsteCorazon >= 1f)
        {
            // Medio corazón (1 punto)
            corazon.sprite = corazonMedio;
        }
        else
        {
            // Corazón vacío (0 puntos)
            corazon.sprite = corazonVacio;
        }
    }

    /// <summary>
    /// Curar a Kaven (para power-ups)
    /// </summary>
    public void Curar(float cantidad)
    {
        if (estaMuerto) return;

        vidaActual += cantidad;
        vidaActual = Mathf.Clamp(vidaActual, 0, vidaMaxima);
        ActualizarCorazones();
        Debug.Log($"Kaven curado {cantidad} puntos. Vida actual: {vidaActual}/{vidaMaxima}");
    }

    /// <summary>
    /// Maneja la muerte de Kaven
    /// </summary>
    void Morir()
    {
        if (estaMuerto) return;

        estaMuerto = true;
        Debug.Log("¡KAVEN HA MUERTO!");

        // Desactivar controles
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        // Desactivar scripts de movimiento
        New_CharacterController movimiento = GetComponent<New_CharacterController>();
        if (movimiento != null)
        {
            movimiento.enabled = false;
        }

        AtaqueBaculo ataque = GetComponent<AtaqueBaculo>();
        if (ataque != null)
        {
            ataque.enabled = false;
        }

        // Reiniciar nivel después de un tiempo
        if (reiniciarNivelAlMorir)
        {
            Invoke(nameof(ReiniciarNivel), tiempoAntesDeReiniciar);
        }
    }

    /// <summary>
    /// Reinicia el nivel actual
    /// </summary>
    void ReiniciarNivel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Método para verificar si está vivo
    public bool EstaVivo()
    {
        return !estaMuerto && vidaActual > 0;
    }

    // Método para obtener vida actual
    public float GetVidaActual()
    {
        return vidaActual;
    }

    // Método para obtener vida máxima
    public float GetVidaMaxima()
    {
        return vidaMaxima;
    }
}