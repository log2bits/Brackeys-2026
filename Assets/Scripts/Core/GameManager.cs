using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

public class GameManager
{
    public enum GameState
    {
        PREGAME,
        INNERROOM,
        OUTERROOM,
        TRANSITIONROOM,
        GAMEOVER
    }

    public GameState state;

    // Managers
    //public ProceduralRoomGen proceduralRoomGenManager;
    public WorldState worldState = new WorldState();

    private static GameManager theInstance;
    public static GameManager Instance
    {
        get
        {
            if (theInstance == null)
                theInstance = new GameManager();
            return theInstance; 
        }
    }

    //public GameObject player;

    private GameManager()
    {
        Debug.Log("GameManager initalized");
        //proceduralRoomGenManager.GenerateProcess();
        ProceduralRoomGen.Instance.GenerateProcess();
    }
    
}
