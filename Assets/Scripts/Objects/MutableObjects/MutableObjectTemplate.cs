using UnityEngine;
using LogicSolver;
using System.Collections.Generic;

public class MutableObjectTemplate : MonoBehaviour
{
    private List<KnownFact> sharedList;
    
    // GenerateKnownFact
    // Generates an undeclared solution fact, and adds to shared list
    public void GenerateKnownFact(string[] possibleValues, string template)
    {
        KnownFact emptyFact = new KnownFact();
        emptyFact.possibleValues = possibleValues;
        emptyFact.template = template;

        sharedList.Add(emptyFact);
    }

    // GenerateRandomSolution
    // Defines the actual value with the random number given
    public void GenerateRandomSolution(int factIdx, int randNum)
    {
        sharedList[factIdx].actualValue = sharedList[factIdx].possibleValues[randNum];
    }

    // AddSharedList
    public void AddSharedList(KnownFact emptyFact)
    {
        sharedList.Add(emptyFact);
    }

    // ClearSharedList
    public void ClearSharedList()
    {
        sharedList.Clear();
    }

}
