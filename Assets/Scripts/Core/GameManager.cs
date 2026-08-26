public class GameManager
{
    // Singleton
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
    // Singleton

    private GameManager()
    {
        ResetGameManager();
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

    public WorldState worldState;
    public int currentRoom = 0;
    public int lives;
    public float mainCameraZBeforeZoom;

    public void ResetGameManager()
    {
        worldState = new WorldState();
        lives = 3;
    }
}
