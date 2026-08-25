using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ProceduralHelperGen
{
    public static class ProceduralObjectGen
    {
        public static void GenerateRandomForEachSprite(GameObject givenObject, System.Random procRandGen)
        {
            SpriteRenderer[] allRenderers = givenObject.GetComponentsInChildren<SpriteRenderer>(true);
            List<SpriteData> spriteDatas = givenObject.GetComponent<MemorableObjectTemplate>().GetSpriteDatas();


            for (int rendererIdx = 0; rendererIdx < allRenderers.Count(); rendererIdx++)
            {
                SpriteRenderer sr = allRenderers[rendererIdx];
                // Skip the parent object's renderer
                if (sr.gameObject == givenObject) continue;
                
                Sprite randSprite = GetRandomSprite(spriteDatas[rendererIdx], procRandGen.Next(0, spriteDatas[rendererIdx].sprites.Count));
                
                SetSprite(sr, randSprite);
            }
        }
        public static Sprite GetRandomSprite(SpriteData spriteData, int randNum)
        {
            return spriteData.sprites[randNum];
        }
        public static void SetSprite(SpriteRenderer spriteRenderer, Sprite sprite)
        {
            spriteRenderer.sprite = sprite;
        }
    }
}