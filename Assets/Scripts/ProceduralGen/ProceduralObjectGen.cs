using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ProceduralHelperGen
{
    public static class ProceduralObjectGen
    {
        
        public static void GenerateRandomForEachSprite(GameObject givenObject, List<ObjectPropertyData> objectPropertyData, System.Random procRandGen)
        {
            MemorableObjectTemplate memObject = givenObject.GetComponent<MemorableObjectTemplate>();
            SpriteRenderer[] allRenderers = memObject.GetSpriteRenderers();
            memObject.Place(objectPropertyData);
            
            for (int idx = 0; idx < allRenderers.Count(); idx++)
            {
                SpriteRenderer sr = allRenderers[idx];
                
                int randActualValue = procRandGen.Next(0, objectPropertyData[idx].sprites.Count);

                memObject.GetActualProperties().Add(objectPropertyData[idx].propertyName, objectPropertyData[idx].values[randActualValue]);

                memObject.SetSprite(idx, objectPropertyData[idx].sprites[randActualValue]);
            }
        }
        public static Sprite GetRandomSprite(SpriteData spriteData, int randNum)
        {
            return spriteData.sprites[randNum];
        }
    }
}