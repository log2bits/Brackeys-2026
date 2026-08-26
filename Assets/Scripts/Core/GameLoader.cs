using UnityEngine;

public class GameLoader : MonoBehaviour
{
    private void Start()
    {
        GameManager.Instance.ResetGameManager();
        GameManager.Instance.state = GameManager.GameState.OUTERROOM;
        ProceduralRoomGen.Instance.GenerateProcess();
    }    
}
