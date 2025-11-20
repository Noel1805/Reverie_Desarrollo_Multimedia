using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MenuController : MonoBehaviour
{
    [Header("Referencias a los Botones")]
    public Button buttonPlay;
    public Button buttonQuit;

    [Header("Referencias a Canvas")]
    public GameObject menuPrincipal;
    public GameObject canvasControles;
    public Button buttonSkip;

    [Header("Configuración de Transición")]
    [SerializeField] private float transitionDuration = 1f;
    [SerializeField] private float canvasTransitionDuration = 1.5f; // Transición entre canvas (más lenta)
    [SerializeField] private Color fadeColor = Color.black;

    private GameObject fadePanel;
    private Image fadeImage;
    private bool isTransitioning = false;

    void Start()
    {
        // Crear panel de transición
        CreateFadePanel();

        // Asignar las funciones a los botones
        if (buttonPlay != null)
        {
            buttonPlay.onClick.AddListener(ShowControls);
        }

        if (buttonQuit != null)
        {
            buttonQuit.onClick.AddListener(QuitGame);
        }

        if (buttonSkip != null)
        {
            buttonSkip.onClick.AddListener(PlayGame);
        }

        // Asegurar que solo el menú principal esté activo al inicio
        if (menuPrincipal != null)
            menuPrincipal.SetActive(true);

        if (canvasControles != null)
            canvasControles.SetActive(false);

        // Fade in al iniciar la escena
        StartCoroutine(FadeIn());
    }

    private void CreateFadePanel()
    {
        // Crear GameObject para el panel de fade
        fadePanel = new GameObject("FadePanel");
        fadePanel.transform.SetParent(transform.root, false);

        // Obtener o crear Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            fadePanel.transform.SetParent(canvas.transform, false);
        }

        // Configurar RectTransform para cubrir toda la pantalla
        RectTransform rectTransform = fadePanel.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;

        // Agregar Image component
        fadeImage = fadePanel.AddComponent<Image>();
        fadeImage.color = fadeColor;

        // Poner el panel al frente de todo
        fadePanel.transform.SetAsLastSibling();

        // Iniciar completamente opaco
        Color initialColor = fadeColor;
        initialColor.a = 1f;
        fadeImage.color = initialColor;
    }

    // Fade in (aparecer desde negro)
    private IEnumerator FadeIn()
    {
        if (fadeImage == null) yield break;

        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / transitionDuration);

            Color color = fadeColor;
            color.a = alpha;
            fadeImage.color = color;

            yield return null;
        }

        // Asegurar que quede completamente transparente
        Color finalColor = fadeColor;
        finalColor.a = 0f;
        fadeImage.color = finalColor;

        fadePanel.SetActive(false);
    }

    // Fade out (desvanecer a negro)
    private IEnumerator FadeOut(string sceneName)
    {
        if (fadeImage == null) yield break;

        fadePanel.SetActive(true);
        fadePanel.transform.SetAsLastSibling();

        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / transitionDuration);

            Color color = fadeColor;
            color.a = alpha;
            fadeImage.color = color;

            yield return null;
        }

        // Asegurar que quede completamente opaco
        Color finalColor = fadeColor;
        finalColor.a = 1f;
        fadeImage.color = finalColor;

        // Cargar la nueva escena
        SceneManager.LoadScene(sceneName);
    }

    // Transición entre canvas (dentro de la misma escena)
    private IEnumerator TransitionBetweenCanvas(GameObject fromCanvas, GameObject toCanvas)
    {
        if (fadeImage == null) yield break;

        fadePanel.SetActive(true);
        fadePanel.transform.SetAsLastSibling();

        float elapsed = 0f;
        float halfDuration = canvasTransitionDuration * 0.5f;

        Debug.Log("Iniciando fade out del canvas actual...");

        // Fade out
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / halfDuration);

            Color color = fadeColor;
            color.a = alpha;
            fadeImage.color = color;

            yield return null;
        }

        // Asegurar opacidad completa
        Color midColor = fadeColor;
        midColor.a = 1f;
        fadeImage.color = midColor;

        Debug.Log("Cambiando canvas...");

        // Cambiar canvas en el punto más oscuro
        if (fromCanvas != null)
            fromCanvas.SetActive(false);

        if (toCanvas != null)
            toCanvas.SetActive(true);

        // Pequeña pausa en el medio para que se note el cambio
        yield return new WaitForSeconds(0.1f);

        elapsed = 0f;

        Debug.Log("Iniciando fade in del nuevo canvas...");

        // Fade in
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / halfDuration);

            Color color = fadeColor;
            color.a = alpha;
            fadeImage.color = color;

            yield return null;
        }

        // Asegurar que quede completamente transparente
        Color finalColor = fadeColor;
        finalColor.a = 0f;
        fadeImage.color = finalColor;

        fadePanel.SetActive(false);

        Debug.Log("Transición completada");
    }

    // Método para mostrar los controles (botón Play)
    public void ShowControls()
    {
        Debug.Log("ShowControls llamado");

        if (menuPrincipal == null)
        {
            Debug.LogError("Menu Principal no está asignado!");
            return;
        }

        if (canvasControles == null)
        {
            Debug.LogError("Canvas Controles no está asignado!");
            return;
        }

        if (isTransitioning) return;

        isTransitioning = true;
        Debug.Log("Iniciando transición a controles...");

        StartCoroutine(ShowControlsCoroutine());
    }

    private IEnumerator ShowControlsCoroutine()
    {
        yield return StartCoroutine(TransitionBetweenCanvas(menuPrincipal, canvasControles));
        isTransitioning = false;
        Debug.Log("Transición completada");
    }

    // MÉTODO ALTERNATIVO SIMPLE (para pruebas)
    public void ShowControlsSimple()
    {
        Debug.Log("ShowControlsSimple llamado");

        if (menuPrincipal != null)
        {
            menuPrincipal.SetActive(false);
            Debug.Log("Menu desactivado");
        }

        if (canvasControles != null)
        {
            canvasControles.SetActive(true);
            Debug.Log("Controles activados");
        }
    }
    public void PlayGame()
    {
        if (isTransitioning) return;

        isTransitioning = true;
        Debug.Log("Cargando SampleScene...");

        // Iniciar transición a la escena del juego
        StartCoroutine(FadeOut("SampleScene"));
    }

    // Método para el botón Quit
    public void QuitGame()
    {
        if (isTransitioning) return;

        isTransitioning = true;
        Debug.Log("Saliendo del juego...");

        // Hacer fade out antes de salir
        StartCoroutine(FadeOutAndQuit());
    }

    private IEnumerator FadeOutAndQuit()
    {
        if (fadeImage != null)
        {
            fadePanel.SetActive(true);
            fadePanel.transform.SetAsLastSibling();

            float elapsed = 0f;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / transitionDuration);

                Color color = fadeColor;
                color.a = alpha;
                fadeImage.color = color;

                yield return null;
            }
        }

        // Salir del juego
        Application.Quit();

        // Para que funcione en el Editor de Unity
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void OnDestroy()
    {
        // Limpiar los listeners al destruir
        if (buttonPlay != null)
        {
            buttonPlay.onClick.RemoveListener(ShowControls);
        }

        if (buttonQuit != null)
        {
            buttonQuit.onClick.RemoveListener(QuitGame);
        }

        if (buttonSkip != null)
        {
            buttonSkip.onClick.RemoveListener(PlayGame);
        }

        // Limpiar el panel de fade
        if (fadePanel != null)
        {
            Destroy(fadePanel);
        }
    }
}