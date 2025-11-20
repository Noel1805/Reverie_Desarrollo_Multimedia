using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

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

    [Header("Configuración de Game Over")]
    [SerializeField] private GameObject canvasGameOver;
    [SerializeField] private float tiempoAntesDeGameOver = 1.5f;
    [SerializeField] private float duracionTransicion = 1f;
    [SerializeField] private Color fadeColor = Color.black;

    private CharacterController characterController;
    private bool estaMuerto = false;
    private GameObject fadePanel;
    private Image fadeImage;

    void Start()
    {
        vidaActual = vidaMaxima;
        characterController = GetComponent<CharacterController>();
        ActualizarCorazones();

        // Asegurar que el canvas de Game Over esté desactivado
        if (canvasGameOver != null)
        {
            canvasGameOver.SetActive(false);
        }

        // Crear panel de fade
        CreateFadePanel();
    }

    private void CreateFadePanel()
    {
        // Buscar o crear el panel de fade
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            fadePanel = new GameObject("FadePanelGameOver");
            fadePanel.transform.SetParent(canvas.transform, false);

            RectTransform rectTransform = fadePanel.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;

            fadeImage = fadePanel.AddComponent<Image>();
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);

            fadePanel.SetActive(false);
        }
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
            corazon.sprite = corazonLleno;
        }
        else if (vidaParaEsteCorazon >= 1f)
        {
            corazon.sprite = corazonMedio;
        }
        else
        {
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

        // Mostrar pantalla de Game Over después de un tiempo
        StartCoroutine(MostrarGameOver());
    }

    /// <summary>
    /// Corrutina para mostrar el Game Over con transición
    /// </summary>
    private IEnumerator MostrarGameOver()
    {
        Debug.Log("=== INICIANDO GAME OVER ===");

        // Esperar un momento antes de la transición
        yield return new WaitForSeconds(tiempoAntesDeGameOver);

        if (canvasGameOver == null)
        {
            Debug.LogError("¡Canvas_Game_Over no está asignado en el Inspector!");
            yield break;
        }

        Debug.Log($"Canvas Game Over encontrado: {canvasGameOver.name}");

        // Asegurar que el RectTransform ocupe toda la pantalla
        RectTransform rectTransform = canvasGameOver.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.SetAsLastSibling(); // Poner al frente
        }

        // Obtener o añadir Canvas Group para controlar visibilidad
        CanvasGroup canvasGroup = canvasGameOver.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = canvasGameOver.AddComponent<CanvasGroup>();
        }

        // Activar el canvas pero invisible
        canvasGameOver.SetActive(true);
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // Fade to black (oscurecer pantalla)
        if (fadePanel != null && fadeImage != null)
        {
            fadePanel.SetActive(true);
            fadePanel.transform.SetAsLastSibling();

            float elapsed = 0f;
            while (elapsed < duracionTransicion)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / duracionTransicion);
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
                yield return null;
            }
        }

        // IMPORTANTE: Desbloquear y mostrar el cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("Cursor desbloqueado y visible");

        // Pausar el juego
        Time.timeScale = 0f;
        Debug.Log("Juego pausado - Time.timeScale = 0");

        // Hacer fade in del canvas de Game Over
        float elapsedFadeIn = 0f;
        while (elapsedFadeIn < duracionTransicion)
        {
            elapsedFadeIn += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedFadeIn / duracionTransicion);
            canvasGroup.alpha = alpha;
            yield return null;
        }

        // Asegurar visibilidad completa y habilitar interacción
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // Desvanecer el panel negro
        if (fadePanel != null && fadeImage != null)
        {
            float elapsedFadeOut = 0f;
            while (elapsedFadeOut < duracionTransicion * 0.5f)
            {
                elapsedFadeOut += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsedFadeOut / (duracionTransicion * 0.5f));
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
                yield return null;
            }
            fadePanel.SetActive(false);
        }

        Debug.Log("=== GAME OVER COMPLETADO - Botones listos para usar ===");
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

    void OnDestroy()
    {
        if (fadePanel != null)
        {
            Destroy(fadePanel);
        }

        // Restaurar el cursor al destruir el script
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}