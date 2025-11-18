using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MenuController : MonoBehaviour
{
    [Header("Referencias a los Botones")]
    public Button buttonPlay;
    public Button buttonQuit;

    [Header("Configuración de Transición")]
    [SerializeField] private float transitionDuration = 1f;
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
            buttonPlay.onClick.AddListener(PlayGame);
        }

        if (buttonQuit != null)
        {
            buttonQuit.onClick.AddListener(QuitGame);
        }

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

    // Método para el botón Play
    public void PlayGame()
    {
        if (isTransitioning) return;

        isTransitioning = true;
        Debug.Log("Cargando SampleScene...");

        // Iniciar transición
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
            buttonPlay.onClick.RemoveListener(PlayGame);
        }

        if (buttonQuit != null)
        {
            buttonQuit.onClick.RemoveListener(QuitGame);
        }

        // Limpiar el panel de fade
        if (fadePanel != null)
        {
            Destroy(fadePanel);
        }
    }
}