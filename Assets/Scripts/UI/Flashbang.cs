using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class FlashbangEffect : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject flashbangCanvas;
    [SerializeField] private Volume postProcessVolume;

    [SerializeField] private Volume brightnessVolume;
    [SerializeField] private GameObject cutsceneCanvas;

    [Header("Settings")]
    [SerializeField] private float flashDuration = 1.0f;
    [SerializeField] private float fadeDuration = 3.0f;
    [SerializeField] private float brightnessStartWeight = 0.4f;

    private Coroutine flashCoroutine;

    public void TriggerFlashbang()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(DoFlashbang());
    }

    private IEnumerator DoFlashbang()
    {
        // Instantly make the screen white
        canvasGroup.alpha = 1f;
        postProcessVolume.weight = 1f;
        brightnessVolume.weight = brightnessStartWeight;

        // Stay completely white for a moment
        yield return new WaitForSeconds(flashDuration);

        // Hide the cutscene while the screen is still white
        if (cutsceneCanvas != null)
        {
            cutsceneCanvas.SetActive(false);
        }

        // Fade the flash away
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fadeDuration;

            canvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);
            postProcessVolume.weight = Mathf.Lerp(1f, 0f, progress);
            brightnessVolume.weight = Mathf.Lerp(brightnessStartWeight, 0f, progress);

            yield return null;
        }

        // Make sure everything is fully reset
        canvasGroup.alpha = 0f;
        postProcessVolume.weight = 0f;
        brightnessVolume.weight = 0f;
        
        flashbangCanvas.SetActive(false);
        flashCoroutine = null;
    }
}