using System;
using System.Collections;
using System.Linq;
using FMOD.Studio;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Difficulty References")]
    [SerializeField] private DifficultyTemplate[] difficulties;
    [SerializeField] private TextMeshProUGUI difficultiesText;
    [SerializeField] private TextMeshProUGUI difficultiesDescription;
    [SerializeField] private Slider difficultiesSlider;

    [Header("New Game/Play References")]
    [SerializeField] private RectTransform newGameUIElements;
    [SerializeField] private RectTransform playUIElements;
    [SerializeField] private Vector2 newGameHiddenPosition;
    [SerializeField] private Vector2 newGameShownPosition;
    [SerializeField] private Vector2 playHiddenPosition;
    [SerializeField] private Vector2 playShownPosition;

    [Header("Misc References")]
    [SerializeField] private GameObject optionsMenuUIHolder;

    [SerializeField] private TMP_InputField seedInputField;

    [SerializeField] private GameObject creditsMenuUIHolder;
    [SerializeField] private OptionsMenu optionsMenuScript;

    [Header("Parameters")]
    [SerializeField] private float menuTransitionTime;
    [Header("Menu Music")]
    [SerializeField] private EventReference menuMusic;
    private EventInstance menuMusicInstance;
    private bool goingToMainScene = false;
    private bool transitioning = false;
    private int difficultyIndex = 0;

    private void Start()
    {
        GameManager.Instance.state = GameManager.GameState.MAINMENU;
        optionsMenuScript.LoadOptions();
        SetDifficulty(0);

        // music register incase web build
        #if !UNITY_WEBGL
        StartMenuMusic();
        #elif UNITY_WEBGL
        RegisterMenuMusic();
        #endif
        

        SceneTransition.Instance.FadeFromBlack();
    }

    void RegisterMenuMusic()
    {
        EventBus.Instance.Register(EventBus.EventName.PlayerClicked, StartMenuMusic);
    }

    void StartMenuMusic()
    {
        FMODUnity.RuntimeManager.CoreSystem.mixerResume();

        menuMusicInstance = AudioManager.Instance.CreateInstance(menuMusic);
        menuMusicInstance.start();
        menuMusicInstance.release();
        // if null or not registered, it wont be a problem, will ignore
        EventBus.Instance.Deregister(EventBus.EventName.PlayerClicked, StartMenuMusic);
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

        if (difficultyIndex > GetHighestUnlockedDifficulty())
        {
            MainCameraMove.Instance.ShakeCamera(0.35f, 0.15f, 0.015f, 0.85f);
            return;
        }

        StopAllCoroutines();
        goingToMainScene = true;
        GameManager.Instance.currentSeed = GameManager.StringToRandomInt(seedInputField.text);
        GameManager.Instance.currentDifficulty = difficulties[difficultyIndex];
        SceneTransition.Instance.FadeToBlack(PlayGameFinish);
    }

    private void PlayGameFinish()
    {
        menuMusicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        SceneManager.LoadSceneAsync("Game");
    }

    public void SetDifficulty(float difficulty)
    {
        int difficultyIndex = Mathf.RoundToInt(difficulty);
        this.difficultyIndex = difficultyIndex;
        
        if (difficultyIndex > GetHighestUnlockedDifficulty())
        {
            difficultiesText.text = "Locked";
            difficultiesDescription.text = "???";
            return;
        }

        difficultiesText.text = difficulties[difficultyIndex].difficultyName;
        difficultiesDescription.text = difficulties[difficultyIndex].difficultyDescription;
    }

    public void FinishSetDifficulty()
    {
        difficultiesSlider.value = difficultyIndex;
    }

    private int GetHighestUnlockedDifficulty()
    {
        HighestBeatenDifficulty highestBeatenDifficulty = SaveSystem.GetHighestBeatenDifficulty();
        int highestDifficulty = -1;
        if (highestBeatenDifficulty != null)
        {
            highestDifficulty = highestBeatenDifficulty.highestBeatenDifficulty;
        }
        
        foreach (DifficultyTemplate difficulty in difficulties)
        {
            if (difficulty.difficultyRequiredToUnlock > highestDifficulty)
            {
                return difficulty.solverDifficulty - 1;
            }
        }
        return difficulties.Length - 1;
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
        menuMusicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        Debug.Log("Quitting game... will not work in Unity game preview (only in a built version of the game)");
        Application.Quit();
    }

    public void LoadCredits()
    {
        if (goingToMainScene || transitioning)
        {
            return;
        }

        creditsMenuUIHolder.SetActive(true);
    }

    public void ReturnFromCredits()
    {
        creditsMenuUIHolder.SetActive(false);
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

    [System.Serializable]
    public class HighestBeatenDifficulty
    {
        public int highestBeatenDifficulty;

        public HighestBeatenDifficulty(int highestBeatenDifficulty)
        {
            this.highestBeatenDifficulty = highestBeatenDifficulty;
        }
    }
}
