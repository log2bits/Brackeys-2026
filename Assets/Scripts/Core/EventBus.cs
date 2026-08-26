using UnityEngine;
using System;
using System.Collections.Generic;

public class EventBus
{
    private static EventBus theInstance;
    public static EventBus Instance
    {
        get
        {
            if (theInstance == null)
                theInstance = new EventBus();
            return theInstance;
        }
    }

    public enum EventName
    {
        GameOver,
        GameStart,
        LostLife
    }


    // Action list below (all called once, not constantly) (relative to the player)
    // DoGameOver called when causing death 
    public event Action OnGameOver;
    public void DoGameOver()
    {
        OnGameOver?.Invoke();
    }
    // OnGameStart called when  
    public event Action OnGameStart;
    public void DoGameStart()
    {
        OnGameStart?.Invoke();
    }

    // OnLostLife called when guessing wrong
    public event Action OnLostLife;
    public void DoLostLife()
    {
        OnLostLife?.Invoke();
    }

    // Note: You can do Action<Vector3> for example, but for dictionary storage we would have to change

    private Dictionary<Action, Delegate> activeEventListeners = new();
    // register an action
    public void Register(EventName eventName, Action listener)
    {
        switch (eventName)
        {
            case EventName.GameOver:
                Action handlerGameOver = () => listener();
                OnGameOver += handlerGameOver;
                activeEventListeners[listener] = handlerGameOver;
                break;
            case EventName.GameStart:
                Action handlerGameStart = () => listener();
                OnGameStart += handlerGameStart;
                activeEventListeners[listener] = handlerGameStart;
                break;
            case EventName.LostLife:
                Action handlerLostLife = () => listener();
                OnLostLife += handlerLostLife;
                activeEventListeners[listener] = handlerLostLife;
                break;
            default: throw new Exception("Failed Register: Given eventName does not exist as a register - " + eventName);
        }
        
    
    }

    // unregister an action
    public void Deregister(EventName eventName, Action listener)
    {
        if (activeEventListeners.TryGetValue(listener, out Delegate wrapper))
        {
            switch (eventName)
            {
                case EventName.GameOver:
                    OnGameOver -= (Action)wrapper;
                    break;
                case EventName.GameStart:
                    OnGameStart -= (Action)wrapper;
                    break;
                case EventName.LostLife:
                    OnLostLife -= (Action)wrapper;
                    break;
                default:
                throw new Exception("Failed Deregister: Given eventName does not exist as a Deregister - " + eventName);
            }
        }
        activeEventListeners.Remove(listener);
    }
}
