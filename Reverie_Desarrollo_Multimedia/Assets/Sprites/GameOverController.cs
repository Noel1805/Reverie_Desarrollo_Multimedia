using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverController : MonoBehaviour
{
    [Header("Referencias a los Botones")]
    public Button buttonRestart;
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

        // CRÍTICO: Asegurar que el cursor esté visible y desbloqueado
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("GameOverController Start: Cursor desbloqueado y visible");

        // Asignar funciones a los botones
        if (buttonRestart != null)
        {
            buttonRestart.onClick.AddListener(Restart);
            Debug.Log("✓ Botón Restart configurado correctamente");
        }
        else
        {
            Debug.LogError("✗ buttonRestart NO está asignado en el Inspector!");
        }

        if (buttonQuit != null)
        {
            buttonQuit.onClick.AddListener(QuitToMenu);
            Debug.Log("✓ Botón Quit configurado correctamente");
        }
        else
        {
            Debug.LogError("✗ buttonQuit NO está asignado en el Inspector!");
        }
    }

    void OnEnable()
    {
        // Cada vez que el Game Over se active, forzar cursor visible
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("GameOverController OnEnable: Cursor forzado a visible");
    }

    void Update()
    {
        // IMPORTANTE: Forzar el cursor visible mientras el Game Over esté activo
        // Esto previene que otros scripts lo bloqueen
        if (gameObject.activeInHierarchy)
        {
            if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            if (!Cursor.visible)
            {
                Cursor.visible = true;
            }
        }
    }

    private void CreateFadePanel()
    {
        fadePanel = new GameObject("FadePanelGameOverController");
        fadePanel.transform.SetParent(transform, false);

        RectTransform rectTransform = fadePanel.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;

        fadeImage = fadePanel.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);

        fadePanel.SetActive(false);
    }

    /// <summary>
    /// Reiniciar el nivel actual
    /// </summary>
    public void Restart()
    {
        if (isTransitioning) return;

        isTransitioning = true;
        Debug.Log("Reiniciando nivel...");

        StartCoroutine(RestartCoroutine());
    }

    private IEnumerator RestartCoroutine()
    {
        // Mantener cursor visible durante la transición
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Fade out
        if (fadePanel != null && fadeImage != null)
        {
            fadePanel.SetActive(true);
            fadePanel.transform.SetAsLastSibling();

            float elapsed = 0f;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime; // Usar unscaledDeltaTime por seguridad
                float alpha = Mathf.Lerp(0f, 1f, elapsed / transitionDuration);
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
                yield return null;
            }

            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
        }

        // Reanudar el tiempo DESPUÉS del fade
        Time.timeScale = 1f;

        // Pequeña espera para asegurar estabilidad
        yield return new WaitForSecondsRealtime(0.1f);

        // Recargar la escena actual
        Debug.Log("Cargando escena: " + SceneManager.GetActiveScene().name);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Volver al menú principal
    /// </summary>
    public void QuitToMenu()
    {
        if (isTransitioning) return;

        isTransitioning = true;
        Debug.Log("Volviendo al menú...");

        StartCoroutine(QuitToMenuCoroutine());
    }

    private IEnumerator QuitToMenuCoroutine()
    {
        // Mantener cursor visible durante la transición
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Fade out
        if (fadePanel != null && fadeImage != null)
        {
            fadePanel.SetActive(true);
            fadePanel.transform.SetAsLastSibling();

            float elapsed = 0f;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime; // Usar unscaledDeltaTime por seguridad
                float alpha = Mathf.Lerp(0f, 1f, elapsed / transitionDuration);
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
                yield return null;
            }

            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
        }

        // Reanudar el tiempo DESPUÉS del fade
        Time.timeScale = 1f;

        // Pequeña espera para asegurar estabilidad
        yield return new WaitForSecondsRealtime(0.1f);

        // Cargar la escena del menú
        // IMPORTANTE: Cambia "Menu" por el nombre EXACTO de tu escena de menú
        string menuSceneName = "Menu"; // Cambia esto si tu escena tiene otro nombre

        // Verificar si la escena existe en el build
        bool sceneExists = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneName == menuSceneName)
            {
                sceneExists = true;
                break;
            }
        }

        if (sceneExists)
        {
            Debug.Log($"Cargando escena: {menuSceneName}");
            SceneManager.LoadScene(menuSceneName);
        }
        else
        {
            Debug.LogError($"¡ERROR! La escena '{menuSceneName}' no existe en Build Settings. Agrega la escena en File → Build Settings");
            // Como alternativa, cargar la primera escena del build (usualmente el menú)
            Debug.Log("Intentando cargar la primera escena del build como fallback...");
            SceneManager.LoadScene(0);
        }
    }

    void OnDestroy()
    {
        // Asegurarse de que el tiempo se reanude
        Time.timeScale = 1f;

        // Restaurar el cursor por si acaso
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Limpiar listeners
        if (buttonRestart != null)
        {
            buttonRestart.onClick.RemoveListener(Restart);
        }

        if (buttonQuit != null)
        {
            buttonQuit.onClick.RemoveListener(QuitToMenu);
        }

        if (fadePanel != null)
        {
            Destroy(fadePanel);
        }

        Debug.Log("GameOverController destruido - Cursor restaurado");
    }
}