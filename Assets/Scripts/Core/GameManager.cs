using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

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
        
    }
    
}
