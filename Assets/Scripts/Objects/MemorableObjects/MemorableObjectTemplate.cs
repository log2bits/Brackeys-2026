using UnityEngine;
using LogicSolver;
using System.Collections.Generic;

public class MemorableObjectTemplate : MonoBehaviour
{
    protected List<KnownFact> sharedList = new List<KnownFact>();
    [SerializeField] private List<SpriteData> spriteDatas;
    //private GameObject memObject;
    
    // GenerateEmptyFact
    // Generates an undeclared solution fact, and adds to shared list
    public void GenerateEmptyFact(string[] possibleValues, string template)
    {
        KnownFact emptyFact = new KnownFact();
        emptyFact.possibleValues = possibleValues;
        emptyFact.template = template;

        sharedList.Add(emptyFact);
    }

    // SetActualValue
    // Sets the solution of a knownFact within the array, with given randNum, and the index of that fact
    public void SetActualValue(int factIdx, int randNum)
    {
        string potentialValueFound = sharedList[factIdx].possibleValues[randNum];
        sharedList[factIdx].actualValue = potentialValueFound;

        
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

    // GetSpriteDataList
    // Returns the sprite data of this object's list
    public List<SpriteData> GetSpriteDatas()
    {
        return spriteDatas;
    }

}
