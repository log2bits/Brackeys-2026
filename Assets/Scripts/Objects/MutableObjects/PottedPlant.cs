using System;
using System.Collections.Generic;
using UnityEngine;
using LogicSolver;

public class PottedPlant : MutableObjectTemplate
{
    List<KnownFact> sharedList;

    // Constructor
    public PottedPlant()
    {
        // initialize the plant's potential possible values
        GenerateKnownFact(new string[] {"green", "blue", "yellow", "red"}, "the flower pot in the last room held a {0} flower");

        GenerateKnownFact(new string[] {"brown", "black", "white"}, "the flower pot in the last room was a {0} pot");
    }

}
