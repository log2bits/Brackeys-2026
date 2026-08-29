using System.Text;

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
        MAINMENU,
        CUTSCENE,
        INNERROOM,
        OUTERROOM,
        TRANSITIONROOM,
        ZOOMING,
        GAMEOVER
    }

    public GameState state;

    public WorldState worldState;
    public DifficultyTemplate currentDifficulty;
    public int currentRoom;
    public int currentSeed;
    public int lives;
    public float mainCameraZBeforeZoom;

    public void ResetGameManager()
    {
        worldState = new WorldState();
        lives = 3;
        currentRoom = 0;
    }

    public static string GenerateRandomString()
    {
        string randomString = "";
        string sampleString = "234567890bcdfghjklmnpqrstvwxzBCDFGHJKLMNPQRSTVWXZ";
        for (int i = 0; i < 6; i++)
        {
            randomString += sampleString[UnityEngine.Random.Range(0, sampleString.Length)];
        }
        return randomString;
    }

    public static int StringToRandomInt(string input)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        long total = 0;
        foreach (byte b in bytes) total = (total * 256 + b) % int.MaxValue;
        return (int)total;
    }
}
