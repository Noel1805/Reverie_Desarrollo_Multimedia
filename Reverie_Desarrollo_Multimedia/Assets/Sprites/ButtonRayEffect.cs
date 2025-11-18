using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonRayEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Configuración de Escala")]
    [SerializeField] private float hoverScale = 1.15f;
    [SerializeField] private float scaleDuration = 0.2f;

    [Header("Configuración de Resplandor")]
    [SerializeField] private Color glowColor1 = new Color(0.3f, 0.7f, 1f, 1f); // Azul cyan
    [SerializeField] private Color glowColor2 = new Color(1f, 0.3f, 0.8f, 1f); // Rosa/Magenta
    [SerializeField] private float colorTransitionSpeed = 1.5f;
    [SerializeField] private float pulseSpeed = 3f;
    [SerializeField] private float minIntensity = 0.5f;
    [SerializeField] private float maxIntensity = 1f;
    [SerializeField] private float glowSize = 150f;

    private Image buttonImage;
    private GameObject glowObject;
    private Image glowImage;
    private Color originalColor;
    private Vector3 originalScale;
    private bool isHovering = false;
    private Coroutine scaleCoroutine;
    private Coroutine pulseCoroutine;

    void Start()
    {
        originalScale = transform.localScale;
        buttonImage = GetComponent<Image>();

        if (buttonImage != null)
        {
            originalColor = buttonImage.color;
        }

        CreateGlowEffect();
    }

    private void CreateGlowEffect()
    {
        // Crear objeto para el resplandor
        glowObject = new GameObject("GlowEffect");
        glowObject.transform.SetParent(transform, false);

        // Configurar RectTransform
        RectTransform glowRect = glowObject.AddComponent<RectTransform>();
        glowRect.anchorMin = new Vector2(0.5f, 0.5f);
        glowRect.anchorMax = new Vector2(0.5f, 0.5f);
        glowRect.pivot = new Vector2(0.5f, 0.5f);
        glowRect.anchoredPosition = Vector2.zero;
        glowRect.sizeDelta = new Vector2(glowSize, glowSize);

        // Mover detrás del botón
        glowObject.transform.SetAsFirstSibling();

        // Agregar Image component
        glowImage = glowObject.AddComponent<Image>();

        // Crear sprite circular con gradiente
        Texture2D glowTexture = CreateRadialGradientTexture(256);
        Sprite glowSprite = Sprite.Create(
            glowTexture,
            new Rect(0, 0, glowTexture.width, glowTexture.height),
            new Vector2(0.5f, 0.5f)
        );

        glowImage.sprite = glowSprite;
        glowImage.color = new Color(glowColor1.r, glowColor1.g, glowColor1.b, 0f);

        glowObject.SetActive(false);
    }

    private Texture2D CreateRadialGradientTexture(int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float maxDistance = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float normalizedDistance = distance / maxDistance;

                // Crear gradiente suave desde el centro
                float alpha = Mathf.Clamp01(1f - normalizedDistance);
                alpha = Mathf.Pow(alpha, 2f); // Hacer el gradiente más suave

                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        return texture;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;

        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleButton(originalScale * hoverScale));

        if (glowObject != null)
            glowObject.SetActive(true);

        if (pulseCoroutine != null)
            StopCoroutine(pulseCoroutine);
        pulseCoroutine = StartCoroutine(PulseGlow());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;

        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleButton(originalScale));

        if (pulseCoroutine != null)
            StopCoroutine(pulseCoroutine);
        pulseCoroutine = StartCoroutine(FadeOutGlow());
    }

    private IEnumerator ScaleButton(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < scaleDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / scaleDuration);
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale;
    }

    private IEnumerator PulseGlow()
    {
        if (glowImage == null) yield break;

        // Fade in inicial
        float fadeInTime = 0.2f;
        float elapsed = 0f;

        while (elapsed < fadeInTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, minIntensity, elapsed / fadeInTime);
            glowImage.color = new Color(glowColor1.r, glowColor1.g, glowColor1.b, alpha);
            yield return null;
        }

        // Pulso continuo con transición de colores
        while (isHovering)
        {
            float time = Time.time;

            // Pulsación de intensidad
            float pulse = Mathf.Lerp(minIntensity, maxIntensity, (Mathf.Sin(time * pulseSpeed) + 1f) / 2f);

            // Transición de color entre color1 y color2
            float colorLerp = (Mathf.Sin(time * colorTransitionSpeed) + 1f) / 2f;
            Color currentColor = Color.Lerp(glowColor1, glowColor2, colorLerp);

            // Aplicar color con pulsación de alpha
            glowImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, pulse);

            // Rotar ligeramente para más dinamismo
            if (glowObject != null)
            {
                glowObject.transform.Rotate(0f, 0f, Time.deltaTime * 20f);
            }

            yield return null;
        }
    }

    private IEnumerator FadeOutGlow()
    {
        if (glowImage == null) yield break;

        Color startColor = glowImage.color;
        float elapsed = 0f;
        float fadeDuration = 0.3f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            float alpha = Mathf.Lerp(startColor.a, 0f, t);
            glowImage.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        glowImage.color = new Color(glowColor1.r, glowColor1.g, glowColor1.b, 0f);

        if (glowObject != null)
            glowObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (glowObject != null)
            Destroy(glowObject);
    }
}