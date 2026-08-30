using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    // Singleton -------------------------------
    private static SceneTransition _instance;

    public static SceneTransition Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            Debug.LogWarning("Multiple SceneTransition scripts. Bad!");
        }
        else
        {
            _instance = this;
        }
    }
    // Singleton

    [Header("References")]
    [SerializeField] private Image fadeImage;

    [Header("Parameters")]
    [SerializeField] private float fadeTime;

    public void FadeToBlack(Action fadeCompleted = null)
    {
        CoroutineManager.Instance.Run(FadeImage(fadeImage, 0f, 1f, fadeTime, fadeCompleted));
    }

    public void FadeFromBlack(Action fadeCompleted = null)
    {
        CoroutineManager.Instance.Run(FadeImage(fadeImage, 1f, 0f, fadeTime, fadeCompleted));
    }

    private IEnumerator FadeImage(Image image, float startAlpha, float endAlpha, float fadeTime, Action fadeCompleted = null)
    {
        float timeStartedFade = Time.time;

        float alphaSet;
        while (Time.time - timeStartedFade < fadeTime)
        {
            yield return null;

            alphaSet = Mathf.Lerp(startAlpha, endAlpha, (Time.time - timeStartedFade) / fadeTime);
            image.color = new Color(image.color.r, image.color.g, image.color.b, alphaSet);
        }
        fadeCompleted?.Invoke();
    }
}
