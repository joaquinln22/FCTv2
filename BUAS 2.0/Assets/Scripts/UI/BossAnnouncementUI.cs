using System.Collections;
using UnityEngine;
using TMPro;

public class BossAnnouncementUI : MonoBehaviour
{
    [Header("Refs")]
    public CanvasGroup group;
    public TextMeshProUGUI label;

    [Header("Timing (s)")]
    public float fadeIn = 0.25f;
    public float hold   = 2.0f;
    public float fadeOut= 0.40f;

    private Coroutine running;

    private void Reset()
    {
        group = GetComponent<CanvasGroup>();
        label = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Awake()
    {
        // Mantener SIEMPRE activo y oculto por alpha
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        if (group != null)
        {
            group.alpha = 0f;
            group.blocksRaycasts = false;
        }
    }

    public void Show(string message, float? customHold = null)
    {
        // Asegura que está activo ANTES de empezar la coroutine
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        if (!enabled) enabled = true;

        if (label != null) label.text = message;

        if (running != null) StopCoroutine(running);
        running = StartCoroutine(ShowRoutine(customHold ?? hold));
    }

    private IEnumerator ShowRoutine(float holdSeconds)
    {
        // Fade in
        yield return Fade(0f, 1f, fadeIn);

        // Mantener visible
        float t = 0f;
        while (t < holdSeconds)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        // Fade out (dejamos el GO activo; solo bajamos alpha)
        yield return Fade(1f, 0f, fadeOut);

        running = null;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        group.alpha = from;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        group.alpha = to;
    }
}
