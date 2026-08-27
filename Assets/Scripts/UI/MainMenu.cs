using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DifficultyTemplate[] difficulties;
    [SerializeField] private TextMeshProUGUI difficultiesText;

    [SerializeField] private RectTransform newGameUIElements;
    [SerializeField] private RectTransform playUIElements;
    [SerializeField] private Vector2 newGameHiddenPosition;
    [SerializeField] private Vector2 newGameShownPosition;
    [SerializeField] private Vector2 playHiddenPosition;
    [SerializeField] private Vector2 playShownPosition;

    [SerializeField] private TMP_InputField seedInputField;

    [SerializeField] private GameObject optionsMenuUIHolder;
    [SerializeField] private OptionsMenu optionsMenuScript;

    [Header("Parameters")]
    [SerializeField] private float menuTransitionTime;

    private bool goingToMainScene = false;
    private bool transitioning = false;
    private int difficultyIndex = 0;

    private void Start()
    {
        GameManager.Instance.state = GameManager.GameState.PREGAME;
        optionsMenuScript.LoadOptions();
        SetDifficulty(0);
    }

    public void NewGame()
    {
        if (goingToMainScene || transitioning)
        {
            return;
        }

        StopAllCoroutines();
        transitioning = true;
        StartCoroutine(MoveRect(newGameUIElements, newGameShownPosition, newGameHiddenPosition, menuTransitionTime, FinishTransition));
        StartCoroutine(MoveRect(playUIElements, playHiddenPosition, playShownPosition, menuTransitionTime, FinishTransition));

        seedInputField.text = GameManager.GenerateRandomString();
    }

    public void BackToNewGame()
    {
        if (goingToMainScene || transitioning)
        {
            return;
        }

        StopAllCoroutines();
        transitioning = true;
        StartCoroutine(MoveRect(newGameUIElements, newGameHiddenPosition, newGameShownPosition, menuTransitionTime, FinishTransition));
        StartCoroutine(MoveRect(playUIElements, playShownPosition, playHiddenPosition, menuTransitionTime, FinishTransition));
    }

    private void FinishTransition()
    {
        transitioning = false;
    }

    public void PlayGame()
    {
        if (goingToMainScene || transitioning)
        {
            return;
        }

        StopAllCoroutines();
        goingToMainScene = true;
        GameManager.Instance.currentSeed = GameManager.StringToRandomInt(seedInputField.text);
        GameManager.Instance.currentDifficulty = difficulties[difficultyIndex];
        PlayGameFinish();
    }

    private void PlayGameFinish()
    {
        SceneManager.LoadSceneAsync("Game");
    }

    public void SetDifficulty(float difficulty)
    {
        int difficultyIndex = Mathf.FloorToInt(difficulty);
        this.difficultyIndex = difficultyIndex;

        difficultiesText.text = difficulties[difficultyIndex].difficultyName;
    }

    private IEnumerator MoveRect(RectTransform rectTransform, Vector2 startPosition, Vector2 endPosition, float time, Action moveCompleted = null)
    {
        float timeStartedFade = Time.time;
        Vector2 positionSet;
        while (Time.time - timeStartedFade < time)
        {
            yield return null;

            positionSet = Vector2.Lerp(startPosition, endPosition, (Time.time - timeStartedFade) / time);
            rectTransform.anchoredPosition = positionSet;
        }
        moveCompleted?.Invoke();
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game... will not work in Unity game preview (only in a built version of the game)");
        Application.Quit();
    }

    public void LoadOptions()
    {
        if (goingToMainScene || transitioning)
        {
            return;
        }

        optionsMenuUIHolder.SetActive(true);
    }

    //used by the return button in the options menu
    public void ReturnFromOptions()
    {
        optionsMenuUIHolder.SetActive(false);
    }
}
