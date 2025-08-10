using UnityEngine;
using UnityEngine.EventSystems; // Required for pointer events

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private CanvasGroup targetCanvasGroup; // The UI element to fade
    [SerializeField] private float fadeDuration = 0.3f;
    public GameObject bubble;

    private Coroutine fadeCoroutine;

    public void OnPointerEnter(PointerEventData eventData)
    {
        bubble.SetActive(true);
        StartFade(1f); // Fade in
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartFade(0f); // Fade out
    }

    private void StartFade(float targetAlpha)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeTo(targetAlpha));
    }

    private System.Collections.IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = targetCanvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            targetCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }
        
        if (targetAlpha == 0f)
        {
            bubble.SetActive(false);
        }

        targetCanvasGroup.alpha = targetAlpha;
    }
}