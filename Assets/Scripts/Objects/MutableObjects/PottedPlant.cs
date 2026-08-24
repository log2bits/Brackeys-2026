using System;
using System.Collections.Generic;
using UnityEngine;
using LogicSolver;

public class PottedPlant : MonoBehaviour
{
    KnownFact FlowerColor = new KnownFact();
    KnownFact PotColor = new KnownFact();

    // Constructor
    public PottedPlant()
    {
        // initialize the plant's potential possible values
        FlowerColor.possibleValues = new string[] {"green", "blue", "yellow", "red"};
        FlowerColor.template = "the flower pot in the last room held a {0} flower";

        PotColor.possibleValues = new string[] {"brown", "black", "white"};
        PotColor.template = "the flower pot in the last room was a {0} pot";
        
    }

    public void GenerateRandomFlower(int randNum)
    {
        FlowerColor.actualValue = FlowerColor.possibleValues[randNum];

    }

    public void GenerateRandomPot(int randNum)
    {
        PotColor.actualValue = PotColor.possibleValues[randNum];
    }
}
