using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class VidaKaven : MonoBehaviour
{
    [Header("Configuración de Vida")]
    [SerializeField] private float vidaMaxima = 6f; 
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


    [Header("Power-Ups / Estados especiales")]
    [SerializeField] private bool invulnerablePorPowerUp = false;
    [SerializeField] private float tiempoRestanteInvulnerabilidad = 0f;
    private Coroutine invulnerabilidadPowerUpActiva;

    private CharacterController characterController;
    private bool estaMuerto = false;
    private GameObject fadePanel;
    private Image fadeImage;

    void Start()
    {
        vidaActual = vidaMaxima;
        characterController = GetComponent<CharacterController>();
        ActualizarCorazones();


        if (canvasGameOver != null)
        {
            canvasGameOver.SetActive(false);
        }


        CreateFadePanel();
    }

    private void CreateFadePanel()
    {

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


    public void RecibirDano(float cantidad, bool ignorarInvulnerabilidad = false)
    {
        if (estaMuerto) return;


        if (invulnerablePorPowerUp)
            return;


        if (!ignorarInvulnerabilidad && tiempoInvulnerabilidad > 0f)
        {
            if (Time.time - tiempoUltimoDano < tiempoInvulnerabilidad)
            {
                Debug.Log("Kaven está invulnerable por cooldown de daño");
                return;
            }
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


        ActualizarCorazonIndividual(corazon1, vidaActual);


        ActualizarCorazonIndividual(corazon2, vidaActual - 2f);


        ActualizarCorazonIndividual(corazon3, vidaActual - 4f);
    }


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


    public void Curar(float cantidad)
    {
        if (estaMuerto) return;

        vidaActual += cantidad;
        vidaActual = Mathf.Clamp(vidaActual, 0, vidaMaxima);
        ActualizarCorazones();
        Debug.Log($"Kaven curado {cantidad} puntos. Vida actual: {vidaActual}/{vidaMaxima}");
    }


    public void ActivarInvulnerabilidadTemporal(float duracion)
    {
        if (estaMuerto) return;

        if (!gameObject.activeInHierarchy)
            return;


        if (invulnerabilidadPowerUpActiva != null)
        {
            StopCoroutine(invulnerabilidadPowerUpActiva);
        }

        invulnerabilidadPowerUpActiva = StartCoroutine(InvulnerabilidadTemporalCoroutine(duracion));
    }


    private IEnumerator InvulnerabilidadTemporalCoroutine(float duracion)
    {
        invulnerablePorPowerUp = true;
        tiempoRestanteInvulnerabilidad = duracion;
        Debug.Log($"[VidaKaven] 🛡 Invulnerabilidad ACTIVADA por {duracion} segundos (PowerUp Pera)");

        while (tiempoRestanteInvulnerabilidad > 0f)
        {
            tiempoRestanteInvulnerabilidad -= Time.deltaTime;
            yield return null;
        }

        invulnerablePorPowerUp = false;
        tiempoRestanteInvulnerabilidad = 0f;
        invulnerabilidadPowerUpActiva = null;
        Debug.Log("[VidaKaven] ⏱ Invulnerabilidad de power-up FINALIZADA");
    }


    void Morir()
    {
        if (estaMuerto) return;

        estaMuerto = true;
        Debug.Log("¡KAVEN HA MUERTO!");


        if (characterController != null)
        {
            characterController.enabled = false;
        }


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


        StartCoroutine(MostrarGameOver());
    }


    private IEnumerator MostrarGameOver()
    {
        Debug.Log("=== INICIANDO GAME OVER ===");


        yield return new WaitForSeconds(tiempoAntesDeGameOver);

        if (canvasGameOver == null)
        {
            Debug.LogError("¡Canvas_Game_Over no está asignado en el Inspector!");
            yield break;
        }

        Debug.Log($"Canvas Game Over encontrado: {canvasGameOver.name}");


        RectTransform rectTransform = canvasGameOver.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.SetAsLastSibling(); 
        }


        CanvasGroup canvasGroup = canvasGameOver.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = canvasGameOver.AddComponent<CanvasGroup>();
        }


        canvasGameOver.SetActive(true);
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;


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


        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("Cursor desbloqueado y visible");


        Time.timeScale = 0f;
        Debug.Log("Juego pausado - Time.timeScale = 0");


        float elapsedFadeIn = 0f;
        while (elapsedFadeIn < duracionTransicion)
        {
            elapsedFadeIn += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedFadeIn / duracionTransicion);
            canvasGroup.alpha = alpha;
            yield return null;
        }


        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;


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


    public bool EstaVivo()
    {
        return !estaMuerto && vidaActual > 0;
    }


    public float GetVidaActual()
    {
        return vidaActual;
    }


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


        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
