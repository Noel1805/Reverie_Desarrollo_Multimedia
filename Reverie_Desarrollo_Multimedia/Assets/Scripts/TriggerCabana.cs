using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class TriggerCabana : MonoBehaviour
{
    [Header("Referencias de Canvas")]
    [SerializeField] private GameObject canvasFinalBueno;
    [SerializeField] private GameObject canvasFinalMalo;

    [Header("Configuración de Transición")]
    [SerializeField] private float duracionFade = 1.5f;
    [SerializeField] private float tiempoAntesDeActivarCanvas = 1f;
    [SerializeField] private Color colorFade = Color.black;

    [Header("Frutas Necesarias")]
    [SerializeField] private int frutasNecesariasParaBuenFinal = 9;

    private bool yaActivado = false;
    private GameObject fadePanel;
    private Image fadeImage;
    private CharacterController playerController;
    private New_CharacterController playerMovement;

    void Start()
    {

        if (canvasFinalBueno != null)
            canvasFinalBueno.SetActive(false);

        if (canvasFinalMalo != null)
            canvasFinalMalo.SetActive(false);


        CreateFadePanel();


        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError("❌ TriggerCabana necesita un Collider!");
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning("⚠️ El Collider de la cabaña debe tener 'Is Trigger' activado");
        }
    }

    private void CreateFadePanel()
    {

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            fadePanel = new GameObject("FadePanelCabana");
            fadePanel.transform.SetParent(canvas.transform, false);

            RectTransform rectTransform = fadePanel.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;

            fadeImage = fadePanel.AddComponent<Image>();
            fadeImage.color = new Color(colorFade.r, colorFade.g, colorFade.b, 0f);
            fadeImage.raycastTarget = false; 

            fadePanel.SetActive(false);
        }
        else
        {
            Debug.LogError("❌ No se encontró un Canvas en la escena para el fade");
        }
    }

    void OnTriggerEnter(Collider other)
    {

        if (yaActivado) return;

        if (other.CompareTag("Player"))
        {
            yaActivado = true;


            playerController = other.GetComponent<CharacterController>();
            playerMovement = other.GetComponent<New_CharacterController>();

            Debug.Log("🏠 Jugador entró a la cabaña - Iniciando transición al final");


            StartCoroutine(TransicionAlFinal());
        }
    }

    private IEnumerator TransicionAlFinal()
    {

        if (playerController != null)
            playerController.enabled = false;
        if (playerMovement != null)
            playerMovement.enabled = false;


        if (fadePanel != null && fadeImage != null)
        {
            fadePanel.SetActive(true);
            fadePanel.transform.SetAsLastSibling(); 

            float elapsed = 0f;
            while (elapsed < duracionFade)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / duracionFade);
                fadeImage.color = new Color(colorFade.r, colorFade.g, colorFade.b, alpha);
                yield return null;
            }
        }


        yield return new WaitForSeconds(tiempoAntesDeActivarCanvas);


        int frutasRecogidas = ObtenerFrutasRecogidas();
        bool todosLasConsiguio = (frutasRecogidas >= frutasNecesariasParaBuenFinal);

        if (todosLasConsiguio)
        {
            Debug.Log($"✨ FINAL BUENO - Frutas: {frutasRecogidas}/{frutasNecesariasParaBuenFinal}");
            ActivarFinalBueno();
        }
        else
        {
            Debug.Log($"😢 FINAL MALO - Frutas: {frutasRecogidas}/{frutasNecesariasParaBuenFinal}");
            ActivarFinalMalo();
        }


        if (fadePanel != null && fadeImage != null)
        {
            float elapsed = 0f;
            while (elapsed < duracionFade)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / duracionFade);
                fadeImage.color = new Color(colorFade.r, colorFade.g, colorFade.b, alpha);
                yield return null;
            }

            fadePanel.SetActive(false);
        }


        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("🎬 Transición al final completada");
    }


    private int ObtenerFrutasRecogidas()
    {
        if (FruitCounterUI.Instance != null)
        {

            return FruitCounterUI.Instance.GetFrutasRecogidas();
        }

        Debug.LogError("❌ No se encontró FruitCounterUI.Instance");
        return 0;
    }

    private void ActivarFinalBueno()
    {
        if (canvasFinalBueno != null)
        {
            canvasFinalBueno.SetActive(true);


            RectTransform rectTransform = canvasFinalBueno.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.sizeDelta = Vector2.zero;
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.SetAsLastSibling();
            }


            CanvasGroup canvasGroup = canvasFinalBueno.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = canvasFinalBueno.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            Debug.LogError("❌ Canvas_Final_Bueno no está asignado en el Inspector!");
        }
    }

    private void ActivarFinalMalo()
    {
        if (canvasFinalMalo != null)
        {
            canvasFinalMalo.SetActive(true);


            RectTransform rectTransform = canvasFinalMalo.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.sizeDelta = Vector2.zero;
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.SetAsLastSibling();
            }


            CanvasGroup canvasGroup = canvasFinalMalo.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = canvasFinalMalo.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            Debug.LogError("❌ Canvas_Final_Malo no está asignado en el Inspector!");
        }
    }

    void OnDrawGizmos()
    {

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f); 
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
}