using System.Collections.Generic;
using LogicSolver;

public class PottedPlant : MemorableObjectTemplate
{
    List<KnownFact> sharedList;

    // Constructor
    public PottedPlant()
    {
        // initialize the plant's potential possible values
        GenerateKnownFact(new string[] {"red", "purple", "blue"}, "the flower pot in the last room held a {0} flower");

        GenerateKnownFact(new string[] {"brown", "gray", "green"}, "the flower pot in the last room was a {0} pot");
    }

}
