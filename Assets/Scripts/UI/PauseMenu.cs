using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    // Singleton -------------------------------
    private static PauseMenu _instance;

    public static PauseMenu Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            Debug.LogWarning("Multiple PauseMenu scripts. Bad!");
        }
        else
        {
            _instance = this;
        }
    }
    // Singleton -------------------------------

    [Header("References")]
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject optionsMenuUIHolder;
    [SerializeField] private OptionsMenu optionsMenuScript;
    [SerializeField] private GameObject controlsMenuUI;

    private bool isPaused = false;
    private bool returningToMainMenu = false;
    private float previousTimescale;
    private InputAction pauseInput;

    private void Start()
    {
        pauseInput = InputSystem.actions.FindAction("Pause");
        optionsMenuScript.LoadOptions();
    }

    private void Update()
    {
        if (GameManager.Instance.state == GameManager.GameState.CUTSCENE || GameManager.Instance.state == GameManager.GameState.ENDCUTSCENE)
        {
            return;
        }
        
        if (!pauseInput.WasPressedThisFrame())
        {
            return;
        }

        if (isPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void LoadSavedOptions()
    {
        optionsMenuScript.LoadOptions();
    }

    public void Resume()
    {
        if (returningToMainMenu)
        {
            return;
        }

        pauseMenuUI.SetActive(false);
        optionsMenuUIHolder.SetActive(false);
        controlsMenuUI.SetActive(false);
        Time.timeScale = previousTimescale;
        isPaused = false;
        AudioManager.Instance.PauseFmodSounds(false);
    }

    public void Pause()
    {   
        pauseMenuUI.SetActive(true);
        previousTimescale = Time.timeScale;
        Time.timeScale = 0f;
        isPaused = true;
        AudioManager.Instance.PauseFmodSounds(true);
    }

    // Used by the menu button in the pause menu UI
    public void LoadMainMenu()
    {
        if (returningToMainMenu)
        {
            return;
        }
        returningToMainMenu = true;

        Time.timeScale = 1f;
        AudioManager.Instance.PauseFmodSounds(false);
        
        SceneTransition.Instance.FadeToBlack(MainMenuFinish);
    }

    private void MainMenuFinish()
    {
        SceneManager.LoadSceneAsync("MainMenu");
    }

    // Used by the options button in the pause menu UI
    public void LoadOptions()
    {
        pauseMenuUI.SetActive(false);
        optionsMenuUIHolder.SetActive(true);
    }

    // Used by the return button in the options menu
    public void ReturnFromOptions()
    {
        optionsMenuUIHolder.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

    // Used by the quit button in the pause menu UI
    public void QuitGame()
    {
        Debug.Log("Quitting game... will not work in Unity game preview (only in a built version of the game)");
        Application.Quit();
    }

    public bool GetIsPaused()
    {
        return isPaused;
    }
}
