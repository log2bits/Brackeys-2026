using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingCutsceneController : MonoBehaviour
{
	// Singleton -------------------------------
    private static EndingCutsceneController _instance;

    public static EndingCutsceneController Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            Debug.LogWarning("Multiple EndingCutsceneController scripts. Bad!");
        }
        else
        {
            _instance = this;
        }
    }
    // Singleton -------------------------------

	//[Header("References")]

    [Header("Parameters")]
    [SerializeField] private string endingDialogue = "I KNEW that thing was lying to me! I need to get the hell out of here.";
    [SerializeField] private float cameraMoveForwardDistance = 40f;
    [SerializeField] private float cameraMoveForwardTime = 10f;

	public void EndGame()
    {
		CoroutineManager.Instance.Run(WaitForDialogue());
	}

	private IEnumerator WaitForDialogue()
    {
		while (!Dialogue.Instance.StartDialogue(endingDialogue, true, RunEndGameCoroutine))
		{
			yield return null;
            GameManager.Instance.ChangeGameState(GameManager.GameState.ENDCUTSCENE);
		}
	}

	private void RunEndGameCoroutine(){
		CoroutineManager.Instance.Run(EndGameCoroutine());
	}

	private IEnumerator EndGameCoroutine()
    {
        GameManager.Instance.ChangeGameState(GameManager.GameState.ENDCUTSCENE);

        MainMenu.HighestBeatenDifficulty getHighestBeatenDifficulty = SaveSystem.GetHighestBeatenDifficulty();
        int highestDifficulty = -1;
        if (getHighestBeatenDifficulty != null)
        {
            highestDifficulty = getHighestBeatenDifficulty.highestBeatenDifficulty;
        }

        if (highestDifficulty < GameManager.Instance.currentDifficulty.solverDifficulty)
        {
            MainMenu.HighestBeatenDifficulty highestBeatenDifficulty = new MainMenu.HighestBeatenDifficulty(GameManager.Instance.currentDifficulty.solverDifficulty);
        	SaveSystem.SaveHighestBeatenDifficulty(highestBeatenDifficulty);
        }

        MainCameraMove.Instance.MoveCamera(MainCameraMove.Instance.transform.position + new Vector3(0, 0, cameraMoveForwardDistance), cameraMoveForwardTime);
        
        yield return new WaitForSeconds(cameraMoveForwardTime / 2f);
        SceneTransition.Instance.FadeToBlack(MainMenuFinish);
    }

    private void MainMenuFinish()
    {
        SceneManager.LoadSceneAsync("MainMenu");
    }
}