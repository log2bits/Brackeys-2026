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

    // Action list below (all called once, not constantly) (relative to the player)
    // DoDeath called when causing death 
    public event Action OnDeath;
    public void DoDamage()
    {
        OnDeath?.Invoke();
    }

    // Note: You can do Action<Vector3> for example, but for dictionary storage we would have to change

    private Dictionary<Action, Delegate> activeWrappers = new();
    // register an action
    public void Register(string eventName, Action listener)
    {
        switch (eventName)
        {
            case "on-damage":
                Action handlerOnDeath = () => listener();
                OnDeath += handlerOnDeath;
                activeWrappers[listener] = handlerOnDeath;
                break;
            default: throw new Exception("Failed Register: Given eventName does not exist as a register - " + eventName);
        }
    
    }

    // unregister an action
    public void Deregister(string eventName, Action listener)
    {
        if (activeWrappers.TryGetValue(listener, out Delegate wrapper))
        {
            switch (eventName)
            {
                case "on-damage":
                    OnDeath -= (Action)wrapper;
                    break;
                default:
                throw new Exception("Failed Deregister: Given eventName does not exist as a Deregister - " + eventName);
            }
        }
        activeWrappers.Remove(listener);
    }
}
