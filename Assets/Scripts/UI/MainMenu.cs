using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject optionsMenuUIHolder;
    [SerializeField] private OptionsMenu optionsMenuScript;

    // For use when scene transitions are added
    private bool goingToMainScene = false;

    private void Start()
    {
        optionsMenuScript.LoadOptions();
    }

    public void PlayGame()
    {
        if (goingToMainScene)
        {
            return;
        }

        StopAllCoroutines();
        goingToMainScene = true;
        PlayGameFinish();
    }

    private void PlayGameFinish()
    {
        SceneManager.LoadSceneAsync("Game");
    }

    /*private IEnumerator MoveRect(RectTransform rectTransform, float deltaY, float time, Action moveCompleted = null)
    {
        float timeStartedFade = Time.time;
        float startingY = rectTransform.anchoredPosition.y;
        float ySet;
        while (Time.time - timeStartedFade < time)
        {
            yield return null;

            ySet = Mathf.SmoothStep(startingY, startingY + deltaY, (Time.time - timeStartedFade) / time);
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, ySet);
        }
        moveCompleted?.Invoke();
    }*/

    public void QuitGame()
    {
        Debug.Log("Quitting game... will not work in Unity game preview (only in a built version of the game)");
        Application.Quit();
    }

    public void LoadOptions()
    {
        optionsMenuUIHolder.SetActive(true);
    }

    //used by the return button in the options menu
    public void ReturnFromOptions()
    {
        optionsMenuUIHolder.SetActive(false);
    }
}
