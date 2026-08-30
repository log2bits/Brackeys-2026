using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class FlashbangEffect : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private GameObject flashbangCanvas;
    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private GameObject cutsceneCanvas;

    [Header("Settings")]
    [SerializeField] private float flashDuration = 0.5f;
    [SerializeField] private float fadeDuration = 2.0f;

    private IEnumerator flashCoroutine;

    public void TriggerFlashbang()
    {
        if (flashCoroutine != null)
        {
            CoroutineManager.Instance.Stop(flashCoroutine);
        }

        flashCoroutine = DoFlashbang();
        CoroutineManager.Instance.Run(flashCoroutine);
    }

    private IEnumerator DoFlashbang()
    {
        // Instantly make the screen white
        postProcessVolume.weight = 1f;

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

            postProcessVolume.weight = Mathf.Lerp(1f, 0f, progress);
            postProcessVolume.weight *= postProcessVolume.weight;

            yield return null;
        }

        // Make sure everything is fully reset
        postProcessVolume.weight = 0f;
        
        flashbangCanvas.SetActive(false);
        flashCoroutine = null;

        GameManager.Instance.ChangeGameState(GameManager.GameState.OUTERROOM);
    }
}