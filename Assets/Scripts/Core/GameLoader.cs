using UnityEngine;

public class GameLoader : MonoBehaviour
{
    private void Start()
    {
        ProceduralRoomGen.Instance.GenerateProcess();
        GameManager.Instance.state = GameManager.GameState.OUTERROOM;
    }    
}
