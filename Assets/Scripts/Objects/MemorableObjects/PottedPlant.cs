using System.Collections.Generic;
using LogicSolver;

public class PottedPlant : MemorableObjectTemplate
{
    // Constructor
    public PottedPlant()
    {
        // initialize the plant's potential possible values
        GenerateEmptyFact(new string[] {"red", "purple", "blue"}, "the flower pot in the last room held a {0} flower");

        GenerateEmptyFact(new string[] {"brown", "gray", "green"}, "the flower pot in the last room was a {0} pot");
    }

}
