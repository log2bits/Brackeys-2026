using UnityEngine;
using System;
using System.Collections.Generic;
using FMODUnity;

[Serializable]
public class AmbientEnvironment
{
    [SerializeField] private EventReference ambience;
    [SerializeField] private float chance;
    public EventReference GetAmbience()
    {
        return ambience;
    }
    public float GetChance()
    {
        return chance;
    }
}
