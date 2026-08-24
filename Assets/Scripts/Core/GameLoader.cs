using UnityEngine;

public class GameLoader : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.state = GameManager.GameState.OUTERROOM;
    }

    
}
