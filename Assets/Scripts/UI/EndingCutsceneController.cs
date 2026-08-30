using System.Collections;
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
    [SerializeField] private float postDialogueWaitTime = 5f;

	public void EndGame()
    {
		CoroutineManager.Instance.Run(WaitForDialogue());
	}

	private IEnumerator WaitForDialogue()
    {
		while (!Dialogue.Instance.StartDialogue(endingDialogue, true, RunEndGameCoroutine))
		{
			yield return null;
		}
        GameManager.Instance.ChangeGameState(GameManager.GameState.CUTSCENE);
	}

	private void RunEndGameCoroutine(){
		CoroutineManager.Instance.Run(EndGameCoroutine());
	}

	private IEnumerator EndGameCoroutine()
    {
        yield return new WaitForSeconds(postDialogueWaitTime);

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

        SceneManager.LoadSceneAsync("MainMenu");
    }

}