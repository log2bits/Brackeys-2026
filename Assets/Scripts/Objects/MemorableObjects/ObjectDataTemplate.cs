using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[CreateAssetMenu(fileName = "ObjectDataTemplate", menuName = "ScriptableObjects/ObjectDataTemplate")]
public class ObjectDataTemplate : ScriptableObject
{
    [SerializeField] private GameObject objectPrefab;
    [SerializeField] private List<ObjectPropertyData> objectPropertyDatas;

    public GameObject GetObjectPrefab()
    {
        return objectPrefab;
    }

    public List<ObjectPropertyData> GetObjectPropertyDatas()
    {
        return objectPropertyDatas;
    }
    
}

[System.Serializable]
public class ObjectPropertyData
{
    public string propertyName;
    public string template;
    public List<string> values;
    public List<Sprite> sprites;
}
