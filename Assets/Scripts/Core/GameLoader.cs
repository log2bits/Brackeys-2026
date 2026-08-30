using UnityEngine;

public class GameLoader : MonoBehaviour
{
    private void Start()
    {
        GameManager.Instance.ResetGameManager();
        GameManager.Instance.state = GameManager.GameState.CUTSCENE;
        ProceduralRoomGen.Instance.GenerateProcess();
        
    }    
}
