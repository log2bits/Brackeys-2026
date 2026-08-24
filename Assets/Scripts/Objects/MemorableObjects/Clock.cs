using UnityEngine;
using LogicSolver;
using System.Collections.Generic;

public class Clock : MemorableObjectTemplate
{
    List<KnownFact> sharedList;

    // Constructor
    public Clock()
    {
        // initialize the plant's potential possible values
        GenerateKnownFact(new string[] {"two", "four", "six", "eight"}, "the clock in the last room read a {0} o'clock");

        GenerateKnownFact(new string[] {"blue", "red", "black", "white"}, "the clock in the last room was a {0} color");
    }

    
}
