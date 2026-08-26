using UnityEngine;
using System.Collections.Generic;

public class MemorableObjectTemplate : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] spriteRenderers;
    private Dictionary<string, string> actualProperties = new Dictionary<string, string>();
    public int room;
    
    /*
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
    */

    public void SetSprite(int index, Sprite sprite)
    {
        spriteRenderers[index].sprite = sprite;
    }
    public SpriteRenderer[] GetSpriteRenderers()
    {
        return spriteRenderers;
    }
    public Dictionary<string, string> GetActualProperties()
    {
        return actualProperties;
    }
    
}
