using UnityEngine;

public class GameManager
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameManager();
            }
            return _instance; 
        }
    }

    private GameManager()
    {
        Debug.Log("GameManager initalized");        
    }

    public enum GameState
    {
        PREGAME,
        INNERROOM,
        OUTERROOM,
        TRANSITIONROOM,
        ZOOMING,
        GAMEOVER
    }

    public GameState state;

    // Managers
    //public ProceduralRoomGen proceduralRoomGenManager;
    public WorldState worldState = new WorldState();

    public float mainCameraZBeforeZoom;
    
}
