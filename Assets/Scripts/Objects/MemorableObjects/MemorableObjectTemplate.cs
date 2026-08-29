using UnityEngine;
using System.Collections.Generic;

public class MemorableObjectTemplate : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] spriteRenderers;
    private Dictionary<string, string> actualProperties = new Dictionary<string, string>();

    private List<ObjectPropertyData> builtFrom;

    public void Place(List<ObjectPropertyData> propertyData)
    {
        builtFrom = propertyData;
        actualProperties.Clear();
    }

    public bool WasBuiltFrom(List<ObjectPropertyData> propertyData)
    {
        return builtFrom == propertyData;
    }

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

    public string GetActualValue(string propertyName)
    {
        string found;
        return actualProperties.TryGetValue(propertyName, out found) ? found : null;
    }
}